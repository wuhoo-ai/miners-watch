using System;
using UnityEngine;

namespace MinersWatch
{
    public enum EnemyType { Rockworm, Shadow, Lavabeast, Guardian }
    public enum EnemyState { Spawning, Moving, Attacking, Dead }

    /// <summary>Runtime enemy configuration. Not ScriptableObject for EditMode friendliness.</summary>
    [Serializable]
    public class EnemyDef
    {
        public EnemyType type;
        public int hp;
        public int damage;
        public float speed;
        public DepthLevel[] depthLevels;

        public static EnemyDef Create(EnemyType type, int hp, int dmg, float speed, DepthLevel[] levels) =>
            new EnemyDef { type = type, hp = hp, damage = dmg, speed = speed, depthLevels = levels };
    }

    public static class EnemyPresets
    {
        public static EnemyDef Rockworm => EnemyDef.Create(EnemyType.Rockworm, 30, 10, 2f,
            new[] { DepthLevel.Shallow });
        public static EnemyDef Shadow => EnemyDef.Create(EnemyType.Shadow, 80, 20, 3.5f,
            new[] { DepthLevel.Medium });
        public static EnemyDef Lavabeast => EnemyDef.Create(EnemyType.Lavabeast, 200, 35, 5f,
            new[] { DepthLevel.Deep });
        public static EnemyDef Guardian => EnemyDef.Create(EnemyType.Guardian, 500, 50, 3f,
            new[] { DepthLevel.Deep });
    }
}
