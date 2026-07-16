using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MinersWatch
{
    /// <summary>
    /// Scene controller: loads/unloads scenes, handles fade transitions.
    /// Used by MainMenuUI and GameOverUI for navigation.
    /// </summary>
    public class SceneController : MonoBehaviour
    {
        [Header("Scene Names")]
        [SerializeField] private string _shallowScene = "ShallowCave";
        [SerializeField] private string _midScene = "MidCave";
        [SerializeField] private string _deepScene = "DeepCave";
        [SerializeField] private string _surfaceScene = "Surface";
        [SerializeField] private string _mainMenuScene = "MainMenu";
        [SerializeField] private string _testGroundScene = "TestGround";

        [Header("Transition")]
        [SerializeField] private float _fadeDuration = 2f;
        [SerializeField] private CanvasGroup _fadeCanvas;

        [Header("References")]
        [SerializeField] private DepthProgression _progression;

        private string _currentScene;

        public string CurrentScene => _currentScene;
        public bool IsOnMainMenu => string.IsNullOrEmpty(_currentScene) || _currentScene == _mainMenuScene;

        public void Init(DepthProgression progression)
        {
            _progression = progression;
            if (_fadeDuration <= 0f) _fadeDuration = 2f;
        }

        private void Awake()
        {
            if (_progression == null)
                _progression = GetComponent<DepthProgression>() ?? GetComponentInParent<DepthProgression>();
        }

        /// <summary>Load the TestGround scene for demo gameplay.</summary>
        public void LoadTestGround()
        {
            if (SceneManager.GetSceneByName(_testGroundScene).isLoaded) return;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Debug.Log("[SceneController] EditMode: skipping LoadTestGround (coroutines require PlayMode)");
                return;
            }
#endif
            LoadScene(_testGroundScene);
        }

        /// <summary>Load the cave scene for a depth level.</summary>
        public void LoadCave(DepthLevel depth)
        {
            string scene = depth switch
            {
                DepthLevel.Shallow => _shallowScene,
                DepthLevel.Medium => _midScene,
                DepthLevel.Deep => _deepScene,
                _ => _shallowScene,
            };
            LoadScene(scene);
        }

        /// <summary>Load the surface (shop/build/defense) scene.</summary>
        public void LoadSurface() => LoadScene(_surfaceScene);

        /// <summary>Return to main menu. Unloads current scene, shows menu UI.</summary>
        public void LoadMainMenu()
        {
            // If we're on the game scene, just unload it (MainMenu is the base scene)
            if (!string.IsNullOrEmpty(_currentScene) && _currentScene != _mainMenuScene)
            {
                StartCoroutine(UnloadAndShowMenu());
                return;
            }
            // If we're already on MainMenu, just show it
            var menuUI = FindObjectOfType<MainMenuUI>();
            if (menuUI != null) menuUI.Show();
        }

        /// <summary>Get scene name for a depth level (testable).</summary>
        public string GetSceneNameForDepth(DepthLevel depth) => depth switch
        {
            DepthLevel.Shallow => _shallowScene,
            DepthLevel.Medium => _midScene,
            DepthLevel.Deep => _deepScene,
            _ => _shallowScene,
        };

        private void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            // Fade out
            if (_fadeCanvas != null)
            {
                float t = 0f;
                while (t < _fadeDuration)
                {
                    t += Time.deltaTime;
                    _fadeCanvas.alpha = Mathf.Clamp01(t / _fadeDuration);
                    yield return null;
                }
            }

            // Unload current + load new
            if (!string.IsNullOrEmpty(_currentScene))
                yield return SceneManager.UnloadSceneAsync(_currentScene);

            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            _currentScene = sceneName;

            // Fade in
            if (_fadeCanvas != null)
            {
                float t = _fadeDuration;
                while (t > 0f)
                {
                    t -= Time.deltaTime;
                    _fadeCanvas.alpha = Mathf.Clamp01(t / _fadeDuration);
                    yield return null;
                }
            }

            // Update progression
            if (_progression != null)
            {
                if (sceneName == _shallowScene) _progression.SetDepth(DepthLevel.Shallow);
                else if (sceneName == _midScene) _progression.SetDepth(DepthLevel.Medium);
                else if (sceneName == _deepScene) _progression.SetDepth(DepthLevel.Deep);
            }
        }

        private IEnumerator UnloadAndShowMenu()
        {
            // Fade out
            if (_fadeCanvas != null)
            {
                float t = 0f;
                while (t < _fadeDuration)
                {
                    t += Time.deltaTime;
                    _fadeCanvas.alpha = Mathf.Clamp01(t / _fadeDuration);
                    yield return null;
                }
            }

            // Unload current scene
            if (!string.IsNullOrEmpty(_currentScene))
                yield return SceneManager.UnloadSceneAsync(_currentScene);
            _currentScene = null;

            // Show main menu UI
            var menuUI = FindObjectOfType<MainMenuUI>();
            if (menuUI != null) menuUI.Show();

            // Fade in
            if (_fadeCanvas != null)
            {
                float t = _fadeDuration;
                while (t > 0f)
                {
                    t -= Time.deltaTime;
                    _fadeCanvas.alpha = Mathf.Clamp01(t / _fadeDuration);
                    yield return null;
                }
            }
        }
    }
}
