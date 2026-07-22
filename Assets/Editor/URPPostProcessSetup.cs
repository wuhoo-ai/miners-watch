using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MinersWatch.Editor
{
    /// <summary>
    /// Creates the URP post-processing VolumeProfile asset and wires it
    /// into scene volumes. Run once via Hermes > Setup URP Post-Process.
    /// </summary>
    public static class URPPostProcessSetup
    {
        const string ProfilePath = "Assets/URPPostProcessProfile.asset";

        [MenuItem("Hermes/Setup URP Post-Process")]
        public static void Setup()
        {
            var profile = CreateProfile();
            InjectVolumeIntoScenes(profile);
            Debug.Log("[URP] Post-process profile created + injected into scenes.");
        }

        static VolumeProfile CreateProfile()
        {
            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (existing != null)
            {
                Debug.Log($"[URP] Profile already exists at {ProfilePath}, reusing.");
                return existing;
            }

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();

            // Bloom — subtle, for crystal glow
            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.value = 0.15f;
            bloom.threshold.value = 0.9f;
            bloom.scatter.value = 0.7f;
            bloom.tint.value = new Color(0.6f, 0.7f, 1f);
            bloom.active = true;

            // Vignette — subtle dark edges for cave atmosphere
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.value = 0.3f;
            vignette.smoothness.value = 0.4f;
            vignette.color.value = new Color(0.05f, 0.05f, 0.1f);
            vignette.active = true;

            // Color Adjustments — day/night filter controlled by DayNightPostProcess
            var colorAdj = profile.Add<ColorAdjustments>(true);
            colorAdj.colorFilter.value = Color.white;
            colorAdj.postExposure.value = 0f;
            colorAdj.contrast.value = 5f;
            colorAdj.active = true;

            AssetDatabase.CreateAsset(profile, ProfilePath);
            AssetDatabase.SaveAssets();
            return profile;
        }

        static void InjectVolumeIntoScenes(VolumeProfile profile)
        {
            string[] scenePaths = {
                "Assets/Scenes/Surface.unity",
                "Assets/Scenes/ShallowCave.unity",
                "Assets/Scenes/MidCave.unity",
                "Assets/Scenes/DeepCave.unity",
            };

            foreach (var path in scenePaths)
            {
                var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path,
                    UnityEditor.SceneManagement.OpenSceneMode.Single);

                // Check if a global Volume already exists
                var existing = Object.FindObjectOfType<Volume>();
                if (existing != null && existing.isGlobal)
                {
                    existing.sharedProfile = profile;
                    EditorUtility.SetDirty(existing);
                    Debug.Log($"[URP] Updated existing global Volume in {System.IO.Path.GetFileName(path)}");
                }
                else
                {
                    var go = new GameObject("GlobalVolume", typeof(Volume));
                    var vol = go.GetComponent<Volume>();
                    vol.isGlobal = true;
                    vol.sharedProfile = profile;
                    vol.priority = 0;
                    Debug.Log($"[URP] Added global Volume to {System.IO.Path.GetFileName(path)}");
                }

                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            }
        }
    }
}
