using System.Collections.Generic;
using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Shop logic: sell minerals, buy upgrades, buy defenses.
    /// Bridges InventorySystem ↔ UpgradeSystem.
    /// </summary>
    public class ShopSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventorySystem _inventory;
        [SerializeField] private UpgradeSystem _upgrades;

        public InventorySystem Inventory => _inventory;
        public UpgradeSystem Upgrades => _upgrades;

        public void Init(InventorySystem inventory, UpgradeSystem upgrades)
        {
            _inventory = inventory;
            _upgrades = upgrades;
            if (_upgrades != null)
            {
                _upgrades.OnUpgraded -= OnUpgradeApplied; // prevent double-sub
                _upgrades.OnUpgraded += OnUpgradeApplied;
            }
        }

        private void OnUpgradeApplied(UpgradeType type)
        {
            if (type == UpgradeType.Backpack && _inventory != null)
            {
                int level = _upgrades.GetLevel(UpgradeType.Backpack);
                int capacity = level switch { 2 => 20, 3 => 30, _ => 10 };
                _inventory.SetCapacity(capacity);
            }
        }

        private void Awake()
        {
            if (_inventory == null) _inventory = GetComponent<InventorySystem>() ?? GetComponentInParent<InventorySystem>();
            if (_upgrades == null) _upgrades = GetComponent<UpgradeSystem>() ?? GetComponentInParent<UpgradeSystem>();
            if (_upgrades != null)
                _upgrades.OnUpgraded += OnUpgradeApplied;
        }

        private void OnDestroy()
        {
            if (_upgrades != null)
                _upgrades.OnUpgraded -= OnUpgradeApplied;
        }

        /// <summary>Sell all minerals in inventory. Returns total gold earned.</summary>
        public int SellAllMinerals()
        {
            if (_inventory == null || _upgrades == null) return 0;

            int total = 0;
            var items = new List<InventoryItem>(_inventory.Items);
            foreach (var item in items)
            {
                int value = Mathf.RoundToInt(item.sellPrice * item.count);
                total += value;
                _inventory.RemoveItem(item.mineralType, item.count);
            }

            if (total > 0)
                _upgrades.AddGold(total);

            return total;
        }

        /// <summary>Buy an upgrade. Returns false if insufficient gold or max level.</summary>
        public bool BuyUpgrade(UpgradeType type)
        {
            if (_upgrades == null) return false;
            return _upgrades.BuyUpgrade(type);
        }

        /// <summary>Check if player can afford upgrade.</summary>
        public bool CanAffordUpgrade(UpgradeType type)
        {
            if (_upgrades == null) return false;
            int cost = _upgrades.GetUpgradeCost(type);
            return cost > 0 && _upgrades.Gold >= cost;
        }

        /// <summary>Buy a defense structure. Checks gold + iron from inventory.</summary>
        public bool BuyDefense(DefenseType type)
        {
            if (_inventory == null || _upgrades == null) return false;

            var def = GetDefenseDef(type);
            if (_upgrades.Gold < def.costGold) return false;
            if (def.costIron > 0 && _inventory.GetCount(MineralType.Iron) < def.costIron) return false;

            _upgrades.AddGold(-def.costGold);
            if (def.costIron > 0)
                _inventory.RemoveItem(MineralType.Iron, def.costIron);

            return true;
        }

        /// <summary>Check if player can afford defense.</summary>
        public bool CanAffordDefense(DefenseType type)
        {
            if (_inventory == null || _upgrades == null) return false;
            var def = GetDefenseDef(type);
            return _upgrades.Gold >= def.costGold &&
                   _inventory.GetCount(MineralType.Iron) >= def.costIron;
        }

        // Hardcoded defense costs from GDD §3.3 / tasks.json T005
        private static DefenseDef GetDefenseDef(DefenseType type) => type switch
        {
            DefenseType.Wall => DefenseDef.Create(DefenseType.Wall, 50, 0, 50),
            DefenseType.SpikeTrap => DefenseDef.Create(DefenseType.SpikeTrap, 80, 5, 30, dmg: 20, uses: 3),
            DefenseType.Turret => DefenseDef.Create(DefenseType.Turret, 200, 3, 40, dmg: 15, interval: 2f),
            _ => DefenseDef.Create(DefenseType.Wall, 50, 0, 50),
        };
    }
}
