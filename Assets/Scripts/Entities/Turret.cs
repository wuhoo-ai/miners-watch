using System.Collections.Generic;
using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Turret: auto-attacks nearest enemy within range at night.
    /// Uses CircleCollider2D trigger for enemy detection.
    /// Testable via FireAtTime() and Init().
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class Turret : DefenseEntity
    {
        [SerializeField] private float _fireInterval = 2f;
        [SerializeField] private int _damage = 15;
        [SerializeField] private float _range = 5f;
        private float _lastFireTime = -999f;

        private List<Enemy> _enemiesInRange = new List<Enemy>();
        private CircleCollider2D _trigger;
        private DayNightCycle _cycle;

        public int Damage => _damage;
        public float Range => _range;
        public float FireInterval => _fireInterval;
        public float TimeSinceLastFire => Time.time - _lastFireTime;
        public bool CanFire => TimeSinceLastFire >= _fireInterval;
        public List<Enemy> EnemiesInRange => _enemiesInRange;

        // ── init ──────────────────────────────────────────────

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
            _trigger = GetComponent<CircleCollider2D>();
            _trigger.isTrigger = true;
            _trigger.radius = _range;
        }

        private void Start()
        {
            _cycle = GameRoot.Get<DayNightCycle>();
        }

        // ── auto-fire loop (night only) ───────────────────────

        private void Update()
        {
            if (_cycle == null || _cycle.CurrentPhase == DayNightPhase.Day) return;
            if (IsDestroyed) return;

            if (CanFire)
            {
                var target = GetNearestEnemyTransform();
                if (target != null)
                {
                    _lastFireTime = Time.time;
                    var enemy = target.GetComponent<Enemy>();
                    if (enemy != null) enemy.TakeDamage(_damage);
                }
            }
        }

        // ── enemy detection ───────────────────────────────────

        private void OnTriggerEnter2D(Collider2D other)
        {
            var enemy = other.GetComponent<Enemy>();
            if (enemy != null && !_enemiesInRange.Contains(enemy))
                _enemiesInRange.Add(enemy);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var enemy = other.GetComponent<Enemy>();
            if (enemy != null)
                _enemiesInRange.Remove(enemy);
        }

        private Transform GetNearestEnemyTransform()
        {
            Transform nearest = null;
            float minDist = float.MaxValue;
            Vector3 myPos = transform.position;

            for (int i = _enemiesInRange.Count - 1; i >= 0; i--)
            {
                var e = _enemiesInRange[i];
                if (e == null || e.IsDead)
                {
                    _enemiesInRange.RemoveAt(i);
                    continue;
                }
                float d = Vector3.Distance(myPos, e.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    nearest = e.transform;
                }
            }
            return nearest;
        }

        // ── manual fire (existing API) ────────────────────────

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
