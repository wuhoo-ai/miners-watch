using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MinersWatch
{
    /// <summary>Cave random event types.</summary>
    public enum RandomEventType
    {
        CaveIn,         // 塌方 — falling rock damage
        TreasureChest,  // 宝箱 — free minerals
        HotSpring,      // 温泉 — heal
        WormNest        // 虫巢 — extra enemies
    }

    /// <summary>Configuration for a single random event type.</summary>
    [Serializable]
    public class RandomEventConfig
    {
        public RandomEventType type;
        [Range(0f, 1f)] public float weight;       // relative probability weight
        public int cooldownTurns;                    // min turns before this event can repeat
        public int damage;                           // CaveIn: falling rock damage
        public MineralType rewardMineral;            // TreasureChest: mineral type
        public int rewardCount;                      // TreasureChest: mineral count
        public int healAmount;                       // HotSpring: HP restored
        public int extraEnemyCount;                  // WormNest: number of extra enemies
        public EnemyType extraEnemyType;             // WormNest: enemy type to spawn

        public static RandomEventConfig Default(RandomEventType type) => type switch
        {
            RandomEventType.CaveIn => new RandomEventConfig
            {
                type = RandomEventType.CaveIn,
                weight = 0.25f,
                cooldownTurns = 3,
                damage = 15,
            },
            RandomEventType.TreasureChest => new RandomEventConfig
            {
                type = RandomEventType.TreasureChest,
                weight = 0.25f,
                cooldownTurns = 4,
                rewardMineral = MineralType.Gold,
                rewardCount = 3,
            },
            RandomEventType.HotSpring => new RandomEventConfig
            {
                type = RandomEventType.HotSpring,
                weight = 0.25f,
                cooldownTurns = 3,
                healAmount = 25,
            },
            RandomEventType.WormNest => new RandomEventConfig
            {
                type = RandomEventType.WormNest,
                weight = 0.25f,
                cooldownTurns = 5,
                extraEnemyCount = 3,
                extraEnemyType = EnemyType.Rockworm,
            },
            _ => null,
        };
    }

    /// <summary>Result of applying a random event effect.</summary>
    public struct RandomEventResult
    {
        public RandomEventType type;
        public int damageDealt;
        public int healApplied;
        public MineralType rewardMineral;
        public int rewardCount;
        public int extraEnemiesSpawned;
        public EnemyType extraEnemyType;
    }

    /// <summary>
    /// Random event system for cave exploration.
    /// Probability-weighted event selection with per-event cooldown and
    /// consecutive-same-event prevention.
    /// Core logic is pure C# callable from EditMode tests via Init().
    /// </summary>
    public class RandomEventSystem : MonoBehaviour
    {
        [Header("Global Config")]
        [SerializeField] private int _globalCooldownTurns = 2; // min turns between ANY events

        private RandomEventConfig[] _configs;
        private int _turnsSinceLastEvent;
        private readonly Dictionary<RandomEventType, int> _cooldownsRemaining = new Dictionary<RandomEventType, int>();
        private RandomEventType? _lastEventType;

        // Dependencies for effect application (nullable — effects skipped if absent)
        private PlayerHP _playerHP;
        private InventorySystem _inventory;

        /// <summary>Fires when a random event triggers. UI/VFX subscribes.</summary>
        public event Action<RandomEventResult> OnEventTriggered;

        public int TurnsSinceLastEvent => _turnsSinceLastEvent;
        public RandomEventType? LastEventType => _lastEventType;
        public IReadOnlyList<RandomEventConfig> Configs => _configs;

        // ── lifecycle ───────────────────────────────────────────

        /// <summary>Explicit init for EditMode tests where Awake may not fire.</summary>
        public void Init(PlayerHP playerHP = null, InventorySystem inventory = null, RandomEventConfig[] configs = null)
        {
            _playerHP = playerHP;
            _inventory = inventory;
            _configs = configs ?? DefaultConfigs();
            _turnsSinceLastEvent = 999; // allow immediate first trigger
            _lastEventType = null;
            _cooldownsRemaining.Clear();
            foreach (var cfg in _configs)
                _cooldownsRemaining[cfg.type] = 0;
            if (_globalCooldownTurns <= 0) _globalCooldownTurns = 2;
        }

        private void Awake() => Init();

        private static RandomEventConfig[] DefaultConfigs() => new[]
        {
            RandomEventConfig.Default(RandomEventType.CaveIn),
            RandomEventConfig.Default(RandomEventType.TreasureChest),
            RandomEventConfig.Default(RandomEventType.HotSpring),
            RandomEventConfig.Default(RandomEventType.WormNest),
        };

        // ── turn management ─────────────────────────────────────

        /// <summary>Advance one exploration turn. Decrements cooldowns.</summary>
        public void AdvanceTurn()
        {
            _turnsSinceLastEvent++;
            var keys = new List<RandomEventType>(_cooldownsRemaining.Keys);
            foreach (var key in keys)
            {
                if (_cooldownsRemaining[key] > 0)
                    _cooldownsRemaining[key]--;
            }
        }

        // ── event triggering ────────────────────────────────────

        /// <summary>
        /// Try to trigger a random event using a deterministic random value [0,1).
        /// Returns null if on global cooldown or no eligible events.
        /// Testable entry point — pass Random.value at runtime.
        /// </summary>
        public RandomEventType? TryTrigger(float randomValue)
        {
            if (_turnsSinceLastEvent < _globalCooldownTurns)
                return null;

            // Build eligible pool: exclude events on cooldown and the last triggered event
            var eligible = new List<RandomEventConfig>();
            float totalWeight = 0f;

            foreach (var cfg in _configs)
            {
                if (cfg == null) continue;
                if (_cooldownsRemaining.TryGetValue(cfg.type, out int cd) && cd > 0) continue;
                if (_lastEventType.HasValue && cfg.type == _lastEventType.Value) continue;

                eligible.Add(cfg);
                totalWeight += cfg.weight;
            }

            if (eligible.Count == 0 || totalWeight <= 0f)
                return null;

            // Weighted selection
            float roll = Mathf.Clamp01(randomValue) * totalWeight;
            float cumulative = 0f;
            RandomEventConfig selected = eligible[eligible.Count - 1]; // fallback to last

            foreach (var cfg in eligible)
            {
                cumulative += cfg.weight;
                if (roll < cumulative)
                {
                    selected = cfg;
                    break;
                }
            }

            // Update state
            _lastEventType = selected.type;
            _turnsSinceLastEvent = 0;
            _cooldownsRemaining[selected.type] = selected.cooldownTurns;

            return selected.type;
        }

        /// <summary>Runtime convenience: uses UnityEngine.Random.</summary>
        public RandomEventType? TryTriggerRandom() => TryTrigger(Random.value);

        // ── effect application ──────────────────────────────────

        /// <summary>
        /// Apply the effect for a given event type.
        /// Returns a result struct describing what happened.
        /// Safe to call without dependencies — missing refs are skipped.
        /// </summary>
        public RandomEventResult ApplyEffect(RandomEventType type)
        {
            var cfg = GetConfig(type);
            var result = new RandomEventResult { type = type };

            if (cfg == null) return result;

            switch (type)
            {
                case RandomEventType.CaveIn:
                    result.damageDealt = cfg.damage;
                    if (_playerHP != null)
                        _playerHP.TakeDamage(cfg.damage);
                    break;

                case RandomEventType.TreasureChest:
                    result.rewardMineral = cfg.rewardMineral;
                    result.rewardCount = cfg.rewardCount;
                    if (_inventory != null)
                        _inventory.AddItem(cfg.rewardMineral, GetSellPrice(cfg.rewardMineral), cfg.rewardCount);
                    break;

                case RandomEventType.HotSpring:
                    result.healApplied = cfg.healAmount;
                    if (_playerHP != null)
                        HealPlayer(cfg.healAmount);
                    break;

                case RandomEventType.WormNest:
                    result.extraEnemiesSpawned = cfg.extraEnemyCount;
                    result.extraEnemyType = cfg.extraEnemyType;
                    // Actual spawning handled by caller (EnemySpawner) at runtime
                    break;
            }

            OnEventTriggered?.Invoke(result);
            return result;
        }

        /// <summary>Trigger + apply in one call. Returns null if no event triggered.</summary>
        public RandomEventResult? TriggerAndApply(float randomValue)
        {
            var type = TryTrigger(randomValue);
            if (!type.HasValue) return null;
            return ApplyEffect(type.Value);
        }

        // ── query ───────────────────────────────────────────────

        /// <summary>Get config for a specific event type.</summary>
        public RandomEventConfig GetConfig(RandomEventType type)
        {
            if (_configs == null) return null;
            foreach (var cfg in _configs)
            {
                if (cfg != null && cfg.type == type)
                    return cfg;
            }
            return null;
        }

        /// <summary>Check if a specific event type is currently on cooldown.</summary>
        public bool IsOnCooldown(RandomEventType type)
        {
            return _cooldownsRemaining.TryGetValue(type, out int cd) && cd > 0;
        }

        /// <summary>Get list of currently eligible event types (not on cooldown, not last).</summary>
        public List<RandomEventType> GetEligibleEvents()
        {
            var eligible = new List<RandomEventType>();
            if (_configs == null) return eligible;

            foreach (var cfg in _configs)
            {
                if (cfg == null) continue;
                if (IsOnCooldown(cfg.type)) continue;
                if (_lastEventType.HasValue && cfg.type == _lastEventType.Value) continue;
                eligible.Add(cfg.type);
            }
            return eligible;
        }

        // ── helpers ─────────────────────────────────────────────

        private void HealPlayer(int amount)
        {
            if (_playerHP == null || amount <= 0) return;
            _playerHP.Heal(amount);
        }

        private static float GetSellPrice(MineralType type) => type switch
        {
            MineralType.Iron => 15f,
            MineralType.Gold => 40f,
            MineralType.Crystal => 100f,
            MineralType.Obsidian => 300f,
            _ => 5f,
        };
    }
}
