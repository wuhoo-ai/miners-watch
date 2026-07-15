using System;
using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Runtime inventory item. Wraps a MineralType with stack count and sell price.
    /// Separate from MineralData (ScriptableObject) to avoid EditMode serialization issues.
    /// </summary>
    [Serializable]
    public class InventoryItem
    {
        public MineralType mineralType;
        public int count;
        public float sellPrice;

        public InventoryItem(MineralType type, float price, int count = 1)
        {
            mineralType = type;
            sellPrice = price;
            this.count = count;
        }
    }
}
