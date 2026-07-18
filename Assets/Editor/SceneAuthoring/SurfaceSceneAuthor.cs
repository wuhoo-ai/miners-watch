using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MinersWatch.Editor
{
    /// <summary>
    /// W5: Authors the Surface scene (shop + build grid + defense zone + core) as a real .unity asset.
    /// Run once via menu Hermes/Author Surface Scene, review, commit. Idempotent: rebuilds from scratch.
    /// Layout: 1920x1080 @ ortho 5.4 (19.2x10.8 world units), pixel art PPU 16.
    /// </summary>
    public static class SurfaceSceneAuthor
    {
        const string ScenePath = "Assets/Scenes/Surface.unity";
        const int UILayer = 5;
        const float GroundTopY = -3.0f;

        [MenuItem("Hermes/Author Surface Scene")]
        public static void Author()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildCamera();
            BuildEnvironment();
            var systems = BuildSystems();
            BuildPlayer(systems);
            BuildWorldMarkers();
            BuildCanvas(systems);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings();
            Debug.Log("[SurfaceSceneAuthor] Surface.unity authored + saved.");
        }

        // ---------- world ----------

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
            var col = ground.AddComponent<BoxCollider2D>();
            col.size = new Vector2(19.2f, 1f);

            foreach (var (n, x) in new[] { ("LeftWall", -9.7f), ("RightWall", 9.7f) })
            {
                var w = Obj(n, env.transform, new Vector3(x, 0, 0));
                w.layer = 6;
                w.AddComponent<BoxCollider2D>().size = new Vector2(1f, 10.8f);
            }
        }

        static GameObject BuildSystems()
        {
            var go = new GameObject("Systems");
            var inv = go.AddComponent<InventorySystem>();
            var upg = go.AddComponent<UpgradeSystem>();
            var shop = go.AddComponent<ShopSystem>();
            var build = go.AddComponent<BuildSystem>();
            go.AddComponent<DayNightCycle>();
            go.AddComponent<WaveManager>();
            go.AddComponent<DepthProgression>();
            go.AddComponent<SceneController>();

            Wire(shop, ("_inventory", inv), ("_upgrades", upg));
            Wire(build, ("_shop", shop));
            var so = new SerializedObject(build);
            so.FindProperty("_cellSize").floatValue = 1.2f;
            so.ApplyModifiedPropertiesWithoutUndo();
            return go;
        }

        static void BuildPlayer(GameObject systems)
        {
            var p = Obj("Player", null, new Vector3(-6f, GroundTopY + 1.5f, 0));
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
            var so = new SerializedObject(pc);
            so.FindProperty("minX").floatValue = -8.5f;
            so.FindProperty("maxX").floatValue = 8.5f;
            so.ApplyModifiedPropertiesWithoutUndo();

            p.AddComponent<StaminaSystem>();
            var hp = p.AddComponent<PlayerHP>();
            Wire(hp, ("_upgrades", systems.GetComponent<UpgradeSystem>()));
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
            {
                float x = (i - 7) * 1.2f;
                var cell = Obj($"Cell_{i:D2}", grid.transform, new Vector3(x, GroundTopY + 0.05f, 0));
                var csr = cell.AddComponent<SpriteRenderer>();
                csr.sprite = S("Assets/Sprites/UI/ui_stamina_bar.png"); // any white-ish sprite as cell marker
                csr.color = new Color(1f, 1f, 1f, 0.18f);
                csr.sortingOrder = -5;
                csr.drawMode = SpriteDrawMode.Sliced;
                csr.size = new Vector2(1.1f, 0.35f);
            }

            Obj("EnemySpawnPoint", null, new Vector3(-9.2f, GroundTopY + 1.5f, 0));
        }

        // ---------- UI ----------

        static void BuildCanvas(GameObject systems)
        {
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                new GameObject("EventSystem").AddComponent<UnityEngine.EventSystems.EventSystem>()
                    .gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            var canvas = new GameObject("GameCanvas", typeof(RectTransform));
            canvas.layer = UILayer;
            canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvas.AddComponent<GraphicRaycaster>();
            var ct = canvas.transform;

            // Back button (top-left) — same convention as TestGround
            var bb = Btn("BackToMenuBtn", "← 菜单", ct, new Vector2(0, 1), new Vector2(30, -30), new Vector2(400, 120), new Color(0.9f, 0.45f, 0.05f), 52);
            bb.AddComponent<BackToMenu>();

            // Stamina bar under back button
            BuildStaminaBar(ct, new Vector2(30, -180));

            // Day/night HUD (top-center)
            var phase = Label("PhaseText", "白天", ct, new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(500, 90), 64, Color.white);
            var timer = Label("TimerText", "120", ct, new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(500, 80), 56, new Color(1f, 0.95f, 0.6f));
            var warn = Panel("WarningPanel", ct, new Vector2(0.5f, 1), new Vector2(0, -250), new Vector2(900, 100), new Color(0.85f, 0.2f, 0.1f, 0.85f));
            Label("L", "⚠ 夜晚将至，准备防御！", warn.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(880, 90), 52, Color.white);
            warn.SetActive(false);
            var dnUI = canvas.AddComponent<DayNightUI>();
            Wire(dnUI, ("_cycle", systems.GetComponent<DayNightCycle>()),
                       ("_phaseText", phase.GetComponent<Text>()),
                       ("_timerText", timer.GetComponent<Text>()),
                       ("_warningPanel", warn));

            // Gold (top-right)
            var coin = Img("CoinIcon", ct, new Vector2(1, 1), new Vector2(-330, -40), new Vector2(96, 96), "Assets/Sprites/UI/ui_coin.png");
            var gold = Label("GoldText", "$0", ct, new Vector2(1, 1), new Vector2(-40, -40), new Vector2(280, 96), 64, new Color(1f, 0.85f, 0.2f));
            gold.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            var goldRt = gold.GetComponent<RectTransform>(); goldRt.pivot = new Vector2(1, 1);
            var coinRt = coin.GetComponent<RectTransform>(); coinRt.pivot = new Vector2(1, 1);

            // Shop panel (right side, 4 big buttons)
            var shopPanel = Panel("ShopPanel", ct, new Vector2(1, 0.5f), new Vector2(-30, 60), new Vector2(660, 700), new Color(0.08f, 0.08f, 0.12f, 0.72f));
            var spr = shopPanel.GetComponent<RectTransform>(); spr.pivot = new Vector2(1, 0.5f);
            var sell = PanelBtn(shopPanel, 0, "卖出全部矿物", new Color(0.15f, 0.55f, 0.2f));
            var pick = PanelBtn(shopPanel, 1, "升级镐 $200", new Color(0.25f, 0.35f, 0.6f));
            var armor = PanelBtn(shopPanel, 2, "升级护甲 $150", new Color(0.25f, 0.35f, 0.6f));
            var pack = PanelBtn(shopPanel, 3, "升级背包 $100", new Color(0.25f, 0.35f, 0.6f));

            // Build panel (bottom-left, 3 icon buttons)
            var buildPanel = Panel("BuildPanel", ct, new Vector2(0, 0), new Vector2(30, 30), new Vector2(1060, 190), new Color(0.08f, 0.08f, 0.12f, 0.72f));
            var bpr = buildPanel.GetComponent<RectTransform>(); bpr.pivot = Vector2.zero;
            var wall = BuildBtn(buildPanel, 0, "木墙 $50", "Assets/Sprites/UI/ui_build_wall.png");
            var trap = BuildBtn(buildPanel, 1, "陷阱 $80", "Assets/Sprites/UI/ui_build_spike_trap.png");
            var turret = BuildBtn(buildPanel, 2, "炮塔 $200", "Assets/Sprites/UI/ui_build_turret.png");

            var shopUI = canvas.AddComponent<ShopUI>();
            Wire(shopUI, ("_shop", systems.GetComponent<ShopSystem>()),
                         ("_goldText", gold.GetComponent<Text>()),
                         ("_sellAllButton", sell), ("_buyPickaxeButton", pick),
                         ("_buyArmorButton", armor), ("_buyBackpackButton", pack),
                         ("_buyWallButton", wall), ("_buySpikeTrapButton", trap), ("_buyTurretButton", turret));

            // Inventory slots (bottom-center)
            var invBar = Panel("InventoryBar", ct, new Vector2(0.5f, 0), new Vector2(160, 30), new Vector2(640, 130), new Color(0.08f, 0.08f, 0.12f, 0.6f));
            var ibr = invBar.GetComponent<RectTransform>(); ibr.pivot = new Vector2(0.5f, 0);
            var hl = invBar.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 12; hl.padding = new RectOffset(12, 12, 12, 12);
            hl.childForceExpandWidth = false; hl.childForceExpandHeight = false;
            hl.childAlignment = TextAnchor.MiddleLeft;
            var slotTpl = new GameObject("SlotTemplate", typeof(RectTransform), typeof(Image));
            slotTpl.layer = UILayer;
            slotTpl.transform.SetParent(invBar.transform, false);
            slotTpl.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            slotTpl.GetComponent<Image>().color = new Color(1, 1, 1, 0.9f);
            var slotTxt = Label("Count", "0", slotTpl.transform, new Vector2(1, 0), new Vector2(-8, 8), new Vector2(60, 44), 36, Color.black);
            slotTxt.GetComponent<Text>().alignment = TextAnchor.LowerRight;
            slotTpl.SetActive(false);
            var invUI = canvas.AddComponent<InventoryUI>();
            Wire(invUI, ("_inventory", systems.GetComponent<InventorySystem>()),
                        ("_slotContainer", invBar.transform), ("_slotPrefab", slotTpl));

            // Game over overlay (hidden)
            var ov = Panel("GameOverOverlay", ct, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.9f));
            Fill(ov);
            Label("GameOverText", "游戏结束", ov.transform, new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(800, 160), 112, new Color(0.9f, 0.2f, 0.2f));
            var rb2 = Btn("RestartBtn", "重新开始", ov.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -60), new Vector2(680, 160), new Color(0.2f, 0.5f, 0.2f), 56);
            var mb2 = Btn("GOMainMenuBtn", "返回主菜单", ov.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -240), new Vector2(680, 160), new Color(0.2f, 0.5f, 0.2f), 56);
            Center(rb2); Center(mb2);
            var goUI = ov.AddComponent<GameOverUI>();
            Wire(goUI, ("_gameOverPanel", ov), ("_victoryPanel", ov),
                       ("_restartButton", rb2.GetComponent<Button>()), ("_mainMenuButton", mb2.GetComponent<Button>()),
                       ("_sceneController", systems.GetComponent<SceneController>()));
            ov.SetActive(false);
        }

        static void BuildStaminaBar(Transform parent, Vector2 pos)
        {
            var root = new GameObject("StaminaBar", typeof(RectTransform), typeof(Slider));
            root.layer = UILayer;
            root.transform.SetParent(parent, false);
            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(480, 48);

            var bg = Img("Background", root.transform, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, null);
            Fill(bg); bg.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            var fillArea = new GameObject("FillArea", typeof(RectTransform));
            fillArea.layer = UILayer; fillArea.transform.SetParent(root.transform, false);
            Fill(fillArea);
            var fill = Img("Fill", fillArea.transform, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, null);
            Fill(fill); fill.GetComponent<Image>().color = new Color(0.3f, 0.85f, 0.3f);

            var slider = root.GetComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.minValue = 0; slider.maxValue = 100; slider.value = 100;
            slider.interactable = false; slider.transition = Selectable.Transition.None;

            var ui = root.AddComponent<StaminaBarUI>();
            Wire(ui, ("slider", slider), ("fillImage", fill.GetComponent<Image>()));
        }

        // ---------- helpers ----------

        static GameObject Obj(string n, Transform p, Vector3 pos)
        {
            var g = new GameObject(n);
            if (p != null) g.transform.SetParent(p, false);
            g.transform.position = pos;
            return g;
        }

        static Sprite S(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

        static Font F() => Font.CreateDynamicFontFromOSFont("Arial", 14) ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        static GameObject UIObj(string n, Transform p, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var g = new GameObject(n, typeof(RectTransform));
            g.layer = UILayer;
            g.transform.SetParent(p, false);
            var rt = g.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return g;
        }

        static GameObject Label(string n, string txt, Transform p, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c)
        {
            var g = UIObj(n, p, anchor, pos, size);
            var t = g.AddComponent<Text>();
            t.text = txt; t.fontSize = fs; t.fontStyle = FontStyle.Bold;
            t.color = c; t.alignment = TextAnchor.MiddleCenter; t.font = F();
            if (anchor.y >= 1f) { var rt = g.GetComponent<RectTransform>(); rt.pivot = new Vector2(rt.pivot.x, 1); }
            return g;
        }

        static GameObject Panel(string n, Transform p, Vector2 anchor, Vector2 pos, Vector2 size, Color c)
        {
            var g = UIObj(n, p, anchor, pos, size);
            g.AddComponent<Image>().color = c;
            return g;
        }

        static GameObject Img(string n, Transform p, Vector2 anchor, Vector2 pos, Vector2 size, string spritePath)
        {
            var g = UIObj(n, p, anchor, pos, size);
            var i = g.AddComponent<Image>();
            if (spritePath != null) { i.sprite = S(spritePath); i.preserveAspect = true; }
            return g;
        }

        static GameObject Btn(string n, string label, Transform p, Vector2 anchor, Vector2 pos, Vector2 size, Color c, int fs)
        {
            var g = UIObj(n, p, anchor, pos, size);
            var rt = g.GetComponent<RectTransform>();
            rt.pivot = anchor;
            g.AddComponent<Image>().color = c;
            g.AddComponent<Button>();
            Label("L", label, g.transform, new Vector2(0.5f, 0.5f), Vector2.zero, size, fs, Color.white);
            return g;
        }

        static Button PanelBtn(GameObject panel, int idx, string label, Color c)
        {
            var b = Btn($"Btn_{idx}", label, panel.transform, new Vector2(0.5f, 1), new Vector2(0, -20 - idx * 170), new Vector2(620, 150), c, 54);
            var rt = b.GetComponent<RectTransform>(); rt.pivot = new Vector2(0.5f, 1);
            return b.GetComponent<Button>();
        }

        static Button BuildBtn(GameObject panel, int idx, string label, string iconPath)
        {
            var b = Btn($"BuildBtn_{idx}", "", panel.transform, new Vector2(0, 0.5f), new Vector2(20 + idx * 345, 0), new Vector2(325, 150), new Color(0.35f, 0.3f, 0.2f), 44);
            var rt = b.GetComponent<RectTransform>(); rt.pivot = new Vector2(0, 0.5f);
            Img("Icon", b.transform, new Vector2(0, 0.5f), new Vector2(20, 0), new Vector2(110, 110), iconPath)
                .GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
            Label("T", label, b.transform, new Vector2(0.5f, 0.5f), new Vector2(60, 0), new Vector2(200, 140), 44, Color.white);
            return b.GetComponent<Button>();
        }

        static void Fill(GameObject g)
        {
            var rt = g.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static void Center(GameObject g)
        {
            var rt = g.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        static void Wire(Component target, params (string field, Object value)[] refs)
        {
            var so = new SerializedObject(target);
            foreach (var (field, value) in refs)
            {
                var prop = so.FindProperty(field);
                if (prop == null) { Debug.LogWarning($"[SurfaceSceneAuthor] {target.GetType().Name}.{field} not found"); continue; }
                prop.objectReferenceValue = value;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddToBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.All(s => s.path != ScenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        /// <summary>Deterministic 1920x1080 capture incl. overlay UI (temporarily switches canvas to camera space).</summary>
        [MenuItem("Hermes/Capture Surface 1920")]
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
            var file = System.IO.Path.Combine(dir, "surface_capture.png");
            System.IO.File.WriteAllBytes(file, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(rt);
            Debug.Log($"[Capture] saved {file}");
        }
    }
}
