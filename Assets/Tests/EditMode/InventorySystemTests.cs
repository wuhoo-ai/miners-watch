using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    public class InventorySystemTests
    {
        private GameObject _go;
        private InventorySystem _inv;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("TestInventory");
            _inv = _go.AddComponent<InventorySystem>();
            _inv.Init();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void CanCreateComponent()
        {
            Assert.AreEqual(10, _inv.Capacity);
            Assert.AreEqual(5, _inv.MaxStackSize);
            Assert.AreEqual(0, _inv.UsedSlots);
        }

        [Test]
        public void AddItem_IncreasesCount()
        {
            bool result = _inv.AddItem(MineralType.Iron, 15f, 1);
            Assert.IsTrue(result);
            Assert.AreEqual(1, _inv.GetCount(MineralType.Iron));
            Assert.AreEqual(1, _inv.UsedSlots);
        }

        [Test]
        public void AddItem_StacksOnExistingSlot()
        {
            _inv.AddItem(MineralType.Iron, 15f, 3);
            _inv.AddItem(MineralType.Iron, 15f, 2);
            // 3+2 = 5, should be one stack at max
            Assert.AreEqual(1, _inv.UsedSlots);
            Assert.AreEqual(5, _inv.GetCount(MineralType.Iron));
        }

        [Test]
        public void AddItem_6Iron_CreatesTwoSlots()
        {
            _inv.AddItem(MineralType.Iron, 15f, 6);
            // First slot: 5, Second slot: 1
            Assert.AreEqual(2, _inv.UsedSlots);
            Assert.AreEqual(6, _inv.GetCount(MineralType.Iron));
        }

        [Test]
        public void AddItem_FailsWhenFull()
        {
            // Fill 10 slots with different mineral types
            var types = new[] {
                MineralType.Stone, MineralType.Iron, MineralType.Gold,
                MineralType.Crystal, MineralType.Obsidian,
                // Reuse same types — each gets new slot since different stacks
            };
            // Add 10 unique slots by using Stone x1, Iron x1, etc repeatedly
            for (int i = 0; i < 10; i++)
            {
                var type = (MineralType)(i % 5);
                bool added = _inv.AddItem(type, 5f, 5); // 5 each = full stacks, new slots
                if (!added) break;
            }
            // Now inventory should be full (10 slots, all at max stack 5)
            Assert.AreEqual(10, _inv.UsedSlots);
            // Try adding another item — should fail
            bool extraResult = _inv.AddItem(MineralType.Stone, 5f, 1);
            Assert.IsFalse(extraResult);
        }

        [Test]
        public void RemoveItem_DecreasesCount()
        {
            _inv.AddItem(MineralType.Gold, 40f, 3);
            bool result = _inv.RemoveItem(MineralType.Gold, 1);
            Assert.IsTrue(result);
            Assert.AreEqual(2, _inv.GetCount(MineralType.Gold));
            Assert.AreEqual(1, _inv.UsedSlots);
        }

        [Test]
        public void RemoveItem_FailsWhenInsufficient()
        {
            _inv.AddItem(MineralType.Gold, 40f, 2);
            bool result = _inv.RemoveItem(MineralType.Gold, 5);
            Assert.IsFalse(result);
            Assert.AreEqual(2, _inv.GetCount(MineralType.Gold));
        }

        [Test]
        public void RemoveItem_ToZero_RemovesSlot()
        {
            _inv.AddItem(MineralType.Crystal, 100f, 3);
            _inv.RemoveItem(MineralType.Crystal, 3);
            Assert.AreEqual(0, _inv.UsedSlots);
            Assert.AreEqual(0, _inv.GetCount(MineralType.Crystal));
        }

        [Test]
        public void HasItem_ChecksCount()
        {
            _inv.AddItem(MineralType.Obsidian, 300f, 3);
            Assert.IsTrue(_inv.HasItem(MineralType.Obsidian, 1));
            Assert.IsTrue(_inv.HasItem(MineralType.Obsidian, 3));
            Assert.IsFalse(_inv.HasItem(MineralType.Obsidian, 4));
            Assert.IsFalse(_inv.HasItem(MineralType.Iron, 1));
        }

        [Test]
        public void GetCount_AggregatesAcrossStacks()
        {
            // Add 5 iron (fills one stack), then 3 more (second stack)
            _inv.AddItem(MineralType.Iron, 15f, 5);
            _inv.AddItem(MineralType.Iron, 15f, 3);
            Assert.AreEqual(2, _inv.UsedSlots);
            Assert.AreEqual(8, _inv.GetCount(MineralType.Iron));
        }

        [Test]
        public void HasFreeSlot_Stackable()
        {
            _inv.AddItem(MineralType.Iron, 15f, 3);
            Assert.IsTrue(_inv.HasFreeSlot(MineralType.Iron)); // can stack more
        }

        [Test]
        public void HasFreeSlot_NewSlotWhenCapNotFull()
        {
            Assert.IsTrue(_inv.HasFreeSlot(MineralType.Iron)); // empty inventory
        }

        [Test]
        public void AddItem_NegativeCount_ReturnsFalse()
        {
            bool result = _inv.AddItem(MineralType.Stone, 5f, -1);
            Assert.IsFalse(result);
        }

        [Test]
        public void RemoveItem_NegativeCount_ReturnsFalse()
        {
            _inv.AddItem(MineralType.Stone, 5f, 1);
            bool result = _inv.RemoveItem(MineralType.Stone, -1);
            Assert.IsFalse(result);
        }
    }
}
