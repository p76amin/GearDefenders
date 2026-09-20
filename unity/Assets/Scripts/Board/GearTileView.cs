using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GearDefenders
{
    public class GearTileView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
    {
        public Vector2Int Coord { get; private set; }
        public GearTileState State { get; private set; }

        UnityEngine.UI.Image _bg;
        UnityEngine.UI.Image _icon;
        UnityEngine.UI.Image _glow;
        TextMeshProUGUI _rank;
        TextMeshProUGUI _rate;
        RectTransform _iconRt;
        GridController _grid;
        bool _dragging;
        bool _pointerMoved;
        GameObject _ghost;

        public void Bind(GridController grid, Vector2Int coord, GearTileState state)
        {
            _grid = grid;
            Coord = coord;
            State = state;
            _bg = transform.Find("Bg").GetComponent<UnityEngine.UI.Image>();
            _glow = transform.Find("Glow").GetComponent<UnityEngine.UI.Image>();
            _icon = transform.Find("Icon").GetComponent<UnityEngine.UI.Image>();
            _iconRt = _icon.rectTransform;
            _rank = transform.Find("Rank").GetComponent<TextMeshProUGUI>();
            _rate = transform.Find("Rate").GetComponent<TextMeshProUGUI>();
            Refresh();
        }

        public void Refresh()
        {
            if (State == null) return;
            bool empty = State.IsEmpty;
            _bg.color = empty ? UiTheme.CellEmpty : Color.Lerp(UiTheme.CellEmpty, State.Tint, 0.35f);
            _glow.enabled = State.IsConnectedToCore && State.TileType != GearTileType.Empty;
            _glow.color = UiTheme.ConnectedGlow;
            _icon.enabled = !empty;
            if (!empty)
            {
                _icon.preserveAspect = true;
                _icon.sprite = State.TileType == GearTileType.PowerCore
                    ? _grid.CoreSprite
                    : _grid.GearSprite;
                _icon.color = State.Tint;
            }

            if (State.TileType == GearTileType.Unit || State.TileType == GearTileType.Boost)
            {
                _rank.text = "R" + State.Rank;
                _rank.enabled = true;
            }
            else
            {
                _rank.enabled = false;
            }

            if (State.TileType == GearTileType.Unit)
            {
                _rate.enabled = true;
                _rate.text = State.IsConnectedToCore
                    ? State.CurrentProductionRate.ToString("0.00") + "/s"
                    : "0.00/s";
                _rate.color = State.IsConnectedToCore ? UiTheme.Coin : new Color(1f, 0.4f, 0.35f);
            }
            else
            {
                _rate.enabled = false;
            }
        }

        public void TickVisual(float dt)
        {
            if (State == null || State.IsEmpty) return;
            if (State.MeshLockTimer > 0f)
            {
                State.MeshLockTimer -= dt;
                float t = 1f - Mathf.Clamp01(State.MeshLockTimer / _grid.Config.meshLockDuration);
                _iconRt.localScale = Vector3.one * Mathf.Lerp(0.78f, 1f, t);
                return;
            }

            _iconRt.localScale = Vector3.one;
            if (State.TileType == GearTileType.PowerCore)
                _iconRt.Rotate(0f, 0f, -_grid.Config.coreRotationSpeed * dt);
            else if (State.IsConnectedToCore)
                _iconRt.Rotate(0f, 0f, -_grid.Config.connectedGearRotationSpeed * dt);
        }

        public void PulseMesh()
        {
            State.MeshLockTimer = _grid.Config.meshLockDuration;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_grid.IsEditable || !State.IsPlaceable) return;
            _dragging = true;
            _pointerMoved = true;
            _ghost = CreateGhost();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging || _ghost == null) return;
            _ghost.transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_ghost != null) Destroy(_ghost);
            _ghost = null;
            _dragging = false;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!_grid.IsEditable) return;
            var sourceTile = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<GearTileView>() : null;
            if (sourceTile != null && sourceTile != this)
            {
                _grid.HandleTileDrop(sourceTile, this);
                return;
            }

            var card = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<ShopCardView>() : null;
            if (card != null)
                _grid.Shop.TryPlaceCardOnCell(card, this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_pointerMoved)
            {
                _pointerMoved = false;
                return;
            }

            if (_grid.Shop != null && _grid.Shop.HasSelectedCard)
            {
                if (_grid.IsEditable)
                    _grid.Shop.TryPlaceSelectedOnCell(this);
                return;
            }

            if (State.TileType == GearTileType.Unit)
                _grid.ShowTooltip(this);
        }

        GameObject CreateGhost()
        {
            var canvas = GetComponentInParent<Canvas>();
            var go = new GameObject("DragGhost", typeof(RectTransform), typeof(CanvasGroup), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(canvas.transform, false);
            go.GetComponent<CanvasGroup>().blocksRaycasts = false;
            var img = go.GetComponent<UnityEngine.UI.Image>();
            img.sprite = _icon.sprite;
            img.color = _icon.color;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(88, 88);
            return go;
        }
    }
}
