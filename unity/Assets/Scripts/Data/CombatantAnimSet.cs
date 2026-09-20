using UnityEngine;

namespace GearDefenders
{
    public enum CombatantAnimState
    {
        Idle,
        Walk,
        Attack,
        Hurt,
        Death
    }

    [CreateAssetMenu(menuName = "Gear Defenders/Combatant Anim Set", fileName = "CombatantAnimSet")]
    public class CombatantAnimSet : ScriptableObject
    {
        public Texture2D idleSheet;
        public Texture2D walkSheet;
        public Texture2D attackSheet;
        public Texture2D hurtSheet;
        public Texture2D deathSheet;
        public SpriteSheetKind sheetKind = SpriteSheetKind.Horizontal;
        public int frameWidth;
        public int gridCellSize = 64;
        public int pixelsPerUnit = 32;
        public float idleFps = 8f;
        public float walkFps = 10f;
        public float attackFps = 12f;
        public float hurtFps = 12f;
        public float deathFps = 10f;

        Sprite[] _idle;
        Sprite[] _walk;
        Sprite[] _attack;
        Sprite[] _hurt;
        Sprite[] _death;

        public Sprite Portrait
        {
            get
            {
                var idle = Idle;
                return idle != null && idle.Length > 0 ? idle[0] : null;
            }
        }

        public Sprite[] Idle => _idle ??= Slice(idleSheet);
        public Sprite[] Walk => _walk ??= Slice(walkSheet);
        public Sprite[] Attack => _attack ??= Slice(attackSheet);
        public Sprite[] Hurt => _hurt ??= Slice(hurtSheet);
        public Sprite[] Death => _death ??= Slice(deathSheet);

        public Sprite[] Frames(CombatantAnimState state)
        {
            switch (state)
            {
                case CombatantAnimState.Walk: return Walk;
                case CombatantAnimState.Attack: return Attack;
                case CombatantAnimState.Hurt: return Hurt;
                case CombatantAnimState.Death: return Death;
                default: return Idle;
            }
        }

        public float Fps(CombatantAnimState state)
        {
            switch (state)
            {
                case CombatantAnimState.Walk: return walkFps;
                case CombatantAnimState.Attack: return attackFps;
                case CombatantAnimState.Hurt: return hurtFps;
                case CombatantAnimState.Death: return deathFps;
                default: return idleFps;
            }
        }

        public float Duration(CombatantAnimState state)
        {
            var frames = Frames(state);
            float fps = Mathf.Max(0.01f, Fps(state));
            return frames == null || frames.Length == 0 ? 0f : frames.Length / fps;
        }

        Sprite[] Slice(Texture2D sheet)
        {
            if (sheetKind == SpriteSheetKind.Grid)
                return SpriteSheetSlicer.SliceGrid(sheet, gridCellSize > 0 ? gridCellSize : 64, pixelsPerUnit);
            return SpriteSheetSlicer.SliceHorizontal(sheet, frameWidth, pixelsPerUnit);
        }
    }

    public enum SpriteSheetKind
    {
        Horizontal,
        Grid
    }
}
