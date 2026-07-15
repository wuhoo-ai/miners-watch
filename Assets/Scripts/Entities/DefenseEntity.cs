using UnityEngine;

namespace MinersWatch
{
    /// <summary>Base class for all defense structures. Testable HP/damage logic.</summary>
    public abstract class DefenseEntity : MonoBehaviour
    {
        [SerializeField] protected int _maxHP = 50;
        protected int _currentHP;

        public int MaxHP => _maxHP;
        public int CurrentHP => _currentHP;
        public bool IsDestroyed => _currentHP <= 0;

        public virtual void Init(int hp)
        {
            _maxHP = hp;
            _currentHP = hp;
        }

        protected virtual void Awake()
        {
            if (_maxHP <= 0) _maxHP = 50;
            _currentHP = _maxHP;
        }

        public virtual void TakeDamage(int damage)
        {
            if (damage <= 0) return;
            _currentHP = Mathf.Max(0, _currentHP - damage);
        }
    }
}
