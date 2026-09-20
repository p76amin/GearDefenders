using UnityEngine;

namespace GearDefenders
{
    [CreateAssetMenu(menuName = "Gear Defenders/Enemy Definition", fileName = "Enemy")]
    public class EnemyDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public float hp = 40f;
        public float atk = 8f;
        public float critChance = 0.05f;
        public float attackInterval = 1.1f;
        public float attackRange = 36f;
        public float moveSpeed = 55f;
        public Color tint = new Color(0.55f, 0.22f, 0.22f);
        public Sprite sprite;
    }
}
