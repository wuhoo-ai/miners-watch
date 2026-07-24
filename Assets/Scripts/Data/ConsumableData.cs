using System;

namespace MinersWatch
{
    /// <summary>
    /// Consumable item types.
    /// </summary>
    public enum ConsumableType
    {
        Torch,       // 火把：照明 + 驱散 Shadow
        Bomb,        // 炸药：破坏岩壁/大量采矿
        HealPotion,  // 治疗药水：恢复 HP
        SpeedScroll  // 加速卷轴：提升移速/攻速
    }

    /// <summary>
    /// Consumable item definition. Runtime data class.
    /// </summary>
    [Serializable]
    public class ConsumableDef
    {
        public ConsumableType type;
        public string displayName;
        public MineralType material;        // Crafting material
        public int materialCount;           // Material quantity
        public float duration;              // Effect duration (seconds)
        public float effectValue;           // Effect magnitude (HP restored, speed boost, etc.)
        public string description;          // UI description

        public static ConsumableDef Create(
            ConsumableType type, string name,
            MineralType mat, int matCount,
            float duration, float value, string desc)
        {
            return new ConsumableDef
            {
                type = type,
                displayName = name,
                material = mat,
                materialCount = matCount,
                duration = duration,
                effectValue = value,
                description = desc
            };
        }

        public bool IsValid()
        {
            return materialCount > 0 && effectValue > 0f;
        }
    }

    /// <summary>Preset consumables.</summary>
    public static class ConsumablePresets
    {
        public static ConsumableDef Torch => ConsumableDef.Create(
            ConsumableType.Torch, "火把",
            MineralType.Iron, 1,
            duration: 30f,
            value: 5f, // Light radius
            desc: "照明30秒，驱散Shadow"
        );

        public static ConsumableDef Bomb => ConsumableDef.Create(
            ConsumableType.Bomb, "炸药",
            MineralType.Iron, 2,
            duration: 0f, // Instant
            value: 100f, // Mining yield multiplier
            desc: "瞬间破坏岩壁，获得100单位矿物"
        );

        public static ConsumableDef HealPotion => ConsumableDef.Create(
            ConsumableType.HealPotion, "治疗药水",
            MineralType.Crystal, 1,
            duration: 0f, // Instant
            value: 50f, // HP restored
            desc: "恢复50点HP"
        );

        public static ConsumableDef SpeedScroll => ConsumableDef.Create(
            ConsumableType.SpeedScroll, "加速卷轴",
            MineralType.Gold, 2,
            duration: 10f,
            value: 1.5f, // Speed multiplier (150%)
            desc: "移速+50%，持续10秒"
        );

        public static ConsumableDef[] All => new[]
        {
            Torch, Bomb, HealPotion, SpeedScroll
        };
    }
}
