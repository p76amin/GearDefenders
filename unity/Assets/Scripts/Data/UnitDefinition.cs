using UnityEngine;

namespace GearDefenders
{
    [CreateAssetMenu(menuName = "Gear Defenders/Unit Definition", fileName = "Unit")]
    public class UnitDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public int rank = 1;
        public int shopCost = 10;
        public float baseHP = 80f;
        public float baseATK = 12f;
        public float critChance = 0.1f;
        public float attackInterval = 1f;
        public float attackRange = 48f;
        public bool isRanged;
        public float moveSpeed = 90f;
        public float baseProductionRate = 0.35f;
        public UnitDefinition mergesInto;
        public Color tint = Color.white;
        public Sprite sprite;
    }
}
