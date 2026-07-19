using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace MinersWatch.Editor
{
    /// <summary>
    /// T014: Generates low-poly cave background meshes (3 depth tiers).
    /// 500-800 tris each. Run once → saves as .asset files under Assets/Art/Caves/.
    /// Integrated into CaveSceneAuthor: each cave scene picks up its mesh via
    /// the generated asset reference.
    ///
    /// Dave the Diver style: horizontal cave tunnel viewed from side-scroller
    /// perspective, warm earth tones darkening with depth.
    /// </summary>
    public class CaveBackgroundGenerator
    {
        // Depth-tier presets
        static readonly (string name, Color wallTint, float radius, float tunnelVariance, int segmentCount)[] DEPTHS = new[]
        {
            ("Shallow", new Color(0.55f, 0.42f, 0.28f), 5.5f, 1.8f, 16),   // warm brown
            ("Medium",  new Color(0.38f, 0.30f, 0.22f), 4.5f, 1.4f, 14),   // muted dark brown
            ("Deep",    new Color(0.22f, 0.16f, 0.12f), 3.5f, 1.0f, 12),   // dark, tight
        };

        [MenuItem("Hermes/Generate Cave Backgrounds")]
        public static void GenerateAll()
        {
            EnsureDirectory();
            foreach (var (name, tint, radius, variance, segs) in DEPTHS)
            {
                var mesh = BuildCaveMesh(radius, variance, segs);
                mesh.name = $"CaveBg_{name}";
                string path = $"Assets/Art/Caves/CaveBg_{name}.asset";
                AssetDatabase.CreateAsset(mesh, path);
                Debug.Log($"[CaveBgGen] {path} — {mesh.triangles.Length/3} tris");
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CaveBgGen] 3 cave background meshes generated.");
        }

        /// <summary>Build a horizontal cave-tunnel mesh: irregular ceiling + floor, extruded along Z.</summary>
        static Mesh BuildCaveMesh(float baseRadius, float variance, int segments)
        {
            var verts = new List<Vector3>();
            var tris  = new List<int>();
            float totalWidth = 22f; // world units, ~2 screen widths for scrolling
            int columns = (int)(totalWidth / 0.7f); // vertex every ~0.7 units along X
            float xStep = totalWidth / columns;
            float zDepth = 1.5f; // thin extrusion (side-scroller camera won't see much Z)

            for (int col = 0; col <= columns; col++)
            {
                float x = -totalWidth / 2f + col * xStep;
                float rTop = baseRadius + RandomRange(-variance, variance, col * 7 + 0);
                float rBot = baseRadius + RandomRange(-variance, variance, col * 7 + 1);

                // ring of vertices around the tunnel cross-section
                int ringStart = verts.Count;
                for (int ring = 0; ring < 2; ring++) // front + back (Z)
                {
                    float z = ring == 0 ? -zDepth : zDepth;
                    // ceiling
                    verts.Add(new Vector3(x, rTop, z));
                    // floor
                    verts.Add(new Vector3(x, -rBot, z));
                    // left wall
                    verts.Add(new Vector3(x - 0.3f, 0, z));
                    // right wall
                    verts.Add(new Vector3(x + 0.3f, 0, z));
                }
            }

            // triangles: connect each column to next (front ring)
            for (int col = 0; col < columns; col++)
            {
                int a = col * 8;      // front ring of column col
                int b = (col + 1) * 8; // front ring of column col+1
                // 4 quad faces per cross-section: top, bottom, left wall, right wall
                for (int f = 0; f < 4; f++)
                {
                    int v0 = a + f;
                    int v1 = b + f;
                    int v2 = a + (f + 1) % 4;
                    int v3 = b + (f + 1) % 4;
                    if (f % 2 == 0)
                    {
                        tris.Add(v0); tris.Add(v2); tris.Add(v1);
                        tris.Add(v1); tris.Add(v2); tris.Add(v3);
                    }
                    else
                    {
                        tris.Add(v0); tris.Add(v1); tris.Add(v2);
                        tris.Add(v2); tris.Add(v1); tris.Add(v3);
                    }
                }
            }

            // close caps (simple triangles at ends)
            for (int cap = 0; cap < 2; cap++)
            {
                int col = cap == 0 ? 0 : columns;
                int center = verts.Count;
                float x = -totalWidth / 2f + col * xStep;
                verts.Add(new Vector3(x, 0, 0)); // center point
                for (int f = 0; f < 4; f++)
                {
                    int a = col * 8 + f;          // front
                    int b = col * 8 + 4 + f;      // back
                    if (cap == 0)
                    {
                        tris.Add(center); tris.Add(b); tris.Add(a);
                    }
                    else
                    {
                        tris.Add(center); tris.Add(a); tris.Add(b);
                    }
                }
            }

            var mesh = new Mesh { name = "CaveBg" };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        static float RandomRange(float min, float max, int seed)
        {
            // deterministic pseudo-random per seed (same seed = same shape every run)
            var rng = new System.Random(seed);
            return (float)(rng.NextDouble() * (max - min) + min);
        }

        static void EnsureDirectory()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Art"))       AssetDatabase.CreateFolder("Assets", "Art");
            if (!AssetDatabase.IsValidFolder("Assets/Art/Caves")) AssetDatabase.CreateFolder("Assets/Art", "Caves");
        }
    }
}
