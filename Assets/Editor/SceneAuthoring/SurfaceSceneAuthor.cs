using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static MinersWatch.Editor.SceneKit;

namespace MinersWatch.Editor
{
    /// <summary>
    /// W5: Authors the Surface scene (shop + build grid + defense zone + core) as a real .unity asset.
    /// Run via menu Hermes/Author Surface Scene, review, commit. Idempotent: rebuilds from scratch.
    /// Layout: 1920x1080 @ ortho 5.4 (19.2x10.8 world units), pixel art PPU 16.
    /// 方案A: shared systems live on the persistent GameRoot (auto-created at play start);
    /// scene UI leaves those refs null and resolves them through Awake fallback chains.
    /// </summary>
    public static class SurfaceSceneAuthor
    {
        const string ScenePath = "Assets/Scenes/Surface.unity";
        const float GroundTopY = -3.0f;

        [MenuItem("Hermes/Author Surface Scene")]
        public static void Author()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildCamera();
            BuildEnvironment();
            BuildSystems();
            BuildPlayer();
            BuildWorldMarkers();
            BuildCanvas();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuild(ScenePath);
            Debug.Log("[SurfaceSceneAuthor] Surface.unity authored + saved.");
        }

        static void BuildCamera()
        {
            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            go.tag = "MainCamera";
            var cam = go.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.45f, 0.7f, 0.95f); // sky fallback
            go.transform.position = new Vector3(0, 0, -10);
        }

        static void BuildEnvironment()
        {
            var env = new GameObject("Environment");

            var bg = Obj("SurfaceBG", env.transform, new Vector3(0, 0, 5));
            var sr = bg.AddComponent<SpriteRenderer>();
            sr.sprite = S("Assets/Sprites/Environment/surface_bg.png");
            sr.sortingOrder = -10;
            if (sr.sprite != null)
            {
                float k = 19.2f / (sr.sprite.rect.width / sr.sprite.pixelsPerUnit);
                bg.transform.localScale = new Vector3(k, k, 1);
            }

            var ground = Obj("Ground", env.transform, new Vector3(0, GroundTopY - 0.5f, 0));
            ground.layer = 6;
            ground.AddComponent<BoxCollider2D>().size = new Vector2(19.2f, 1f);

            foreach (var (n, x) in new[] { ("LeftWall", -9.7f), ("RightWall", 9.7f) })
            {
                var w = Obj(n, env.transform, new Vector3(x, 0, 0));
                w.layer = 6;
                w.AddComponent<BoxCollider2D>().size = new Vector2(1f, 10.8f);
            }
        }

        static void BuildSystems()
        {
            // Surface-local systems only; shared ones live on GameRoot (方案A).
            var go = new GameObject("Systems");
            var build = go.AddComponent<BuildSystem>();
            go.AddComponent<WaveManager>();
            SetFloats(build, ("_cellSize", 1.2f));
        }

        static void BuildPlayer()
        {
            var p = Obj("Player", null, new Vector3(-6f, GroundTopY + 1.5f, 0));
            p.tag = "Player"; // StaminaBarUI discovers the player by tag
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
            SetFloats(pc, ("minX", -8.5f), ("maxX", 8.5f));

            p.AddComponent<StaminaSystem>();
            p.AddComponent<PlayerHP>(); // _upgrades resolves from GameRoot at runtime
        }

        static void BuildWorldMarkers()
        {
            // Base core: right edge, what enemies attack
            var core = Obj("BaseCore", null, new Vector3(8.2f, GroundTopY + 1f, 0));
            var sr = core.AddComponent<SpriteRenderer>();
            sr.sprite = S("Assets/Sprites/Items/icon_crystal.png");
            sr.sortingOrder = 5;
            core.AddComponent<BoxCollider2D>().isTrigger = true;
            core.AddComponent<BaseCore>();

            // Build grid: 15 cells x 1.2 units, centered
            var grid = new GameObject("BuildGrid");
            for (int i = 0; i < 15; i++)
                Rect2D($"Cell_{i:D2}", grid.transform, new Vector3((i - 7) * 1.2f, GroundTopY + 0.05f, 0),
                       new Vector2(1.1f, 0.35f), new Color(1f, 1f, 1f, 0.18f), -5);

            // Pre-place 4 turrets on cells 2, 6, 10, 14 (spread across the grid)
            // Turret's [RequireComponent] auto-adds CircleCollider2D.
            int[] turretCells = { 2, 6, 10, 14 };
            foreach (int i in turretCells)
            {
                var cell = grid.transform.Find($"Cell_{i:D2}");
                if (cell != null) cell.gameObject.AddComponent<Turret>();
            }

            Obj("EnemySpawnPoint", null, new Vector3(-9.2f, GroundTopY + 1.5f, 0));
        }

        static void BuildCanvas()
        {
            var canvas = MakeCanvas();
            var ct = canvas.transform;

            // Back button (top-left)
            var bb = Btn("BackToMenuBtn", "← 菜单", ct, new Vector2(0, 1), new Vector2(30, -30), new Vector2(400, 120), new Color(0.9f, 0.45f, 0.05f), 52);
            bb.AddComponent<BackToMenu>();

            BuildStaminaBar(ct, new Vector2(30, -180));
            BuildDayNightHUD(canvas);

            // Gold (top-right)
            var coin = Img("CoinIcon", ct, new Vector2(1, 1), new Vector2(-330, -40), new Vector2(96, 96), "Assets/Sprites/UI/ui_coin.png");
            coin.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
            var gold = Label("GoldText", "$0", ct, new Vector2(1, 1), new Vector2(-40, -40), new Vector2(280, 96), 64, new Color(1f, 0.85f, 0.2f));
            gold.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            gold.GetComponent<RectTransform>().pivot = new Vector2(1, 1);

            // Shop panel (right side, 4 big buttons)
            var shopPanel = Panel("ShopPanel", ct, new Vector2(1, 0.5f), new Vector2(-30, 60), new Vector2(660, 700), new Color(0.08f, 0.08f, 0.12f, 0.72f));
            shopPanel.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f);
            var sell = PanelBtn(shopPanel, 0, "卖出全部矿物", new Color(0.15f, 0.55f, 0.2f));
            var pick = PanelBtn(shopPanel, 1, "升级镐 $200", new Color(0.25f, 0.35f, 0.6f));
            var armor = PanelBtn(shopPanel, 2, "升级护甲 $150", new Color(0.25f, 0.35f, 0.6f));
            var pack = PanelBtn(shopPanel, 3, "升级背包 $100", new Color(0.25f, 0.35f, 0.6f));

            // Build panel (bottom-left, 3 icon buttons)
            var buildPanel = Panel("BuildPanel", ct, new Vector2(0, 0), new Vector2(30, 30), new Vector2(1060, 190), new Color(0.08f, 0.08f, 0.12f, 0.72f));
            buildPanel.GetComponent<RectTransform>().pivot = Vector2.zero;
            var wall = BuildBtn(buildPanel, 0, "木墙 $50", "Assets/Sprites/UI/ui_build_wall.png");
            var trap = BuildBtn(buildPanel, 1, "陷阱 $80", "Assets/Sprites/UI/ui_build_spike_trap.png");
            var turret = BuildBtn(buildPanel, 2, "炮塔 $200", "Assets/Sprites/UI/ui_build_turret.png");

            var shopUI = canvas.AddComponent<ShopUI>();
            Wire(shopUI, ("_goldText", gold.GetComponent<Text>()),
                         ("_sellAllButton", sell), ("_buyPickaxeButton", pick),
                         ("_buyArmorButton", armor), ("_buyBackpackButton", pack),
                         ("_buyWallButton", wall), ("_buySpikeTrapButton", trap), ("_buyTurretButton", turret)); // _shop ← GameRoot fallback

            BuildInventoryBar(canvas, new Vector2(160, 30));
            BuildTouchControls(canvas, withMine: false, joyPos: new Vector2(60, 250)); // above build panel

            // Cave entry panel (top-left, under stamina bar): 3 depth buttons, locks via CaveEntryUI
            var cavePanel = Panel("CaveEntryPanel", ct, new Vector2(0, 1), new Vector2(30, -260), new Vector2(460, 560), new Color(0.08f, 0.08f, 0.12f, 0.72f));
            cavePanel.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            var shallowB = PanelBtn(cavePanel, 0, "浅层洞穴", new Color(0.5f, 0.35f, 0.2f));
            var midB = PanelBtn(cavePanel, 1, "中层 🔒$500", new Color(0.35f, 0.35f, 0.4f));
            var deepB = PanelBtn(cavePanel, 2, "深层 🔒$2000", new Color(0.4f, 0.15f, 0.12f));
            foreach (var b in new[] { shallowB, midB, deepB })
                b.GetComponent<RectTransform>().sizeDelta = new Vector2(420, 150);
            var caveUI = cavePanel.AddComponent<CaveEntryUI>();
            Wire(caveUI, ("_shallowButton", shallowB), ("_midButton", midB), ("_deepButton", deepB),
                         ("_midLabel", midB.transform.Find("L").GetComponent<Text>()),
                         ("_deepLabel", deepB.transform.Find("L").GetComponent<Text>()));

            // Game over overlay (hidden)
            var ov = Panel("GameOverOverlay", ct, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.9f));
            Fill(ov);
            Label("GameOverText", "游戏结束", ov.transform, new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(800, 160), 112, new Color(0.9f, 0.2f, 0.2f));
            var rb2 = Btn("RestartBtn", "重新开始", ov.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -60), new Vector2(680, 160), new Color(0.2f, 0.5f, 0.2f), 56);
            var mb2 = Btn("GOMainMenuBtn", "返回主菜单", ov.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -240), new Vector2(680, 160), new Color(0.2f, 0.5f, 0.2f), 56);
            Center(rb2); Center(mb2);
            var goUI = ov.AddComponent<GameOverUI>();
            Wire(goUI, ("_gameOverPanel", ov), ("_victoryPanel", ov),
                       ("_restartButton", rb2.GetComponent<Button>()), ("_mainMenuButton", mb2.GetComponent<Button>())); // _sceneController ← runtime find
            ov.SetActive(false);
        }

        static Button BuildBtn(GameObject panel, int idx, string label, string iconPath)
        {
            var b = Btn($"BuildBtn_{idx}", "", panel.transform, new Vector2(0, 0.5f), new Vector2(20 + idx * 345, 0), new Vector2(325, 150), new Color(0.35f, 0.3f, 0.2f), 44);
            b.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
            Img("Icon", b.transform, new Vector2(0, 0.5f), new Vector2(20, 0), new Vector2(110, 110), iconPath)
                .GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
            Label("T", label, b.transform, new Vector2(0.5f, 0.5f), new Vector2(60, 0), new Vector2(200, 140), 44, Color.white);
            return b.GetComponent<Button>();
        }

        /// <summary>Deterministic 1920x1080 capture of the ACTIVE scene incl. overlay UI (temp camera-space canvas).</summary>
        [MenuItem("Hermes/Capture Game 1920")]
        public static void Capture()
        {
            var cam = Camera.main;
            if (cam == null) { Debug.LogError("[Capture] no main camera"); return; }
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                .Where(c => c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
            foreach (var c in canvases) { c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = cam; c.planeDistance = 1f; }
            Canvas.ForceUpdateCanvases();

            var rt = new RenderTexture(1920, 1080, 24);
            var prev = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();
            cam.targetTexture = prev;

            RenderTexture.active = rt;
            var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            foreach (var c in canvases) { c.renderMode = RenderMode.ScreenSpaceOverlay; c.worldCamera = null; }

            var dir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), ".hermes-bridge");
            System.IO.Directory.CreateDirectory(dir);
            var file = System.IO.Path.Combine(dir, $"{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}_capture.png");
            System.IO.File.WriteAllBytes(file, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(rt);
            Debug.Log($"[Capture] saved {file}");
        }
    }
}
