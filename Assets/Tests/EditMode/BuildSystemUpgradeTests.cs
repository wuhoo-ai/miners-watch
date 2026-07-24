using NUnit.Framework;
using UnityEngine;

namespace MinersWatch.Tests
{
    /// <summary>
    /// EditMode tests for BuildSystem upgrade functionality.
    /// Uses explicit Init() — does NOT rely on Awake().
    /// </summary>
    public class BuildSystemUpgradeTests
    {
        private ShopSystem _shop;
        private InventorySystem _inventory;
        private BuildSystem _build;

        [SetUp]
        public void SetUp()
        {
            var shopGo = new GameObject("TestShop");
            _shop = shopGo.AddComponent<ShopSystem>();
            var upgrades = shopGo.AddComponent<UpgradeSystem>();
            upgrades.Init();
            upgrades.AddGold(1000); // Plenty of gold for testing

            var invGo = new GameObject("TestInventory");
            _inventory = invGo.AddComponent<InventorySystem>();
            _inventory.Init();
            _inventory.SetCapacity(10);
            _inventory.AddItem(MineralType.Iron, 15f, 50);
            _inventory.AddItem(MineralType.Stone, 5f, 50);

            _shop.Init(_inventory, upgrades);

            var buildGo = new GameObject("TestBuild");
            _build = buildGo.AddComponent<BuildSystem>();
            _build.Init(_shop, _inventory);
        }

        [TearDown]
        public void TearDown()
        {
            if (_shop != null) Object.DestroyImmediate(_shop.gameObject);
            if (_inventory != null) Object.DestroyImmediate(_inventory.gameObject);
            if (_build != null) Object.DestroyImmediate(_build.gameObject);
        }

        [Test]
        public void PlaceDefense_SetsLevelToWood()
        {
            // Act
            bool result = _build.PlaceDefense(DefenseType.Wall, 0);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(BuildLevel.Wood, _build.GetBuildingLevel(0));
        }

        [Test]
        public void CanUpgrade_WithSufficientMaterials_ReturnsTrue()
        {
            // Arrange: place a wall at cell 0
            _build.PlaceDefense(DefenseType.Wall, 0);

            // Act: can upgrade to Stone? (needs 50 gold, 10 iron, 5 stone)
            bool canUpgrade = _build.CanUpgrade(0, BuildLevel.Stone);

            // Assert
            Assert.IsTrue(canUpgrade);
        }

        [Test]
        public void CanUpgrade_WithInsufficientMaterials_ReturnsFalse()
        {
            // Arrange: place wall, but clear inventory
            _build.PlaceDefense(DefenseType.Wall, 0);
            _inventory.Clear();

            // Act
            bool canUpgrade = _build.CanUpgrade(0, BuildLevel.Stone);

            // Assert
            Assert.IsFalse(canUpgrade);
        }

        [Test]
        public void CanUpgrade_SkippingLevel_ReturnsFalse()
        {
            // Arrange: place wall at Wood level
            _build.PlaceDefense(DefenseType.Wall, 0);

            // Act: try to upgrade directly to Iron (skipping Stone)
            bool canUpgrade = _build.CanUpgrade(0, BuildLevel.Iron);

            // Assert: must be sequential
            Assert.IsFalse(canUpgrade);
        }

        [Test]
        public void UpgradeBuilding_DeductsMaterialsAndIncreasesLevel()
        {
            // Arrange
            _build.PlaceDefense(DefenseType.Wall, 0);
            int ironBefore = _inventory.GetCount(MineralType.Iron);

            // Act
            bool result = _build.UpgradeBuilding(0, BuildLevel.Stone);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(BuildLevel.Stone, _build.GetBuildingLevel(0));
            Assert.AreEqual(ironBefore - 10, _inventory.GetCount(MineralType.Iron));
        }

        [Test]
        public void UpgradeBuilding_WithInsufficientMaterials_ReturnsFalse()
        {
            // Arrange
            _build.PlaceDefense(DefenseType.Wall, 0);
            _inventory.Clear();

            // Act
            bool result = _build.UpgradeBuilding(0, BuildLevel.Stone);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(BuildLevel.Wood, _build.GetBuildingLevel(0));
        }

        [Test]
        public void GetHpMultiplier_ReturnsCorrectValues()
        {
            Assert.AreEqual(1, _build.GetHpMultiplier(BuildLevel.Wood));
            Assert.AreEqual(2, _build.GetHpMultiplier(BuildLevel.Stone));
            Assert.AreEqual(4, _build.GetHpMultiplier(BuildLevel.Iron));
        }

        [Test]
        public void CanUpgrade_EmptyCell_ReturnsFalse()
        {
            // Cell 5 has no building
            bool canUpgrade = _build.CanUpgrade(5, BuildLevel.Stone);
            Assert.IsFalse(canUpgrade);
        }

        [Test]
        public void ClearCell_ResetsLevelToWood()
        {
            // Arrange: place and upgrade to Stone
            _build.PlaceDefense(DefenseType.Wall, 0);
            _build.UpgradeBuilding(0, BuildLevel.Stone);

            // Act
            _build.ClearCell(0);

            // Assert
            Assert.AreEqual(BuildLevel.Wood, _build.GetBuildingLevel(0));
        }

        [Test]
        public void GetTrapVariantDef_ReturnsCorrectPreset()
        {
            var slowDef = _build.GetTrapVariantDef(TrapVariant.Slow);
            Assert.IsNotNull(slowDef);
            Assert.AreEqual("减速陷阱", slowDef.displayName);
            Assert.AreEqual(2f, slowDef.effectValue, 0.01f);
        }

        [Test]
        public void GetTurretVariantDef_ReturnsCorrectPreset()
        {
            var arrowDef = _build.GetTurretVariantDef(TurretVariant.Arrow);
            Assert.IsNotNull(arrowDef);
            Assert.AreEqual("箭塔", arrowDef.displayName);
            Assert.AreEqual(20, arrowDef.damage);
        }
    }
}
