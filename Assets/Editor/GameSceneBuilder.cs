using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MinersWatch.Editor
{
    /// <summary>
    /// Auto-builds in-game UI (back button, game over panel) for TestGround scene during CI builds.
    /// Safe to delete once TestGround scene is set up manually in Unity Editor.
    /// </summary>
    public class GameSceneBuilder : IProcessSceneWithReport
    {
        public int callbackOrder => -90;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (scene.name != "TestGround") return;
            if (GameObject.Find("GameCanvas") != null) return;

            BuildGameUI();
        }

        private static void BuildGameUI()
        {
            // Ensure EventSystem
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Canvas
            var canvasGo = new GameObject("GameCanvas");
            canvasGo.layer = LayerMask.NameToLayer("UI");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // --- Back button (top-right) ---
            var backBtnGo = new GameObject("BackToMenuBtn");
            backBtnGo.layer = LayerMask.NameToLayer("UI");
            backBtnGo.transform.SetParent(canvasGo.transform, false);
            var backRt = backBtnGo.AddComponent<RectTransform>();
            backRt.anchorMin = new Vector2(1, 1);
            backRt.anchorMax = new Vector2(1, 1);
            backRt.pivot = new Vector2(1, 1);
            backRt.sizeDelta = new Vector2(120, 36);
            backRt.anchoredPosition = new Vector2(-20, -20);

            var backImg = backBtnGo.AddComponent<Image>();
            backImg.color = new Color(0f, 0f, 0f, 0.5f);
            var backBtn = backBtnGo.AddComponent<Button>();

            var backLabel = new GameObject("Label");
            backLabel.layer = LayerMask.NameToLayer("UI");
            backLabel.transform.SetParent(backBtnGo.transform, false);
            var backLabelText = backLabel.AddComponent<Text>();
            backLabelText.text = "← 菜单";
            backLabelText.font = GetFont();
            backLabelText.fontSize = 18;
            backLabelText.color = Color.white;
            backLabelText.alignment = TextAnchor.MiddleCenter;
            var backLabelRt = backLabel.GetComponent<RectTransform>();
            backLabelRt.anchorMin = Vector2.zero;
            backLabelRt.anchorMax = Vector2.one;
            backLabelRt.offsetMin = Vector2.zero;
            backLabelRt.offsetMax = Vector2.zero;

            // --- GameOver panel (hidden initially) ---
            var overlay = new GameObject("GameOverOverlay");
            overlay.layer = LayerMask.NameToLayer("UI");
            overlay.transform.SetParent(canvasGo.transform, false);
            var overlayRt = overlay.AddComponent<RectTransform>();
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.offsetMin = Vector2.zero;
            overlayRt.offsetMax = Vector2.zero;
            var overlayImg = overlay.AddComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.85f);

            // GameOver text
            var goText = new GameObject("GameOverText");
            goText.layer = LayerMask.NameToLayer("UI");
            goText.transform.SetParent(overlay.transform, false);
            var goTextComp = goText.AddComponent<Text>();
            goTextComp.text = "游戏结束";
            goTextComp.font = GetFont();
            goTextComp.fontSize = 56;
            goTextComp.color = new Color(0.9f, 0.2f, 0.2f, 1f);
            goTextComp.alignment = TextAnchor.MiddleCenter;
            var goTextRt = goText.GetComponent<RectTransform>();
            goTextRt.anchorMin = new Vector2(0.5f, 0.55f);
            goTextRt.anchorMax = new Vector2(0.5f, 0.55f);
            goTextRt.sizeDelta = new Vector2(500, 80);
            goTextRt.anchoredPosition = Vector2.zero;

            // Restart button
            var restartBtnGo = CreateButton("RestartBtn", "重新开始", overlay.transform,
                new Vector2(0, -40), new Color(0.2f, 0.5f, 0.2f));
            var restartBtn = restartBtnGo.GetComponent<Button>();

            // MainMenu button
            var menuBtnGo = CreateButton("GOMainMenuBtn", "返回主菜单", overlay.transform,
                new Vector2(0, -130), new Color(0.3f, 0.3f, 0.3f));
            var menuBtn = menuBtnGo.GetComponent<Button>();

            // Attach GameOverUI
            var gameOverUI = overlay.AddComponent<GameOverUI>();
            var so = new SerializedObject(gameOverUI);
            so.FindProperty("_gameOverPanel").objectReferenceValue = overlay;
            so.FindProperty("_victoryPanel").objectReferenceValue = overlay; // reuse for now
            so.FindProperty("_restartButton").objectReferenceValue = restartBtn;
            so.FindProperty("_mainMenuButton").objectReferenceValue = menuBtn;
            so.ApplyModifiedProperties();

            // Also wire GameOverUI to SceneController
            var sc = Object.FindObjectOfType<SceneController>();
            if (sc != null)
            {
                so = new SerializedObject(gameOverUI);
                so.FindProperty("_sceneController").objectReferenceValue = sc;
                so.ApplyModifiedProperties();
            }

            // Wire back button
            if (sc != null)
            {
                backBtn.onClick.AddListener(() => sc.LoadMainMenu());
            }

            // Hide overlay initially
            overlay.SetActive(false);

            Debug.Log("[GameSceneBuilder] In-game UI built successfully.");
        }

        private static GameObject CreateButton(string name, string label, Transform parent, Vector2 pos, Color bgColor)
        {
            var go = new GameObject(name);
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(240, 56);
            rt.anchoredPosition = pos;

            var img = go.AddComponent<Image>();
            img.color = bgColor;
            go.AddComponent<Button>();

            var lbl = new GameObject("Label");
            lbl.layer = LayerMask.NameToLayer("UI");
            lbl.transform.SetParent(go.transform, false);
            var txt = lbl.AddComponent<Text>();
            txt.text = label;
            txt.font = GetFont();
            txt.fontSize = 24;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            var lblRt = lbl.GetComponent<RectTransform>();
            lblRt.anchorMin = Vector2.zero;
            lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = Vector2.zero;
            lblRt.offsetMax = Vector2.zero;

            return go;
        }

        private static Font GetFont()
        {
            return Resources.GetBuiltinResource<Font>("Arial.ttf")
                ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
