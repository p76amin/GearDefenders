using UnityEngine;

namespace GearDefenders
{
    public static class PlaceholderArt
    {
        public static Sprite Solid(Color color, int size = 32)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        public static Sprite Circle(Color color, int size = 64, float radiusNorm = 0.46f)
        {
            return Radial(size, (uv, d) => d <= radiusNorm ? color : Color.clear);
        }

        public static Sprite Ring(Color color, int size = 64, float outer = 0.46f, float inner = 0.22f)
        {
            return Radial(size, (uv, d) => d <= outer && d >= inner ? color : Color.clear);
        }

        public static Sprite Gear(Color color, int size = 96, int teeth = 8)
        {
            Color dark = color * 0.55f;
            dark.a = 1f;
            return Radial(size, (uv, d) =>
            {
                float angle = Mathf.Atan2(uv.y - 0.5f, uv.x - 0.5f);
                float tooth = Mathf.Abs(Mathf.Cos(angle * teeth));
                float outer = 0.38f + tooth * 0.12f;
                if (d > outer) return Color.clear;
                if (d < 0.14f) return new Color(0.12f, 0.08f, 0.05f, 1f);
                if (d < 0.22f) return dark;
                return color;
            });
        }

        public static Sprite Cactus(Color color, int size = 48)
        {
            var tex = NewTex(size);
            var pixels = new Color[size * size];
            float cx = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - cx) / size;
                    float dy = y / (float)size;
                    bool stem = Mathf.Abs(dx) < 0.12f && dy > 0.08f && dy < 0.92f;
                    bool armL = dy > 0.45f && dy < 0.62f && dx > -0.32f && dx < -0.08f;
                    bool armR = dy > 0.55f && dy < 0.7f && dx > 0.08f && dx < 0.3f;
                    pixels[y * size + x] = stem || armL || armR ? color : Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0f), 100f);
        }

        public static Sprite Triangle(Color color, int size = 48)
        {
            var tex = NewTex(size);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                float t = y / (float)(size - 1);
                float half = t * 0.5f;
                for (int x = 0; x < size; x++)
                {
                    float nx = x / (float)(size - 1) - 0.5f;
                    pixels[y * size + x] = Mathf.Abs(nx) <= half ? color : Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.2f), 100f);
        }

        static Sprite Radial(int size, System.Func<Vector2, float, Color> fn)
        {
            var tex = NewTex(size);
            var pixels = new Color[size * size];
            float inv = 1f / (size - 1);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var uv = new Vector2(x * inv, y * inv);
                    float d = Vector2.Distance(uv, new Vector2(0.5f, 0.5f));
                    pixels[y * size + x] = fn(uv, d);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        static Texture2D NewTex(int size)
        {
            return new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
        }
    }
}
