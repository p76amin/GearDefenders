using System;
using UnityEngine;

namespace GearDefenders
{
    [Serializable]
    public class EnemySpawnEntry
    {
        public EnemyDefinition enemy;
        public int count = 6;
        public float startDelay = 0.4f;
        public float spawnDelay = 0.75f;
    }

    [CreateAssetMenu(menuName = "Gear Defenders/Round Definition", fileName = "Round")]
    public class RoundDefinition : ScriptableObject
    {
        public int roundIndex = 1;
        public float shopDuration = 20f;
        public EnemySpawnEntry[] spawns = Array.Empty<EnemySpawnEntry>();

        public int TotalEnemies
        {
            get
            {
                if (spawns == null) return 0;
                int total = 0;
                for (int i = 0; i < spawns.Length; i++)
                {
                    if (spawns[i] != null)
                        total += Mathf.Max(0, spawns[i].count);
                }
                return total;
            }
        }
    }
}
