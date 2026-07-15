using UnityEditor;
using UnityEngine;

namespace MinersWatch.Editor
{
    /// <summary>
    /// Auto-configures pixel art texture import settings.
    /// Point filter (no blur), uncompressed, spritesheet slicing.
    /// </summary>
    public class AutoImportPixelArt : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Sprites/")) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 16;

            // Spritesheet auto-slice: player_* spritesheets are horizontal strips
            if (assetPath.Contains("player_") && !assetPath.Contains("_0"))
            {
                // player_idle.png = 192×48 → 4 frames × 48
                // player_walk.png = 288×48 → 6 frames × 48
                // player_mine.png = 192×48 → 4 frames × 48
                // player_attack.png = 192×48 → 4 frames × 48
                importer.spriteImportMode = SpriteImportMode.Multiple;

                var meta = new SpriteMetaData[GetFrameCount(assetPath)];
                int frameW = 48;
                int frameH = 48;
                string baseName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

                for (int i = 0; i < meta.Length; i++)
                {
                    meta[i] = new SpriteMetaData
                    {
                        name = $"{baseName}_{i}",
                        rect = new Rect(i * frameW, 0, frameW, frameH),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0f),
                    };
                }
                importer.spritesheet = meta;
            }
        }

        private static int GetFrameCount(string path)
        {
            if (path.Contains("player_idle") || path.Contains("player_mine") || path.Contains("player_attack"))
                return 4;
            if (path.Contains("player_walk"))
                return 6;
            return 1;
        }
    }
}
