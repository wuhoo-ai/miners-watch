using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    public class UpgradeSystemTests
    {
        private GameObject _go;
        private UpgradeSystem _up;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("TestUpgrades");
            _up = _go.AddComponent<UpgradeSystem>();
            _up.Init();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_go);

        [Test]
        public void Init_StartsAtLevel1()
        {
            Assert.AreEqual(1, _up.GetLevel(UpgradeType.Pickaxe));
            Assert.AreEqual(1, _up.GetLevel(UpgradeType.Armor));
            Assert.AreEqual(1, _up.GetLevel(UpgradeType.Backpack));
            Assert.AreEqual(0, _up.Gold);
        }

        [Test]
        public void GetUpgradeCost_PickaxeLv1to2()
        {
            Assert.AreEqual(200, _up.GetUpgradeCost(UpgradeType.Pickaxe));
        }

        [Test]
        public void GetUpgradeCost_MaxLevel_ReturnsNegative()
        {
            _up.AddGold(9999);
            _up.BuyUpgrade(UpgradeType.Pickaxe); // Lv1→2
            _up.BuyUpgrade(UpgradeType.Pickaxe); // Lv2→3
            Assert.AreEqual(-1, _up.GetUpgradeCost(UpgradeType.Pickaxe));
        }

        [Test]
        public void BuyUpgrade_Success()
        {
            _up.AddGold(200);
            bool result = _up.BuyUpgrade(UpgradeType.Pickaxe);
            Assert.IsTrue(result);
            Assert.AreEqual(2, _up.GetLevel(UpgradeType.Pickaxe));
            Assert.AreEqual(0, _up.Gold);
        }

        [Test]
        public void BuyUpgrade_InsufficientGold()
        {
            _up.AddGold(100);
            bool result = _up.BuyUpgrade(UpgradeType.Pickaxe);
            Assert.IsFalse(result);
            Assert.AreEqual(1, _up.GetLevel(UpgradeType.Pickaxe));
            Assert.AreEqual(100, _up.Gold);
        }

        [Test]
        public void BuyUpgrade_MaxLevel_Fails()
        {
            _up.AddGold(9999);
            _up.BuyUpgrade(UpgradeType.Pickaxe);
            _up.BuyUpgrade(UpgradeType.Pickaxe);
            Assert.AreEqual(3, _up.GetLevel(UpgradeType.Pickaxe));
            bool result = _up.BuyUpgrade(UpgradeType.Pickaxe); // already max
            Assert.IsFalse(result);
        }

        [Test]
        public void BuyUpgrade_AllThree()
        {
            _up.AddGold(450); // 200 + 150 + 100 = 450
            Assert.IsTrue(_up.BuyUpgrade(UpgradeType.Pickaxe));
            Assert.IsTrue(_up.BuyUpgrade(UpgradeType.Armor));
            Assert.IsTrue(_up.BuyUpgrade(UpgradeType.Backpack));
            Assert.AreEqual(2, _up.GetLevel(UpgradeType.Pickaxe));
            Assert.AreEqual(2, _up.GetLevel(UpgradeType.Armor));
            Assert.AreEqual(2, _up.GetLevel(UpgradeType.Backpack));
            Assert.AreEqual(0, _up.Gold);
        }
    }

    public class ShopSystemTests
    {
        private GameObject _go;
        private InventorySystem _inv;
        private UpgradeSystem _up;
        private ShopSystem _shop;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("TestShop");
            _inv = _go.AddComponent<InventorySystem>();
            _inv.Init();
            _up = _go.AddComponent<UpgradeSystem>();
            _up.Init();
            _shop = _go.AddComponent<ShopSystem>();
            _shop.Init(_inv, _up);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_go);

        [Test]
        public void SellAllMinerals_5Iron_Returns75Gold()
        {
            _inv.AddItem(MineralType.Iron, 15f, 5);
            int earned = _shop.SellAllMinerals();
            Assert.AreEqual(75, earned); // 5 × $15
            Assert.AreEqual(75, _up.Gold);
            Assert.AreEqual(0, _inv.GetCount(MineralType.Iron));
        }

        [Test]
        public void SellAllMinerals_EmptyInventory_Returns0()
        {
            int earned = _shop.SellAllMinerals();
            Assert.AreEqual(0, earned);
            Assert.AreEqual(0, _up.Gold);
        }

        [Test]
        public void SellAllMinerals_MixedMinerals()
        {
            _inv.AddItem(MineralType.Stone, 5f, 3);
            _inv.AddItem(MineralType.Gold, 40f, 2);
            int earned = _shop.SellAllMinerals();
            Assert.AreEqual(95, earned); // 3×5 + 2×40
            Assert.AreEqual(95, _up.Gold);
            Assert.AreEqual(0, _inv.UsedSlots);
        }

        [Test]
        public void BuyUpgrade_AfterSell()
        {
            _inv.AddItem(MineralType.Iron, 15f, 14); // 14 × $15 = $210
            _shop.SellAllMinerals();
            Assert.AreEqual(210, _up.Gold);

            bool result = _shop.BuyUpgrade(UpgradeType.Pickaxe);
            Assert.IsTrue(result);
            Assert.AreEqual(2, _up.GetLevel(UpgradeType.Pickaxe));
            Assert.AreEqual(10, _up.Gold); // 210 - 200
        }

        [Test]
        public void CanAffordUpgrade()
        {
            Assert.IsFalse(_shop.CanAffordUpgrade(UpgradeType.Pickaxe));
            _up.AddGold(200);
            Assert.IsTrue(_shop.CanAffordUpgrade(UpgradeType.Pickaxe));
        }

        [Test]
        public void BuyDefense_Wall()
        {
            _up.AddGold(50);
            bool result = _shop.BuyDefense(DefenseType.Wall);
            Assert.IsTrue(result);
            Assert.AreEqual(0, _up.Gold);
        }

        [Test]
        public void BuyDefense_SpikeTrap_NeedsIron()
        {
            _up.AddGold(80);
            Assert.IsFalse(_shop.BuyDefense(DefenseType.SpikeTrap)); // no iron

            _inv.AddItem(MineralType.Iron, 15f, 5);
            Assert.IsTrue(_shop.BuyDefense(DefenseType.SpikeTrap));
            Assert.AreEqual(0, _up.Gold);
            Assert.AreEqual(0, _inv.GetCount(MineralType.Iron)); // 5 - 5
        }

        [Test]
        public void BuyDefense_InsufficientGold()
        {
            _up.AddGold(30);
            Assert.IsFalse(_shop.BuyDefense(DefenseType.Wall)); // needs 50
        }
    }
}
