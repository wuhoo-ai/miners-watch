using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MinersWatch.Editor
{
    /// <summary>
    /// Auto-builds MainMenu UI during CI builds (no manual Editor work needed).
    /// Runs via IProcessSceneWithReport — triggered before every build.
    /// Safe to delete once the MainMenu scene is set up manually in Unity Editor.
    /// </summary>
    public class MainMenuBuilder : IProcessSceneWithReport
    {
        public int callbackOrder => -100; // run early

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (scene.name != "MainMenu") return;
            if (GameObject.Find("MainCanvas") != null) return; // already built

            BuildMainMenuUI();
        }

        private static void BuildMainMenuUI()
        {
            // 1. EventSystem (required for UI interaction)
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // 2. Canvas
            var canvasGo = new GameObject("MainCanvas");
            canvasGo.layer = LayerMask.NameToLayer("UI");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // 3. Background panel
            var bg = CreateUIElement("BgPanel", canvasGo.transform);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.08f, 0.12f, 1f); // dark cave feel
            StretchRect(bg);

            // 4. Title
            var title = CreateUIElement("Title", canvasGo.transform);
            var titleText = title.AddComponent<Text>();
            titleText.text = "矿工守夜";
            titleText.font = GetFont();
            titleText.fontSize = 72;
            titleText.color = new Color(1f, 0.85f, 0.3f, 1f); // gold
            titleText.alignment = TextAnchor.MiddleCenter;
            var titleRt = title.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.7f);
            titleRt.anchorMax = new Vector2(0.5f, 0.7f);
            titleRt.sizeDelta = new Vector2(600, 100);
            titleRt.anchoredPosition = Vector2.zero;

            // 5. Subtitle
            var sub = CreateUIElement("Subtitle", canvasGo.transform);
            var subText = sub.AddComponent<Text>();
            subText.text = "Miner's Watch";
            subText.font = GetFont();
            subText.fontSize = 28;
            subText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            subText.alignment = TextAnchor.MiddleCenter;
            var subRt = sub.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0.5f, 0.63f);
            subRt.anchorMax = new Vector2(0.5f, 0.63f);
            subRt.sizeDelta = new Vector2(400, 50);
            subRt.anchoredPosition = Vector2.zero;

            // 6. Button container (removed VLG — manual positioning is more reliable in batch mode)
            // btnContainer kept but unused; buttons are placed directly on canvas

            // 7. Buttons (manual positioning, no VLG to avoid batch-mode layout issues)
            float btnY = -40;
            CreateMenuButton("NewGameBtn", "新游戏", canvasGo.transform, new Vector2(0, btnY),
                new Color(0.15f, 0.55f, 0.15f), Color.white);
            btnY -= 76;
            CreateMenuButton("ContinueBtn", "继续游戏", canvasGo.transform, new Vector2(0, btnY),
                new Color(0.25f, 0.25f, 0.30f), new Color(0.5f, 0.5f, 0.5f));
            btnY -= 76;
            CreateMenuButton("SettingsBtn", "设置", canvasGo.transform, new Vector2(0, btnY),
                new Color(0.25f, 0.25f, 0.30f), new Color(0.5f, 0.5f, 0.5f));

            // 8. Version text
            var ver = CreateUIElement("Version", canvasGo.transform);
            var verText = ver.AddComponent<Text>();
            verText.text = "v0.1.0 — Demo";
            verText.font = GetFont();
            verText.fontSize = 16;
            verText.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            verText.alignment = TextAnchor.MiddleRight;
            var verRt = ver.GetComponent<RectTransform>();
            verRt.anchorMin = new Vector2(1, 0);
            verRt.anchorMax = new Vector2(1, 0);
            verRt.pivot = new Vector2(1, 0);
            verRt.sizeDelta = new Vector2(200, 30);
            verRt.anchoredPosition = new Vector2(-20, 20);

            // 9. Attach MainMenuUI script
            var menuUI = canvasGo.AddComponent<MainMenuUI>();
            // Wire buttons via reflection (they'll be found in MainMenuUI.Awake by name)
            var newGameBtn = GameObject.Find("NewGameBtn");
            var continueBtn = GameObject.Find("ContinueBtn");
            var settingsBtn = GameObject.Find("SettingsBtn");

            // Use SerializedObject to wire up private fields
            var so = new SerializedObject(menuUI);
            so.FindProperty("_newGameButton").objectReferenceValue = newGameBtn?.GetComponent<Button>();
            so.FindProperty("_continueButton").objectReferenceValue = continueBtn?.GetComponent<Button>();
            so.FindProperty("_settingsButton").objectReferenceValue = settingsBtn?.GetComponent<Button>();
            so.FindProperty("_mainCanvas").objectReferenceValue = canvas;
            // SceneController is found dynamically in MainMenuUI
            so.ApplyModifiedProperties();

            Debug.Log("[MainMenuBuilder] MainMenu UI built successfully.");
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void StretchRect(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void CreateMenuButton(string name, string label, Transform parent, Vector2 anchoredPos, Color bgColor, Color textColor)
        {
            var go = CreateUIElement(name, parent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(260, 60);
            rt.anchoredPosition = anchoredPos;

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = bgColor * 1.4f;
            colors.pressedColor = bgColor * 0.7f;
            colors.disabledColor = bgColor * 0.5f;
            btn.colors = colors;

            // Label
            var labelGo = CreateUIElement("Label", go.transform);
            var labelText = labelGo.AddComponent<Text>();
            labelText.text = label;
            labelText.font = GetFont();
            labelText.fontSize = 28;
            labelText.fontStyle = FontStyle.Bold;
            labelText.color = textColor;
            labelText.alignment = TextAnchor.MiddleCenter;
            StretchRect(labelGo);
        }

        private static Font GetFont()
        {
            // Unity 6 compatible: try OS font, fallback to built-in, then null (uses default)
            Font f = Font.CreateDynamicFontFromOSFont("Arial", 14);
            if (f != null) return f;
            f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f != null) return f;
            // Return null — Text component will use its own default
            return null;
        }
    }
}
