using System;
using System.Collections.Generic;

namespace MinersWatch
{
    /// <summary>Serializable save data. Pure C# — no MonoBehaviour, no Unity types.</summary>
    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 1;

        public int version;
        public int depthLevel;
        public int gold;
        public Dictionary<string, int> upgradeLevels; // "Pickaxe"→2, "Armor"→1, "Backpack"→3
        public List<InventoryEntry> inventory;
        public bool[] defenseGrid;
        public int waveProgress;

        public static SaveData CreateDefault() => new SaveData
        {
            version = CurrentVersion,
            depthLevel = 0,   // Shallow
            gold = 0,
            upgradeLevels = new Dictionary<string, int>
            {
                { "Pickaxe", 1 }, { "Armor", 1 }, { "Backpack", 1 }
            },
            inventory = new List<InventoryEntry>(),
            defenseGrid = new bool[15],
            waveProgress = 0,
        };
    }

    /// <summary>Lightweight inventory entry for serialization.</summary>
    [Serializable]
    public class InventoryEntry
    {
        public string mineralType;
        public int count;
        public float sellPrice;

        public static InventoryEntry FromItem(InventoryItem item) => new InventoryEntry
        {
            mineralType = item.mineralType.ToString(),
            count = item.count,
            sellPrice = item.sellPrice,
        };

        public MineralType GetMineralType() =>
            Enum.TryParse<MineralType>(mineralType, out var t) ? t : MineralType.Stone;
    }
}
