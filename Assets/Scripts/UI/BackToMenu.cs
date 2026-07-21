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

            GameRoot.AutoSave(); // save progress before leaving game

            // Unload whichever game scene this button lives in (additively loaded on top of MainMenu)
            var ownScene = gameObject.scene;
            if (ownScene.isLoaded && ownScene.name != "MainMenu")
            {
                SceneManager.UnloadSceneAsync(ownScene.name);
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
