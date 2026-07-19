using System.Collections.Generic;
using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// W9: Auto-turret placed on the build grid. At night, scans for enemies
    /// in range and fires at the nearest one. Cooldown-gated hitscan (no projectile
    /// system — damage applied directly to Enemy via TakeDamage).
    ///
    /// Deactivates automatically during day. Testable via Init+Update with mock enemies.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class Turret : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float _range = 6f;
        [SerializeField] private float _cooldown = 1.5f;
        [SerializeField] private int _damage = 15;

        private float _cooldownTimer;
        private DayNightCycle _cycle;
        private List<Enemy> _enemiesInRange = new List<Enemy>();
        private CircleCollider2D _trigger;

        public float Range => _range;
        public float CooldownProgress => _cooldown > 0f ? _cooldownTimer / _cooldown : 0f;
        public List<Enemy> EnemiesInRange => _enemiesInRange;

        /// <summary>Spend dt. Returns true if a shot was fired this tick (test hook).</summary>
        internal bool ManualTick(float dt)
        {
            _cooldownTimer += dt;
            if (_cooldownTimer >= _cooldown)
            {
                Fire();
                _cooldownTimer = 0f;
                return true;
            }
            return false;
        }

        // ── lifecycle ────────────────────────────────────────

        private void Awake()
        {
            _trigger = GetComponent<CircleCollider2D>();
            _trigger.isTrigger = true;
            _trigger.radius = _range;
        }

        private void Start()
        {
            _cycle = GameRoot.Get<DayNightCycle>();
        }

        private void Update()
        {
            if (_cycle == null || _cycle.CurrentPhase == DayNightPhase.Day) return;

            _cooldownTimer += Time.deltaTime;
            if (_cooldownTimer >= _cooldown)
            {
                Fire();
                _cooldownTimer = 0f;
            }
        }

        // ── targeting ────────────────────────────────────────

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

        private Enemy GetNearestEnemy()
        {
            Enemy nearest = null;
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
                    nearest = e;
                }
            }
            return nearest;
        }

        private void Fire()
        {
            var target = GetNearestEnemy();
            if (target != null)
                target.TakeDamage(_damage);
        }

        // ── testable init (EditMode) ─────────────────────────

        public void Init(float range, float cooldown, int damage)
        {
            _range = range;
            _cooldown = cooldown;
            _damage = damage;
            if (_trigger != null) _trigger.radius = _range;
        }
    }
}
