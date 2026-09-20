using UnityEngine;

namespace GearDefenders
{
    public static class UiTheme
    {
        public static readonly Color Sand = Hex("#C9A36A");
        public static readonly Color SandDark = Hex("#8C6A3A");
        public static readonly Color ArenaSky = Hex("#E8C888");
        public static readonly Color BoardBg = Hex("#2A1C12");
        public static readonly Color CellEmpty = Hex("#3B2A1C");
        public static readonly Color CellStroke = Hex("#6A4A2A");
        public static readonly Color BottomBg = Hex("#1A120C");
        public static readonly Color HudDark = new Color(0.08f, 0.05f, 0.03f, 0.72f);
        public static readonly Color Accent = Hex("#F0B030");
        public static readonly Color Coin = Hex("#F6D15A");
        public static readonly Color BattleBtn = Hex("#C63A2E");
        public static readonly Color RefreshBtn = Hex("#3D6FA8");
        public static readonly Color PauseBtn = Hex("#4A3A2A");
        public static readonly Color SpeedBtn = Hex("#2F6B4F");
        public static readonly Color HpFill = Hex("#3CBF5A");
        public static readonly Color HpBack = Hex("#3A1A16");
        public static readonly Color Core = Hex("#F4A020");
        public static readonly Color Archer = Hex("#3FA86A");
        public static readonly Color Barbarian = Hex("#C4483A");
        public static readonly Color Horseman = Hex("#3A6FBF");
        public static readonly Color Boost = Hex("#E0A020");
        public static readonly Color EnemyGrunt = Hex("#7A3030");
        public static readonly Color EnemyRunner = Hex("#A85A2A");
        public static readonly Color EnemyBrute = Hex("#4A2048");
        public static readonly Color Text = Color.white;
        public static readonly Color TextDim = new Color(1f, 1f, 1f, 0.7f);
        public static readonly Color ConnectedGlow = new Color(1f, 0.82f, 0.35f, 0.35f);
        public static readonly Color Cactus = Hex("#3A7A3A");
        public static readonly Color Bridge = Hex("#8B5A2B");
        public static readonly Color Guardian = Hex("#D8C4A0");

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }
    }
}
