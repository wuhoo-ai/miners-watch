using System.Collections.Generic;
using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Inventory system: item storage with stacking and capacity limits.
    /// Core logic is pure C# callable from EditMode tests via Init().
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        [Header("Capacity")]
        [SerializeField] private int _capacity = 10;
        [SerializeField] private int _maxStackSize = 5;

        private List<InventoryItem> _items = new List<InventoryItem>();

        public int Capacity => _capacity;
        public int MaxStackSize => _maxStackSize;
        public IReadOnlyList<InventoryItem> Items => _items;
        public int UsedSlots => _items.Count;

        public event System.Action<InventoryItem> OnItemAdded;
        public event System.Action<InventoryItem> OnItemRemoved;

        /// <summary>Explicit init for EditMode tests where Awake may not fire.</summary>
        public void Init()
        {
            if (_capacity <= 0) _capacity = 10;
            if (_maxStackSize <= 0) _maxStackSize = 5;
        }

        private void Awake() => Init();

        /// <summary>
        /// Add items to inventory. Tries to stack on existing slot first,
        /// then creates new slots as needed. Returns false if inventory is full.
        /// </summary>
        public bool AddItem(MineralType type, float sellPrice, int count = 1)
        {
            if (count <= 0) return false;

            // Try stack on existing slot first
            var existing = _items.Find(i => i.mineralType == type);
            if (existing != null)
            {
                int space = _maxStackSize - existing.count;
                int toAdd = Mathf.Min(space, count);
                existing.count += toAdd;
                OnItemAdded?.Invoke(existing);

                int remainder = count - toAdd;
                if (remainder > 0)
                    return AddNewSlot(type, sellPrice, remainder);

                return true;
            }

            return AddNewSlot(type, sellPrice, count);
        }

        private bool AddNewSlot(MineralType type, float sellPrice, int count)
        {
            if (_items.Count >= _capacity) return false;

            int stackCount = Mathf.Min(count, _maxStackSize);
            var item = new InventoryItem(type, sellPrice, stackCount);
            _items.Add(item);
            OnItemAdded?.Invoke(item);

            int remainder = count - stackCount;
            if (remainder > 0)
                return AddNewSlot(type, sellPrice, remainder);

            return true;
        }

        /// <summary>Remove items by type. Returns false if not enough items.</summary>
        public bool RemoveItem(MineralType type, int count = 1)
        {
            if (count <= 0) return false;

            var item = _items.Find(i => i.mineralType == type);
            if (item == null || item.count < count) return false;

            item.count -= count;
            OnItemRemoved?.Invoke(item);

            if (item.count <= 0)
                _items.Remove(item);

            return true;
        }

        /// <summary>Check if inventory has at least the specified count of an item.</summary>
        public bool HasItem(MineralType type, int count = 1)
        {
            return GetCount(type) >= count;
        }

        /// <summary>Get total count of an item across all stacks.</summary>
        public int GetCount(MineralType type)
        {
            int total = 0;
            foreach (var item in _items)
            {
                if (item.mineralType == type)
                    total += item.count;
            }
            return total;
        }

        /// <summary>Check if inventory has any free slot (including stackable space).</summary>
        public bool HasFreeSlot(MineralType type)
        {
            // Stackable
            var existing = _items.Find(i => i.mineralType == type);
            if (existing != null && existing.count < _maxStackSize)
                return true;

            // New slot
            return _items.Count < _capacity;
        }
    }
}
