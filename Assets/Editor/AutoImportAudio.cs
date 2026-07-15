using UnityEditor;
using UnityEngine;

namespace MinersWatch.Editor
{
    public class AutoImportAudio : AssetPostprocessor
    {
        private void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith("Assets/Audio/")) return;

            var importer = (AudioImporter)assetImporter;

            if (assetPath.Contains("BGM/"))
            {
                importer.defaultSampleSettings = new AudioImporterSampleSettings
                {
                    loadType = AudioClipLoadType.Streaming,
                    compressionFormat = AudioCompressionFormat.Vorbis,
                    quality = 0.5f,
                };
            }
            else if (assetPath.Contains("SFX/"))
            {
                importer.defaultSampleSettings = new AudioImporterSampleSettings
                {
                    loadType = AudioClipLoadType.DecompressOnLoad,
                    compressionFormat = AudioCompressionFormat.PCM,
                };
            }
        }
    }
}
