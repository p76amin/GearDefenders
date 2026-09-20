using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GearDefenders
{
    public class GameplayBootstrap : MonoBehaviour
    {
        [SerializeField] GameCatalog catalog;

        void Awake()
        {
            Application.runInBackground = true;
            if (catalog == null)
                catalog = RuntimeCatalogFactory.Create();

            CombatantArtLibrary.Bind(catalog);
            EnsureEventSystem();
            Build(catalog);
        }

        static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        void Build(GameCatalog catalog)
        {
            var gearSprite = CombatantArtLibrary.GearSprite;
            if (gearSprite == null && catalog.board != null)
                gearSprite = catalog.board.gearSprite;
            if (gearSprite == null)
                gearSprite = PlaceholderArt.Gear(Color.white);

            var coreSprite = CombatantArtLibrary.CoreSprite;
            if (coreSprite == null && catalog.board != null)
                coreSprite = catalog.board.coreSprite;
            if (coreSprite == null)
                coreSprite = PlaceholderArt.Gear(UiTheme.Core, 128, 10);

            var unitSprite = CombatantArtLibrary.Archer() != null && CombatantArtLibrary.Archer().Portrait != null
                ? CombatantArtLibrary.Archer().Portrait
                : PlaceholderArt.Triangle(Color.white);
            var enemySprite = CombatantArtLibrary.Grunt() != null && CombatantArtLibrary.Grunt().Portrait != null
                ? CombatantArtLibrary.Grunt().Portrait
                : PlaceholderArt.Circle(Color.white, 48, 0.46f);
            var ringSprite = PlaceholderArt.Ring(Color.white, 64, 0.48f, 0.32f);
            var cactus = PlaceholderArt.Cactus(UiTheme.Cactus);

            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(576, 1280);
            scaler.matchWidthOrHeight = 0.5f;

            var root = canvasGo.GetComponent<RectTransform>();
            UiFactory.Stretch(root);

            var top = CreateRegion("TopArena", root, 0.66f, 1f, UiTheme.ArenaSky);
            var board = CreateRegion("BoardGrid", root, 0.25f, 0.66f, UiTheme.BoardBg);
            var bottom = CreateRegion("BottomBar", root, 0f, 0.25f, UiTheme.BottomBg);

            BuildArenaDecor(top, cactus);
            var field = BuildArenaField(top);
            var hudBits = BuildHud(top);
            var guardianBits = BuildGuardian(top);
            var previewRoot = BuildPreviewRow(top);
            var gridRoot = BuildGridRoot(board);
            var shopBits = BuildBottom(bottom);

            var tooltipGo = UiFactory.Image("Tooltip", root, new Color(0.1f, 0.07f, 0.04f, 0.94f), null, true);
            UiFactory.AnchorFill(tooltipGo.rectTransform, new Vector2(0.12f, 0.28f), new Vector2(0.88f, 0.42f), Vector2.zero, Vector2.zero);
            var tipTitle = UiFactory.Label("Title", tooltipGo.transform, "", 22, UiTheme.Accent, TextAlignmentOptions.Top);
            UiFactory.Stretch(tipTitle.rectTransform, 0.06f, 0.58f, 0.94f, 0.94f);
            var tipBody = UiFactory.Label("Body", tooltipGo.transform, "", 18, Color.white, TextAlignmentOptions.TopLeft);
            UiFactory.Stretch(tipBody.rectTransform, 0.06f, 0.08f, 0.94f, 0.58f);
            tipBody.textWrappingMode = TextWrappingModes.Normal;
            var tooltip = tooltipGo.gameObject.AddComponent<TooltipCardUI>();
            tooltip.Bind(tipTitle, tipBody);

            var overlayGo = UiFactory.Image("Overlay", root, new Color(0f, 0f, 0f, 0.62f), null, false);
            UiFactory.Stretch(overlayGo.rectTransform);
            var panel = UiFactory.Image("Panel", overlayGo.transform, new Color(0.16f, 0.1f, 0.06f, 0.96f));
            UiFactory.AnchorFill(panel.rectTransform, new Vector2(0.12f, 0.38f), new Vector2(0.88f, 0.62f), Vector2.zero, Vector2.zero);
            var ovTitle = UiFactory.Label("Title", panel.transform, "", 30, UiTheme.Accent);
            UiFactory.Stretch(ovTitle.rectTransform, 0.06f, 0.55f, 0.94f, 0.92f);
            var ovBody = UiFactory.Label("Body", panel.transform, "", 20, Color.white);
            ovBody.textWrappingMode = TextWrappingModes.Normal;
            UiFactory.Stretch(ovBody.rectTransform, 0.08f, 0.1f, 0.92f, 0.55f);
            var overlay = overlayGo.gameObject.AddComponent<OverlayPopupUI>();
            overlay.Bind(ovTitle, ovBody);

            var systems = new GameObject("Systems");
            var game = systems.AddComponent<GameManager>();
            var economy = systems.AddComponent<EconomyManager>();
            var grid = systems.AddComponent<GridController>();
            var shop = systems.AddComponent<ShopController>();
            var arena = systems.AddComponent<BattleArena>();
            var hud = systems.AddComponent<HUDController>();

            hud.Bind(hudBits.round, hudBits.level, hudBits.timer, hudBits.speed, shopBits.coins, shopBits.kills, guardianBits.hpLabel, guardianBits.hpFill, hudBits.timerFill, previewRoot, enemySprite);
            Canvas.ForceUpdateCanvases();
            grid.Initialize(catalog.board, shop, gridRoot, gearSprite, coreSprite, tooltip);
            shop.Initialize(catalog, economy, grid, shopBits.cards, gearSprite);
            arena.Initialize(field, field, unitSprite, enemySprite, ringSprite, catalog.board.startingBaseHP);
            game.Wire(catalog, economy, grid, shop, arena, hud, overlay);

            hudBits.pause.onClick.AddListener(game.TogglePause);
            hudBits.stats.onClick.AddListener(game.ShowStatsPlaceholder);
            hudBits.speedBtn.onClick.AddListener(game.ToggleSpeed);
            shopBits.refresh.onClick.AddListener(game.RequestRefresh);
            shopBits.battle.onClick.AddListener(game.RequestBattle);

            overlayGo.transform.SetAsLastSibling();
            tooltipGo.transform.SetAsLastSibling();

            game.StartRun();
        }

        static RectTransform CreateRegion(string name, Transform parent, float minY, float maxY, Color color)
        {
            var img = UiFactory.Image(name, parent, color);
            UiFactory.Stretch(img.rectTransform, 0f, minY, 1f, maxY);
            return img.rectTransform;
        }

        static void BuildArenaDecor(RectTransform top, Sprite cactus)
        {
            PlaceCactus(top, cactus, 0.12f, 0.42f);
            PlaceCactus(top, cactus, 0.86f, 0.5f);
            PlaceCactus(top, cactus, 0.22f, 0.28f);
            var ground = UiFactory.Image("Ground", top, UiTheme.Sand);
            UiFactory.Stretch(ground.rectTransform, 0f, 0f, 1f, 0.38f);
            var dune = UiFactory.Image("Dune", top, UiTheme.SandDark);
            UiFactory.Stretch(dune.rectTransform, 0f, 0.18f, 1f, 0.28f);
        }

        static void PlaceCactus(RectTransform top, Sprite cactus, float ax, float ay)
        {
            var img = UiFactory.Image("Cactus", top, Color.white, cactus);
            img.preserveAspect = true;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
            rt.sizeDelta = new Vector2(36, 56);
        }

        static RectTransform BuildArenaField(RectTransform top)
        {
            var field = UiFactory.Rect("ArenaField", top);
            UiFactory.Stretch(field, 0.04f, 0.34f, 0.96f, 0.78f);
            return field;
        }

        static (UnityEngine.UI.Button pause, UnityEngine.UI.Button stats, UnityEngine.UI.Button speedBtn, TextMeshProUGUI speed, TextMeshProUGUI round, TextMeshProUGUI level, TextMeshProUGUI timer, UnityEngine.UI.Image timerFill)
            BuildHud(RectTransform top)
        {
            var bar = UiFactory.Image("HudBar", top, UiTheme.HudDark);
            UiFactory.Stretch(bar.rectTransform, 0f, 0.78f, 1f, 1f);

            var pause = UiFactory.Button("Pause", bar.transform, "II", UiTheme.PauseBtn, 18);
            UiFactory.AnchorFill(pause.GetComponent<RectTransform>(), new Vector2(0.02f, 0.52f), new Vector2(0.14f, 0.92f), Vector2.zero, Vector2.zero);

            var stats = UiFactory.Button("Stats", bar.transform, "=", UiTheme.PauseBtn, 20);
            UiFactory.AnchorFill(stats.GetComponent<RectTransform>(), new Vector2(0.15f, 0.52f), new Vector2(0.27f, 0.92f), Vector2.zero, Vector2.zero);

            var speed = UiFactory.Button("Speed", bar.transform, "x1", UiTheme.SpeedBtn, 18);
            UiFactory.AnchorFill(speed.GetComponent<RectTransform>(), new Vector2(0.02f, 0.08f), new Vector2(0.16f, 0.46f), Vector2.zero, Vector2.zero);
            var speedLabel = speed.transform.Find("Label").GetComponent<TextMeshProUGUI>();

            var buff = UiFactory.Button("Buff", bar.transform, "Buff", new Color(0.35f, 0.28f, 0.18f), 14);
            UiFactory.AnchorFill(buff.GetComponent<RectTransform>(), new Vector2(0.17f, 0.08f), new Vector2(0.34f, 0.46f), Vector2.zero, Vector2.zero);
            buff.interactable = false;

            var level = UiFactory.Label("Level", bar.transform, "Level 12", 16, UiTheme.TextDim, TextAlignmentOptions.TopRight);
            UiFactory.Stretch(level.rectTransform, 0.55f, 0.52f, 0.82f, 0.95f);
            var round = UiFactory.Label("Round", bar.transform, "Wave 1/12", 20, Color.white, TextAlignmentOptions.BottomRight);
            UiFactory.Stretch(round.rectTransform, 0.5f, 0.08f, 0.82f, 0.6f);

            var timerBg = UiFactory.Image("TimerBg", bar.transform, new Color(0.1f, 0.08f, 0.05f, 0.9f));
            UiFactory.AnchorFill(timerBg.rectTransform, new Vector2(0.84f, 0.08f), new Vector2(0.98f, 0.92f), Vector2.zero, Vector2.zero);
            var timerFill = UiFactory.Image("TimerFill", timerBg.transform, UiTheme.Accent);
            timerFill.sprite = PlaceholderArt.Ring(UiTheme.Accent, 64, 0.48f, 0.28f);
            timerFill.type = UnityEngine.UI.Image.Type.Filled;
            timerFill.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
            timerFill.fillOrigin = (int)UnityEngine.UI.Image.Origin360.Top;
            timerFill.fillClockwise = false;
            var timer = UiFactory.Label("Timer", timerBg.transform, "20", 20, Color.white);

            return (pause, stats, speed, speedLabel, round, level, timer, timerFill);
        }

        static (TextMeshProUGUI hpLabel, UnityEngine.UI.Image hpFill) BuildGuardian(RectTransform top)
        {
            var hero = UiFactory.Image("Guardian", top, UiTheme.Guardian, PlaceholderArt.Triangle(UiTheme.Guardian));
            hero.preserveAspect = true;
            var heroRt = hero.rectTransform;
            heroRt.anchorMin = heroRt.anchorMax = new Vector2(0.5f, 0.30f);
            heroRt.sizeDelta = new Vector2(54, 64);

            var name = UiFactory.Label("GuardianName", top, "Guardian", 14, Color.white, TextAlignmentOptions.MidlineLeft);
            name.overflowMode = TextOverflowModes.Overflow;
            UiFactory.AnchorFill(name.rectTransform, new Vector2(0.04f, 0.255f), new Vector2(0.28f, 0.30f), Vector2.zero, Vector2.zero);

            var hpBack = UiFactory.Image("BaseHpBack", top, UiTheme.HpBack, null, false);
            UiFactory.Stretch(hpBack.rectTransform, 0.3f, 0.205f, 0.92f, 0.255f);
            var hpFill = UiFactory.Image("BaseHpFill", hpBack.transform, UiTheme.HpFill);
            hpFill.type = UnityEngine.UI.Image.Type.Filled;
            hpFill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            var hpLabel = UiFactory.Label("HpValue", hpBack.transform, "930", 16, Color.white);

            var bridge = UiFactory.Image("Bridge", top, UiTheme.Bridge);
            UiFactory.Stretch(bridge.rectTransform, 0.08f, 0.08f, 0.92f, 0.19f);
            for (int i = 0; i < 3; i++)
            {
                float x = 0.18f + i * 0.28f;
                var lane = UiFactory.Image($"Lane{i + 1}", bridge.transform, Color.Lerp(UiTheme.Bridge, Color.black, 0.25f));
                UiFactory.Stretch(lane.rectTransform, x, 0.15f, x + 0.16f, 0.85f);
                UiFactory.Label("N", lane.transform, (i + 1).ToString(), 16, Color.white);
            }

            return (hpLabel, hpFill);
        }

        static RectTransform BuildPreviewRow(RectTransform top)
        {
            var row = UiFactory.Rect("EnemyPreview", top);
            UiFactory.Stretch(row, 0.2f, 0.78f, 0.8f, 0.85f);
            var layout = row.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 8f;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            return row;
        }

        static RectTransform BuildGridRoot(RectTransform board)
        {
            var title = UiFactory.Label("BoardHint", board, "Connect gears to the Power Core", 14, UiTheme.TextDim);
            UiFactory.Stretch(title.rectTransform, 0.04f, 0.94f, 0.96f, 0.99f);

            var grid = UiFactory.Rect("Grid", board);
            UiFactory.Stretch(grid, 0.04f, 0.03f, 0.96f, 0.93f);
            var layout = grid.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            layout.spacing = new Vector2(6, 6);
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.startCorner = UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft;
            layout.startAxis = UnityEngine.UI.GridLayoutGroup.Axis.Horizontal;
            return grid;
        }

        static (ShopCardView[] cards, UnityEngine.UI.Button refresh, UnityEngine.UI.Button battle, TextMeshProUGUI coins, TextMeshProUGUI kills)
            BuildBottom(RectTransform bottom)
        {
            var eco = UiFactory.Rect("Economy", bottom);
            UiFactory.Stretch(eco, 0.04f, 0.78f, 0.96f, 0.98f);
            var kills = UiFactory.Label("Kills", eco, "Kills 0/0", 22, Color.white, TextAlignmentOptions.Left);
            UiFactory.Stretch(kills.rectTransform, 0f, 0f, 0.55f, 1f);
            var coins = UiFactory.Label("Coins", eco, "Coins 40", 22, UiTheme.Coin, TextAlignmentOptions.Right);
            UiFactory.Stretch(coins.rectTransform, 0.45f, 0f, 1f, 1f);

            var cardsRoot = UiFactory.Rect("Cards", bottom);
            UiFactory.Stretch(cardsRoot, 0.03f, 0.30f, 0.97f, 0.76f);
            var h = cardsRoot.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            h.spacing = 10f;
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childForceExpandWidth = true;
            h.childForceExpandHeight = true;
            h.childControlWidth = true;
            h.childControlHeight = true;

            var cards = new ShopCardView[3];
            for (int i = 0; i < 3; i++)
            {
                var cardImg = UiFactory.Image($"Card{i + 1}", cardsRoot, UiTheme.CellStroke, null, true);
                var card = cardImg.gameObject.AddComponent<ShopCardView>();
                var icon = UiFactory.Image("Icon", cardImg.transform, Color.white);
                UiFactory.Stretch(icon.rectTransform, 0.18f, 0.34f, 0.82f, 0.88f);
                var name = UiFactory.Label("Name", cardImg.transform, "", 14, Color.white, TextAlignmentOptions.Bottom);
                UiFactory.Stretch(name.rectTransform, 0.04f, 0.16f, 0.96f, 0.36f);
                var cost = UiFactory.Label("Cost", cardImg.transform, "", 16, UiTheme.Coin, TextAlignmentOptions.Bottom);
                UiFactory.Stretch(cost.rectTransform, 0.04f, 0.02f, 0.96f, 0.18f);
                cards[i] = card;
            }

            var actions = UiFactory.Rect("Actions", bottom);
            UiFactory.Stretch(actions, 0.03f, 0.04f, 0.97f, 0.28f);
            var refresh = UiFactory.Button("Refresh", actions, "Refresh  3", UiTheme.RefreshBtn, 20);
            UiFactory.Stretch(refresh.GetComponent<RectTransform>(), 0f, 0f, 0.62f, 1f);
            var battle = UiFactory.Button("Battle", actions, "Battle", UiTheme.BattleBtn, 24);
            UiFactory.Stretch(battle.GetComponent<RectTransform>(), 0.66f, 0f, 1f, 1f);

            return (cards, refresh, battle, coins, kills);
        }
    }
}
