using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MinersWatch.Editor
{
    /// <summary>Shared primitives for scene authoring scripts (Surface + Caves). Editor-only.</summary>
    internal static class SceneKit
    {
        internal const int UILayer = 5;

        // ---------- world ----------

        internal static GameObject Obj(string n, Transform p, Vector3 pos)
        {
            var g = new GameObject(n);
            if (p != null) g.transform.SetParent(p, false);
            g.transform.position = pos;
            return g;
        }

        internal static Sprite S(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

        internal static Font F() => Font.CreateDynamicFontFromOSFont("Arial", 14) ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        /// <summary>Flat colored rectangle built from a sliced white-ish sprite. Placeholder visuals.</summary>
        internal static GameObject Rect2D(string n, Transform p, Vector3 pos, Vector2 size, Color c, int sorting)
        {
            var g = Obj(n, p, pos);
            var sr = g.AddComponent<SpriteRenderer>();
            sr.sprite = S("Assets/Sprites/UI/ui_stamina_bar.png");
            sr.color = c;
            sr.sortingOrder = sorting;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            return g;
        }

        // ---------- UI ----------

        internal static GameObject MakeCanvas()
        {
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                new GameObject("EventSystem").AddComponent<UnityEngine.EventSystems.EventSystem>()
                    .gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            var canvas = new GameObject("GameCanvas", typeof(RectTransform));
            canvas.layer = UILayer;
            canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvas.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        internal static GameObject UIObj(string n, Transform p, Vector2 anchor, Vector2 pos, Vector2 size)
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

        internal static GameObject Label(string n, string txt, Transform p, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c)
        {
            var g = UIObj(n, p, anchor, pos, size);
            var t = g.AddComponent<Text>();
            t.text = txt; t.fontSize = fs; t.fontStyle = FontStyle.Bold;
            t.color = c; t.alignment = TextAnchor.MiddleCenter; t.font = F();
            if (anchor.y >= 1f) { var rt = g.GetComponent<RectTransform>(); rt.pivot = new Vector2(rt.pivot.x, 1); }
            return g;
        }

        internal static GameObject Panel(string n, Transform p, Vector2 anchor, Vector2 pos, Vector2 size, Color c)
        {
            var g = UIObj(n, p, anchor, pos, size);
            g.AddComponent<Image>().color = c;
            return g;
        }

        internal static GameObject Img(string n, Transform p, Vector2 anchor, Vector2 pos, Vector2 size, string spritePath)
        {
            var g = UIObj(n, p, anchor, pos, size);
            var i = g.AddComponent<Image>();
            if (spritePath != null) { i.sprite = S(spritePath); i.preserveAspect = true; }
            return g;
        }

        internal static GameObject Btn(string n, string label, Transform p, Vector2 anchor, Vector2 pos, Vector2 size, Color c, int fs)
        {
            var g = UIObj(n, p, anchor, pos, size);
            g.GetComponent<RectTransform>().pivot = anchor;
            g.AddComponent<Image>().color = c;
            g.AddComponent<Button>();
            Label("L", label, g.transform, new Vector2(0.5f, 0.5f), Vector2.zero, size, fs, Color.white);
            return g;
        }

        internal static Button PanelBtn(GameObject panel, int idx, string label, Color c)
        {
            var b = Btn($"Btn_{idx}", label, panel.transform, new Vector2(0.5f, 1), new Vector2(0, -20 - idx * 170), new Vector2(620, 150), c, 54);
            b.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
            return b.GetComponent<Button>();
        }

        internal static void Fill(GameObject g)
        {
            var rt = g.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        internal static void Center(GameObject g) => g.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

        /// <summary>Slider-based stamina bar wired to StaminaBarUI (player found by tag at runtime).</summary>
        internal static void BuildStaminaBar(Transform parent, Vector2 pos)
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

        /// <summary>Bottom inventory bar with an inactive slot template, wired to InventoryUI (_inventory ← GameRoot fallback).</summary>
        internal static void BuildInventoryBar(GameObject canvas, Vector2 pos)
        {
            var invBar = Panel("InventoryBar", canvas.transform, new Vector2(0.5f, 0), pos, new Vector2(640, 130), new Color(0.08f, 0.08f, 0.12f, 0.6f));
            invBar.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
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
            Wire(invUI, ("_slotContainer", invBar.transform), ("_slotPrefab", slotTpl));
        }

        /// <summary>Top-center day/night HUD wired to DayNightUI (_cycle ← GameRoot fallback).</summary>
        internal static void BuildDayNightHUD(GameObject canvas)
        {
            var ct = canvas.transform;
            var phase = Label("PhaseText", "白天", ct, new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(500, 90), 64, Color.white);
            var timer = Label("TimerText", "120", ct, new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(500, 80), 56, new Color(1f, 0.95f, 0.6f));
            var warn = Panel("WarningPanel", ct, new Vector2(0.5f, 1), new Vector2(0, -250), new Vector2(900, 100), new Color(0.85f, 0.2f, 0.1f, 0.85f));
            Label("L", "⚠ 夜晚将至，准备防御！", warn.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(880, 90), 52, Color.white);
            warn.SetActive(false);
            var dnUI = canvas.AddComponent<DayNightUI>();
            Wire(dnUI, ("_phaseText", phase.GetComponent<Text>()),
                       ("_timerText", timer.GetComponent<Text>()),
                       ("_warningPanel", warn));
        }

        /// <summary>Touch overlay: virtual joystick + jump (+ optional mine) buttons. Self-hides on non-touch platforms.</summary>
        internal static void BuildTouchControls(GameObject canvas, bool withMine, Vector2 joyPos)
        {
            var root = UIObj("TouchControls", canvas.transform, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Fill(root);
            root.AddComponent<TouchControlsRoot>();

            // Joystick (bottom-left area)
            var joyBg = Panel("Joystick", root.transform, new Vector2(0, 0), joyPos, new Vector2(300, 300), new Color(1f, 1f, 1f, 0.12f));
            joyBg.GetComponent<RectTransform>().pivot = Vector2.zero;
            var knob = Panel("Knob", joyBg.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(130, 130), new Color(1f, 1f, 1f, 0.45f));
            var joy = joyBg.AddComponent<VirtualJoystick>();
            Wire(joy, ("_knob", knob.GetComponent<RectTransform>()));

            // Jump (bottom-right)
            var jump = Panel("JumpBtn", root.transform, new Vector2(1, 0), new Vector2(-60, 60), new Vector2(240, 240), new Color(0.3f, 0.8f, 0.4f, 0.35f));
            jump.GetComponent<RectTransform>().pivot = new Vector2(1, 0);
            Label("L", "跳", jump.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220, 220), 96, Color.white);
            jump.AddComponent<TouchActionButton>().SetKind(TouchActionButton.Kind.Jump);

            if (withMine)
            {
                var mine = Panel("MineBtn", root.transform, new Vector2(1, 0), new Vector2(-60, 340), new Vector2(240, 240), new Color(0.9f, 0.7f, 0.2f, 0.35f));
                mine.GetComponent<RectTransform>().pivot = new Vector2(1, 0);
                Label("L", "挖", mine.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220, 220), 96, Color.white);
                mine.AddComponent<TouchActionButton>().SetKind(TouchActionButton.Kind.Mine);
            }
        }

        // ---------- plumbing ----------

        internal static void Wire(Component target, params (string field, Object value)[] refs)
        {
            var so = new SerializedObject(target);
            foreach (var (field, value) in refs)
            {
                var prop = so.FindProperty(field);
                if (prop == null) { Debug.LogWarning($"[SceneKit] {target.GetType().Name}.{field} not found"); continue; }
                prop.objectReferenceValue = value;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void SetFloats(Component target, params (string field, float value)[] vals)
        {
            var so = new SerializedObject(target);
            foreach (var (field, value) in vals)
            {
                var prop = so.FindProperty(field);
                if (prop == null) { Debug.LogWarning($"[SceneKit] {target.GetType().Name}.{field} not found"); continue; }
                prop.floatValue = value;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void AddSceneToBuild(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.All(s => s.path != scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        internal static void EnsureTag(string tag)
        {
            var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tags = tm.FindProperty("tags");
            for (int i = 0; i < tags.arraySize; i++)
                if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            tm.ApplyModifiedProperties();
        }
    }
}
