using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MinersWatch.Editor
{
    /// <summary>
    /// Auto-builds MainMenu UI before CI builds. Uses IPreprocessBuildWithReport
    /// to modify the scene before the build pipeline processes it.
    /// Safe to delete once the MainMenu scene is set up manually in Unity Editor.
    /// </summary>
    public class MainMenuBuilder : IPreprocessBuildWithReport
    {
        public int callbackOrder => -100;

        public void OnPreprocessBuild(BuildReport report)
        {
            // Only process if MainMenu scene is in the build
            var mainMenuScene = SceneManager.GetSceneByName("MainMenu");
            if (!mainMenuScene.isLoaded)
            {
                // Need to load it to modify
                mainMenuScene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Additive);
            }
            if (!mainMenuScene.IsValid() || GameObject.Find("MainCanvas") != null) return;

            BuildMainMenuUI();
            EditorSceneManager.SaveScene(mainMenuScene);
            Debug.Log("[MainMenuBuilder] Scene saved with UI.");
        }

        private static void BuildMainMenuUI()
        {
            // 0. Set UI layer
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer < 0) { Debug.LogWarning("[MainMenuBuilder] No UI layer found"); uiLayer = 5; }

            // 1. EventSystem
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                new GameObject("EventSystem")
                    .AddComponent<UnityEngine.EventSystems.EventSystem>()
                    .gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // 2. Canvas (full-screen, overlay)
            var canvasGo = new GameObject("MainCanvas", typeof(RectTransform));
            canvasGo.layer = uiLayer;
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // 3. Background
            var bg = new GameObject("BgPanel", typeof(RectTransform));
            bg.layer = uiLayer;
            bg.transform.SetParent(canvasGo.transform, false);
            Stretch(bg.GetComponent<RectTransform>());
            bg.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.08f, 1f);

            // 4. Title — "矿工守夜"
            MakeText("Title", "矿工守夜", 72, new Color(1f, 0.85f, 0.2f),
                new Vector2(0.5f, 0.75f), new Vector2(600, 100), canvasGo.transform, uiLayer);

            // 5. Subtitle — "Miner's Watch"
            MakeText("Subtitle", "Miner's Watch", 28, new Color(0.5f, 0.5f, 0.55f),
                new Vector2(0.5f, 0.66f), new Vector2(400, 50), canvasGo.transform, uiLayer);

            // 6. Buttons — big, bold, impossible to miss
            float y = -50;
            MakeButton("NewGameBtn",    "新游戏",     new Vector2(0, y),      new Color(0.15f, 0.7f, 0.15f), Color.white, canvasGo.transform, uiLayer);
            y -= 90;
            MakeButton("ContinueBtn",   "继续游戏",   new Vector2(0, y),      new Color(0.3f, 0.3f, 0.35f),    Color.white, canvasGo.transform, uiLayer);
            y -= 90;
            MakeButton("SettingsBtn",   "设置",       new Vector2(0, y),      new Color(0.3f, 0.3f, 0.35f),    Color.white, canvasGo.transform, uiLayer);

            // 7. Version
            MakeText("Version", "v0.1.0", 16, new Color(0.25f, 0.25f, 0.3f),
                new Vector2(1f, 0f), new Vector2(200, 30), canvasGo.transform, uiLayer,
                new Vector2(1, 0), new Vector2(-20, 20));

            // 8. Attach MainMenuUI
            var menuUI = canvasGo.AddComponent<MainMenuUI>();
            var so = new SerializedObject(menuUI);
            so.FindProperty("_newGameButton").objectReferenceValue  = GameObject.Find("NewGameBtn")?.GetComponent<Button>();
            so.FindProperty("_continueButton").objectReferenceValue = GameObject.Find("ContinueBtn")?.GetComponent<Button>();
            so.FindProperty("_settingsButton").objectReferenceValue = GameObject.Find("SettingsBtn")?.GetComponent<Button>();
            so.FindProperty("_mainCanvas").objectReferenceValue = canvas;
            so.ApplyModifiedProperties();

            Debug.Log("[MainMenuBuilder] Done — title + 3 buttons + version.");
        }

        private static void MakeText(string name, string text, int fontSize, Color color,
            Vector2 anchor, Vector2 size, Transform parent, int layer,
            Vector2 pivot = default, Vector2 anchoredPos = default)
        {
            if (pivot == default) pivot = new Vector2(0.5f, 0.5f);
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = layer;
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            var t = go.AddComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = GetFont();
        }

        private static void MakeButton(string name, string label, Vector2 pos, Color bg, Color fg,
            Transform parent, int layer)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = layer;
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(320, 80);
            rt.anchoredPosition = pos;

            var img = go.AddComponent<Image>();
            img.color = bg;

            var btn = go.AddComponent<Button>();
            var cb = btn.colors;
            cb.normalColor = bg;
            cb.highlightedColor = bg * 1.5f;
            cb.pressedColor = bg * 0.6f;
            btn.colors = cb;

            // White outline for visibility
            var outline = go.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(2, 2);

            // Label
            var lbl = new GameObject("Label", typeof(RectTransform));
            lbl.layer = layer;
            lbl.transform.SetParent(go.transform, false);
            Stretch(lbl.GetComponent<RectTransform>());
            var t = lbl.AddComponent<Text>();
            t.text = label;
            t.fontSize = 32;
            t.fontStyle = FontStyle.Bold;
            t.color = fg;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = GetFont();
        }

        private static Font GetFont()
        {
            Font f = Font.CreateDynamicFontFromOSFont("Arial", 14);
            return f ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
