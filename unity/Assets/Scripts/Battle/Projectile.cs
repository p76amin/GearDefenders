using UnityEngine;
using UnityEngine.UI;

namespace GearDefenders
{
    public class Projectile : MonoBehaviour
    {
        Combatant _target;
        float _damage;
        bool _crit;
        BattleArena _arena;
        RectTransform _rt;
        const float Speed = 420f;

        public void Launch(BattleArena arena, Vector2 from, Combatant target, float damage, bool crit)
        {
            _arena = arena;
            _target = target;
            _damage = damage;
            _crit = crit;
            _rt = (RectTransform)transform;
            _rt.anchorMin = _rt.anchorMax = new Vector2(0.5f, 0.5f);
            _rt.sizeDelta = new Vector2(14, 8);
            _rt.anchoredPosition = from;
            var img = gameObject.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(1f, 0.92f, 0.45f);
            img.raycastTarget = false;
        }

        void Update()
        {
            if (_target == null || !_target.IsAlive)
            {
                Destroy(gameObject);
                return;
            }

            Vector2 dest = _target.Rect.anchoredPosition;
            _rt.anchoredPosition = Vector2.MoveTowards(_rt.anchoredPosition, dest, Speed * Time.deltaTime);
            Vector2 dir = dest - _rt.anchoredPosition;
            if (dir.sqrMagnitude > 1f)
                _rt.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

            if (Vector2.Distance(_rt.anchoredPosition, dest) <= 8f)
            {
                _arena.ApplyHit(_target, _damage, _crit);
                Destroy(gameObject);
            }
        }
    }
}
