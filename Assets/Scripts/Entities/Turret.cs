using UnityEngine;

namespace MinersWatch
{
    /// <summary>Turret: auto-attacks enemies within range, fires at fixed interval.</summary>
    public class Turret : DefenseEntity
    {
        [SerializeField] private float _fireInterval = 2f;
        [SerializeField] private int _damage = 15;
        [SerializeField] private float _range = 5f;
        private float _lastFireTime = -999f;

        public int Damage => _damage;
        public float Range => _range;
        public float FireInterval => _fireInterval;
        public float TimeSinceLastFire => Time.time - _lastFireTime;
        public bool CanFire => TimeSinceLastFire >= _fireInterval;

        public override void Init(int hp)
        {
            base.Init(hp);
            if (_fireInterval <= 0f) _fireInterval = 2f;
            if (_damage <= 0) _damage = 15;
            if (_range <= 0f) _range = 5f;
            _lastFireTime = -999f;
        }

        protected override void Awake()
        {
            base.Awake();
            _lastFireTime = -999f;
        }

        /// <summary>Fire at target within range. Returns damage dealt, 0 if on cooldown or destroyed.</summary>
        public int Fire(Transform target)
        {
            if (IsDestroyed) return 0;
            if (!CanFire) return 0;
            if (target == null) return 0;

            float dist = Vector2.Distance(transform.position, target.position);
            if (dist > _range) return 0;

            _lastFireTime = Time.time;
            return _damage;
        }

        /// <summary>Test-only fire with explicit time. Avoids Time.time dependency.</summary>
        public int FireAtTime(Transform target, float currentTime)
        {
            if (IsDestroyed) return 0;
            if (currentTime - _lastFireTime < _fireInterval) return 0;
            if (target == null) return 0;

            float dist = Vector2.Distance(transform.position, target.position);
            if (dist > _range) return 0;

            _lastFireTime = currentTime;
            return _damage;
        }
    }
}
