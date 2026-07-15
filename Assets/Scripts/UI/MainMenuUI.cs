using UnityEngine;
using UnityEngine.UI;

namespace MinersWatch
{
    /// <summary>
    /// Main menu UI: New Game / Continue / Settings.
    /// Minimal — delegates to scene loading (T010).
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _settingsButton;

        [Header("References")]
        [SerializeField] private SaveSystem _saveSystem;

        public void Init(SaveSystem saveSystem)
        {
            _saveSystem = saveSystem;
            WireButtons();
            RefreshButtons();
        }

        private void Awake()
        {
            if (_saveSystem == null)
                _saveSystem = GetComponent<SaveSystem>() ?? GetComponentInParent<SaveSystem>();
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
            // Scene loading handled by SceneController (T010)
            Debug.Log("[MainMenu] New Game");
        }

        private void OnContinue()
        {
            Debug.Log("[MainMenu] Continue — load save");
        }

        private void OnSettings()
        {
            Debug.Log("[MainMenu] Settings");
        }

        public void RefreshButtons()
        {
            if (_continueButton != null)
                _continueButton.interactable = _saveSystem != null && _saveSystem.HasSave();
        }
    }
}
