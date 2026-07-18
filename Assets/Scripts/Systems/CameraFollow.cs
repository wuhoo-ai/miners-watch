using UnityEngine;

namespace MinersWatch
{
    /// <summary>Smooth horizontal camera follow with X clamping. Caves only (surface camera is static).</summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _smoothTime = 0.15f;
        [SerializeField] private float _minX = -20f;
        [SerializeField] private float _maxX = 20f;

        private float _velocity;

        public void Init(Transform target, float minX, float maxX)
        {
            _target = target; _minX = minX; _maxX = maxX;
        }

        private void LateUpdate()
        {
            if (_target == null) return;
            float x = Mathf.SmoothDamp(transform.position.x, _target.position.x, ref _velocity, _smoothTime);
            transform.position = new Vector3(Mathf.Clamp(x, _minX, _maxX), transform.position.y, transform.position.z);
        }
    }
}
