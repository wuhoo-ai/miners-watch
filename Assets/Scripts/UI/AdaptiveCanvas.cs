using UnityEngine;
using UnityEngine.UI;

namespace MinersWatch
{
    /// <summary>
    /// Auto-adjusts Canvas Scaler reference resolution based on device aspect ratio.
    /// Wide screens → 1920×1080, narrow phones → 1600×900.
    /// Attach to the root Canvas.
    /// </summary>
    public class AdaptiveCanvas : MonoBehaviour
    {
        [Header("Reference Resolutions")]
        [SerializeField] private Vector2 _wideRef = new Vector2(1920, 1080);
        [SerializeField] private Vector2 _narrowRef = new Vector2(1600, 900);
        [SerializeField] private float _wideThreshold = 1.6f; // aspect > this = wide

        private void Start()
        {
            var scaler = GetComponent<CanvasScaler>();
            if (scaler == null) return;

            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            scaler.referenceResolution = aspect >= _wideThreshold ? _wideRef : _narrowRef;
            scaler.matchWidthOrHeight = 0.5f;

            Debug.Log($"[AdaptiveCanvas] screen={Screen.width}x{Screen.height} aspect={aspect:F2} ref={scaler.referenceResolution}");
        }
    }
}
