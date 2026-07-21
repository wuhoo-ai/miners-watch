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
        [SerializeField] private Canvas _settingsCanvas;

        private bool _wired;

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
            if (_wired) return;
            _wired = true;

            if (_newGameButton != null)
            {
                _newGameButton.onClick.RemoveAllListeners();
                _newGameButton.onClick.AddListener(OnNewGame);
            }
            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveAllListeners();
                _continueButton.onClick.AddListener(OnContinue);
            }
            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveAllListeners();
                _settingsButton.onClick.AddListener(OnSettings);
            }
        }

        private void OnNewGame()
        {
            Debug.Log("[MainMenu] New Game → Resetting systems → Loading Surface");
            GameRoot.ResetAll();
            var sc = _sceneController != null ? _sceneController : GameRoot.Get<SceneController>();
            if (sc != null)
            {
                sc.LoadSurface();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Surface", UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }
            if (_mainCanvas != null)
                _mainCanvas.enabled = false;
        }

        private void OnContinue()
        {
            Debug.Log("[MainMenu] Continue — loading save and resuming");

            var ss = _saveSystem ?? GameRoot.Get<SaveSystem>();
            if (ss == null || !ss.HasSave())
            {
                Debug.LogWarning("[MainMenu] Continue: no save found, starting new game");
                OnNewGame();
                return;
            }

            var data = ss.Load();
            if (data == null)
            {
                Debug.LogWarning("[MainMenu] Continue: save load failed, starting new game");
                OnNewGame();
                return;
            }

            // Ensure GameRoot exists before restoring
            GameRoot.EnsureExists();

            // Restore state from save (before loading scene so scene sees correct state)
            GameRoot.ResetAll();
            GameRoot.RestoreFromSave(data);

            var sc = _sceneController ?? GameRoot.Get<SceneController>();
            if (sc != null)
            {
                sc.LoadSurface();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Surface", UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }

            if (_mainCanvas != null)
                _mainCanvas.enabled = false;
        }

        private void OnSettings()
        {
            Debug.Log("[MainMenu] Settings");
            if (_settingsCanvas != null)
            {
                _settingsCanvas.enabled = !_settingsCanvas.enabled;
            }
        }

        public void RefreshButtons()
        {
            if (_continueButton != null)
            {
                bool hasSave = GameRoot.HasSave();
                _continueButton.interactable = hasSave;
                Debug.Log($"[MainMenu] Continue button: {(hasSave ? "enabled" : "disabled (no save)")}");
            }
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
