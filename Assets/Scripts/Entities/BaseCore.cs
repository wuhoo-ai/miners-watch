using UnityEngine;

namespace MinersWatch
{
    /// <summary>Base core: the structure players must defend at night. Takes damage from enemies.</summary>
    public class BaseCore : MonoBehaviour
    {
        [SerializeField] private int _maxHP = 200;
        private int _currentHP;

        public int CurrentHP => _currentHP;
        public int MaxHP => _maxHP;
        public bool IsDestroyed => _currentHP <= 0;

        public void Init(int hp)
        {
            _maxHP = hp;
            _currentHP = hp;
        }

        private void Awake()
        {
            if (_maxHP <= 0) _maxHP = 200;
            _currentHP = _maxHP;
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || IsDestroyed) return;
            _currentHP = Mathf.Max(0, _currentHP - damage);
        }
    }
}
