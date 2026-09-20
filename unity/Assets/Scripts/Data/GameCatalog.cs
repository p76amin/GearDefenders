using UnityEngine;

namespace GearDefenders
{
    [CreateAssetMenu(menuName = "Gear Defenders/Game Catalog", fileName = "GameCatalog")]
    public class GameCatalog : ScriptableObject
    {
        public BoardConfig board;
        public UnitDefinition[] shopUnits;
        public BoostDefinition[] shopBoosts;
        public EnemyDefinition[] enemies;
        public RoundDefinition[] rounds;
    }
}
