using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MinersWatch.Editor
{
    public class GameSceneBuilder : IProcessSceneWithReport
    {
        public int callbackOrder => -90;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (scene.name != "TestGround") return;
            if (GameObject.Find("GameCanvas") != null) return;
            BuildUI();
        }

        static void BuildUI()
        {
            int L = 5;

            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                new GameObject("EventSystem").AddComponent<UnityEngine.EventSystems.EventSystem>()
                    .gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            var go = new GameObject("GameCanvas", typeof(RectTransform));
            go.layer = L;
            go.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            go.AddComponent<GraphicRaycaster>();

            // Back button: top-left, orange, 400x120, direct scene unload
            var bb = C("BackToMenuBtn", go.transform, L);
            var br = bb.GetComponent<RectTransform>();
            br.anchorMin = br.anchorMax = br.pivot = new Vector2(0, 1);
            br.sizeDelta = new Vector2(400, 120);
            br.anchoredPosition = new Vector2(30, -30);
            bb.AddComponent<Image>().color = new Color(0.9f, 0.45f, 0.05f);
            var bbtn = bb.AddComponent<Button>();

            var bl = C("L", bb.transform, L); FS(bl);
            var bt = bl.AddComponent<Text>();
            bt.text = "← 菜单"; bt.fontSize = 52; bt.fontStyle = FontStyle.Bold;
            bt.color = Color.white; bt.alignment = TextAnchor.MiddleCenter; bt.font = GF();

            // Direct unload TestGround + show MainMenu canvas
            bbtn.onClick.AddListener(() => {
                SceneManager.UnloadSceneAsync("TestGround");
                var mc = GameObject.Find("MainCanvas");
                if (mc != null) mc.GetComponent<Canvas>().enabled = true;
            });

            // GameOver overlay
            var ov = C("GameOverOverlay", go.transform, L); FS(ov);
            ov.AddComponent<Image>().color = new Color(0, 0, 0, 0.9f);

            var gt = C("GameOverText", ov.transform, L);
            var gr = gt.GetComponent<RectTransform>();
            gr.anchorMin = gr.anchorMax = new Vector2(0.5f, 0.55f); gr.sizeDelta = new Vector2(800, 160);
            var gtt = gt.AddComponent<Text>();
            gtt.text = "游戏结束"; gtt.fontSize = 112; gtt.color = new Color(0.9f, 0.2f, 0.2f);
            gtt.alignment = TextAnchor.MiddleCenter; gtt.font = GF();

            var rb = MB("RestartBtn", "重新开始", new Vector2(0, -60), ov.transform, L);
            var mb = MB("GOMainMenuBtn", "返回主菜单", new Vector2(0, -220), ov.transform, L);

            var ui = ov.AddComponent<GameOverUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("_gameOverPanel").objectReferenceValue = ov;
            so.FindProperty("_victoryPanel").objectReferenceValue = ov;
            so.FindProperty("_restartButton").objectReferenceValue = rb?.GetComponent<Button>();
            so.FindProperty("_mainMenuButton").objectReferenceValue = mb?.GetComponent<Button>();
            so.ApplyModifiedProperties();

            ov.SetActive(false);
        }

        static GameObject C(string n, Transform p, int l) { var g = new GameObject(n, typeof(RectTransform)); g.layer = l; g.transform.SetParent(p, false); return g; }
        static GameObject MB(string n, string lb, Vector2 pos, Transform p, int L)
        {
            var g = C(n, p, L); var r = g.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f); r.sizeDelta = new Vector2(680, 160); r.anchoredPosition = pos;
            g.AddComponent<Image>().color = new Color(0.2f, 0.5f, 0.2f); g.AddComponent<Button>();
            var l = C("L", g.transform, L); FS(l);
            var t = l.AddComponent<Text>(); t.text = lb; t.fontSize = 56; t.fontStyle = FontStyle.Bold;
            t.color = Color.white; t.alignment = TextAnchor.MiddleCenter; t.font = GF();
            return g;
        }
        static void FS(GameObject g) { var r = g.GetComponent<RectTransform>(); r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
        static Font GF() => Font.CreateDynamicFontFromOSFont("Arial", 14) ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
}
