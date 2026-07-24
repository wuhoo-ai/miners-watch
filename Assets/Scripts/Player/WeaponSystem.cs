using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Simple melee weapon system. Spawns a short-lived hitbox in front of the player.
    /// Attack input comes from TouchInput (mobile) or keyboard (E key).
    /// Combo system: consecutive attacks within combo window increase damage.
    /// </summary>
    public class WeaponSystem : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private float _attackRange = 1.8f;
        [SerializeField] private float _attackCooldown = 0.4f;
        [SerializeField] private int _baseDamage = 10;
        [SerializeField] private LayerMask _enemyLayer = -1;

        [Header("Combo")]
        [SerializeField] private float _comboWindow = 0.8f;
        [SerializeField] private int _maxCombo = 3;
        [SerializeField] private float _comboDamageMultiplier = 0.2f;

        [Header("References")]
        [SerializeField] private Transform _attackOrigin;

        private float _lastAttackTime = -99f;
        private UpgradeSystem _upgrades;
        private int _comboCount;
        private float _comboTimer;

        public int ComboCount => _comboCount;
        public float ComboTimer => _comboTimer;
        public float ComboDamageMultiplier => GetComboMultiplier();

        /// <summary>Test-friendly attack with explicit time.</summary>
        public bool TryAttackTest(float currentTime)
        {
            if (currentTime - _lastAttackTime < _attackCooldown) return false;

            // Update combo
            if (currentTime - _lastAttackTime <= _comboWindow)
            {
                _comboCount = Mathf.Min(_comboCount + 1, _maxCombo);
            }
            else
            {
                _comboCount = 1;
            }

            _lastAttackTime = currentTime;
            _comboTimer = _comboWindow;
            return true;
        }

        /// <summary>Test-friendly combo timer update.</summary>
        public void UpdateComboTest(float deltaTime)
        {
            _comboTimer -= deltaTime;
            if (_comboTimer <= 0f)
            {
                _comboCount = 0;
            }
        }

        private void Awake()
        {
            if (_attackOrigin == null) _attackOrigin = transform;
            _upgrades = GetComponentInParent<UpgradeSystem>() ?? GameRoot.Get<UpgradeSystem>();
        }

        private void Update()
        {
            // Decay combo timer
            if (_comboTimer > 0f)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f)
                {
                    _comboCount = 0;
                }
            }
        }

        /// <summary>Try to attack. Returns true if an attack was executed.</summary>
        public bool TryAttack()
        {
            if (Time.time - _lastAttackTime < _attackCooldown) return false;

            // Update combo
            if (Time.time - _lastAttackTime <= _comboWindow)
            {
                _comboCount = Mathf.Min(_comboCount + 1, _maxCombo);
            }
            else
            {
                _comboCount = 1;
            }

            _lastAttackTime = Time.time;
            _comboTimer = _comboWindow;

            int dmg = GetDamage();
            Vector2 origin = _attackOrigin.position;
            Vector2 dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

            // Overlap circle in attack direction
            var hits = Physics2D.OverlapCircleAll(origin + dir * _attackRange * 0.7f, _attackRange * 0.6f, _enemyLayer);
            int hitCount = 0;
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(dmg);
                    DamagePopup.Show(hit.transform.position, $"-{dmg}", Color.red);
                    hitCount++;
                }
            }

            if (hitCount == 0)
            {
                // Miss — show a small "whoosh" indicator
                DamagePopup.Show(origin + dir * _attackRange, "", new Color(0.6f, 0.6f, 0.6f, 0f));
            }

            AudioManager.SFX("sfx_enemy_hit_01");
            ScreenShake.Trigger(0.08f, 0.3f);
            return true;
        }

        private int GetDamage()
        {
            int level = _upgrades?.GetLevel(UpgradeType.Pickaxe) ?? 1;
            int baseDmg = level switch
            {
                2 => 15,
                3 => 22,
                _ => _baseDamage,
            };

            // Apply combo multiplier
            float multiplier = GetComboMultiplier();
            return Mathf.RoundToInt(baseDmg * multiplier);
        }

        private float GetComboMultiplier()
        {
            if (_comboCount <= 1) return 1f;
            return 1f + (_comboCount - 1) * _comboDamageMultiplier;
        }

        private void OnDrawGizmosSelected()
        {
            if (_attackOrigin == null) return;
            Gizmos.color = Color.red;
            Vector2 o = _attackOrigin.position;
            Vector2 d = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            Gizmos.DrawWireSphere(o + d * _attackRange * 0.7f, _attackRange * 0.6f);
        }
    }
}
