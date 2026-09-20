using System.Collections.Generic;
using UnityEngine;

namespace GearDefenders
{
    public static class SpriteSheetSlicer
    {
        public static Sprite[] SliceHorizontal(Texture2D tex, int frameWidth, int pixelsPerUnit = 32)
        {
            if (tex == null)
                return System.Array.Empty<Sprite>();

            int width = frameWidth > 0 ? frameWidth : InferHorizontalFrameWidth(tex);
            if (width <= 0)
                width = tex.width;
            int count = Mathf.Max(1, tex.width / width);
            var sprites = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                var rect = new Rect(i * width, 0, width, tex.height);
                sprites[i] = Create(tex, rect, pixelsPerUnit, i);
            }

            return sprites;
        }

        public static Sprite[] SliceGrid(Texture2D tex, int cellSize, int pixelsPerUnit = 32)
        {
            if (tex == null || cellSize <= 0)
                return System.Array.Empty<Sprite>();

            int cols = tex.width / cellSize;
            int rows = tex.height / cellSize;
            var list = new List<Sprite>(cols * rows);
            int index = 0;
            for (int row = 0; row < rows; row++)
            {
                int y = tex.height - (row + 1) * cellSize;
                for (int col = 0; col < cols; col++)
                {
                    var rect = new Rect(col * cellSize, y, cellSize, cellSize);
                    if (IsVisuallyEmpty(tex, rect))
                        continue;
                    list.Add(Create(tex, rect, pixelsPerUnit, index));
                    index++;
                }
            }

            return list.ToArray();
        }

        public static int InferHorizontalFrameWidth(Texture2D tex)
        {
            if (tex == null)
                return 0;
            if (tex.width % tex.height == 0)
                return tex.height;
            return 0;
        }

        static bool IsVisuallyEmpty(Texture2D tex, Rect rect)
        {
            if (tex == null || !tex.isReadable)
                return false;

            int x = Mathf.RoundToInt(rect.x);
            int y = Mathf.RoundToInt(rect.y);
            int w = Mathf.RoundToInt(rect.width);
            int h = Mathf.RoundToInt(rect.height);
            var pixels = tex.GetPixels(x, y, w, h);
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a > 0.03f)
                    return false;
            }

            return true;
        }

        static Sprite Create(Texture2D tex, Rect rect, int pixelsPerUnit, int index)
        {
            var sprite = Sprite.Create(tex, rect, new Vector2(0.5f, 0.15f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = tex.name + "_" + index;
            return sprite;
        }
    }
}
