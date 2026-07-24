using System;
using UnityEngine;

namespace MinersWatch
{
    public enum UpgradeType { Pickaxe, Armor, Backpack }
    public enum DefenseType { Wall, SpikeTrap, Turret }
    public enum BuildLevel { Wood, Stone, Iron }
    public enum TrapVariant { Slow, Poison, Knockback }
    public enum TurretVariant { Arrow, Fire, Ice }

    /// <summary>Runtime data for upgrades. Not a ScriptableObject — avoids EditMode serialization issues.</summary>
    [Serializable]
    public class UpgradeDef
    {
        public UpgradeType type;
        public int level;
        public int cost;
        public string description;

        public static UpgradeDef Create(UpgradeType type, int level, int cost, string desc) =>
            new UpgradeDef { type = type, level = level, cost = cost, description = desc };
    }

    /// <summary>Runtime data for defense structures.</summary>
    [Serializable]
    public class DefenseDef
    {
        public DefenseType type;
        public int costGold;
        public int costIron;
        public int hp;
        public int damage;
        public float interval; // seconds between attacks (turret)
        public int uses;       // durability (spike trap)

        public static DefenseDef Create(DefenseType type, int gold, int iron, int hp, int dmg = 0, float interval = 0f, int uses = 0) =>
            new DefenseDef { type = type, costGold = gold, costIron = iron, hp = hp, damage = dmg, interval = interval, uses = uses };
    }

    /// <summary>Building level definition for upgrade system.</summary>
    [Serializable]
    public class BuildLevelDef
    {
        public BuildLevel level;
        public int hpMultiplier;          // HP multiplier (1x, 2x, 4x)
        public int costGold;
        public int costIron;
        public int costStone;             // New: stone cost for upgrades

        public static BuildLevelDef Create(BuildLevel level, int hpMult, int gold, int iron, int stone) =>
            new BuildLevelDef { level = level, hpMultiplier = hpMult, costGold = gold, costIron = iron, costStone = stone };
    }

    /// <summary>Preset building levels.</summary>
    public static class BuildLevelPresets
    {
        public static BuildLevelDef Wood => BuildLevelDef.Create(BuildLevel.Wood, 1, 0, 0, 0);
        public static BuildLevelDef Stone => BuildLevelDef.Create(BuildLevel.Stone, 2, 50, 10, 5);
        public static BuildLevelDef Iron => BuildLevelDef.Create(BuildLevel.Iron, 4, 150, 30, 0);

        public static BuildLevelDef[] All => new[] { Wood, Stone, Iron };
    }

    /// <summary>Trap variant definition.</summary>
    [Serializable]
    public class TrapVariantDef
    {
        public TrapVariant variant;
        public string displayName;
        public float effectValue;          // Slow: duration, Poison: DPS, Knockback: force
        public int costGold;
        public int costIron;

        public static TrapVariantDef Create(TrapVariant variant, string name, float value, int gold, int iron) =>
            new TrapVariantDef { variant = variant, displayName = name, effectValue = value, costGold = gold, costIron = iron };
    }

    /// <summary>Preset trap variants.</summary>
    public static class TrapVariantPresets
    {
        public static TrapVariantDef Slow => TrapVariantDef.Create(TrapVariant.Slow, "减速陷阱", 2f, 80, 5);
        public static TrapVariantDef Poison => TrapVariantDef.Create(TrapVariant.Poison, "毒陷阱", 10f, 120, 8);
        public static TrapVariantDef Knockback => TrapVariantDef.Create(TrapVariant.Knockback, "弹射陷阱", 5f, 100, 6);

        public static TrapVariantDef[] All => new[] { Slow, Poison, Knockback };
    }

    /// <summary>Turret variant definition.</summary>
    [Serializable]
    public class TurretVariantDef
    {
        public TurretVariant variant;
        public string displayName;
        public int damage;
        public float interval;             // Seconds between shots
        public int costGold;
        public int costIron;

        public static TurretVariantDef Create(TurretVariant variant, string name, int dmg, float interval, int gold, int iron) =>
            new TurretVariantDef { variant = variant, displayName = name, damage = dmg, interval = interval, costGold = gold, costIron = iron };
    }

    /// <summary>Preset turret variants.</summary>
    public static class TurretVariantPresets
    {
        public static TurretVariantDef Arrow => TurretVariantDef.Create(TurretVariant.Arrow, "箭塔", 20, 1f, 100, 10);
        public static TurretVariantDef Fire => TurretVariantDef.Create(TurretVariant.Fire, "火焰塔", 30, 1.5f, 150, 15);
        public static TurretVariantDef Ice => TurretVariantDef.Create(TurretVariant.Ice, "冰冻塔", 15, 2f, 120, 12);

        public static TurretVariantDef[] All => new[] { Arrow, Fire, Ice };
    }
}
