using UnityEngine;
using UnityEngine.EventSystems;

namespace MinersWatch
{
    /// <summary>Standalone jump button — no serialized enums (native-crash safe, 2026-07-19 incident).</summary>
    public class JumpButton : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData e) => TouchInput.PressJump();
    }

    /// <summary>Standalone mine button — no serialized enums.</summary>
    public class MineButton : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData e) => TouchInput.PressMine();
    }
}
