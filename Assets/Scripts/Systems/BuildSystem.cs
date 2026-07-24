using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Build system: validates and executes defense placement and upgrades.
    /// Bridges ShopSystem (resource checks) with DefenseEntity instantiation.
    /// Supports building levels (Wood/Stone/Iron), trap variants, and turret variants.
    /// Pure logic testable via Init() — scene placement deferred to PlayMode.
    /// </summary>
    public class BuildSystem : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private int _gridWidth = 15;
        [SerializeField] private float _cellSize = 1f;

        [Header("References")]
        [SerializeField] private ShopSystem _shop;
        [SerializeField] private InventorySystem _inventory;

        // Grid occupancy: true = occupied
        private bool[] _grid;
        private BuildLevel[] _levels;     // Track level per cell
        private int[] _variants;          // Track variant per cell (0=base)

        public int GridWidth => _gridWidth;
        public ShopSystem Shop => _shop;

        public void Init(ShopSystem shop, InventorySystem inventory = null)
        {
            _shop = shop;
            _inventory = inventory;
            if (_gridWidth <= 0) _gridWidth = 15;
            if (_cellSize <= 0f) _cellSize = 1f;
            _grid = new bool[_gridWidth];
            _levels = new BuildLevel[_gridWidth];
            _variants = new int[_gridWidth];
        }

        private void Awake()
        {
            if (_shop == null) _shop = GetComponent<ShopSystem>() ?? GetComponentInParent<ShopSystem>() ?? GameRoot.Get<ShopSystem>();
            if (_inventory == null) _inventory = GameRoot.Get<InventorySystem>();
            _grid = new bool[_gridWidth];
            _levels = new BuildLevel[_gridWidth];
            _variants = new int[_gridWidth];
        }

        /// <summary>Check if a grid cell is valid for placement.</summary>
        public bool IsCellValid(int x)
        {
            return x >= 0 && x < _gridWidth && !_grid[x];
        }

        /// <summary>Check if player can afford and place this defense type.</summary>
        public bool CanPlaceDefense(DefenseType type)
        {
            return _shop != null && _shop.CanAffordDefense(type);
        }

        /// <summary>
        /// Attempt to place a defense at grid position.
        /// Validates: cell open, resources sufficient.
        /// Returns true if placement succeeded (resources deducted, cell marked).
        /// </summary>
        public bool PlaceDefense(DefenseType type, int gridX)
        {
            if (!IsCellValid(gridX)) return false;
            if (_shop == null) return false;
            if (!_shop.BuyDefense(type)) return false;

            _grid[gridX] = true;
            _levels[gridX] = BuildLevel.Wood; // Base level
            _variants[gridX] = 0;
            return true;
        }

        /// <summary>Remove a defense from the grid (e.g., destroyed).</summary>
        public void ClearCell(int x)
        {
            if (x >= 0 && x < _gridWidth)
            {
                _grid[x] = false;
                _levels[x] = BuildLevel.Wood;
                _variants[x] = 0;
            }
        }

        // ========== Building Level Upgrades ==========

        /// <summary>Get current building level at grid position.</summary>
        public BuildLevel GetBuildingLevel(int cellIndex)
        {
            if (cellIndex < 0 || cellIndex >= _gridWidth) return BuildLevel.Wood;
            if (!_grid[cellIndex]) return BuildLevel.Wood;
            return _levels[cellIndex];
        }

        /// <summary>Check if a building can be upgraded to the target level.</summary>
        public bool CanUpgrade(int cellIndex, BuildLevel targetLevel)
        {
            if (cellIndex < 0 || cellIndex >= _gridWidth) return false;
            if (!_grid[cellIndex]) return false;

            BuildLevel current = _levels[cellIndex];
            if ((int)targetLevel <= (int)current) return false; // Must be higher
            if (targetLevel - current > 1) return false;        // Must be sequential

            // Check materials
            var def = GetBuildLevelDef(targetLevel);
            if (def == null) return false;

            return HasUpgradeMaterials(def);
        }

        /// <summary>
        /// Upgrade building at cellIndex to target level.
        /// Deducts materials and updates level.
        /// Returns false if upgrade not possible.
        /// </summary>
        public bool UpgradeBuilding(int cellIndex, BuildLevel targetLevel)
        {
            if (!CanUpgrade(cellIndex, targetLevel)) return false;

            var def = GetBuildLevelDef(targetLevel);
            if (def == null) return false;

            // Deduct materials
            if (!DeductUpgradeMaterials(def)) return false;

            _levels[cellIndex] = targetLevel;
            return true;
        }

        /// <summary>Get HP multiplier for a building at given level.</summary>
        public int GetHpMultiplier(BuildLevel level)
        {
            var def = GetBuildLevelDef(level);
            return def != null ? def.hpMultiplier : 1;
        }

        // ========== Trap Variants ==========

        /// <summary>Check if player can afford a trap variant.</summary>
        public bool CanPlaceTrapVariant(TrapVariant variant)
        {
            var def = GetTrapVariantDef(variant);
            if (def == null) return false;
            return _shop != null && _shop.CanAffordGold(def.costGold);
        }

        /// <summary>Get trap variant definition.</summary>
        public TrapVariantDef GetTrapVariantDef(TrapVariant variant)
        {
            foreach (var d in TrapVariantPresets.All)
            {
                if (d.variant == variant) return d;
            }
            return null;
        }

        // ========== Turret Variants ==========

        /// <summary>Check if player can afford a turret variant.</summary>
        public bool CanPlaceTurretVariant(TurretVariant variant)
        {
            var def = GetTurretVariantDef(variant);
            if (def == null) return false;
            return _shop != null && _shop.CanAffordGold(def.costGold);
        }

        /// <summary>Get turret variant definition.</summary>
        public TurretVariantDef GetTurretVariantDef(TurretVariant variant)
        {
            foreach (var d in TurretVariantPresets.All)
            {
                if (d.variant == variant) return d;
            }
            return null;
        }

        // ========== Helpers ==========

        private static BuildLevelDef GetBuildLevelDef(BuildLevel level)
        {
            foreach (var d in BuildLevelPresets.All)
            {
                if (d.level == level) return d;
            }
            return null;
        }

        private bool HasUpgradeMaterials(BuildLevelDef def)
        {
            if (_inventory == null) return false;
            if (_shop == null) return false;
            if (def.costGold > 0 && _shop.Upgrades != null && _shop.Upgrades.Gold < def.costGold) return false;
            if (def.costIron > 0 && !_inventory.HasItem(MineralType.Iron, def.costIron)) return false;
            if (def.costStone > 0 && !_inventory.HasItem(MineralType.Stone, def.costStone)) return false;
            return true;
        }

        private bool DeductUpgradeMaterials(BuildLevelDef def)
        {
            if (_shop == null) return false;
            if (def.costGold > 0 && _shop.Upgrades != null)
            {
                if (_shop.Upgrades.Gold < def.costGold) return false;
                _shop.Upgrades.AddGold(-def.costGold);
            }
            if (def.costIron > 0 && _inventory != null)
            {
                if (!_inventory.RemoveItem(MineralType.Iron, def.costIron)) return false;
            }
            if (def.costStone > 0 && _inventory != null)
            {
                if (!_inventory.RemoveItem(MineralType.Stone, def.costStone)) return false;
            }
            return true;
        }
    }
}
