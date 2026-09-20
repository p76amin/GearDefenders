using UnityEngine;

namespace GearDefenders
{
    [CreateAssetMenu(menuName = "Gear Defenders/Board Config", fileName = "BoardConfig")]
    public class BoardConfig : ScriptableObject
    {
        public int gridWidth = 6;
        public int gridHeight = 5;
        public Vector2Int powerCorePosition = new Vector2Int(2, 2);
        public float coreRotationSpeed = 110f;
        public float connectedGearRotationSpeed = 70f;
        public float meshLockDuration = 0.14f;
        public int startingCoins = 40;
        public int refreshCost = 3;
        public int startingBaseHP = 930;
        public int totalRounds = 12;
        public float rewardDuration = 1.6f;
        public int levelLabel = 12;
    }
}
