using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Player/NPC HP component. Reads armor upgrade level for HP multiplier.
    /// Base HP: 100. Armor Lv2: +20% (120), Lv3: +50% (150), Lv3+: +100% (200).
    /// </summary>
    public class PlayerHP : MonoBehaviour
    {
        [SerializeField] private int _baseHP = 100;
        [SerializeField] private UpgradeSystem _upgrades;

        private int _currentHP;
        private int _maxHP;

        public int CurrentHP => _currentHP;
        public int MaxHP => _maxHP;
        public bool IsDead => _currentHP <= 0;

        public void Init(UpgradeSystem upgrades)
        {
            _upgrades = upgrades;
            if (_baseHP <= 0) _baseHP = 100;
            RecalculateMaxHP();
            _currentHP = _maxHP;
        }

        private void Awake()
        {
            if (_upgrades == null)
                _upgrades = GetComponent<UpgradeSystem>() ?? GetComponentInParent<UpgradeSystem>() ?? GameRoot.Get<UpgradeSystem>();
            RecalculateMaxHP();
            _currentHP = _maxHP;
        }

        public void RecalculateMaxHP()
        {
            int level = _upgrades != null ? _upgrades.GetLevel(UpgradeType.Armor) : 1;
            float mult = level switch
            {
                2 => 1.2f,
                3 => 1.5f,
                4 => 2.0f,
                _ => 1.0f,
            };
            _maxHP = Mathf.RoundToInt(_baseHP * mult);
            if (_currentHP > _maxHP) _currentHP = _maxHP;
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || IsDead) return;
            _currentHP = Mathf.Max(0, _currentHP - damage);
        }
    }
}
