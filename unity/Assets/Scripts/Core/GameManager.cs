using UnityEngine;

namespace GearDefenders
{
    public class GameManager : MonoBehaviour
    {
        public GamePhase Phase { get; private set; } = GamePhase.Shop;
        public int CurrentRound { get; private set; } = 1;
        public float ShopTimeRemaining { get; private set; }
        public int SpeedMultiplier { get; private set; } = 1;
        public bool Paused { get; private set; }

        public GameCatalog Catalog { get; private set; }
        public EconomyManager Economy { get; private set; }
        public GridController Grid { get; private set; }
        public ShopController Shop { get; private set; }
        public BattleArena Arena { get; private set; }
        public HUDController Hud { get; private set; }

        OverlayPopupUI _overlay;
        float _rewardTimer;
        float _shopDuration;
        bool _baseDead;

        public RoundDefinition CurrentRoundDef =>
            Catalog != null && Catalog.rounds != null && CurrentRound >= 1 && CurrentRound <= Catalog.rounds.Length
                ? Catalog.rounds[CurrentRound - 1]
                : null;

        public void Wire(
            GameCatalog catalog,
            EconomyManager economy,
            GridController grid,
            ShopController shop,
            BattleArena arena,
            HUDController hud,
            OverlayPopupUI overlay)
        {
            Catalog = catalog;
            Economy = economy;
            Grid = grid;
            Shop = shop;
            Arena = arena;
            Hud = hud;
            _overlay = overlay;

            Grid.UnitProduced += OnUnitProduced;
            Arena.EnemyKilled += OnEnemyKilled;
            Arena.BaseDestroyed += OnBaseDestroyed;
            Arena.BaseHpChanged += Hud.SetBaseHp;
            Economy.CoinsChanged += Hud.SetCoins;
            Economy.KillsChanged += Hud.SetKills;
        }

        public void StartRun()
        {
            CurrentRound = 1;
            SpeedMultiplier = 1;
            Paused = false;
            _baseDead = false;
            ApplyTimeScale();
            Economy.ResetRun(Catalog.board.startingCoins);
            Hud.SetSpeed(SpeedMultiplier);
            EnterShop();
        }

        public void TogglePause()
        {
            if (Phase == GamePhase.GameOver || Phase == GamePhase.StageComplete) return;
            Paused = !Paused;
            ApplyTimeScale();
            if (Paused)
                _overlay.Show("Paused", "Tap Pause again to resume.");
            else if (Phase != GamePhase.Reward)
                _overlay.Hide();
        }

        public void ToggleSpeed()
        {
            SpeedMultiplier = SpeedMultiplier == 1 ? 2 : 1;
            Hud.SetSpeed(SpeedMultiplier);
            ApplyTimeScale();
        }

        public void RequestBattle()
        {
            if (Phase != GamePhase.Shop || Paused) return;
            EnterBattle();
        }

        public void RequestRefresh()
        {
            if (Phase != GamePhase.Shop || Paused) return;
            Shop.TryRefresh();
        }

        public void ShowStatsPlaceholder()
        {
            _overlay.Show("Stats", "Run stats overlay is a placeholder in MVP.");
        }

        void Update()
        {
            if (Paused) return;

            float dt = Time.deltaTime;
            Grid.Tick(dt, Phase == GamePhase.Battle);
            Arena.Tick(dt);

            switch (Phase)
            {
                case GamePhase.Shop:
                    ShopTimeRemaining -= dt;
                    Hud.SetTimer(ShopTimeRemaining, _shopDuration);
                    if (ShopTimeRemaining <= 0f)
                        EnterBattle();
                    break;
                case GamePhase.Battle:
                    if (_baseDead) break;
                    if (Arena.AllCleared)
                        EnterReward();
                    break;
                case GamePhase.Reward:
                    _rewardTimer -= dt;
                    if (_rewardTimer <= 0f)
                        AdvanceRound();
                    break;
            }
        }

        void EnterShop()
        {
            Phase = GamePhase.Shop;
            var round = CurrentRoundDef;
            _shopDuration = round != null ? round.shopDuration : 20f;
            ShopTimeRemaining = _shopDuration;
            Shop.IsEditable = true;
            Grid.IsEditable = true;
            Shop.Reroll();
            Economy.PrepareRound(round != null ? round.TotalEnemies : 0);
            Hud.SetRound(CurrentRound, Catalog.board.totalRounds, Catalog.board.levelLabel);
            Hud.SetTimer(ShopTimeRemaining, _shopDuration);
            Hud.SetEnemyPreview(round);
            Arena.EndCombat();
            Arena.ClearUnits();
            _overlay.Hide();
        }

        void EnterBattle()
        {
            Phase = GamePhase.Battle;
            Shop.IsEditable = false;
            Grid.IsEditable = false;
            Shop.ClearSelection();
            Hud.SetTimer(0f, 1f);
            var round = CurrentRoundDef;
            if (round != null)
                Arena.BeginRound(round);
            SpawnOpeningArmy();
        }

        void SpawnOpeningArmy()
        {
            int w = Catalog.board.gridWidth;
            int h = Catalog.board.gridHeight;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var tile = Grid.Tiles[x, y];
                    if (tile.CanProduce)
                        Arena.SpawnPlayerUnit(tile.Unit);
                }
            }
        }

        void EnterReward()
        {
            Phase = GamePhase.Reward;
            Arena.EndCombat();
            _rewardTimer = Catalog.board.rewardDuration;
            _overlay.Show("Round Clear", $"Wave {CurrentRound} complete.\nBoard and coins carry over.");
        }

        void AdvanceRound()
        {
            if (CurrentRound >= Catalog.board.totalRounds)
            {
                Phase = GamePhase.StageComplete;
                _overlay.Show("Stage Complete", "12/12 waves cleared. The Power Core holds.");
                return;
            }

            CurrentRound++;
            EnterShop();
        }

        void OnUnitProduced(GearTileState tile)
        {
            if (Phase != GamePhase.Battle || tile.Unit == null) return;
            Arena.SpawnPlayerUnit(tile.Unit);
        }

        void OnEnemyKilled()
        {
            Economy.RegisterKill();
        }

        void OnBaseDestroyed()
        {
            if (_baseDead) return;
            _baseDead = true;
            Phase = GamePhase.GameOver;
            Arena.EndCombat();
            Shop.IsEditable = false;
            Grid.IsEditable = false;
            _overlay.Show("Defeat", "The Guardian fell. The Power Core is overrun.");
        }

        void ApplyTimeScale()
        {
            Time.timeScale = Paused ? 0f : SpeedMultiplier;
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
