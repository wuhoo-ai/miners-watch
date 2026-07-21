using System.Collections;
using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Simple camera shake via position offset. Triggers on damage, night transition, etc.
    /// Static Trigger() for convenient one-shot shakes from anywhere.
    /// </summary>
    public class ScreenShake : MonoBehaviour
    {
        [Header("Defaults")]
        [SerializeField] private float _defaultIntensity = 0.15f;
        [SerializeField] private float _defaultDuration = 0.3f;

        private static ScreenShake _instance;
        private Vector3 _originalPos;
        private Coroutine _activeShake;

        public static ScreenShake Instance => _instance;

        private void Awake()
        {
            _instance = this;
            _originalPos = transform.localPosition;
        }

        /// <summary>One-shot shake from anywhere.</summary>
        public static void Trigger(float intensity = -1f, float duration = -1f)
        {
            if (_instance == null) return;
            _instance.Shake(intensity < 0 ? _instance._defaultIntensity : intensity,
                           duration < 0 ? _instance._defaultDuration : duration);
        }

        public void Shake(float intensity, float duration)
        {
            if (_activeShake != null) StopCoroutine(_activeShake);
            _activeShake = StartCoroutine(ShakeRoutine(intensity, duration));
        }

        private IEnumerator ShakeRoutine(float intensity, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float decay = 1f - elapsed / duration;
                float x = Random.Range(-1f, 1f) * intensity * decay;
                float y = Random.Range(-1f, 1f) * intensity * decay;
                transform.localPosition = _originalPos + new Vector3(x, y, 0);
                yield return null;
            }
            transform.localPosition = _originalPos;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
