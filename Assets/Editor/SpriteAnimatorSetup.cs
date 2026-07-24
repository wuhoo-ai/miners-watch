using UnityEngine;
using UnityEditor;
using MinersWatch;

namespace MinersWatch.Editor
{
    /// <summary>
    /// One-click setup: adds SpriteAnimator to Player and Enemy objects in the scene.
    /// Menu: MinersWatch > Setup Sprite Animators
    /// </summary>
    public static class SpriteAnimatorSetup
    {
        [MenuItem("MinersWatch/Setup Sprite Animators")]
        public static void SetupAll()
        {
            int count = 0;

            // Find all PlayerController objects
            var players = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                EnsureAnimator(player.gameObject);
                count++;
            }

            // Find all Enemy objects
            var enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                EnsureAnimator(enemy.gameObject);
                count++;
            }

            // Also find objects with EnemyAI (covers variants)
            var ais = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
            foreach (var ai in ais)
            {
                EnsureAnimator(ai.gameObject);
                count++;
            }

            Debug.Log($"[SpriteAnimatorSetup] Configured {count} objects with SpriteAnimator.");
        }

        [MenuItem("MinersWatch/Setup Sprite Animators (Selected)")]
        public static void SetupSelected()
        {
            var selected = Selection.gameObjects;
            if (selected.Length == 0)
            {
                Debug.LogWarning("[SpriteAnimatorSetup] No objects selected.");
                return;
            }

            int count = 0;
            foreach (var go in selected)
            {
                EnsureAnimator(go);
                count++;
            }

            Debug.Log($"[SpriteAnimatorSetup] Configured {count} selected objects.");
        }

        private static void EnsureAnimator(GameObject go)
        {
            // Ensure SpriteRenderer exists
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = Undo.AddComponent<SpriteRenderer>(go);
            }

            // Add SpriteAnimator if missing
            var anim = go.GetComponent<SpriteAnimator>();
            if (anim == null)
            {
                anim = Undo.AddComponent<SpriteAnimator>(go);
            }

            Undo.RecordObject(anim, "Setup SpriteAnimator");
            EditorUtility.SetDirty(go);
        }
    }
}
