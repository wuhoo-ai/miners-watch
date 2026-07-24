using NUnit.Framework;
using UnityEngine;

namespace MinersWatch.Tests
{
    /// <summary>
    /// EditMode tests for ConsumableSystem.
    /// Uses explicit Init() — does NOT rely on Awake().
    /// </summary>
    public class ConsumableSystemTests
    {
        private InventorySystem _inventory;
        private ConsumableSystem _consumable;

        [SetUp]
        public void SetUp()
        {
            var invGo = new GameObject("TestInventory");
            _inventory = invGo.AddComponent<InventorySystem>();
            _inventory.Init();
            _inventory.SetCapacity(10);

            var conGo = new GameObject("TestConsumable");
            _consumable = conGo.AddComponent<ConsumableSystem>();
            _consumable.Init(_inventory);
        }

        [TearDown]
        public void TearDown()
        {
            if (_inventory != null) Object.DestroyImmediate(_inventory.gameObject);
            if (_consumable != null) Object.DestroyImmediate(_consumable.gameObject);
        }

        [Test]
        public void CanUse_WithSufficientMaterials_ReturnsTrue()
        {
            // Torch needs Iron×1
            _inventory.AddItem(MineralType.Iron, 15f, 3);
            Assert.IsTrue(_consumable.CanUse(ConsumableType.Torch));
        }

        [Test]
        public void CanUse_WithInsufficientMaterials_ReturnsFalse()
        {
            // Torch needs Iron×1, inventory empty
            Assert.IsFalse(_consumable.CanUse(ConsumableType.Torch));
        }

        [Test]
        public void Use_DeductsMaterials()
        {
            // Bomb needs Iron×2
            _inventory.AddItem(MineralType.Iron, 15f, 5);
            bool result = _consumable.Use(ConsumableType.Bomb);
            Assert.IsTrue(result);
            Assert.AreEqual(3, _inventory.GetCount(MineralType.Iron));
        }

        [Test]
        public void Use_WithInsufficientMaterials_ReturnsFalse()
        {
            _inventory.AddItem(MineralType.Iron, 15f, 1); // Bomb needs 2
            bool result = _consumable.Use(ConsumableType.Bomb);
            Assert.IsFalse(result);
            Assert.AreEqual(1, _inventory.GetCount(MineralType.Iron)); // Unchanged
        }

        [Test]
        public void Use_TimedEffect_ActivatesWithCorrectDuration()
        {
            _inventory.AddItem(MineralType.Iron, 15f, 3);
            _consumable.Use(ConsumableType.Torch); // 30s duration

            Assert.IsTrue(_consumable.IsEffectActive(ConsumableType.Torch));
            Assert.AreEqual(30f, _consumable.GetEffectDuration(ConsumableType.Torch), 0.01f);
        }

        [Test]
        public void Tick_DecreasesDuration()
        {
            _inventory.AddItem(MineralType.Iron, 15f, 3);
            _consumable.Use(ConsumableType.Torch);

            _consumable.Tick(10f); // 30s - 10s = 20s remaining
            Assert.AreEqual(20f, _consumable.GetEffectDuration(ConsumableType.Torch), 0.01f);
            Assert.IsTrue(_consumable.IsEffectActive(ConsumableType.Torch));
        }

        [Test]
        public void Tick_ExpiresWhenDurationReachesZero()
        {
            _inventory.AddItem(MineralType.Iron, 15f, 3);
            _consumable.Use(ConsumableType.Torch);

            _consumable.Tick(30f); // 30s - 30s = 0s, should expire
            Assert.IsFalse(_consumable.IsEffectActive(ConsumableType.Torch));
        }

        [Test]
        public void Use_InstantEffect_DoesNotCreateTimedEffect()
        {
            _inventory.AddItem(MineralType.Crystal, 100f, 3);
            bool instantFired = false;
            float instantValue = 0f;
            _consumable.OnInstantEffect += (type, value) =>
            {
                instantFired = true;
                instantValue = value;
            };

            bool result = _consumable.Use(ConsumableType.HealPotion); // Instant

            Assert.IsTrue(result);
            Assert.IsTrue(instantFired);
            Assert.AreEqual(50f, instantValue, 0.01f); // HealPotion effectValue = 50
            Assert.IsFalse(_consumable.IsEffectActive(ConsumableType.HealPotion));
        }

        [Test]
        public void Use_ReplacesExistingTimer()
        {
            _inventory.AddItem(MineralType.Iron, 15f, 5);
            _consumable.Use(ConsumableType.Torch); // First use: 30s
            _consumable.Tick(20f);                  // 10s remaining

            _consumable.Use(ConsumableType.Torch); // Re-use: resets to 30s
            Assert.AreEqual(30f, _consumable.GetEffectDuration(ConsumableType.Torch), 0.01f);
        }

        [Test]
        public void ClearAllEffects_RemovesAllTimers()
        {
            _inventory.AddItem(MineralType.Iron, 15f, 5);
            _inventory.AddItem(MineralType.Gold, 40f, 5);
            _consumable.Use(ConsumableType.Torch);       // 30s
            _consumable.Use(ConsumableType.SpeedScroll); // 10s

            _consumable.ClearAllEffects();

            Assert.IsFalse(_consumable.IsEffectActive(ConsumableType.Torch));
            Assert.IsFalse(_consumable.IsEffectActive(ConsumableType.SpeedScroll));
        }
    }
}
