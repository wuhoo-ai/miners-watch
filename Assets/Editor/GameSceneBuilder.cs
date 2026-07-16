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

            Debug.Log("[GameSceneBuilder] Building game UI...");
            BuildUI();
            Debug.Log("[GameSceneBuilder] Done.");
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
            var canvasGo = new GameObject("GameCanvas", typeof(RectTransform));
            canvasGo.layer = layer;
            canvasGo.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            // Back button
            var bb = NewChild("BackToMenuBtn", canvasGo.transform, layer);
            var br = bb.GetComponent<RectTransform>();
            br.anchorMin = br.anchorMax = br.pivot = new Vector2(1, 1);
            br.sizeDelta = new Vector2(140, 44);
            br.anchoredPosition = new Vector2(-20, -20);
            bb.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);
            var bbtn = bb.AddComponent<Button>();
            var bl = NewChild("Label", bb.transform, layer);
            FullStretch(bl);
            var bt = bl.AddComponent<Text>();
            bt.text = "← 菜单";
            bt.fontSize = 20;
            bt.color = Color.white;
            bt.alignment = TextAnchor.MiddleCenter;
            bt.font = GetFont();

            // GameOver overlay
            var overlay = NewChild("GameOverOverlay", canvasGo.transform, layer);
            FullStretch(overlay);
            overlay.AddComponent<Image>().color = new Color(0, 0, 0, 0.9f);

            var goText = NewChild("GameOverText", overlay.transform, layer);
            var grt = goText.GetComponent<RectTransform>();
            grt.anchorMin = grt.anchorMax = new Vector2(0.5f, 0.55f);
            grt.sizeDelta = new Vector2(500, 80);
            var gt = goText.AddComponent<Text>();
            gt.text = "游戏结束";
            gt.fontSize = 56;
            gt.color = new Color(0.9f, 0.2f, 0.2f);
            gt.alignment = TextAnchor.MiddleCenter;
            gt.font = GetFont();

            // Buttons in overlay
            var rb = MakeOverlayBtn("RestartBtn", "重新开始", new Vector2(0, -30), overlay.transform, layer);
            var mb = MakeOverlayBtn("GOMainMenuBtn", "返回主菜单", new Vector2(0, -120), overlay.transform, layer);

            // Wire GameOverUI
            var ui = overlay.AddComponent<GameOverUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("_gameOverPanel").objectReferenceValue = overlay;
            so.FindProperty("_victoryPanel").objectReferenceValue = overlay;
            so.FindProperty("_restartButton").objectReferenceValue = rb?.GetComponent<Button>();
            so.FindProperty("_mainMenuButton").objectReferenceValue = mb?.GetComponent<Button>();
            var sc = Object.FindObjectOfType<SceneController>();
            so.FindProperty("_sceneController").objectReferenceValue = sc;
            so.ApplyModifiedProperties();
            if (sc != null) bbtn.onClick.AddListener(() => sc.LoadMainMenu());

            overlay.SetActive(false);
        }

        static GameObject NewChild(string n, Transform p, int layer)
        {
            var go = new GameObject(n, typeof(RectTransform));
            go.layer = layer;
            go.transform.SetParent(p, false);
            return go;
        }

        static GameObject MakeOverlayBtn(string name, string label, Vector2 pos, Transform parent, int layer)
        {
            var go = NewChild(name, parent, layer);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(280, 64);
            rt.anchoredPosition = pos;
            go.AddComponent<Image>().color = new Color(0.2f, 0.5f, 0.2f);
            go.AddComponent<Button>();

            var l = NewChild("Label", go.transform, layer);
            FullStretch(l);
            var t = l.AddComponent<Text>();
            t.text = label;
            t.fontSize = 26;
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
