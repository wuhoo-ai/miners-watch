using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MinersWatch
{
    /// <summary>
    /// Attached to the back button on game scenes.
    /// At runtime, wires up onClick to unload the current scene and show MainMenu.
    /// </summary>
    public class BackToMenu : MonoBehaviour
    {
        private void Awake()
        {
            var btn = GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(OnBackClicked);
            }
        }

        private void OnBackClicked()
        {
            Debug.Log("[BackToMenu] Returning to MainMenu");

            // Unload the game scene (additively loaded on top of MainMenu)
            var tgScene = SceneManager.GetSceneByName("TestGround");
            if (tgScene.isLoaded)
            {
                SceneManager.UnloadSceneAsync("TestGround");
            }

            // Show the MainCanvas that was hidden when entering the game
            var mc = GameObject.Find("MainCanvas");
            if (mc != null)
            {
                var canvas = mc.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.enabled = true;
                    Debug.Log("[BackToMenu] MainCanvas shown");
                }
            }
            else
            {
                Debug.LogWarning("[BackToMenu] MainCanvas not found — falling back to scene load");
                SceneManager.LoadScene("MainMenu");
            }
        }
    }
}
