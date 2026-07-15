using System;
using System.Collections.Generic;
using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Manages gold currency and upgrade levels. Pure logic, testable via Init().
    /// </summary>
    public class UpgradeSystem : MonoBehaviour
    {
        [Header("Upgrade Costs")]
        [SerializeField] private int pickaxeLv2Cost = 200;
        [SerializeField] private int pickaxeLv3Cost = 800;
        [SerializeField] private int armorLv2Cost = 150;
        [SerializeField] private int armorLv3Cost = 600;
        [SerializeField] private int backpackLv2Cost = 100;
        [SerializeField] private int backpackLv3Cost = 400;

        private int _gold;
        private Dictionary<UpgradeType, int> _levels = new Dictionary<UpgradeType, int>();

        public int Gold => _gold;
        public event Action<int> OnGoldChanged;

        public void Init()
        {
            _gold = 0;
            foreach (UpgradeType t in Enum.GetValues(typeof(UpgradeType)))
                _levels[t] = 1;
            GuardCosts();
        }

        private void Awake() => Init();

        private void GuardCosts()
        {
            if (pickaxeLv2Cost <= 0) pickaxeLv2Cost = 200;
            if (pickaxeLv3Cost <= 0) pickaxeLv3Cost = 800;
            if (armorLv2Cost <= 0) armorLv2Cost = 150;
            if (armorLv3Cost <= 0) armorLv3Cost = 600;
            if (backpackLv2Cost <= 0) backpackLv2Cost = 100;
            if (backpackLv3Cost <= 0) backpackLv3Cost = 400;
        }

        public int GetLevel(UpgradeType type) => _levels.TryGetValue(type, out var l) ? l : 1;

        public int GetUpgradeCost(UpgradeType type)
        {
            int level = GetLevel(type);
            if (level >= 3) return -1; // max level

            return type switch
            {
                UpgradeType.Pickaxe => level == 1 ? pickaxeLv2Cost : pickaxeLv3Cost,
                UpgradeType.Armor => level == 1 ? armorLv2Cost : armorLv3Cost,
                UpgradeType.Backpack => level == 1 ? backpackLv2Cost : backpackLv3Cost,
                _ => -1
            };
        }

        /// <summary>Attempt to buy an upgrade. Returns false if insufficient gold or max level.</summary>
        public bool BuyUpgrade(UpgradeType type)
        {
            int cost = GetUpgradeCost(type);
            if (cost < 0) return false; // max level
            if (_gold < cost) return false;

            _gold -= cost;
            _levels[type] = GetLevel(type) + 1;
            OnGoldChanged?.Invoke(_gold);
            return true;
        }

        public void AddGold(int amount)
        {
            if (amount == 0) return;
            _gold += amount;
            _gold = Mathf.Max(0, _gold);
            OnGoldChanged?.Invoke(_gold);
        }
    }
}
