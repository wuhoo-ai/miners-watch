using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MinersWatch
{
    /// <summary>
    /// Floating damage numbers and gold popups. Pool-based, auto-fades.
    /// Static Show() for convenience — creates a pooled Text above a world position.
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private float _duration = 0.8f;
        [SerializeField] private float _riseSpeed = 40f;

        private Text _text;
        private RectTransform _rt;
        private Canvas _canvas;
        private Camera _cam;
        private float _elapsed;
        private static Font _cachedFont;

        public static DamagePopup Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            _cam = Camera.main;
            _text = GetComponentInChildren<Text>();
            _rt = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            gameObject.SetActive(false);
        }

        /// <summary>Show floating text at world position. Color determines text color.</summary>
        public static void Show(Vector3 worldPos, string msg, Color color)
        {
            if (Instance == null) return;
            Instance.ShowInternal(worldPos, msg, color);
        }

        private void ShowInternal(Vector3 worldPos, string msg, Color color)
        {
            if (string.IsNullOrEmpty(msg)) return;
            StopAllCoroutines();
            _text.text = msg;
            _text.color = color;
            _text.fontSize = color == Color.red ? 42 : 36;
            _text.fontStyle = FontStyle.Bold;
            if (_cachedFont == null)
                _cachedFont = Font.CreateDynamicFontFromOSFont("Arial", 14)
                           ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _text.font = _cachedFont;

            gameObject.SetActive(true);
            Vector3 screenPos = _cam != null ? _cam.WorldToScreenPoint(worldPos) : worldPos;
            _rt.position = screenPos + Vector3.up * Random.Range(-10f, 10f);
            _elapsed = 0f;
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            Vector3 start = _rt.position;
            while (_elapsed < _duration)
            {
                _elapsed += Time.deltaTime;
                float t = _elapsed / _duration;
                _rt.position = start + Vector3.up * _riseSpeed * t;
                _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 1f - t);
                yield return null;
            }
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
