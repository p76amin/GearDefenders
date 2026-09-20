using UnityEngine;

namespace GearDefenders
{
    [CreateAssetMenu(menuName = "Gear Defenders/Boost Definition", fileName = "Boost")]
    public class BoostDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public int rank = 1;
        public int shopCost = 10;
        public float productionBonus = 0.25f;
        public BoostDefinition mergesInto;
        public Color tint = new Color(0.95f, 0.75f, 0.2f);
        public Sprite sprite;
    }
}
