using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Build system: validates and executes defense placement.
    /// Bridges ShopSystem (resource checks) with DefenseEntity instantiation.
    /// Pure logic testable via Init() — scene placement deferred to PlayMode.
    /// </summary>
    public class BuildSystem : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private int _gridWidth = 15;
        [SerializeField] private float _cellSize = 1f;

        [Header("References")]
        [SerializeField] private ShopSystem _shop;

        // Grid occupancy: true = occupied
        private bool[] _grid;

        public int GridWidth => _gridWidth;
        public ShopSystem Shop => _shop;

        public void Init(ShopSystem shop)
        {
            _shop = shop;
            if (_gridWidth <= 0) _gridWidth = 15;
            if (_cellSize <= 0f) _cellSize = 1f;
            _grid = new bool[_gridWidth];
        }

        private void Awake()
        {
            if (_shop == null) _shop = GetComponent<ShopSystem>() ?? GetComponentInParent<ShopSystem>();
            _grid = new bool[_gridWidth];
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
            return true;
        }

        /// <summary>Remove a defense from the grid (e.g., destroyed).</summary>
        public void ClearCell(int x)
        {
            if (x >= 0 && x < _gridWidth)
                _grid[x] = false;
        }
    }
}
