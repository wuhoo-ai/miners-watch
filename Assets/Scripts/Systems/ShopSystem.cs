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

            AudioManager.SFX("sfx_sell"); // cash register!
            return total;
        }

        /// <summary>Buy an upgrade. Returns false if insufficient gold or max level.</summary>
        public bool BuyUpgrade(UpgradeType type)
        {
            if (_upgrades == null) return false;
            return _upgrades.BuyUpgrade(type);
        }

        /// <summary>Check if player can afford a given gold amount.</summary>
        public bool CanAffordGold(int amount)
        {
            return _upgrades != null && _upgrades.Gold >= amount;
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

            // Actually place the building on the BuildGrid
            PlaceDefense(type, def);
            return true;
        }

        private void PlaceDefense(DefenseType type, DefenseDef def)
        {
            Transform grid = GameObject.Find("BuildGrid")?.transform;
            if (grid == null) return;

            // Find first empty cell
            for (int i = 0; i < grid.childCount; i++)
            {
                var cell = grid.GetChild(i);
                if (cell.childCount == 0) // empty slot
                {
                    var go = new GameObject($"Defense_{type}_{i}");
                    go.transform.SetParent(cell, false);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localScale = Vector3.one * 0.6f;

                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = 2;
                    sr.color = type switch
                    {
                        DefenseType.Wall => new Color(0.4f, 0.4f, 0.4f),      // gray
                        DefenseType.SpikeTrap => new Color(0.7f, 0.3f, 0.2f),   // rust
                        DefenseType.Turret => new Color(0.2f, 0.5f, 0.3f),      // green
                        _ => Color.gray,
                    };
                    sr.sprite = SpriteAtlas.WhiteSquare;

                    if (type == DefenseType.Turret)
                    {
                        var turret = go.AddComponent<Turret>();
                        turret.Init(def.hp);
                    }
                    else if (type == DefenseType.Wall)
                    {
                        var wall = go.AddComponent<Wall>();
                        wall.Init(def.hp);
                    }
                    else if (type == DefenseType.SpikeTrap)
                    {
                        var trap = go.AddComponent<SpikeTrap>();
                        trap.Init(def.hp);
                    }

                    return; // placed
                }
            }
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
