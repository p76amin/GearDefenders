using UnityEngine;

namespace GearDefenders
{
    public static class CombatantArtLibrary
    {
        const string GearPath = "Assets/Art/Gears/Gear 10 - Bronze_01.png";
        const string CorePath = "Assets/Art/Gears/Gear 11 - Bronze_03.png";
        const string AnimSetFolder = "Assets/Resources/AnimSets";

        static CombatantAnimSet _archer;
        static CombatantAnimSet _barbarian;
        static CombatantAnimSet _horseman;
        static CombatantAnimSet _grunt;
        static CombatantAnimSet _runner;
        static CombatantAnimSet _brute;
        static Sprite _gear;
        static Sprite _core;

        public static Sprite GearSprite => _gear != null ? _gear : (_gear = LoadBoardSprite(GearPath));
        public static Sprite CoreSprite => _core != null ? _core : (_core = LoadBoardSprite(CorePath));

        public static CombatantAnimSet ForUnitId(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Archer();
            if (id.StartsWith("archer"))
                return Archer();
            if (id.StartsWith("barb"))
                return Barbarian();
            if (id.StartsWith("horse"))
                return Horseman();
            return Archer();
        }

        public static CombatantAnimSet ForEnemyId(string id)
        {
            if (id == "runner")
                return Runner();
            if (id == "brute")
                return Brute();
            return Grunt();
        }

        public static CombatantAnimSet Archer() => _archer ??= LoadOrBuild("Archer", "Assets/Art/Units/Archer", SpriteSheetKind.Horizontal, 80);
        public static CombatantAnimSet Barbarian() => _barbarian ??= LoadOrBuild("Barbarian", "Assets/Art/Units/Barbarian", SpriteSheetKind.Horizontal, 184);
        public static CombatantAnimSet Horseman() => _horseman ??= LoadOrBuild("Horseman", "Assets/Art/Units/Horseman", SpriteSheetKind.Grid, 64, 64);
        public static CombatantAnimSet Grunt() => _grunt ??= LoadOrBuild("Grunt", "Assets/Art/Enemies/Grunt", SpriteSheetKind.Horizontal, 150);
        public static CombatantAnimSet Runner() => _runner ??= LoadOrBuild("Runner", "Assets/Art/Enemies/Runner", SpriteSheetKind.Horizontal, 150);
        public static CombatantAnimSet Brute() => _brute ??= LoadOrBuild("Brute", "Assets/Art/Enemies/Brute", SpriteSheetKind.Horizontal, 150);

        public static void Bind(GameCatalog catalog)
        {
            if (catalog == null)
                return;

            if (catalog.shopUnits != null)
            {
                for (int i = 0; i < catalog.shopUnits.Length; i++)
                    BindUnitChain(catalog.shopUnits[i]);
            }

            if (catalog.enemies != null)
            {
                for (int i = 0; i < catalog.enemies.Length; i++)
                    BindEnemy(catalog.enemies[i]);
            }

            if (catalog.board != null)
            {
                if (catalog.board.gearSprite == null)
                    catalog.board.gearSprite = GearSprite;
                if (catalog.board.coreSprite == null)
                    catalog.board.coreSprite = CoreSprite;
            }
        }

        public static Color VisualTint(Color faction, int rank)
        {
            float amount = Mathf.Clamp01(0.28f + Mathf.Max(0, rank - 1) * 0.12f);
            return Color.Lerp(Color.white, faction, amount);
        }

        static void BindUnitChain(UnitDefinition unit)
        {
            var current = unit;
            while (current != null)
            {
                var set = ForUnitId(current.id);
                if (current.animSet == null)
                    current.animSet = set;
                if (current.sprite == null && current.animSet != null)
                    current.sprite = current.animSet.Portrait;
                current = current.mergesInto;
            }
        }

        static void BindEnemy(EnemyDefinition enemy)
        {
            if (enemy == null)
                return;
            var set = ForEnemyId(enemy.id);
            if (enemy.animSet == null)
                enemy.animSet = set;
            if (enemy.sprite == null && enemy.animSet != null)
                enemy.sprite = enemy.animSet.Portrait;
        }

        static CombatantAnimSet LoadOrBuild(string role, string folder, SpriteSheetKind kind, int frameWidth, int gridCell = 64)
        {
            var set = Resources.Load<CombatantAnimSet>("AnimSets/" + role);
#if UNITY_EDITOR
            if (set == null)
                set = UnityEditor.AssetDatabase.LoadAssetAtPath<CombatantAnimSet>($"{AnimSetFolder}/{role}.asset");
#endif
            if (set == null)
                set = ScriptableObject.CreateInstance<CombatantAnimSet>();

            set.sheetKind = kind;
            set.frameWidth = frameWidth;
            set.gridCellSize = gridCell;
            if (set.idleSheet == null) set.idleSheet = LoadTexture(folder + "/Idle.png");
            if (set.walkSheet == null) set.walkSheet = LoadTexture(folder + "/Walk.png");
            if (set.attackSheet == null) set.attackSheet = LoadTexture(folder + "/Attack.png");
            if (set.hurtSheet == null) set.hurtSheet = LoadTexture(folder + "/Hurt.png");
            if (set.deathSheet == null) set.deathSheet = LoadTexture(folder + "/Death.png");
            return set;
        }

        static Texture2D LoadTexture(string assetPath)
        {
#if UNITY_EDITOR
            var tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (tex != null)
                return tex;
#endif
            string resourcesPath = assetPath.Replace("Assets/Art/", "Art/");
            if (resourcesPath.EndsWith(".png"))
                resourcesPath = resourcesPath.Substring(0, resourcesPath.Length - 4);
            return Resources.Load<Texture2D>(resourcesPath);
        }

        static Sprite LoadBoardSprite(string assetPath)
        {
            var tex = LoadTexture(assetPath);
            if (tex == null)
                return null;
            var frames = SpriteSheetSlicer.SliceHorizontal(tex, tex.height);
            return frames != null && frames.Length > 0 ? frames[0] : null;
        }
    }
}
