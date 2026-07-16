using UnityEngine;
using UnityEngine.UI;

namespace MinersWatch
{
    /// <summary>Game Over / Victory overlay. Wires Restart and MainMenu buttons to SceneController.</summary>
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private SceneController _sceneController;

        private void Awake()
        {
            if (_sceneController == null)
                _sceneController = FindObjectOfType<SceneController>();
            WireButtons();
        }

        private void WireButtons()
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveAllListeners();
                _restartButton.onClick.AddListener(OnRestart);
            }
            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.RemoveAllListeners();
                _mainMenuButton.onClick.AddListener(OnMainMenu);
            }
        }

        public void ShowGameOver()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_victoryPanel != null) _victoryPanel.SetActive(false);
        }

        public void ShowVictory()
        {
            if (_victoryPanel != null) _victoryPanel.SetActive(true);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        public void Hide()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_victoryPanel != null) _victoryPanel.SetActive(false);
        }

        private void OnRestart()
        {
            Debug.Log("[GameOver] Restart");
            Hide();
            if (_sceneController != null)
            {
                _sceneController.LoadMainMenu();
                // TODO: proper restart = reload TestGround scene
            }
        }

        private void OnMainMenu()
        {
            Debug.Log("[GameOver] Return to MainMenu");
            Hide();
            if (_sceneController != null)
            {
                _sceneController.LoadMainMenu();
            }
        }

        /// <summary>Auto-show game over when called by external systems (e.g. BaseCore destroyed).</summary>
        public void TriggerGameOver()
        {
            ShowGameOver();
        }
    }
}
