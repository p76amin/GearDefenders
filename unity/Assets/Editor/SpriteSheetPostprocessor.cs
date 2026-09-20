#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GearDefenders.Editor
{
    public class SpriteSheetPostprocessor : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Art/"))
                return;
            if (!assetPath.EndsWith(".png"))
                return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 32;
            importer.mipmapEnabled = false;
            importer.isReadable = true;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.spritePivot = new Vector2(0.5f, 0.15f);

            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            if (width <= 0 || height <= 0)
                return;

            var rects = BuildRects(assetPath, width, height);
            if (rects.Count == 0)
                return;

            var metas = new SpriteMetaData[rects.Count];
            for (int i = 0; i < rects.Count; i++)
            {
                var r = rects[i];
                metas[i] = new SpriteMetaData
                {
                    name = r.name,
                    rect = r.rect,
                    alignment = (int)SpriteAlignment.Custom,
                    pivot = r.pivot
                };
            }

            importer.spritesheet = metas;
        }

        static List<(string name, Rect rect, Vector2 pivot)> BuildRects(string path, int width, int height)
        {
            var list = new List<(string, Rect, Vector2)>();
            string file = System.IO.Path.GetFileNameWithoutExtension(path);
            bool gear = path.Contains("/Gears/");
            Vector2 pivot = gear ? new Vector2(0.5f, 0.5f) : new Vector2(0.5f, 0.15f);

            if (path.Contains("/Units/Horseman/") && width % 64 == 0 && height % 64 == 0)
            {
                int cell = 64;
                int cols = width / cell;
                int rows = height / cell;
                int index = 0;
                for (int row = 0; row < rows; row++)
                {
                    int y = height - (row + 1) * cell;
                    for (int col = 0; col < cols; col++)
                    {
                        list.Add(($"{file}_{index}", new Rect(col * cell, y, cell, cell), pivot));
                        index++;
                    }
                }

                return list;
            }

            int frameWidth = height;
            if (path.Contains("/Units/Barbarian/"))
                frameWidth = 184;
            else if (path.Contains("/Units/Archer/"))
                frameWidth = 80;
            else if (path.Contains("/Enemies/"))
                frameWidth = 150;

            int count = Mathf.Max(1, width / Mathf.Max(1, frameWidth));
            for (int i = 0; i < count; i++)
                list.Add(($"{file}_{i}", new Rect(i * frameWidth, 0, frameWidth, height), pivot));
            return list;
        }
    }
}
#endif
