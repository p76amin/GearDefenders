using UnityEngine;

namespace GearDefenders
{
    public static class RuntimeCatalogFactory
    {
        public static GameCatalog Create()
        {
            var catalog = ScriptableObject.CreateInstance<GameCatalog>();
            catalog.board = CreateBoard();
            var archer1 = Unit("archer_1", "Archer", 1, 10, 70, 11, 0.12f, 0.95f, 160f, true, 85f, 0.40f, UiTheme.Archer);
            var archer2 = Unit("archer_2", "Archer", 2, 10, 95, 16, 0.14f, 0.9f, 170f, true, 90f, 0.50f, UiTheme.Archer);
            var archer3 = Unit("archer_3", "Archer", 3, 10, 130, 22, 0.16f, 0.85f, 180f, true, 95f, 0.62f, UiTheme.Archer);
            archer1.mergesInto = archer2;
            archer2.mergesInto = archer3;

            var barb1 = Unit("barb_1", "Barbarian", 1, 10, 120, 14, 0.08f, 1.05f, 42f, false, 70f, 0.32f, UiTheme.Barbarian);
            var barb2 = Unit("barb_2", "Barbarian", 2, 10, 170, 20, 0.1f, 1.0f, 44f, false, 74f, 0.40f, UiTheme.Barbarian);
            var barb3 = Unit("barb_3", "Barbarian", 3, 10, 230, 28, 0.12f, 0.95f, 46f, false, 78f, 0.50f, UiTheme.Barbarian);
            barb1.mergesInto = barb2;
            barb2.mergesInto = barb3;

            var horse1 = Unit("horse_1", "Horseman", 1, 15, 95, 16, 0.1f, 0.85f, 48f, false, 120f, 0.28f, UiTheme.Horseman);
            var horse2 = Unit("horse_2", "Horseman", 2, 15, 130, 23, 0.12f, 0.8f, 50f, false, 128f, 0.36f, UiTheme.Horseman);
            var horse3 = Unit("horse_3", "Horseman", 3, 15, 175, 32, 0.14f, 0.75f, 52f, false, 136f, 0.46f, UiTheme.Horseman);
            horse1.mergesInto = horse2;
            horse2.mergesInto = horse3;

            var boost1 = Boost("boost_1", "Cog", 1, 10, 0.25f, UiTheme.Boost);
            var boost2 = Boost("boost_2", "Cog", 2, 10, 0.45f, UiTheme.Boost);
            var boost3 = Boost("boost_3", "Cog", 3, 10, 0.70f, UiTheme.Boost);
            boost1.mergesInto = boost2;
            boost2.mergesInto = boost3;

            catalog.shopUnits = new[] { archer1, barb1, horse1 };
            catalog.shopBoosts = new[] { boost1 };

            var grunt = Enemy("grunt", "Grunt", 36, 7, 0.04f, 1.15f, 36f, 52f, UiTheme.EnemyGrunt);
            var runner = Enemy("runner", "Runner", 24, 6, 0.06f, 1.0f, 34f, 78f, UiTheme.EnemyRunner);
            var brute = Enemy("brute", "Brute", 80, 13, 0.05f, 1.25f, 40f, 40f, UiTheme.EnemyBrute);
            catalog.enemies = new[] { grunt, runner, brute };

            catalog.rounds = new RoundDefinition[12];
            for (int i = 0; i < 12; i++)
            {
                var round = ScriptableObject.CreateInstance<RoundDefinition>();
                round.roundIndex = i + 1;
                round.shopDuration = 20f;
                int wave = i + 1;
                if (wave <= 3)
                {
                    round.spawns = new[] { Spawn(grunt, 4 + wave * 2, 0.7f) };
                }
                else if (wave <= 6)
                {
                    round.spawns = new[]
                    {
                        Spawn(grunt, 6 + wave, 0.65f),
                        Spawn(runner, 2 + wave / 2, 0.9f, 2.2f)
                    };
                }
                else if (wave <= 9)
                {
                    round.spawns = new[]
                    {
                        Spawn(grunt, 8, 0.55f),
                        Spawn(runner, 4 + (wave - 6), 0.7f, 1.4f),
                        Spawn(brute, 1 + (wave - 7), 1.1f, 4f)
                    };
                }
                else
                {
                    round.spawns = new[]
                    {
                        Spawn(grunt, 10, 0.45f),
                        Spawn(runner, 8, 0.55f, 1f),
                        Spawn(brute, 3 + (wave - 10), 0.9f, 3f)
                    };
                }

                catalog.rounds[i] = round;
            }

            return catalog;
        }

        static BoardConfig CreateBoard()
        {
            var board = ScriptableObject.CreateInstance<BoardConfig>();
            board.gridWidth = 6;
            board.gridHeight = 5;
            board.powerCorePosition = new Vector2Int(2, 2);
            board.coreRotationSpeed = 110f;
            board.connectedGearRotationSpeed = 70f;
            board.meshLockDuration = 0.14f;
            board.startingCoins = 40;
            board.refreshCost = 3;
            board.startingBaseHP = 930;
            board.totalRounds = 12;
            board.rewardDuration = 1.6f;
            board.levelLabel = 12;
            return board;
        }

        static UnitDefinition Unit(string id, string name, int rank, int cost, float hp, float atk, float crit, float interval, float range, bool ranged, float move, float prod, Color tint)
        {
            var u = ScriptableObject.CreateInstance<UnitDefinition>();
            u.id = id;
            u.displayName = name;
            u.rank = rank;
            u.shopCost = cost;
            u.baseHP = hp;
            u.baseATK = atk;
            u.critChance = crit;
            u.attackInterval = interval;
            u.attackRange = range;
            u.isRanged = ranged;
            u.moveSpeed = move;
            u.baseProductionRate = prod;
            u.tint = tint;
            return u;
        }

        static BoostDefinition Boost(string id, string name, int rank, int cost, float bonus, Color tint)
        {
            var b = ScriptableObject.CreateInstance<BoostDefinition>();
            b.id = id;
            b.displayName = name;
            b.rank = rank;
            b.shopCost = cost;
            b.productionBonus = bonus;
            b.tint = tint;
            return b;
        }

        static EnemyDefinition Enemy(string id, string name, float hp, float atk, float crit, float interval, float range, float move, Color tint)
        {
            var e = ScriptableObject.CreateInstance<EnemyDefinition>();
            e.id = id;
            e.displayName = name;
            e.hp = hp;
            e.atk = atk;
            e.critChance = crit;
            e.attackInterval = interval;
            e.attackRange = range;
            e.moveSpeed = move;
            e.tint = tint;
            return e;
        }

        static EnemySpawnEntry Spawn(EnemyDefinition enemy, int count, float delay, float start = 0.3f)
        {
            return new EnemySpawnEntry
            {
                enemy = enemy,
                count = count,
                spawnDelay = delay,
                startDelay = start
            };
        }
    }
}
