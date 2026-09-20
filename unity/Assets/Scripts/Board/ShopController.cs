using UnityEngine;

namespace GearDefenders
{
    public class ShopController : MonoBehaviour
    {
        public ShopCardView[] Cards { get; private set; }
        public ShopCardView Selected { get; private set; }
        public bool HasSelectedCard => Selected != null && !Selected.IsEmpty;
        public bool IsEditable { get; set; } = true;
        public Sprite GearSprite { get; private set; }

        GameCatalog _catalog;
        EconomyManager _economy;
        GridController _grid;
        readonly System.Random _rng = new System.Random();

        public void Initialize(GameCatalog catalog, EconomyManager economy, GridController grid, ShopCardView[] cards, Sprite gearSprite)
        {
            _catalog = catalog;
            _economy = economy;
            _grid = grid;
            Cards = cards;
            GearSprite = gearSprite;
            for (int i = 0; i < Cards.Length; i++)
                Cards[i].Bind(this);
            Reroll();
        }

        public void Reroll()
        {
            Selected = null;
            for (int i = 0; i < Cards.Length; i++)
                RollSlot(Cards[i]);
        }

        public bool TryRefresh()
        {
            if (!IsEditable) return false;
            int cost = _catalog.board.refreshCost;
            if (!_economy.TrySpend(cost)) return false;
            Reroll();
            return true;
        }

        public void SelectCard(ShopCardView card)
        {
            if (!IsEditable || card.IsEmpty) return;
            Selected = card;
            for (int i = 0; i < Cards.Length; i++)
                Cards[i].SetSelected(Cards[i] == card);
        }

        public void ClearSelection()
        {
            Selected = null;
            if (Cards == null) return;
            for (int i = 0; i < Cards.Length; i++)
                Cards[i].SetSelected(false);
        }

        public bool TryPlaceSelectedOnCell(GearTileView cell)
        {
            return TryPlaceCardOnCell(Selected, cell);
        }

        public bool TryPlaceCardOnCell(ShopCardView card, GearTileView cell)
        {
            if (!IsEditable || card == null || card.IsEmpty || cell == null) return false;
            if (!cell.State.IsEmpty) return false;
            if (!_economy.TrySpend(card.Cost)) return false;

            if (card.IsUnit)
                _grid.PlaceUnit(cell.Coord, card.Unit);
            else
                _grid.PlaceBoost(cell.Coord, card.Boost);

            card.SetEmpty();
            if (Selected == card) ClearSelection();
            return true;
        }

        void RollSlot(ShopCardView card)
        {
            bool rollBoost = _catalog.shopBoosts != null && _catalog.shopBoosts.Length > 0 && _rng.NextDouble() < 0.28;
            if (rollBoost)
            {
                var boost = _catalog.shopBoosts[_rng.Next(_catalog.shopBoosts.Length)];
                card.SetBoost(boost);
            }
            else
            {
                var unit = _catalog.shopUnits[_rng.Next(_catalog.shopUnits.Length)];
                card.SetUnit(unit);
            }
        }
    }
}
