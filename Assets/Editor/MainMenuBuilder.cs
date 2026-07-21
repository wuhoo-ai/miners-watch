using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MinersWatch.Editor
{
    public class MainMenuBuilder : IProcessSceneWithReport
    {
        public int callbackOrder => -100;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (scene.name != "MainMenu") return;
            if (GameObject.Find("MainCanvas") != null) return;
            BuildUI();
        }

        static void BuildUI()
        {
            int L = 5;

            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                new GameObject("EventSystem").AddComponent<UnityEngine.EventSystems.EventSystem>()
                    .gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            var go = new GameObject("MainCanvas", typeof(RectTransform));
            go.layer = L;
            var cv = go.AddComponent<Canvas>(); cv.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            go.AddComponent<GraphicRaycaster>();

            var bg = C("BgPanel", go.transform, L); bg.AddComponent<Image>().color = new Color(0.06f, 0.06f, 0.10f, 1f); FS(bg);

            T("Title", "矿工守夜", 160, new Color(1f, 0.88f, 0.25f), go.transform, L, new Vector2(0.5f, 0.82f), new Vector2(1000, 180));
            T("Subtitle", "Miner's Watch", 64, new Color(0.45f, 0.45f, 0.50f), go.transform, L, new Vector2(0.5f, 0.68f), new Vector2(600, 80));

            var b1 = B("NewGameBtn", "新游戏", new Vector2(0, -60), go.transform, L);
            var b2 = B("ContinueBtn", "继续游戏", new Vector2(0, -260), go.transform, L);
            var b3 = B("SettingsBtn", "设置", new Vector2(0, -460), go.transform, L);

            var m = go.AddComponent<MainMenuUI>();
            var so = new SerializedObject(m);
            so.FindProperty("_newGameButton").objectReferenceValue = b1?.GetComponent<Button>();
            so.FindProperty("_continueButton").objectReferenceValue = b2?.GetComponent<Button>();
            so.FindProperty("_settingsButton").objectReferenceValue = b3?.GetComponent<Button>();
            so.FindProperty("_mainCanvas").objectReferenceValue = cv;

            // Settings panel (overlay, hidden by default)
            var settingsCanvas = BuildSettingsPanel(go.transform, L);
            so.FindProperty("_settingsCanvas").objectReferenceValue = settingsCanvas.GetComponent<Canvas>();
            so.ApplyModifiedProperties();
        }

        static GameObject C(string n, Transform p, int l) { var g = new GameObject(n, typeof(RectTransform)); g.layer = l; g.transform.SetParent(p, false); return g; }
        static void T(string n, string tx, int sz, Color c, Transform p, int l, Vector2 a, Vector2 d)
        {
            var g = C(n, p, l); var r = g.GetComponent<RectTransform>(); r.anchorMin = r.anchorMax = a; r.sizeDelta = d;
            var t = g.AddComponent<Text>(); t.text = tx; t.fontSize = sz; t.color = c; t.alignment = TextAnchor.MiddleCenter; t.font = GF();
        }
        static GameObject B(string n, string lb, Vector2 pos, Transform p, int L)
        {
            var g = C(n, p, L); var r = g.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f); r.sizeDelta = new Vector2(800, 200); r.anchoredPosition = pos;
            g.AddComponent<Image>().color = new Color(0.2f, 0.65f, 0.2f); g.AddComponent<Button>();
            var o = g.AddComponent<Outline>(); o.effectColor = Color.white; o.effectDistance = new Vector2(4, -4);
            var l = C("L", g.transform, L); FS(l);
            var t = l.AddComponent<Text>(); t.text = lb; t.fontSize = 76; t.fontStyle = FontStyle.Bold; t.color = Color.white; t.alignment = TextAnchor.MiddleCenter; t.font = GF();
            return g;
        }
        static void FS(GameObject g) { var r = g.GetComponent<RectTransform>(); r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
        static Font GF() => Font.CreateDynamicFontFromOSFont("Arial", 14) ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        static GameObject BuildSettingsPanel(Transform parent, int L)
        {
            var panel = C("SettingsPanel", parent, L);
            FS(panel);
            panel.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.08f, 0.95f);
            var cv = panel.AddComponent<Canvas>();
            cv.overrideSorting = true;
            cv.sortingOrder = 200;

            // Title
            T("SettingsTitle", "设置", 90, Color.white, panel.transform, L, new Vector2(0.5f, 0.85f), new Vector2(600, 120));

            // Master volume
            T("MasterLabel", "主音量: 80%", 48, new Color(0.8f, 0.8f, 0.8f), panel.transform, L, new Vector2(0.5f, 0.62f), new Vector2(700, 60));
            var masterSlider = MakeSlider("MasterSlider", panel.transform, L, new Vector2(0.5f, 0.56f), 0.8f);

            // SFX volume
            T("SfxLabel", "音效: 100%", 48, new Color(0.8f, 0.8f, 0.8f), panel.transform, L, new Vector2(0.5f, 0.47f), new Vector2(700, 60));
            var sfxSlider = MakeSlider("SfxSlider", panel.transform, L, new Vector2(0.5f, 0.41f), 1f);

            // BGM volume
            T("BgmLabel", "音乐: 60%", 48, new Color(0.8f, 0.8f, 0.8f), panel.transform, L, new Vector2(0.5f, 0.32f), new Vector2(700, 60));
            var bgmSlider = MakeSlider("BgmSlider", panel.transform, L, new Vector2(0.5f, 0.26f), 0.6f);

            // Difficulty
            T("DiffLabel", "难度: 普通", 44, new Color(0.8f, 0.8f, 0.8f), panel.transform, L, new Vector2(0.5f, 0.19f), new Vector2(600, 50));
            var diffEasy = B("DiffEasy", "简单", new Vector2(-160, 250), panel.transform, L);
            diffEasy.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 90);
            diffEasy.transform.Find("L").GetComponent<Text>().fontSize = 44;
            var diffNormal = B("DiffNormal", "普通", new Vector2(0, 250), panel.transform, L);
            diffNormal.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 90);
            diffNormal.transform.Find("L").GetComponent<Text>().fontSize = 44;
            var diffHard = B("DiffHard", "困难", new Vector2(160, 250), panel.transform, L);
            diffHard.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 90);
            diffHard.transform.Find("L").GetComponent<Text>().fontSize = 44;

            // Help text
            T("HelpLabel", "摇杆=移动 | 跳=跳跃 | 击=攻击 | 挖=采矿", 32, new Color(0.5f, 0.5f, 0.55f), panel.transform, L, new Vector2(0.5f, 0.11f), new Vector2(1400, 50));

            // Version
            T("VersionLabel", "矿工守夜 v1.2", 36, new Color(0.35f, 0.35f, 0.4f), panel.transform, L, new Vector2(0.5f, 0.05f), new Vector2(800, 50));

            // Close button
            var closeBtn = B("CloseSettingsBtn", "关闭", new Vector2(0, 540), panel.transform, L);
            closeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 120);
            var closeLabel = closeBtn.transform.Find("L")?.GetComponent<Text>();
            if (closeLabel != null) closeLabel.fontSize = 56;

            // Wire SettingsUI
            var ui = panel.AddComponent<SettingsUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("_masterSlider").objectReferenceValue = masterSlider.GetComponent<Slider>();
            so.FindProperty("_sfxSlider").objectReferenceValue = sfxSlider.GetComponent<Slider>();
            so.FindProperty("_bgmSlider").objectReferenceValue = bgmSlider.GetComponent<Slider>();
            so.FindProperty("_masterLabel").objectReferenceValue = GameObject.Find("MasterLabel")?.GetComponent<Text>();
            so.FindProperty("_sfxLabel").objectReferenceValue = GameObject.Find("SfxLabel")?.GetComponent<Text>();
            so.FindProperty("_bgmLabel").objectReferenceValue = GameObject.Find("BgmLabel")?.GetComponent<Text>();
            so.FindProperty("_versionLabel").objectReferenceValue = GameObject.Find("VersionLabel")?.GetComponent<Text>();
            so.ApplyModifiedProperties();

            // Difficulty button wiring
            var diffLabel = GameObject.Find("DiffLabel")?.GetComponent<Text>();
            diffEasy.GetComponent<Button>().onClick.AddListener(() => {
                GameSettings.Current = GameSettings.Difficulty.Easy;
                if (diffLabel != null) diffLabel.text = "难度: 简单";
            });
            diffNormal.GetComponent<Button>().onClick.AddListener(() => {
                GameSettings.Current = GameSettings.Difficulty.Normal;
                if (diffLabel != null) diffLabel.text = "难度: 普通";
            });
            diffHard.GetComponent<Button>().onClick.AddListener(() => {
                GameSettings.Current = GameSettings.Difficulty.Hard;
                if (diffLabel != null) diffLabel.text = "难度: 困难";
            });

            // Close button hides the panel
            closeBtn.GetComponent<Button>().onClick.AddListener(() => cv.enabled = false);

            cv.enabled = false; // hidden by default
            return panel;
        }

        static GameObject MakeSlider(string name, Transform parent, int L, Vector2 pos, float defaultValue)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            go.layer = L;
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = pos;
            rt.sizeDelta = new Vector2(1200, 60);

            // Background
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.layer = L; bg.transform.SetParent(go.transform, false);
            var bgrt = bg.GetComponent<RectTransform>();
            bgrt.anchorMin = Vector2.zero; bgrt.anchorMax = Vector2.one;
            bgrt.offsetMin = Vector2.zero; bgrt.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);

            // Fill Area
            var fillArea = new GameObject("FillArea", typeof(RectTransform));
            fillArea.layer = L; fillArea.transform.SetParent(go.transform, false);
            var fart = fillArea.GetComponent<RectTransform>();
            fart.anchorMin = Vector2.zero; fart.anchorMax = Vector2.one;
            fart.offsetMin = new Vector2(4, 6); fart.offsetMax = new Vector2(-4, -6);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.layer = L; fill.transform.SetParent(fillArea.transform, false);
            var frt = fill.GetComponent<RectTransform>();
            frt.anchorMin = Vector2.zero; frt.anchorMax = new Vector2(1, 1);
            frt.offsetMin = frt.offsetMax = Vector2.zero;
            fill.GetComponent<Image>().color = new Color(0.3f, 0.7f, 0.3f);

            var slider = go.GetComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.minValue = 0; slider.maxValue = 1; slider.value = defaultValue;
            slider.interactable = true;
            slider.transition = Selectable.Transition.None;

            return go;
        }
    }
}
