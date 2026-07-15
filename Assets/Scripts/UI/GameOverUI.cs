using UnityEngine;
using UnityEngine.UI;

namespace MinersWatch
{
    /// <summary>Game Over / Victory overlay. Activated by external triggers.</summary>
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _mainMenuButton;

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
    }
}
