using UnityEngine;
using UnityEngine.EventSystems;

namespace MinersWatch
{
    /// <summary>Static bridge feeding on-screen touch controls into gameplay input reads.</summary>
    public static class TouchInput
    {
        public static float Horizontal;
        private static bool _jump, _mine;

        public static void PressJump() => _jump = true;
        public static void PressMine() => _mine = true;
        public static bool ConsumeJump() { var v = _jump; _jump = false; return v; }
        public static bool ConsumeMine() { var v = _mine; _mine = false; return v; }
        public static void Reset() { Horizontal = 0f; _jump = _mine = false; }
    }

    /// <summary>Root of the touch UI: hides itself on non-touch platforms.</summary>
    public class TouchControlsRoot : MonoBehaviour
    {
        private void Awake()
        {
            if (!Application.isMobilePlatform && !Input.touchSupported)
                gameObject.SetActive(false);
        }

        private void OnDisable() => TouchInput.Reset();
    }

    /// <summary>Horizontal virtual joystick: drag the knob, feeds TouchInput.Horizontal (-1..1).</summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _knob;
        [SerializeField] private float _radius = 90f;

        private RectTransform _rt;

        private void Awake() => _rt = GetComponent<RectTransform>();

        public void OnPointerDown(PointerEventData e) => OnDrag(e);

        public void OnDrag(PointerEventData e)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, e.position, e.pressEventCamera, out var local);
            float x = Mathf.Clamp(local.x, -_radius, _radius);
            if (_knob != null) _knob.anchoredPosition = new Vector2(x, 0);
            TouchInput.Horizontal = x / _radius;
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (_knob != null) _knob.anchoredPosition = Vector2.zero;
            TouchInput.Horizontal = 0f;
        }
    }

    /// <summary>One-shot action button (jump / mine) for the touch UI.</summary>
    public class TouchActionButton : MonoBehaviour, IPointerDownHandler
    {
        public enum Kind { Jump, Mine }
        [SerializeField] private Kind _kind;

        public void SetKind(Kind kind) => _kind = kind;

        public void OnPointerDown(PointerEventData e)
        {
            if (_kind == Kind.Jump) TouchInput.PressJump();
            else TouchInput.PressMine();
        }
    }
}
