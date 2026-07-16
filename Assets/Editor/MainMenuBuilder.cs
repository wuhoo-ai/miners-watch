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
    }
}
