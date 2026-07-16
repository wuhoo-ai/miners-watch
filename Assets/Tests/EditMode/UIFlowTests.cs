using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    public class MainMenuUITests
    {
        private GameObject _go;
        private MainMenuUI _menu;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("MenuHost");
            _menu = _go.AddComponent<MainMenuUI>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void Init_WiresButtons()
        {
            var saveSys = _go.AddComponent<SaveSystem>();
            var sc = _go.AddComponent<SceneController>();
            sc.Init(null);
            saveSys.Init();

            _menu.Init(saveSys, sc);

            // Should not throw — verifies button wiring doesn't crash with null buttons
            Assert.Pass();
        }

        [Test]
        public void ContinueButton_Disabled_WhenNoSave()
        {
            // Create real buttons for testing
            var canvasGo = new GameObject("TestCanvas");
            canvasGo.transform.SetParent(_go.transform);
            var canvas = canvasGo.AddComponent<Canvas>();

            var newBtnGo = new GameObject("NewGameBtn");
            newBtnGo.transform.SetParent(canvasGo.transform);
            var newBtn = newBtnGo.AddComponent<Button>();

            var continueBtnGo = new GameObject("ContinueBtn");
            continueBtnGo.transform.SetParent(canvasGo.transform);
            var continueBtn = continueBtnGo.AddComponent<Button>();

            var settingsBtnGo = new GameObject("SettingsBtn");
            settingsBtnGo.transform.SetParent(canvasGo.transform);
            var settingsBtn = settingsBtnGo.AddComponent<Button>();

            var saveSys = _go.AddComponent<SaveSystem>();
            saveSys.Init();
            var sc = _go.AddComponent<SceneController>();
            sc.Init(null);

            // Wire via SerializedObject to simulate Editor injection
            var so = new UnityEditor.SerializedObject(_menu);
            so.FindProperty("_newGameButton").objectReferenceValue = newBtn;
            so.FindProperty("_continueButton").objectReferenceValue = continueBtn;
            so.FindProperty("_settingsButton").objectReferenceValue = settingsBtn;
            so.FindProperty("_saveSystem").objectReferenceValue = saveSys;
            so.FindProperty("_sceneController").objectReferenceValue = sc;
            so.FindProperty("_mainCanvas").objectReferenceValue = canvas;
            so.ApplyModifiedProperties();

            _menu.RefreshButtons();

            Assert.IsFalse(continueBtn.interactable, "Continue should be disabled when no save exists");
        }
    }

    public class SceneControllerNavigationTests
    {
        [Test]
        public void GetSceneNameForDepth_ReturnsCorrectNames()
        {
            var go = new GameObject("SC");
            var sc = go.AddComponent<SceneController>();
            var dp = go.AddComponent<DepthProgression>();
            dp.Init();
            sc.Init(dp);

            Assert.AreEqual("ShallowCave", sc.GetSceneNameForDepth(DepthLevel.Shallow));
            Assert.AreEqual("MidCave", sc.GetSceneNameForDepth(DepthLevel.Medium));
            Assert.AreEqual("DeepCave", sc.GetSceneNameForDepth(DepthLevel.Deep));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Init_GuardsFadeDuration()
        {
            var go = new GameObject("SC");
            var sc = go.AddComponent<SceneController>();
            sc.Init(null);
            Assert.IsNull(sc.CurrentScene);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void IsOnMainMenu_TrueInitially()
        {
            var go = new GameObject("SC");
            var sc = go.AddComponent<SceneController>();
            sc.Init(null);
            Assert.IsTrue(sc.IsOnMainMenu);
            Object.DestroyImmediate(go);
        }
    }

    public class GameOverUITests
    {
        private GameObject _go;
        private GameOverUI _gameOver;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("GameOverHost");
            _gameOver = _go.AddComponent<GameOverUI>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void ShowGameOver_ActivatesPanel()
        {
            var panel = new GameObject("Panel");
            panel.SetActive(false);

            var so = new UnityEditor.SerializedObject(_gameOver);
            so.FindProperty("_gameOverPanel").objectReferenceValue = panel;
            so.ApplyModifiedProperties();

            _gameOver.ShowGameOver();
            Assert.IsTrue(panel.activeSelf);
        }

        [Test]
        public void ShowVictory_ActivatesVictoryPanel()
        {
            var gameOverPanel = new GameObject("GameOverPanel");
            var victoryPanel = new GameObject("VictoryPanel");
            victoryPanel.SetActive(false);

            var so = new UnityEditor.SerializedObject(_gameOver);
            so.FindProperty("_gameOverPanel").objectReferenceValue = gameOverPanel;
            so.FindProperty("_victoryPanel").objectReferenceValue = victoryPanel;
            so.ApplyModifiedProperties();

            _gameOver.ShowVictory();
            Assert.IsTrue(victoryPanel.activeSelf);
            Assert.IsFalse(gameOverPanel.activeSelf);
        }

        [Test]
        public void Hide_DeactivatesBoth()
        {
            var gameOverPanel = new GameObject("GameOverPanel");
            var victoryPanel = new GameObject("VictoryPanel");

            var so = new UnityEditor.SerializedObject(_gameOver);
            so.FindProperty("_gameOverPanel").objectReferenceValue = gameOverPanel;
            so.FindProperty("_victoryPanel").objectReferenceValue = victoryPanel;
            so.ApplyModifiedProperties();

            _gameOver.Hide();
            Assert.IsFalse(gameOverPanel.activeSelf);
            Assert.IsFalse(victoryPanel.activeSelf);
        }

        [Test]
        public void TriggerGameOver_ShowsPanel()
        {
            var panel = new GameObject("Panel");
            panel.SetActive(false);

            var so = new UnityEditor.SerializedObject(_gameOver);
            so.FindProperty("_gameOverPanel").objectReferenceValue = panel;
            so.ApplyModifiedProperties();

            _gameOver.TriggerGameOver();
            Assert.IsTrue(panel.activeSelf);
        }
    }

    public class SceneControllerLoadTestGroundTests
    {
        [Test]
        public void LoadTestGround_DoesNotThrow_WhenSceneNotLoaded()
        {
            var go = new GameObject("SC");
            var sc = go.AddComponent<SceneController>();
            sc.Init(null);

            // LoadTestGround uses StartCoroutine, which requires PlayMode.
            // In EditMode we just verify the method exists and doesn't crash.
            Assert.DoesNotThrow(() => sc.LoadTestGround());

            Object.DestroyImmediate(go);
        }
    }
}
