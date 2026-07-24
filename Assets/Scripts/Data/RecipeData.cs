using System;

namespace MinersWatch
{
    /// <summary>
    /// Crafting recipe definition. Runtime data class (not ScriptableObject)
    /// to avoid EditMode serialization issues.
    /// </summary>
    [Serializable]
    public class RecipeDef
    {
        public string id;                          // Unique recipe identifier
        public string displayName;                 // UI display name
        public MineralType[] inputs;               // Input mineral types
        public int[] inputCounts;                  // Input quantities (parallel array)
        public MineralType? outputMineral;         // Output mineral (nullable)
        public int outputCount;                    // Output quantity
        public UpgradeType? unlockUpgrade;         // Unlock upgrade type (nullable)
        public int unlockLevel;                    // Upgrade level to unlock

        /// <summary>Factory method for cleaner instantiation.</summary>
        public static RecipeDef Create(
            string id, string name,
            MineralType[] inputs, int[] counts,
            MineralType? output = null, int outputQty = 1,
            UpgradeType? upgrade = null, int upgradeLvl = 0)
        {
            return new RecipeDef
            {
                id = id,
                displayName = name,
                inputs = inputs,
                inputCounts = counts,
                outputMineral = output,
                outputCount = outputQty,
                unlockUpgrade = upgrade,
                unlockLevel = upgradeLvl
            };
        }

        /// <summary>Validate recipe definition integrity.</summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(id)) return false;
            if (inputs == null || inputCounts == null) return false;
            if (inputs.Length != inputCounts.Length) return false;
            if (inputs.Length == 0) return false;
            for (int i = 0; i < inputCounts.Length; i++)
            {
                if (inputCounts[i] <= 0) return false;
            }
            return true;
        }
    }

    /// <summary>Preset recipes for the game.</summary>
    public static class RecipePresets
    {
        public static RecipeDef SteelIngot => RecipeDef.Create(
            "steel_ingot", "钢锭",
            new[] { MineralType.Iron, MineralType.Gold },
            new[] { 3, 1 },
            upgrade: UpgradeType.Pickaxe, upgradeLvl: 3
        );

        public static RecipeDef EnergyCore => RecipeDef.Create(
            "energy_core", "能量核心",
            new[] { MineralType.Crystal, MineralType.Obsidian },
            new[] { 2, 1 }
        );

        public static RecipeDef GuardianToken => RecipeDef.Create(
            "guardian_token", "守护者之证",
            new[] { MineralType.Obsidian },
            new[] { 5 }
        );

        /// <summary>All available recipes.</summary>
        public static RecipeDef[] All => new[]
        {
            SteelIngot,
            EnergyCore,
            GuardianToken
        };
    }
}
