using UnityEngine;
using UnityEngine.UI;

namespace MinersWatch
{
    /// <summary>
    /// Simple HUD display for inventory: horizontal row of item icons + counts.
    /// Subscribes to InventorySystem events for reactive updates.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventorySystem _inventory;
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private GameObject _slotPrefab; // Has Image + Text child

        /// <summary>Explicit init for test/delayed binding.</summary>
        public void Init(InventorySystem inventory)
        {
            _inventory = inventory;
            if (_inventory != null)
            {
                _inventory.OnItemAdded += _ => RefreshDisplay();
                _inventory.OnItemRemoved += _ => RefreshDisplay();
            }
            RefreshDisplay();
        }

        private void Awake()
        {
            if (_inventory == null)
                _inventory = GetComponent<InventorySystem>() ?? GetComponentInParent<InventorySystem>();

            if (_inventory != null)
            {
                _inventory.OnItemAdded += _ => RefreshDisplay();
                _inventory.OnItemRemoved += _ => RefreshDisplay();
                RefreshDisplay();
            }
        }

        private void OnDestroy()
        {
            if (_inventory != null)
            {
                _inventory.OnItemAdded -= _ => RefreshDisplay();
                _inventory.OnItemRemoved -= _ => RefreshDisplay();
            }
        }

        public void RefreshDisplay()
        {
            if (_slotContainer == null || _slotPrefab == null) return;

            // Clear existing slots
            foreach (Transform child in _slotContainer)
                Destroy(child.gameObject);

            if (_inventory == null) return;

            // Create slot for each inventory item
            foreach (var item in _inventory.Items)
            {
                var slot = Instantiate(_slotPrefab, _slotContainer);
                var text = slot.GetComponentInChildren<Text>();
                if (text != null)
                    text.text = $"{item.mineralType}\n×{item.count}";
            }
        }
    }
}
