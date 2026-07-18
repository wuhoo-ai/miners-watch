using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static MinersWatch.Editor.SceneKit;

namespace MinersWatch.Editor
{
    /// <summary>
    /// W6: Authors the three cave scenes (Shallow/Mid/Deep) as real .unity assets.
    /// GDD: 浅层开阔(棕) / 中层隧道(灰) / 深层狭窄压迫(红). Backgrounds are flat placeholder
    /// rects until T014 FBX models land (W8). Mineral distribution per depth comes from
    /// MineralSpawner.DepthDistributions driven by a LevelConfig asset created here.
    /// </summary>
    public static class CaveSceneAuthor
    {
        struct CaveDef
        {
            public string Scene;
            public DepthLevel Depth;
            public float HalfW;          // cave half-width in world units (开阔→狭窄)
            public Color Wall, Floor;
            public string Title;
        }

        static readonly CaveDef[] Defs =
        {
            new CaveDef { Scene = "ShallowCave", Depth = DepthLevel.Shallow, HalfW = 20f,
                          Wall = new Color(0.42f, 0.30f, 0.20f), Floor = new Color(0.30f, 0.20f, 0.13f), Title = "浅层洞穴" },
            new CaveDef { Scene = "MidCave", Depth = DepthLevel.Medium, HalfW = 15f,
                          Wall = new Color(0.30f, 0.30f, 0.35f), Floor = new Color(0.20f, 0.20f, 0.25f), Title = "中层洞穴" },
            new CaveDef { Scene = "DeepCave", Depth = DepthLevel.Deep, HalfW = 12f,
                          Wall = new Color(0.32f, 0.10f, 0.08f), Floor = new Color(0.20f, 0.06f, 0.05f), Title = "深层洞穴" },
        };

        const float GroundTopY = -3.0f;

        [MenuItem("Hermes/Author Cave Scenes")]
        public static void Author()
        {
            EnsureTag("MineralNode");
            foreach (var def in Defs) AuthorOne(def);
            Debug.Log("[CaveSceneAuthor] 3 cave scenes authored + saved.");
        }

        static void AuthorOne(CaveDef def)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var player = BuildWorld(def);
            BuildSpawner(def);
            BuildCanvas(def);

            // Camera: smooth follow, clamped so the view never leaves the cave
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = def.Floor * 0.5f;
            camGo.transform.position = new Vector3(0, 0, -10);
            var follow = camGo.AddComponent<CameraFollow>();
            float clamp = Mathf.Max(0f, def.HalfW - 9.6f);
            Wire(follow, ("_target", player.transform));
            SetFloats(follow, ("_minX", -clamp), ("_maxX", clamp));

            EditorSceneManager.SaveScene(scene, $"Assets/Scenes/{def.Scene}.unity");
            AddSceneToBuild($"Assets/Scenes/{def.Scene}.unity");
        }

        static GameObject BuildWorld(CaveDef def)
        {
            var env = new GameObject("Environment");
            float w = def.HalfW * 2f;

            // Placeholder cave visuals (replaced by T014 FBX in W8)
            Rect2D("CaveWallBG", env.transform, new Vector3(0, 1.5f, 5), new Vector2(w, 12f), def.Wall, -20);
            Rect2D("CaveFloor", env.transform, new Vector3(0, GroundTopY - 1.2f, 4), new Vector2(w, 2.5f), def.Floor, -15);
            Rect2D("CaveCeiling", env.transform, new Vector3(0, 7.2f, 4), new Vector2(w, 1.8f), def.Floor, -15);

            var ground = Obj("Ground", env.transform, new Vector3(0, GroundTopY - 0.5f, 0));
            ground.layer = 6;
            ground.AddComponent<BoxCollider2D>().size = new Vector2(w, 1f);

            foreach (var (n, x) in new[] { ("LeftWall", -def.HalfW - 0.5f), ("RightWall", def.HalfW + 0.5f) })
            {
                var wall = Obj(n, env.transform, new Vector3(x, 0, 0));
                wall.layer = 6;
                wall.AddComponent<BoxCollider2D>().size = new Vector2(1f, 14f);
            }

            // Player: surface rig + MiningSystem (E to mine, trigger targeting via capsule)
            var p = Obj("Player", null, new Vector3(-def.HalfW + 2f, GroundTopY + 1.5f, 0));
            p.tag = "Player";
            var sr = p.AddComponent<SpriteRenderer>();
            sr.sprite = S("Assets/Sprites/Character/player_idle_01.png");
            sr.sortingOrder = 10;
            var rb = p.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            p.AddComponent<CapsuleCollider2D>().size = new Vector2(1.4f, 2.9f);
            var gc = Obj("GroundCheck", p.transform, Vector3.zero);
            gc.transform.localPosition = new Vector3(0, -1.5f, 0);
            var pc = p.AddComponent<PlayerController>();
            Wire(pc, ("groundCheckPoint", gc.transform));
            SetFloats(pc, ("minX", -def.HalfW + 0.8f), ("maxX", def.HalfW - 0.8f));
            p.AddComponent<StaminaSystem>();
            p.AddComponent<PlayerHP>();
            p.AddComponent<MiningSystem>(); // Inventory/Upgrades ← GameRoot fallback

            return p;
        }

        static void BuildSpawner(CaveDef def)
        {
            var go = new GameObject("MineralSpawner");
            var spawner = go.AddComponent<MineralSpawner>();
            Wire(spawner, ("levelConfig", EnsureConfig(def.Depth)));
            SetFloats(spawner,
                ("spawnMinX", -def.HalfW + 1.5f), ("spawnMaxX", def.HalfW - 1.5f),
                ("spawnMinY", GroundTopY + 0.8f), ("spawnMaxY", 2.5f));
        }

        static LevelConfig EnsureConfig(DepthLevel depth)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            string path = $"Assets/Data/LevelConfig_{depth}.asset";
            var cfg = AssetDatabase.LoadAssetAtPath<LevelConfig>(path);
            if (cfg == null)
            {
                cfg = ScriptableObject.CreateInstance<LevelConfig>();
                cfg.levelDepth = depth;
                AssetDatabase.CreateAsset(cfg, path);
            }
            return cfg;
        }

        static void BuildCanvas(CaveDef def)
        {
            var canvas = MakeCanvas();
            var ct = canvas.transform;

            // Exit (top-left) + stamina under it
            var exit = Btn("ReturnToSurfaceBtn", "⬆ 回地面", ct, new Vector2(0, 1), new Vector2(30, -30), new Vector2(400, 120), new Color(0.9f, 0.45f, 0.05f), 52);
            exit.AddComponent<ReturnToSurface>();
            BuildStaminaBar(ct, new Vector2(30, -180));

            BuildDayNightHUD(canvas);
            BuildInventoryBar(canvas, new Vector2(0, 30));

            // Depth title (top-right) + mining hint (bottom-right)
            var title = Label("CaveTitle", def.Title, ct, new Vector2(1, 1), new Vector2(-40, -40), new Vector2(420, 90), 60, new Color(1f, 0.9f, 0.7f));
            title.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
            var hint = Label("MineHint", "靠近矿物按 E 挖矿", ct, new Vector2(1, 0), new Vector2(-40, 40), new Vector2(560, 70), 40, new Color(1f, 1f, 1f, 0.75f));
            hint.GetComponent<RectTransform>().pivot = new Vector2(1, 0);
        }
    }
}
