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

            Debug.Log("[MainMenuBuilder] Building MainMenu UI...");
            BuildUI();
            Debug.Log("[MainMenuBuilder] Done.");
        }

        static void BuildUI()
        {
            int layer = 5; // UI layer

            // EventSystem
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                new GameObject("EventSystem")
                    .AddComponent<UnityEngine.EventSystems.EventSystem>()
                    .gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Canvas
            var canvasGo = new GameObject("MainCanvas", typeof(RectTransform));
            canvasGo.layer = layer;
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            // Dark background (was red debug — confirmed canvas works)
            var bg = NewChild("BgPanel", canvasGo.transform, layer);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.06f, 0.06f, 0.10f, 1f);
            FullStretch(bg);

            // Title
            var t = NewText("Title", "矿工守夜", 64, Color.white, canvasGo.transform, layer,
                new Vector2(0.5f, 0.82f), new Vector2(500, 80));

            // Buttons — large, green, white border, white text
            var b1 = MakeBtn("NewGameBtn",  "新游戏",   new Vector2(0, -20),  canvasGo.transform, layer);
            var b2 = MakeBtn("ContinueBtn", "继续游戏", new Vector2(0, -100), canvasGo.transform, layer);
            var b3 = MakeBtn("SettingsBtn", "设置",     new Vector2(0, -180), canvasGo.transform, layer);

            // Wire MainMenuUI
            var menu = canvasGo.AddComponent<MainMenuUI>();
            var so = new SerializedObject(menu);
            so.FindProperty("_newGameButton").objectReferenceValue  = b1?.GetComponent<Button>();
            so.FindProperty("_continueButton").objectReferenceValue = b2?.GetComponent<Button>();
            so.FindProperty("_settingsButton").objectReferenceValue = b3?.GetComponent<Button>();
            so.FindProperty("_mainCanvas").objectReferenceValue = canvas;
            so.ApplyModifiedProperties();
        }

        static GameObject NewChild(string n, Transform p, int layer)
        {
            var go = new GameObject(n, typeof(RectTransform));
            go.layer = layer;
            go.transform.SetParent(p, false);
            return go;
        }

        static GameObject NewText(string name, string text, int size, Color c, Transform p, int layer,
            Vector2 anchor, Vector2 dims)
        {
            var go = NewChild(name, p, layer);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.sizeDelta = dims;
            var t = go.AddComponent<Text>();
            t.text = text;
            t.fontSize = size;
            t.color = c;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = GetFont();
            return go;
        }

        static GameObject MakeBtn(string name, string label, Vector2 pos, Transform parent, int layer)
        {
            var go = NewChild(name, parent, layer);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(340, 80);
            rt.anchoredPosition = pos;

            go.AddComponent<Image>().color = new Color(0.2f, 0.65f, 0.2f); // green
            go.AddComponent<Button>();
            // white outline
            var ol = go.AddComponent<Outline>();
            ol.effectColor = Color.white;
            ol.effectDistance = new Vector2(3, -3);

            // Label
            var l = NewChild("Label", go.transform, layer);
            FullStretch(l);
            var t = l.AddComponent<Text>();
            t.text = label;
            t.fontSize = 32;
            t.fontStyle = FontStyle.Bold;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = GetFont();
            return go;
        }

        static void FullStretch(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static Font GetFont()
        {
            return Font.CreateDynamicFontFromOSFont("Arial", 14)
                ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
