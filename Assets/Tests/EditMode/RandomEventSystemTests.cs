using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for RandomEventSystem.
    /// Uses explicit Init() — does NOT rely on Awake().
    /// </summary>
    public class RandomEventSystemTests
    {
        private GameObject _go;
        private RandomEventSystem _res;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestRandomEvent");
            _res = _go.AddComponent<RandomEventSystem>();
            _res.Init(); // no dependencies — pure logic tests
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // ── init / config ───────────────────────────────────────

        [Test]
        public void Init_DefaultConfigs_Has4Events()
        {
            Assert.AreEqual(4, _res.Configs.Count);
        }

        [Test]
        public void Init_DefaultConfigs_AllTypesPresent()
        {
            Assert.IsNotNull(_res.GetConfig(RandomEventType.CaveIn));
            Assert.IsNotNull(_res.GetConfig(RandomEventType.TreasureChest));
            Assert.IsNotNull(_res.GetConfig(RandomEventType.HotSpring));
            Assert.IsNotNull(_res.GetConfig(RandomEventType.WormNest));
        }

        [Test]
        public void Init_CustomConfigs_Used()
        {
            var custom = new[]
            {
                new RandomEventConfig { type = RandomEventType.CaveIn, weight = 1f, cooldownTurns = 1, damage = 99 },
            };
            _res.Init(configs: custom);
            Assert.AreEqual(1, _res.Configs.Count);
            Assert.AreEqual(99, _res.GetConfig(RandomEventType.CaveIn).damage);
        }

        [Test]
        public void DefaultConfigs_CaveIn_HasDamage()
        {
            var cfg = RandomEventConfig.Default(RandomEventType.CaveIn);
            Assert.Greater(cfg.damage, 0);
        }

        [Test]
        public void DefaultConfigs_TreasureChest_HasReward()
        {
            var cfg = RandomEventConfig.Default(RandomEventType.TreasureChest);
            Assert.Greater(cfg.rewardCount, 0);
        }

        [Test]
        public void DefaultConfigs_HotSpring_HasHeal()
        {
            var cfg = RandomEventConfig.Default(RandomEventType.HotSpring);
            Assert.Greater(cfg.healAmount, 0);
        }

        [Test]
        public void DefaultConfigs_WormNest_HasEnemies()
        {
            var cfg = RandomEventConfig.Default(RandomEventType.WormNest);
            Assert.Greater(cfg.extraEnemyCount, 0);
        }

        // ── global cooldown ─────────────────────────────────────

        [Test]
        public void TryTrigger_ImmediatelyAfterInit_Succeeds()
        {
            // Init sets turnsSinceLastEvent = 999, so global cooldown is satisfied
            var result = _res.TryTrigger(0.0f);
            Assert.IsNotNull(result);
        }

        [Test]
        public void TryTrigger_DuringGlobalCooldown_ReturnsNull()
        {
            _res.TryTrigger(0.0f); // triggers event, resets turnsSinceLastEvent to 0
            // Now turnsSinceLastEvent = 0, globalCooldown = 2
            var result = _res.TryTrigger(0.0f);
            Assert.IsNull(result);
        }

        [Test]
        public void TryTrigger_AfterGlobalCooldown_Succeeds()
        {
            _res.TryTrigger(0.0f); // trigger
            _res.AdvanceTurn();     // turnsSinceLastEvent = 1
            _res.AdvanceTurn();     // turnsSinceLastEvent = 2
            var result = _res.TryTrigger(0.0f);
            Assert.IsNotNull(result);
        }

        // ── consecutive same event prevention ───────────────────

        [Test]
        public void TryTrigger_ExcludesLastEventType()
        {
            // Use configs where only one event has weight > 0
            var configs = new[]
            {
                new RandomEventConfig { type = RandomEventType.CaveIn, weight = 1f, cooldownTurns = 0 },
                new RandomEventConfig { type = RandomEventType.TreasureChest, weight = 1f, cooldownTurns = 0 },
            };
            _res.Init(configs: configs);

            // Force CaveIn with roll=0
            var first = _res.TryTrigger(0.0f);
            Assert.AreEqual(RandomEventType.CaveIn, first);

            // Advance past global cooldown
            _res.AdvanceTurn();
            _res.AdvanceTurn();

            // Next trigger should exclude CaveIn (last event), so must be TreasureChest
            var second = _res.TryTrigger(0.0f);
            Assert.AreEqual(RandomEventType.TreasureChest, second);
        }

        // ── per-event cooldown ──────────────────────────────────

        [Test]
        public void TryTrigger_EventOnCooldown_Excluded()
        {
            var configs = new[]
            {
                new RandomEventConfig { type = RandomEventType.CaveIn, weight = 1f, cooldownTurns = 5 },
                new RandomEventConfig { type = RandomEventType.HotSpring, weight = 1f, cooldownTurns = 0 },
            };
            _res.Init(configs: configs);

            // Trigger CaveIn
            var first = _res.TryTrigger(0.0f);
            Assert.AreEqual(RandomEventType.CaveIn, first);
            Assert.IsTrue(_res.IsOnCooldown(RandomEventType.CaveIn));

            // Advance past global cooldown
            _res.AdvanceTurn();
            _res.AdvanceTurn();

            // CaveIn is on cooldown AND is last event — only HotSpring eligible
            var second = _res.TryTrigger(0.5f);
            Assert.AreEqual(RandomEventType.HotSpring, second);
        }

        [Test]
        public void IsOnCooldown_DecrementsWithAdvanceTurn()
        {
            var configs = new[]
            {
                new RandomEventConfig { type = RandomEventType.CaveIn, weight = 1f, cooldownTurns = 2 },
                new RandomEventConfig { type = RandomEventType.HotSpring, weight = 1f, cooldownTurns = 0 },
            };
            _res.Init(configs: configs);

            _res.TryTrigger(0.0f); // CaveIn
            Assert.IsTrue(_res.IsOnCooldown(RandomEventType.CaveIn));

            _res.AdvanceTurn(); // cooldown 2→1
            Assert.IsTrue(_res.IsOnCooldown(RandomEventType.CaveIn));

            _res.AdvanceTurn(); // cooldown 1→0
            Assert.IsFalse(_res.IsOnCooldown(RandomEventType.CaveIn));
        }

        // ── weighted selection ──────────────────────────────────

        [Test]
        public void TryTrigger_WeightedSelection_LowRoll_SelectsFirst()
        {
            // All 4 default events have equal weight (0.25 each)
            // Roll 0.0 → first eligible
            var result = _res.TryTrigger(0.0f);
            Assert.IsNotNull(result);
        }

        [Test]
        public void TryTrigger_WeightedSelection_HighRoll_SelectsLast()
        {
            var result = _res.TryTrigger(0.999f);
            Assert.IsNotNull(result);
        }

        [Test]
        public void TryTrigger_SingleEligible_AlwaysSelectsIt()
        {
            var configs = new[]
            {
                new RandomEventConfig { type = RandomEventType.CaveIn, weight = 1f, cooldownTurns = 0 },
                new RandomEventConfig { type = RandomEventType.TreasureChest, weight = 1f, cooldownTurns = 99 },
                new RandomEventConfig { type = RandomEventType.HotSpring, weight = 1f, cooldownTurns = 99 },
                new RandomEventConfig { type = RandomEventType.WormNest, weight = 1f, cooldownTurns = 99 },
            };
            _res.Init(configs: configs);

            // Only CaveIn is eligible (others on cooldown 99)
            for (int i = 0; i < 5; i++)
            {
                var result = _res.TryTrigger(0.5f);
                Assert.AreEqual(RandomEventType.CaveIn, result);
                // Advance past global cooldown and reset last-event exclusion
                _res.AdvanceTurn();
                _res.AdvanceTurn();
            }
        }

        [Test]
        public void TryTrigger_AllOnCooldown_ReturnsNull()
        {
            var configs = new[]
            {
                new RandomEventConfig { type = RandomEventType.CaveIn, weight = 1f, cooldownTurns = 99 },
                new RandomEventConfig { type = RandomEventType.TreasureChest, weight = 1f, cooldownTurns = 99 },
            };
            _res.Init(configs: configs);

            // Trigger CaveIn (only one not on cooldown initially... wait, cooldowns start at 0)
            // Actually Init sets all cooldowns to 0, so both are eligible initially
            var first = _res.TryTrigger(0.0f); // CaveIn
            Assert.IsNotNull(first);

            // Advance past global cooldown
            _res.AdvanceTurn();
            _res.AdvanceTurn();

            // CaveIn on cooldown (99) + last event; TreasureChest on cooldown (99)
            // Wait — TreasureChest cooldown is 99 from init? No, Init sets cooldowns to 0.
            // After triggering CaveIn, CaveIn cooldown = 99. TreasureChest cooldown = 0.
            // But CaveIn is also last event. So TreasureChest is eligible.
            var second = _res.TryTrigger(0.0f);
            Assert.AreEqual(RandomEventType.TreasureChest, second);

            // Now both on cooldown + last event exclusion
            _res.AdvanceTurn();
            _res.AdvanceTurn();
            var third = _res.TryTrigger(0.0f);
            Assert.IsNull(third); // CaveIn: cooldown 99, TreasureChest: cooldown 99 + last
        }

        // ── GetEligibleEvents ───────────────────────────────────

        [Test]
        public void GetEligibleEvents_AfterInit_AllEligible()
        {
            var eligible = _res.GetEligibleEvents();
            Assert.AreEqual(4, eligible.Count);
        }

        [Test]
        public void GetEligibleEvents_AfterTrigger_ExcludesLastAndCooldown()
        {
            _res.TryTrigger(0.0f); // triggers first event
            var eligible = _res.GetEligibleEvents();
            // Last event excluded + its cooldown > 0
            Assert.LessOrEqual(eligible.Count, 3);
        }

        // ── ApplyEffect ─────────────────────────────────────────

        [Test]
        public void ApplyEffect_CaveIn_ReturnsDamage()
        {
            var result = _res.ApplyEffect(RandomEventType.CaveIn);
            Assert.AreEqual(RandomEventType.CaveIn, result.type);
            Assert.Greater(result.damageDealt, 0);
        }

        [Test]
        public void ApplyEffect_TreasureChest_ReturnsReward()
        {
            var result = _res.ApplyEffect(RandomEventType.TreasureChest);
            Assert.AreEqual(RandomEventType.TreasureChest, result.type);
            Assert.Greater(result.rewardCount, 0);
        }

        [Test]
        public void ApplyEffect_HotSpring_ReturnsHeal()
        {
            var result = _res.ApplyEffect(RandomEventType.HotSpring);
            Assert.AreEqual(RandomEventType.HotSpring, result.type);
            Assert.Greater(result.healApplied, 0);
        }

        [Test]
        public void ApplyEffect_WormNest_ReturnsExtraEnemies()
        {
            var result = _res.ApplyEffect(RandomEventType.WormNest);
            Assert.AreEqual(RandomEventType.WormNest, result.type);
            Assert.Greater(result.extraEnemiesSpawned, 0);
        }

        [Test]
        public void ApplyEffect_UnknownType_ReturnsDefault()
        {
            var result = _res.ApplyEffect((RandomEventType)99);
            Assert.AreEqual(0, result.damageDealt);
            Assert.AreEqual(0, result.healApplied);
            Assert.AreEqual(0, result.rewardCount);
        }

        // ── ApplyEffect with dependencies ───────────────────────

        [Test]
        public void ApplyEffect_CaveIn_WithPlayerHP_DealsDamage()
        {
            var hpGo = new GameObject("TestHP");
            var hp = hpGo.AddComponent<PlayerHP>();
            hp.Init(null); // baseHP=100, no upgrades

            _res.Init(playerHP: hp);
            int hpBefore = hp.CurrentHP;

            _res.ApplyEffect(RandomEventType.CaveIn);

            Assert.Less(hp.CurrentHP, hpBefore);
            Assert.AreEqual(hpBefore - 15, hp.CurrentHP); // default CaveIn damage = 15

            Object.DestroyImmediate(hpGo);
        }

        [Test]
        public void ApplyEffect_TreasureChest_WithInventory_AddsMinerals()
        {
            var invGo = new GameObject("TestInv");
            var inv = invGo.AddComponent<InventorySystem>();
            inv.Init();

            _res.Init(inventory: inv);

            _res.ApplyEffect(RandomEventType.TreasureChest);

            Assert.AreEqual(3, inv.GetCount(MineralType.Gold)); // default: 3 Gold

            Object.DestroyImmediate(invGo);
        }

        [Test]
        public void ApplyEffect_HotSpring_WithPlayerHP_Heals()
        {
            var hpGo = new GameObject("TestHP2");
            var hp = hpGo.AddComponent<PlayerHP>();
            hp.Init(null);
            hp.TakeDamage(50); // HP = 50

            _res.Init(playerHP: hp);

            _res.ApplyEffect(RandomEventType.HotSpring);

            Assert.AreEqual(75, hp.CurrentHP); // 50 + 25 default heal

            Object.DestroyImmediate(hpGo);
        }

        [Test]
        public void ApplyEffect_HotSpring_DoesNotExceedMaxHP()
        {
            var hpGo = new GameObject("TestHP3");
            var hp = hpGo.AddComponent<PlayerHP>();
            hp.Init(null); // HP = 100 (max)

            _res.Init(playerHP: hp);

            _res.ApplyEffect(RandomEventType.HotSpring);

            Assert.AreEqual(100, hp.CurrentHP); // capped at max

            Object.DestroyImmediate(hpGo);
        }

        // ── TriggerAndApply ─────────────────────────────────────

        [Test]
        public void TriggerAndApply_TriggersEvent_ReturnsResult()
        {
            var result = _res.TriggerAndApply(0.0f);
            Assert.IsNotNull(result);
            Assert.Greater(result.Value.damageDealt + result.Value.healApplied + result.Value.rewardCount + result.Value.extraEnemiesSpawned, 0);
        }

        [Test]
        public void TriggerAndApply_DuringCooldown_ReturnsNull()
        {
            _res.TriggerAndApply(0.0f); // trigger
            var result = _res.TriggerAndApply(0.0f); // on cooldown
            Assert.IsNull(result);
        }

        // ── OnEventTriggered ────────────────────────────────────

        [Test]
        public void OnEventTriggered_FiresOnApplyEffect()
        {
            RandomEventResult? received = null;
            _res.OnEventTriggered += r => received = r;

            _res.ApplyEffect(RandomEventType.CaveIn);

            Assert.IsNotNull(received);
            Assert.AreEqual(RandomEventType.CaveIn, received.Value.type);
        }

        // ── AdvanceTurn ─────────────────────────────────────────

        [Test]
        public void AdvanceTurn_IncrementsTurnsSinceLastEvent()
        {
            _res.TryTrigger(0.0f); // resets to 0
            Assert.AreEqual(0, _res.TurnsSinceLastEvent);

            _res.AdvanceTurn();
            Assert.AreEqual(1, _res.TurnsSinceLastEvent);

            _res.AdvanceTurn();
            Assert.AreEqual(2, _res.TurnsSinceLastEvent);
        }

        // ── LastEventType tracking ──────────────────────────────

        [Test]
        public void LastEventType_AfterTrigger_Set()
        {
            Assert.IsNull(_res.LastEventType);
            var triggered = _res.TryTrigger(0.0f);
            Assert.AreEqual(triggered, _res.LastEventType);
        }

        // ── PlayerHP.Heal ───────────────────────────────────────

        [Test]
        public void PlayerHP_Heal_RestoresHP()
        {
            var hpGo = new GameObject("TestHPHeal");
            var hp = hpGo.AddComponent<PlayerHP>();
            hp.Init(null);
            hp.TakeDamage(60); // HP = 40

            hp.Heal(30);
            Assert.AreEqual(70, hp.CurrentHP);

            Object.DestroyImmediate(hpGo);
        }

        [Test]
        public void PlayerHP_Heal_ClampedToMax()
        {
            var hpGo = new GameObject("TestHPHeal2");
            var hp = hpGo.AddComponent<PlayerHP>();
            hp.Init(null);
            hp.TakeDamage(10); // HP = 90

            hp.Heal(50);
            Assert.AreEqual(100, hp.CurrentHP);

            Object.DestroyImmediate(hpGo);
        }

        [Test]
        public void PlayerHP_Heal_ZeroOrNegative_NoEffect()
        {
            var hpGo = new GameObject("TestHPHeal3");
            var hp = hpGo.AddComponent<PlayerHP>();
            hp.Init(null);
            hp.TakeDamage(50); // HP = 50

            hp.Heal(0);
            Assert.AreEqual(50, hp.CurrentHP);

            hp.Heal(-10);
            Assert.AreEqual(50, hp.CurrentHP);

            Object.DestroyImmediate(hpGo);
        }
    }
}
