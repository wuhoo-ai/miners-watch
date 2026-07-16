using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MinersWatch.Editor
{
    /// <summary>
    /// Auto-builds in-game UI (back button, game over panel) for TestGround scene.
    /// Safe to delete once TestGround scene is set up manually in Unity Editor.
    /// </summary>
    public class GameSceneBuilder : IPreprocessBuildWithReport
    {
        public int callbackOrder => -90;

        public void OnPreprocessBuild(BuildReport report)
        {
            var scene = SceneManager.GetSceneByName("TestGround");
            if (!scene.isLoaded)
                scene = EditorSceneManager.OpenScene("Assets/Scenes/TestGround.unity", OpenSceneMode.Additive);
            if (!scene.IsValid() || GameObject.Find("GameCanvas") != null) return;

            BuildGameUI();
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[GameSceneBuilder] Scene saved with game UI.");
        }

        private static void BuildGameUI()
        {
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer < 0) uiLayer = 5;

            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            var canvasGo = new GameObject("GameCanvas");
            canvasGo.layer = uiLayer;
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Back button top-right
            var backBtn = new GameObject("BackToMenuBtn", typeof(RectTransform));
            backBtn.layer = uiLayer;
            backBtn.transform.SetParent(canvasGo.transform, false);
            var br = backBtn.GetComponent<RectTransform>();
            br.anchorMin = br.anchorMax = br.pivot = new Vector2(1, 1);
            br.sizeDelta = new Vector2(140, 44);
            br.anchoredPosition = new Vector2(-20, -20);
            var bImg = backBtn.AddComponent<Image>();
            bImg.color = new Color(0f, 0f, 0f, 0.6f);
            var bb = backBtn.AddComponent<Button>();
            backBtn.AddComponent<Outline>().effectColor = Color.white;

            var bl = new GameObject("Label", typeof(RectTransform));
            bl.layer = uiLayer;
            bl.transform.SetParent(backBtn.transform, false);
            Stretch(bl.GetComponent<RectTransform>());
            var bt = bl.AddComponent<Text>();
            bt.text = "← 菜单";
            bt.fontSize = 20;
            bt.color = Color.white;
            bt.alignment = TextAnchor.MiddleCenter;
            bt.font = GetFont();

            // GameOver overlay (hidden)
            var overlay = new GameObject("GameOverOverlay", typeof(RectTransform));
            overlay.layer = uiLayer;
            overlay.transform.SetParent(canvasGo.transform, false);
            Stretch(overlay.GetComponent<RectTransform>());
            overlay.AddComponent<Image>().color = new Color(0, 0, 0, 0.88f);

            // "游戏结束" text
            var goText = new GameObject("GameOverText", typeof(RectTransform));
            goText.layer = uiLayer;
            goText.transform.SetParent(overlay.transform, false);
            var gort = goText.GetComponent<RectTransform>();
            gort.anchorMin = gort.anchorMax = new Vector2(0.5f, 0.6f);
            gort.sizeDelta = new Vector2(500, 80);
            var got = goText.AddComponent<Text>();
            got.text = "游戏结束";
            got.fontSize = 56;
            got.color = new Color(0.9f, 0.2f, 0.2f);
            got.alignment = TextAnchor.MiddleCenter;
            got.font = GetFont();

            // Restart + MainMenu buttons
            var restartBtn = MakeBtn("RestartBtn", "重新开始", new Vector2(0, -20),
                new Color(0.2f, 0.5f, 0.2f), overlay.transform, uiLayer);
            var menuBtn = MakeBtn("GOMainMenuBtn", "返回主菜单", new Vector2(0, -110),
                new Color(0.35f, 0.35f, 0.4f), overlay.transform, uiLayer);

            // Wire GameOverUI
            var gameOverUI = overlay.AddComponent<GameOverUI>();
            var so = new SerializedObject(gameOverUI);
            so.FindProperty("_gameOverPanel").objectReferenceValue = overlay;
            so.FindProperty("_victoryPanel").objectReferenceValue = overlay;
            so.FindProperty("_restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
            so.FindProperty("_mainMenuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
            var sc = Object.FindObjectOfType<SceneController>();
            so.FindProperty("_sceneController").objectReferenceValue = sc;
            so.ApplyModifiedProperties();

            // Wire back button
            if (sc != null)
                bb.onClick.AddListener(() => sc.LoadMainMenu());

            overlay.SetActive(false);
            Debug.Log("[GameSceneBuilder] Done.");
        }

        private static GameObject MakeBtn(string name, string label, Vector2 pos, Color bg,
            Transform parent, int layer)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = layer;
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(280, 64);
            rt.anchoredPosition = pos;

            go.AddComponent<Image>().color = bg;
            go.AddComponent<Button>();
            go.AddComponent<Outline>().effectColor = Color.white;

            var l = new GameObject("Label", typeof(RectTransform));
            l.layer = layer;
            l.transform.SetParent(go.transform, false);
            Stretch(l.GetComponent<RectTransform>());
            var t = l.AddComponent<Text>();
            t.text = label;
            t.fontSize = 26;
            t.fontStyle = FontStyle.Bold;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = GetFont();
            return go;
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
