using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GearDefenders
{
    public class ShopCardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public bool IsEmpty { get; private set; } = true;
        public bool IsUnit { get; private set; }
        public UnitDefinition Unit { get; private set; }
        public BoostDefinition Boost { get; private set; }
        public int Cost { get; private set; }

        UnityEngine.UI.Image _frame;
        UnityEngine.UI.Image _icon;
        TextMeshProUGUI _name;
        TextMeshProUGUI _cost;
        ShopController _shop;
        GameObject _ghost;
        bool _dragging;

        public void Bind(ShopController shop)
        {
            _shop = shop;
            _frame = GetComponent<UnityEngine.UI.Image>();
            _icon = transform.Find("Icon").GetComponent<UnityEngine.UI.Image>();
            _name = transform.Find("Name").GetComponent<TextMeshProUGUI>();
            _cost = transform.Find("Cost").GetComponent<TextMeshProUGUI>();
            SetEmpty();
        }

        public void SetUnit(UnitDefinition unit)
        {
            IsEmpty = false;
            IsUnit = true;
            Unit = unit;
            Boost = null;
            Cost = unit.shopCost;
            ApplyVisual(unit.displayName, unit.tint, unit.rank);
        }

        public void SetBoost(BoostDefinition boost)
        {
            IsEmpty = false;
            IsUnit = false;
            Unit = null;
            Boost = boost;
            Cost = boost.shopCost;
            ApplyVisual(boost.displayName, boost.tint, boost.rank);
        }

        public void SetEmpty()
        {
            IsEmpty = true;
            IsUnit = false;
            Unit = null;
            Boost = null;
            Cost = 0;
            _icon.enabled = false;
            _name.text = "—";
            _cost.text = "";
            _frame.color = new Color(0.18f, 0.13f, 0.1f, 1f);
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (IsEmpty)
            {
                _frame.color = new Color(0.18f, 0.13f, 0.1f, 1f);
                return;
            }
            _frame.color = selected ? Color.Lerp(UiTheme.Accent, Color.white, 0.2f) : Color.Lerp(UiTheme.CellStroke, Color.black, 0.2f);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_dragging || IsEmpty) return;
            _shop.SelectCard(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsEmpty || !_shop.IsEditable) return;
            _dragging = true;
            _shop.SelectCard(this);
            _ghost = CreateGhost();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_ghost != null)
                _ghost.transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_ghost != null) Destroy(_ghost);
            _ghost = null;
            _dragging = false;
        }

        void ApplyVisual(string displayName, Color tint, int rank)
        {
            _icon.enabled = true;
            _icon.sprite = _shop.GearSprite;
            _icon.color = tint;
            _name.text = displayName + "  R" + rank;
            _cost.text = Cost + "c";
            _frame.color = Color.Lerp(UiTheme.CellStroke, Color.black, 0.2f);
        }

        GameObject CreateGhost()
        {
            var canvas = GetComponentInParent<Canvas>();
            var go = new GameObject("CardGhost", typeof(RectTransform), typeof(CanvasGroup), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(canvas.transform, false);
            go.GetComponent<CanvasGroup>().blocksRaycasts = false;
            var img = go.GetComponent<UnityEngine.UI.Image>();
            img.sprite = _icon.sprite;
            img.color = _icon.color;
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(90, 90);
            return go;
        }
    }
}
