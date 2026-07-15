using UnityEngine;

namespace MinersWatch
{
    /// <summary>Base enemy: HP, damage, speed. Testable via Init().</summary>
    public class Enemy : MonoBehaviour
    {
        [SerializeField] protected int _maxHP = 30;
        [SerializeField] protected int _damage = 10;
        [SerializeField] protected float _speed = 2f;
        protected int _currentHP;

        public int MaxHP => _maxHP;
        public int CurrentHP => _currentHP;
        public int Damage => _damage;
        public float Speed => _speed;
        public bool IsDead => _currentHP <= 0;

        public virtual void Init(int hp, int damage, float speed)
        {
            _maxHP = hp;
            _damage = damage;
            _speed = speed;
            _currentHP = hp;
            GuardDefaults();
        }

        protected virtual void Awake()
        {
            _currentHP = _maxHP;
            GuardDefaults();
        }

        private void GuardDefaults()
        {
            if (_maxHP <= 0) _maxHP = 30;
            if (_damage <= 0) _damage = 10;
            if (_speed <= 0f) _speed = 2f;
        }

        public virtual void TakeDamage(int damage)
        {
            if (damage <= 0 || IsDead) return;
            _currentHP = Mathf.Max(0, _currentHP - damage);
        }
    }
}
