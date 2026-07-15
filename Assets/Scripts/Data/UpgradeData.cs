using System;
using UnityEngine;

namespace MinersWatch
{
    public enum UpgradeType { Pickaxe, Armor, Backpack }
    public enum DefenseType { Wall, SpikeTrap, Turret }

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
}
