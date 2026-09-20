using System.Collections;
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
        RectTransform _bodyRt;
        UnityEngine.UI.Image _cooldown;
        UnityEngine.UI.Image _hpFill;
        UiSpriteAnimator _animator;
        float _attackTimer;
        Combatant _target;
        bool _dying;

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
            var set = def.animSet != null ? def.animSet : CombatantArtLibrary.ForUnitId(def.id);
            var portrait = set != null && set.Portrait != null ? set.Portrait : (def.sprite != null ? def.sprite : arena.UnitSprite);
            BuildVisual(CombatantArtLibrary.VisualTint(def.tint, def.rank), localPos, portrait, set);
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
            var set = def.animSet != null ? def.animSet : CombatantArtLibrary.ForEnemyId(def.id);
            var portrait = set != null && set.Portrait != null ? set.Portrait : (def.sprite != null ? def.sprite : arena.EnemySprite);
            BuildVisual(CombatantArtLibrary.VisualTint(def.tint, 1), localPos, portrait, set);
        }

        public void Tick(float dt)
        {
            if (!IsAlive || _dying) return;
            _target = _arena.FindTarget(this);
            Vector2 pos = Rect.anchoredPosition;
            bool moving = false;

            if (Team == TeamId.Enemy && _target == null)
            {
                Vector2 basePos = _arena.BaseLocalPosition;
                Vector2 next = Vector2.MoveTowards(pos, basePos, MoveSpeed * dt);
                moving = (next - pos).sqrMagnitude > 0.01f;
                ApplyMove(pos, next);
                if (Vector2.Distance(next, basePos) <= 22f)
                {
                    _arena.DamageBase(ATK);
                    Die(false);
                }
                SetMoving(moving);
                return;
            }

            if (_target == null || !_target.IsAlive)
            {
                if (Team == TeamId.Player)
                {
                    var hunt = _arena.NearestEnemy(pos);
                    if (hunt != null)
                    {
                        Vector2 next = Vector2.MoveTowards(pos, hunt.Rect.anchoredPosition, MoveSpeed * dt);
                        moving = (next - pos).sqrMagnitude > 0.01f;
                        ApplyMove(pos, next);
                    }
                }
                SetMoving(moving);
                return;
            }

            float dist = Vector2.Distance(pos, _target.Rect.anchoredPosition);
            if (dist > AttackRange)
            {
                Vector2 next = Vector2.MoveTowards(pos, _target.Rect.anchoredPosition, MoveSpeed * dt);
                moving = (next - pos).sqrMagnitude > 0.01f;
                ApplyMove(pos, next);
                _attackTimer = Mathf.Max(0f, _attackTimer - dt * 0.25f);
                UpdateCooldown();
                SetMoving(moving);
                return;
            }

            SetMoving(false);
            _attackTimer -= dt;
            if (_attackTimer <= 0f)
            {
                _attackTimer = AttackInterval;
                _animator?.PlayAttack();
                _arena.ResolveAttack(this, _target);
            }
            UpdateCooldown();
        }

        public void TakeDamage(float amount)
        {
            if (_dying) return;
            HP -= amount;
            if (_hpFill != null)
                _hpFill.fillAmount = Mathf.Clamp01(HP / MaxHP);
            if (HP <= 0f)
                Die(true);
            else
                _animator?.PlayHurt();
        }

        void Die(bool grantKill)
        {
            if (_dying) return;
            _dying = true;
            HP = 0f;
            _arena.NotifyDeath(this, grantKill);
            if (_cooldown != null)
                _cooldown.enabled = false;
            if (_animator != null)
            {
                bool waiting = false;
                _animator.PlayDeath(() =>
                {
                    if (waiting)
                        StartCoroutine(DestroySoon(0.08f));
                });
                waiting = _animator.IsPlayingDeath;
                if (waiting)
                    return;
            }

            Destroy(gameObject);
        }

        IEnumerator DestroySoon(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (this != null)
                Destroy(gameObject);
        }

        void SetMoving(bool moving)
        {
            _animator?.SetMoving(moving);
        }

        void ApplyMove(Vector2 from, Vector2 to)
        {
            Rect.anchoredPosition = to;
            float dx = to.x - from.x;
            if (_bodyRt == null || Mathf.Abs(dx) < 0.05f)
                return;
            var scale = _bodyRt.localScale;
            scale.x = Mathf.Abs(scale.x) * (dx < 0f ? -1f : 1f);
            _bodyRt.localScale = scale;
        }

        void UpdateCooldown()
        {
            if (_cooldown == null) return;
            _cooldown.fillAmount = AttackInterval <= 0f ? 0f : Mathf.Clamp01(1f - _attackTimer / AttackInterval);
        }

        void BuildVisual(Color tint, Vector2 localPos, Sprite sprite, CombatantAnimSet animSet)
        {
            Rect = transform as RectTransform;
            if (Rect == null)
                Rect = gameObject.AddComponent<RectTransform>();
            Rect.anchorMin = Rect.anchorMax = new Vector2(0.5f, 0.5f);
            Rect.pivot = new Vector2(0.5f, 0.5f);
            Rect.sizeDelta = new Vector2(72, 88);
            Rect.anchoredPosition = localPos;

            var bodyGo = new GameObject("Body", typeof(RectTransform));
            bodyGo.transform.SetParent(transform, false);
            _bodyRt = (RectTransform)bodyGo.transform;
            _bodyRt.anchorMin = _bodyRt.anchorMax = new Vector2(0.5f, 0.5f);
            _bodyRt.pivot = new Vector2(0.5f, 0.15f);
            _bodyRt.sizeDelta = new Vector2(72, 80);
            _bodyRt.anchoredPosition = Vector2.zero;
            _body = bodyGo.AddComponent<UnityEngine.UI.Image>();
            _body.sprite = sprite;
            _body.color = tint;
            _body.preserveAspect = true;
            _body.raycastTarget = false;

            _animator = gameObject.AddComponent<UiSpriteAnimator>();
            if (animSet != null)
                _animator.Bind(_body, animSet);

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
