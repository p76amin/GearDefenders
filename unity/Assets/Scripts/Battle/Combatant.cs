using UnityEngine;
using UnityEngine.UI;

namespace GearDefenders
{
    public class Combatant : MonoBehaviour
    {
        public TeamId Team { get; private set; }
        public string DisplayName { get; private set; }
        public float MaxHP { get; private set; }
        public float HP { get; private set; }
        public float ATK { get; private set; }
        public float CritChance { get; private set; }
        public float AttackInterval { get; private set; }
        public float AttackRange { get; private set; }
        public float MoveSpeed { get; private set; }
        public bool IsRanged { get; private set; }
        public bool IsAlive => HP > 0f;
        public UnitDefinition UnitDef { get; private set; }
        public EnemyDefinition EnemyDef { get; private set; }

        public RectTransform Rect { get; private set; }
        BattleArena _arena;
        UnityEngine.UI.Image _body;
        UnityEngine.UI.Image _cooldown;
        UnityEngine.UI.Image _hpFill;
        float _attackTimer;
        Combatant _target;

        public void SetupPlayer(BattleArena arena, UnitDefinition def, Vector2 localPos)
        {
            _arena = arena;
            UnitDef = def;
            Team = TeamId.Player;
            DisplayName = def.displayName;
            MaxHP = HP = def.baseHP;
            ATK = def.baseATK;
            CritChance = def.critChance;
            AttackInterval = def.attackInterval;
            AttackRange = def.attackRange;
            MoveSpeed = def.moveSpeed;
            IsRanged = def.isRanged;
            BuildVisual(def.tint, localPos, arena.UnitSprite);
        }

        public void SetupEnemy(BattleArena arena, EnemyDefinition def, Vector2 localPos)
        {
            _arena = arena;
            EnemyDef = def;
            Team = TeamId.Enemy;
            DisplayName = def.displayName;
            MaxHP = HP = def.hp;
            ATK = def.atk;
            CritChance = def.critChance;
            AttackInterval = def.attackInterval;
            AttackRange = def.attackRange;
            MoveSpeed = def.moveSpeed;
            IsRanged = false;
            BuildVisual(def.tint, localPos, arena.EnemySprite);
        }

        public void Tick(float dt)
        {
            if (!IsAlive) return;
            _target = _arena.FindTarget(this);
            Vector2 pos = Rect.anchoredPosition;

            if (Team == TeamId.Enemy && _target == null)
            {
                Vector2 basePos = _arena.BaseLocalPosition;
                pos = Vector2.MoveTowards(pos, basePos, MoveSpeed * dt);
                Rect.anchoredPosition = pos;
                if (Vector2.Distance(pos, basePos) <= 22f)
                {
                    _arena.DamageBase(ATK);
                    Die(false);
                }
                return;
            }

            if (_target == null || !_target.IsAlive)
            {
                if (Team == TeamId.Player)
                {
                    var hunt = _arena.NearestEnemy(pos);
                    if (hunt != null)
                        Rect.anchoredPosition = Vector2.MoveTowards(pos, hunt.Rect.anchoredPosition, MoveSpeed * dt);
                }
                return;
            }

            float dist = Vector2.Distance(pos, _target.Rect.anchoredPosition);
            if (dist > AttackRange)
            {
                Rect.anchoredPosition = Vector2.MoveTowards(pos, _target.Rect.anchoredPosition, MoveSpeed * dt);
                _attackTimer = Mathf.Max(0f, _attackTimer - dt * 0.25f);
                UpdateCooldown();
                return;
            }

            _attackTimer -= dt;
            if (_attackTimer <= 0f)
            {
                _attackTimer = AttackInterval;
                _arena.ResolveAttack(this, _target);
            }
            UpdateCooldown();
        }

        public void TakeDamage(float amount)
        {
            HP -= amount;
            if (_hpFill != null)
                _hpFill.fillAmount = Mathf.Clamp01(HP / MaxHP);
            if (HP <= 0f)
                Die(true);
        }

        void Die(bool grantKill)
        {
            HP = 0f;
            _arena.NotifyDeath(this, grantKill);
            Destroy(gameObject);
        }

        void UpdateCooldown()
        {
            if (_cooldown == null) return;
            _cooldown.fillAmount = AttackInterval <= 0f ? 0f : Mathf.Clamp01(1f - _attackTimer / AttackInterval);
        }

        void BuildVisual(Color tint, Vector2 localPos, Sprite sprite)
        {
            Rect = transform as RectTransform;
            if (Rect == null)
                Rect = gameObject.AddComponent<RectTransform>();
            Rect.anchorMin = Rect.anchorMax = new Vector2(0.5f, 0.5f);
            Rect.pivot = new Vector2(0.5f, 0.5f);
            Rect.sizeDelta = new Vector2(48, 56);
            Rect.anchoredPosition = localPos;

            _body = gameObject.AddComponent<UnityEngine.UI.Image>();
            _body.sprite = sprite;
            _body.color = tint;
            _body.raycastTarget = false;

            var cdRt = UiFactory.Rect("Cooldown", transform);
            UiFactory.Stretch(cdRt, 0.1f, 0.1f, 0.9f, 0.9f);
            _cooldown = cdRt.gameObject.AddComponent<UnityEngine.UI.Image>();
            _cooldown.sprite = _arena.RingSprite;
            _cooldown.type = UnityEngine.UI.Image.Type.Filled;
            _cooldown.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
            _cooldown.fillOrigin = (int)UnityEngine.UI.Image.Origin360.Top;
            _cooldown.color = new Color(1f, 1f, 1f, 0.35f);
            _cooldown.raycastTarget = false;
            _cooldown.fillAmount = 0f;

            var barBg = UiFactory.Image("HpBack", transform, UiTheme.HpBack);
            UiFactory.Stretch(barBg.rectTransform, 0.08f, 0.0f, 0.92f, 0.12f);
            var fill = UiFactory.Image("HpFill", barBg.transform, UiTheme.HpFill);
            fill.type = UnityEngine.UI.Image.Type.Filled;
            fill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            _hpFill = fill;
            _hpFill.fillAmount = 1f;

            _attackTimer = AttackInterval * 0.35f;
        }
    }
}
