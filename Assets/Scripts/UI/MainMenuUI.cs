using UnityEngine;
using UnityEngine.UI;

namespace MinersWatch
{
    /// <summary>
    /// Main menu UI: New Game / Continue / Settings.
    /// Auto-wires buttons from scene; delegates navigation to SceneController.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _settingsButton;

        [Header("References")]
        [SerializeField] private SaveSystem _saveSystem;
        [SerializeField] private SceneController _sceneController;
        [SerializeField] private Canvas _mainCanvas;

        public void Init(SaveSystem saveSystem, SceneController sceneController)
        {
            _saveSystem = saveSystem;
            _sceneController = sceneController;
            WireButtons();
            RefreshButtons();
        }

        private void Awake()
        {
            if (_saveSystem == null)
                _saveSystem = GetComponent<SaveSystem>() ?? FindObjectOfType<SaveSystem>();
            if (_sceneController == null)
                _sceneController = FindObjectOfType<SceneController>();
            WireButtons();
            RefreshButtons();
        }

        private void WireButtons()
        {
            if (_newGameButton != null) _newGameButton.onClick.AddListener(OnNewGame);
            if (_continueButton != null) _continueButton.onClick.AddListener(OnContinue);
            if (_settingsButton != null) _settingsButton.onClick.AddListener(OnSettings);
        }

        private void OnNewGame()
        {
            Debug.Log("[MainMenu] New Game → Loading Surface");
            var sc = _sceneController != null ? _sceneController : GameRoot.Get<SceneController>();
            if (sc != null)
            {
                sc.LoadSurface();
            }
            else
            {
                // Fallback: direct scene load
                UnityEngine.SceneManagement.SceneManager.LoadScene("Surface", UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }
            // Hide the menu canvas
            if (_mainCanvas != null)
                _mainCanvas.enabled = false;
        }

        private void OnContinue()
        {
            Debug.Log("[MainMenu] Continue — load save and resume");
            if (_saveSystem != null && _saveSystem.HasSave())
            {
                // TODO: Load save and resume game
                OnNewGame(); // fallback for demo
            }
        }

        private void OnSettings()
        {
            Debug.Log("[MainMenu] Settings — not implemented in demo");
        }

        public void RefreshButtons()
        {
            if (_continueButton != null)
                _continueButton.interactable = _saveSystem != null && _saveSystem.HasSave();
        }

        /// <summary>Called when returning from game scene to re-show the menu.</summary>
        public void Show()
        {
            if (_mainCanvas != null)
                _mainCanvas.enabled = true;
            RefreshButtons();
        }
    }
}
