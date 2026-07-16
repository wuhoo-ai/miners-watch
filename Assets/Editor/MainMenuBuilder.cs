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

            Debug.Log("[MainMenuBuilder] Building...");
            BuildUI();
            Debug.Log("[MainMenuBuilder] Done.");
        }

        static void BuildUI()
        {
            int layer = 5;

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

            // Background
            var bg = NewChild("BgPanel", canvasGo.transform, layer);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.06f, 0.06f, 0.10f, 1f);
            FullStretch(bg);

            // Title — large gold
            NewText("Title", "矿工守夜", 80, new Color(1f, 0.88f, 0.25f),
                canvasGo.transform, layer, new Vector2(0.5f, 0.80f), new Vector2(640, 100));

            // Subtitle
            NewText("Subtitle", "Miner's Watch", 32, new Color(0.5f, 0.5f, 0.55f),
                canvasGo.transform, layer, new Vector2(0.5f, 0.70f), new Vector2(400, 50));

            // Buttons — large, green, white outline
            var b1 = MakeBtn("NewGameBtn",  "新游戏",   new Vector2(0, -30),  canvasGo.transform, layer);
            var b2 = MakeBtn("ContinueBtn", "继续游戏", new Vector2(0, -130), canvasGo.transform, layer);
            var b3 = MakeBtn("SettingsBtn", "设置",     new Vector2(0, -230), canvasGo.transform, layer);

            // Wire MainMenuUI
            var menu = canvasGo.AddComponent<MainMenuUI>();
            var so = new SerializedObject(menu);
            so.FindProperty("_newGameButton").objectReferenceValue  = b1?.GetComponent<Button>();
            so.FindProperty("_continueButton").objectReferenceValue = b2?.GetComponent<Button>();
            so.FindProperty("_settingsButton").objectReferenceValue = b3?.GetComponent<Button>();
            so.FindProperty("_mainCanvas").objectReferenceValue = canvas;
            so.ApplyModifiedProperties();
        }

        static GameObject NewChild(string n, Transform p, int l)
        {
            var go = new GameObject(n, typeof(RectTransform)); go.layer = l; go.transform.SetParent(p, false); return go;
        }

        static void NewText(string name, string text, int size, Color c, Transform p, int l, Vector2 anchor, Vector2 dims)
        {
            var go = NewChild(name, p, l);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor; rt.sizeDelta = dims;
            var t = go.AddComponent<Text>();
            t.text = text; t.fontSize = size; t.color = c; t.alignment = TextAnchor.MiddleCenter;
            t.font = GetFont();
        }

        static GameObject MakeBtn(string name, string label, Vector2 pos, Transform parent, int layer)
        {
            var go = NewChild(name, parent, layer);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400, 100);
            rt.anchoredPosition = pos;
            go.AddComponent<Image>().color = new Color(0.2f, 0.65f, 0.2f);
            go.AddComponent<Button>();
            var ol = go.AddComponent<Outline>();
            ol.effectColor = Color.white; ol.effectDistance = new Vector2(3, -3);

            var l = NewChild("Label", go.transform, layer);
            FullStretch(l);
            var t = l.AddComponent<Text>();
            t.text = label; t.fontSize = 38; t.fontStyle = FontStyle.Bold;
            t.color = Color.white; t.alignment = TextAnchor.MiddleCenter;
            t.font = GetFont();
            return go;
        }

        static void FullStretch(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static Font GetFont() => Font.CreateDynamicFontFromOSFont("Arial", 14)
            ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
}
