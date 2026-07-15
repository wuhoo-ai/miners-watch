using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Boss enemy: Guardian. Special behaviors — slam AoE, rage mode at <50% HP.
    /// Inherits Enemy base class.
    /// </summary>
    public class BossGuardian : Enemy
    {
        [Header("Boss Skills")]
        [SerializeField] private float _slamInterval = 8f;
        [SerializeField] private int _slamDamage = 75;
        [SerializeField] private float _slamRange = 3f;
        [SerializeField] private float _rageSpeedMultiplier = 1.5f;

        private float _slamTimer;
        private bool _isRaging;

        public int SlamDamage => _slamDamage;
        public float SlamRange => _slamRange;
        public bool IsRaging => _isRaging;
        public float RageSpeedMultiplier => _rageSpeedMultiplier;

        public override void Init(int hp, int damage, float speed)
        {
            base.Init(hp, damage, speed);
            _slamTimer = 0f;
            _isRaging = false;
            GuardBossDefaults();
        }

        protected override void Awake()
        {
            base.Awake();
            _slamTimer = 0f;
            _isRaging = false;
            GuardBossDefaults();
        }

        private void GuardBossDefaults()
        {
            if (_slamInterval <= 0f) _slamInterval = 8f;
            if (_slamDamage <= 0) _slamDamage = 75;
            if (_slamRange <= 0f) _slamRange = 3f;
            if (_rageSpeedMultiplier <= 0f) _rageSpeedMultiplier = 1.5f;
        }

        /// <summary>Check rage threshold (50% HP). Returns true when rage activates.</summary>
        public bool CheckRageMode()
        {
            if (_isRaging) return false;

            float hpPercent = (float)_currentHP / _maxHP;
            if (hpPercent <= 0.5f)
            {
                _isRaging = true;
                _speed *= _rageSpeedMultiplier;
                return true;
            }
            return false;
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            CheckRageMode();
        }

        /// <summary>
        /// Advance boss logic by dt. Returns slam damage dealt (0 if on cooldown).
        /// Caller checks if targets in range.
        /// </summary>
        public int TickSlam(float dt)
        {
            if (IsDead) return 0;

            _slamTimer += dt;
            if (_slamTimer >= _slamInterval)
            {
                _slamTimer -= _slamInterval;
                return _slamDamage;
            }
            return 0;
        }

        /// <summary>Get effective speed (rage-boosted).</summary>
        public float GetEffectiveSpeed() => _speed;

        /// <summary>Reset slam timer (test helper).</summary>
        public void ResetSlamTimer() => _slamTimer = 0f;
    }
}
