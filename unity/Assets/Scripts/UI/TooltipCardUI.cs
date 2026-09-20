using TMPro;
using UnityEngine;

namespace GearDefenders
{
    public class TooltipCardUI : MonoBehaviour
    {
        TextMeshProUGUI _title;
        TextMeshProUGUI _body;
        float _hideAt;

        public void Bind(TextMeshProUGUI title, TextMeshProUGUI body)
        {
            _title = title;
            _body = body;
            gameObject.SetActive(false);
        }

        public void ShowUnit(GearTileState state)
        {
            if (state == null || state.Unit == null) return;
            var u = state.Unit;
            _title.text = $"{u.displayName}  ·  Rank {state.Rank}";
            _body.text = $"Lv {state.Rank}   HP {u.baseHP:0}   ATK {u.baseATK:0}\nCrit {u.critChance * 100f:0}%   Atk Speed {1f / Mathf.Max(0.05f, u.attackInterval):0.00}";
            gameObject.SetActive(true);
            _hideAt = Time.unscaledTime + 3.2f;
        }

        void Update()
        {
            if (gameObject.activeSelf && Time.unscaledTime >= _hideAt)
                gameObject.SetActive(false);
        }
    }
}
