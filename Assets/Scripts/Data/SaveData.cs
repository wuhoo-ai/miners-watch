using System;
using System.Collections.Generic;
using System.Linq;

namespace MinersWatch
{
    /// <summary>Serializable upgrade key-value pair for JsonUtility.</summary>
    [Serializable]
    public class UpgradeEntry
    {
        public string key;
        public int value;
    }

    /// <summary>Serializable save data. No Dictionary — uses List<UpgradeEntry> for JsonUtility compat.</summary>
    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 1;

        public int version;
        public int depthLevel;
        public int gold;
        public List<UpgradeEntry> upgradeLevels;
        public List<InventoryEntry> inventory;
        public bool[] defenseGrid;
        public int waveProgress;

        public static SaveData CreateDefault() => new SaveData
        {
            version = CurrentVersion,
            depthLevel = 0,
            gold = 0,
            upgradeLevels = new List<UpgradeEntry>
            {
                new UpgradeEntry { key = "Pickaxe", value = 1 },
                new UpgradeEntry { key = "Armor", value = 1 },
                new UpgradeEntry { key = "Backpack", value = 1 },
            },
            inventory = new List<InventoryEntry>(),
            defenseGrid = new bool[15],
            waveProgress = 0,
        };

        public int GetUpgradeLevel(string key) =>
            upgradeLevels?.FirstOrDefault(e => e.key == key)?.value ?? 1;

        public void SetUpgradeLevel(string key, int value)
        {
            var entry = upgradeLevels?.FirstOrDefault(e => e.key == key);
            if (entry != null) entry.value = value;
        }
    }

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
