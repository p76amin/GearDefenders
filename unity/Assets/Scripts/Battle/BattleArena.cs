using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GearDefenders
{
    public class BattleArena : MonoBehaviour
    {
        public Sprite UnitSprite { get; private set; }
        public Sprite EnemySprite { get; private set; }
        public Sprite RingSprite { get; private set; }
        public Vector2 BaseLocalPosition { get; private set; }
        public float BaseHP { get; private set; }
        public float BaseMaxHP { get; private set; }
        public int EnemiesSpawned { get; private set; }
        public int EnemiesToSpawn { get; private set; }
        public int LivingEnemies => _enemies.Count;
        public bool AllCleared => EnemiesToSpawn > 0 && EnemiesSpawned >= EnemiesToSpawn && _enemies.Count == 0;

        public event System.Action BaseDestroyed;
        public event System.Action EnemyKilled;
        public event System.Action<float, float> BaseHpChanged;

        readonly List<Combatant> _allies = new List<Combatant>();
        readonly List<Combatant> _enemies = new List<Combatant>();
        readonly List<SpawnJob> _jobs = new List<SpawnJob>();

        RectTransform _field;
        RectTransform _numbersRoot;
        bool _active;

        struct SpawnJob
        {
            public EnemyDefinition Enemy;
            public int Remaining;
            public float Timer;
            public float Delay;
        }

        public void Initialize(RectTransform field, RectTransform numbersRoot, Sprite unitSprite, Sprite enemySprite, Sprite ringSprite, float baseHp)
        {
            _field = field;
            _numbersRoot = numbersRoot;
            UnitSprite = unitSprite;
            EnemySprite = enemySprite;
            RingSprite = ringSprite;
            BaseMaxHP = BaseHP = baseHp;
            BaseLocalPosition = new Vector2(0f, -field.rect.height * 0.42f);
            BaseHpChanged?.Invoke(BaseHP, BaseMaxHP);
        }

        public void BeginRound(RoundDefinition round)
        {
            ClearUnits();
            _jobs.Clear();
            EnemiesSpawned = 0;
            EnemiesToSpawn = round.TotalEnemies;
            _active = true;
            if (round.spawns == null) return;
            for (int i = 0; i < round.spawns.Length; i++)
            {
                var s = round.spawns[i];
                if (s == null || s.enemy == null || s.count <= 0) continue;
                _jobs.Add(new SpawnJob
                {
                    Enemy = s.enemy,
                    Remaining = s.count,
                    Timer = s.startDelay,
                    Delay = Mathf.Max(0.15f, s.spawnDelay)
                });
            }
        }

        public void EndCombat()
        {
            _active = false;
            _jobs.Clear();
        }

        public void ClearUnits()
        {
            for (int i = _allies.Count - 1; i >= 0; i--)
                if (_allies[i] != null) Destroy(_allies[i].gameObject);
            for (int i = _enemies.Count - 1; i >= 0; i--)
                if (_enemies[i] != null) Destroy(_enemies[i].gameObject);
            _allies.Clear();
            _enemies.Clear();
            foreach (Transform child in _field)
            {
                if (child.name.StartsWith("Unit_") || child.name.StartsWith("Enemy_") || child.name.StartsWith("Proj"))
                    Destroy(child.gameObject);
            }
        }

        public void SpawnPlayerUnit(UnitDefinition def)
        {
            if (def == null || _field == null) return;
            float w = _field.rect.width;
            float h = _field.rect.height;
            var pos = new Vector2(Random.Range(-w * 0.32f, w * 0.32f), -h * 0.28f + Random.Range(-12f, 18f));
            var go = new GameObject($"Unit_{def.displayName}", typeof(RectTransform));
            go.transform.SetParent(_field, false);
            var unit = go.AddComponent<Combatant>();
            unit.SetupPlayer(this, def, pos);
            _allies.Add(unit);
        }

        public void Tick(float dt)
        {
            if (!_active) return;
            TickSpawns(dt);
            for (int i = _allies.Count - 1; i >= 0; i--)
            {
                if (_allies[i] == null) { _allies.RemoveAt(i); continue; }
                _allies[i].Tick(dt);
            }
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                if (_enemies[i] == null) { _enemies.RemoveAt(i); continue; }
                _enemies[i].Tick(dt);
            }
        }

        public Combatant FindTarget(Combatant self)
        {
            var list = self.Team == TeamId.Player ? _enemies : _allies;
            Combatant best = null;
            float bestDist = float.MaxValue;
            Vector2 pos = self.Rect.anchoredPosition;
            for (int i = 0; i < list.Count; i++)
            {
                var other = list[i];
                if (other == null || !other.IsAlive) continue;
                float d = Vector2.Distance(pos, other.Rect.anchoredPosition);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = other;
                }
            }
            return best;
        }

        public Combatant NearestEnemy(Vector2 pos)
        {
            Combatant best = null;
            float bestDist = float.MaxValue;
            for (int i = 0; i < _enemies.Count; i++)
            {
                var e = _enemies[i];
                if (e == null || !e.IsAlive) continue;
                float d = Vector2.Distance(pos, e.Rect.anchoredPosition);
                if (d < bestDist) { bestDist = d; best = e; }
            }
            return best;
        }

        public void ResolveAttack(Combatant attacker, Combatant defender)
        {
            if (attacker == null || defender == null || !defender.IsAlive) return;
            bool crit = Random.value < attacker.CritChance;
            float dmg = attacker.ATK * (crit ? 1.5f : 1f);
            if (attacker.IsRanged)
            {
                var go = new GameObject("Proj", typeof(RectTransform));
                go.transform.SetParent(_field, false);
                var proj = go.AddComponent<Projectile>();
                proj.Launch(this, attacker.Rect.anchoredPosition, defender, dmg, crit);
            }
            else
            {
                ApplyHit(defender, dmg, crit);
            }
        }

        public void ApplyHit(Combatant defender, float damage, bool crit)
        {
            if (defender == null || !defender.IsAlive) return;
            defender.TakeDamage(damage);
            SpawnDamageNumber(defender.Rect.anchoredPosition, damage, crit);
        }

        public void DamageBase(float amount)
        {
            BaseHP = Mathf.Max(0f, BaseHP - amount);
            BaseHpChanged?.Invoke(BaseHP, BaseMaxHP);
            if (BaseHP <= 0f)
                BaseDestroyed?.Invoke();
        }

        public void NotifyDeath(Combatant unit, bool grantKill)
        {
            if (unit.Team == TeamId.Player)
                _allies.Remove(unit);
            else
            {
                _enemies.Remove(unit);
                if (grantKill)
                    EnemyKilled?.Invoke();
            }
        }

        void TickSpawns(float dt)
        {
            for (int i = 0; i < _jobs.Count; i++)
            {
                var job = _jobs[i];
                if (job.Remaining <= 0) continue;
                job.Timer -= dt;
                if (job.Timer <= 0f)
                {
                    SpawnEnemy(job.Enemy);
                    job.Remaining--;
                    job.Timer = job.Delay;
                }
                _jobs[i] = job;
            }
        }

        void SpawnEnemy(EnemyDefinition def)
        {
            float w = _field.rect.width;
            float h = _field.rect.height;
            var pos = new Vector2(Random.Range(-w * 0.36f, w * 0.36f), h * 0.42f + Random.Range(-8f, 10f));
            var go = new GameObject($"Enemy_{def.displayName}", typeof(RectTransform));
            go.transform.SetParent(_field, false);
            var unit = go.AddComponent<Combatant>();
            unit.SetupEnemy(this, def, pos);
            _enemies.Add(unit);
            EnemiesSpawned++;
        }

        void SpawnDamageNumber(Vector2 localPos, float damage, bool crit)
        {
            var go = new GameObject("Dmg");
            go.transform.SetParent(_numbersRoot != null ? _numbersRoot : _field, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(80, 28);
            rt.anchoredPosition = localPos + new Vector2(Random.Range(-10f, 10f), 18f);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = Mathf.RoundToInt(damage).ToString();
            tmp.fontSize = crit ? 22 : 16;
            tmp.color = crit ? new Color(1f, 0.85f, 0.25f) : Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            var floater = go.AddComponent<DamageNumberFloater>();
            floater.Play();
        }
    }
}
