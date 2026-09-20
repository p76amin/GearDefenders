using UnityEngine;

namespace GearDefenders
{
    public class GearTileState
    {
        public GearTileType TileType = GearTileType.Empty;
        public UnitDefinition Unit;
        public BoostDefinition Boost;
        public int Rank = 1;
        public Vector2Int GridPosition;
        public bool IsConnectedToCore;
        public float CurrentProductionRate;
        public float ProductionAccumulator;
        public float MeshLockTimer;

        public bool IsEmpty => TileType == GearTileType.Empty;
        public bool IsPlaceable => TileType != GearTileType.Empty && TileType != GearTileType.PowerCore;
        public bool CanProduce => TileType == GearTileType.Unit && IsConnectedToCore && CurrentProductionRate > 0f;

        public string FamilyId
        {
            get
            {
                if (TileType == GearTileType.Unit && Unit != null)
                    return Unit.displayName;
                if (TileType == GearTileType.Boost && Boost != null)
                    return Boost.displayName;
                return string.Empty;
            }
        }

        public string DisplayName
        {
            get
            {
                if (TileType == GearTileType.PowerCore) return "Power Core";
                if (TileType == GearTileType.Unit && Unit != null) return Unit.displayName;
                if (TileType == GearTileType.Boost && Boost != null) return Boost.displayName;
                return "Empty";
            }
        }

        public Color Tint
        {
            get
            {
                if (TileType == GearTileType.PowerCore) return UiTheme.Core;
                if (TileType == GearTileType.Unit && Unit != null) return Unit.tint;
                if (TileType == GearTileType.Boost && Boost != null) return Boost.tint;
                return UiTheme.CellEmpty;
            }
        }

        public bool CanMergeWith(GearTileState other)
        {
            if (other == null || this == other) return false;
            if (!IsPlaceable || !other.IsPlaceable) return false;
            if (TileType != other.TileType) return false;
            if (Rank != other.Rank) return false;
            if (TileType == GearTileType.Unit)
                return Unit != null && other.Unit != null && Unit.displayName == other.Unit.displayName;
            if (TileType == GearTileType.Boost)
                return Boost != null && other.Boost != null && Boost.displayName == other.Boost.displayName;
            return false;
        }

        public void Clear()
        {
            TileType = GearTileType.Empty;
            Unit = null;
            Boost = null;
            Rank = 1;
            IsConnectedToCore = false;
            CurrentProductionRate = 0f;
            ProductionAccumulator = 0f;
            MeshLockTimer = 0f;
        }

        public void CopyFrom(GearTileState other)
        {
            TileType = other.TileType;
            Unit = other.Unit;
            Boost = other.Boost;
            Rank = other.Rank;
            IsConnectedToCore = other.IsConnectedToCore;
            CurrentProductionRate = other.CurrentProductionRate;
            ProductionAccumulator = 0f;
            MeshLockTimer = 0f;
        }
    }
}
