using System;
using UnityEngine;

namespace GearDefenders
{
    public class GridController : MonoBehaviour
    {
        public BoardConfig Config { get; private set; }
        public ShopController Shop { get; private set; }
        public GearTileState[,] Tiles { get; private set; }
        public GearTileView[,] Views { get; private set; }
        public bool IsEditable { get; set; } = true;
        public Sprite GearSprite { get; private set; }
        public Sprite CoreSprite { get; private set; }

        public event Action<GearTileState> UnitProduced;
        public event Action<GearTileView> TileTapped;

        TooltipCardUI _tooltip;

        public void Initialize(BoardConfig config, ShopController shop, RectTransform gridRoot, Sprite gearSprite, Sprite coreSprite, TooltipCardUI tooltip)
        {
            Config = config;
            Shop = shop;
            GearSprite = gearSprite;
            CoreSprite = coreSprite;
            _tooltip = tooltip;

            int w = config.gridWidth;
            int h = config.gridHeight;
            Tiles = new GearTileState[w, h];
            Views = new GearTileView[w, h];

            Canvas.ForceUpdateCanvases();
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(gridRoot);
            var layout = gridRoot.GetComponent<UnityEngine.UI.GridLayoutGroup>();
            float cell = Mathf.Min(
                (gridRoot.rect.width - layout.spacing.x * (w - 1) - layout.padding.horizontal) / w,
                (gridRoot.rect.height - layout.spacing.y * (h - 1) - layout.padding.vertical) / h);
            if (cell < 8f) cell = 72f;
            layout.cellSize = new Vector2(cell, cell);
            layout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = w;

            for (int y = h - 1; y >= 0; y--)
            {
                for (int x = 0; x < w; x++)
                {
                    var state = new GearTileState { GridPosition = new Vector2Int(x, y) };
                    Tiles[x, y] = state;
                    var view = CreateCell(gridRoot, new Vector2Int(x, y), state);
                    Views[x, y] = view;
                }
            }

            PlaceCore();
            Recompute();
        }

        public void PlaceCore()
        {
            var p = Config.powerCorePosition;
            var tile = Tiles[p.x, p.y];
            tile.Clear();
            tile.TileType = GearTileType.PowerCore;
            tile.Rank = 1;
        }

        public void PlaceUnit(Vector2Int pos, UnitDefinition unit)
        {
            var tile = Tiles[pos.x, pos.y];
            tile.Clear();
            tile.TileType = GearTileType.Unit;
            tile.Unit = unit;
            tile.Rank = unit.rank;
            Recompute();
        }

        public void PlaceBoost(Vector2Int pos, BoostDefinition boost)
        {
            var tile = Tiles[pos.x, pos.y];
            tile.Clear();
            tile.TileType = GearTileType.Boost;
            tile.Boost = boost;
            tile.Rank = boost.rank;
            Recompute();
        }

        public void HandleTileDrop(GearTileView source, GearTileView target)
        {
            if (!IsEditable || source == null || target == null) return;
            if (source.State.TileType == GearTileType.PowerCore || target.State.TileType == GearTileType.PowerCore)
                return;

            if (target.State.IsEmpty)
            {
                target.State.CopyFrom(source.State);
                source.State.Clear();
                Recompute();
                return;
            }

            if (source.State.CanMergeWith(target.State))
                MergeInto(source, target);
        }

        public void MergeInto(GearTileView source, GearTileView target)
        {
            var src = source.State;
            var dst = target.State;
            if (src.TileType == GearTileType.Unit)
            {
                var next = dst.Unit != null ? dst.Unit.mergesInto : null;
                if (next == null) return;
                dst.Clear();
                dst.TileType = GearTileType.Unit;
                dst.Unit = next;
                dst.Rank = next.rank;
            }
            else if (src.TileType == GearTileType.Boost)
            {
                var next = dst.Boost != null ? dst.Boost.mergesInto : null;
                if (next == null) return;
                dst.Clear();
                dst.TileType = GearTileType.Boost;
                dst.Boost = next;
                dst.Rank = next.rank;
            }
            else return;

            src.Clear();
            Recompute();
        }

        public void Recompute()
        {
            GearConnectionGraph.Recompute(Tiles, Config.powerCorePosition);
            RefreshAll();
        }

        public void RefreshAll()
        {
            if (Views == null) return;
            foreach (var view in Views)
                view.Refresh();
        }

        public void ShowTooltip(GearTileView view)
        {
            TileTapped?.Invoke(view);
            _tooltip?.ShowUnit(view.State);
        }

        public void Tick(float dt, bool produce)
        {
            if (Views == null) return;
            foreach (var view in Views)
                view.TickVisual(dt);

            if (!produce) return;

            int w = Config.gridWidth;
            int h = Config.gridHeight;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var tile = Tiles[x, y];
                    if (!tile.CanProduce) continue;
                    tile.ProductionAccumulator += tile.CurrentProductionRate * dt;
                    if (tile.ProductionAccumulator >= 1f)
                    {
                        tile.ProductionAccumulator -= 1f;
                        Views[x, y].PulseMesh();
                        UnitProduced?.Invoke(tile);
                    }
                }
            }
        }

        GearTileView CreateCell(Transform parent, Vector2Int coord, GearTileState state)
        {
            var root = UiFactory.Rect($"Cell_{coord.x}_{coord.y}", parent);
            var view = root.gameObject.AddComponent<GearTileView>();

            var bg = UiFactory.Image("Bg", root, UiTheme.CellEmpty, null, true);
            UiFactory.Stretch(bg.rectTransform);

            var glow = UiFactory.Image("Glow", root, UiTheme.ConnectedGlow);
            glow.enabled = false;
            UiFactory.Stretch(glow.rectTransform, 0.04f, 0.04f, 0.96f, 0.96f);

            var icon = UiFactory.Image("Icon", root, Color.white);
            icon.enabled = false;
            UiFactory.Stretch(icon.rectTransform, 0.14f, 0.2f, 0.86f, 0.88f);

            var rank = UiFactory.Label("Rank", root, "", 14, UiTheme.Coin, TMPro.TextAlignmentOptions.TopRight);
            UiFactory.Stretch(rank.rectTransform, 0.05f, 0.72f, 0.95f, 0.98f);

            var rate = UiFactory.Label("Rate", root, "", 12, UiTheme.TextDim, TMPro.TextAlignmentOptions.Bottom);
            UiFactory.Stretch(rate.rectTransform, 0.04f, 0.02f, 0.96f, 0.22f);

            view.Bind(this, coord, state);
            return view;
        }
    }
}
