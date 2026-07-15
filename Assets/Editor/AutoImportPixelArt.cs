using UnityEditor;
using UnityEngine;

namespace MinersWatch.Editor
{
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

            if (assetPath.Contains("player_") || assetPath.Contains("enemy_"))
            {
                int frameW, frameH, frames;
                GetSpriteInfo(assetPath, out frameW, out frameH, out frames);

                importer.spriteImportMode = SpriteImportMode.Multiple;
                var meta = new SpriteMetaData[frames];
                string baseName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

                for (int i = 0; i < frames; i++)
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

        private static void GetSpriteInfo(string path, out int w, out int h, out int frames)
        {
            if (path.Contains("player_"))
            {
                w = h = 48;
                frames = path.Contains("walk") ? 6 : 4;
            }
            else
            {
                w = h = 32;
                frames = path.Contains("lavabeast") ? 4 : 2;
            }
        }
    }
}
