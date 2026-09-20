using TMPro;
using UnityEngine;

namespace GearDefenders
{
    public class DamageNumberFloater : MonoBehaviour
    {
        TextMeshProUGUI _tmp;
        RectTransform _rt;
        float _life = 0.7f;
        float _age;

        public void Play()
        {
            _tmp = GetComponent<TextMeshProUGUI>();
            _rt = (RectTransform)transform;
        }

        void Update()
        {
            _age += Time.deltaTime;
            _rt.anchoredPosition += new Vector2(0f, 48f * Time.deltaTime);
            if (_tmp != null)
            {
                var c = _tmp.color;
                c.a = 1f - Mathf.Clamp01(_age / _life);
                _tmp.color = c;
            }
            if (_age >= _life)
                Destroy(gameObject);
        }
    }
}
