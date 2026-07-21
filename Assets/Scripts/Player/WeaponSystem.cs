using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Simple melee weapon system. Spawns a short-lived hitbox in front of the player.
    /// Attack input comes from TouchInput (mobile) or keyboard (E key).
    /// </summary>
    public class WeaponSystem : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private float _attackRange = 1.8f;
        [SerializeField] private float _attackCooldown = 0.4f;
        [SerializeField] private int _baseDamage = 10;
        [SerializeField] private LayerMask _enemyLayer = -1;

        [Header("References")]
        [SerializeField] private Transform _attackOrigin;

        private float _lastAttackTime = -99f;
        private UpgradeSystem _upgrades;
        private int _attackCount;

        public int ComboCount => _attackCount;

        private void Awake()
        {
            if (_attackOrigin == null) _attackOrigin = transform;
            _upgrades = GetComponentInParent<UpgradeSystem>() ?? GameRoot.Get<UpgradeSystem>();
        }

        /// <summary>Try to attack. Returns true if an attack was executed.</summary>
        public bool TryAttack()
        {
            if (Time.time - _lastAttackTime < _attackCooldown) return false;
            _lastAttackTime = Time.time;
            _attackCount++;

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
            return level switch
            {
                2 => 15,
                3 => 22,
                _ => _baseDamage,
            };
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
