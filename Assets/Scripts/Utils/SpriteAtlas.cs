using UnityEngine;

namespace MinersWatch
{
    /// <summary>Runtime sprite atlas — procedural textures usable from any system.</summary>
    public static class SpriteAtlas
    {
        private static Sprite _whiteSquare;

        /// <summary>1×1 white pixel square — tint via SpriteRenderer.color.</summary>
        public static Sprite WhiteSquare
        {
            get
            {
                if (_whiteSquare == null)
                {
                    var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                    var pixels = new Color[16];
                    for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
                    tex.SetPixels(pixels);
                    tex.Apply();
                    _whiteSquare = Sprite.Create(tex,
                        new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
                }
                return _whiteSquare;
            }
        }
    }
}
