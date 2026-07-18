using UnityEngine;
using UnityEngine.UI;

namespace MinersWatch
{
    /// <summary>Cave exit button: returns to the surface scene via the persistent SceneController.</summary>
    public class ReturnToSurface : MonoBehaviour
    {
        private void Awake()
        {
            var btn = GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            var scenes = GameRoot.Get<SceneController>() ?? FindObjectOfType<SceneController>();
            if (scenes != null) scenes.LoadSurface();
            else Debug.LogWarning("[ReturnToSurface] no SceneController found");
        }
    }
}
