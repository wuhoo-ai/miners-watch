using NUnit.Framework;
using UnityEngine;

namespace MinersWatch.Tests
{
    /// <summary>
    /// EditMode tests for CraftingSystem.
    /// Uses explicit Init() — does NOT rely on Awake().
    /// </summary>
    public class CraftingSystemTests
    {
        private InventorySystem _inventory;
        private CraftingSystem _crafting;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("TestInventory");
            _inventory = go.AddComponent<InventorySystem>();
            _inventory.Init(); // Explicit init for EditMode
            _inventory.SetCapacity(10);

            var craftGo = new GameObject("TestCrafting");
            _crafting = craftGo.AddComponent<CraftingSystem>();
            _crafting.Init(_inventory);
        }

        [TearDown]
        public void TearDown()
        {
            if (_inventory != null) Object.DestroyImmediate(_inventory.gameObject);
            if (_crafting != null) Object.DestroyImmediate(_crafting.gameObject);
        }

        [Test]
        public void CanCraft_WithSufficientMaterials_ReturnsTrue()
        {
            // Arrange: add materials for SteelIngot (Iron×3 + Gold×1)
            _inventory.AddItem(MineralType.Iron, 15f, 3);
            _inventory.AddItem(MineralType.Gold, 40f, 1);

            // Act
            bool canCraft = _crafting.CanCraft(RecipePresets.SteelIngot);

            // Assert
            Assert.IsTrue(canCraft);
        }

        [Test]
        public void CanCraft_WithInsufficientMaterials_ReturnsFalse()
        {
            // Arrange: only 2 Iron (needs 3)
            _inventory.AddItem(MineralType.Iron, 15f, 2);
            _inventory.AddItem(MineralType.Gold, 40f, 1);

            // Act
            bool canCraft = _crafting.CanCraft(RecipePresets.SteelIngot);

            // Assert
            Assert.IsFalse(canCraft);
        }

        [Test]
        public void Craft_DeductsMaterialsAndGrantsOutput()
        {
            // Arrange
            _inventory.AddItem(MineralType.Iron, 15f, 5);
            _inventory.AddItem(MineralType.Gold, 40f, 2);
            int ironBefore = _inventory.GetCount(MineralType.Iron);
            int goldBefore = _inventory.GetCount(MineralType.Gold);

            // Act
            bool result = _crafting.Craft(RecipePresets.SteelIngot);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(ironBefore - 3, _inventory.GetCount(MineralType.Iron));
            Assert.AreEqual(goldBefore - 1, _inventory.GetCount(MineralType.Gold));
            // SteelIngot has no output mineral in preset, so no new item added
        }

        [Test]
        public void Craft_WithInsufficientMaterials_ReturnsFalse()
        {
            // Arrange: empty inventory
            // Act
            bool result = _crafting.Craft(RecipePresets.SteelIngot);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(0, _inventory.UsedSlots);
        }

        [Test]
        public void GetAvailableRecipes_ReturnsOnlyCraftableRecipes()
        {
            // Arrange: materials for SteelIngot only
            _inventory.AddItem(MineralType.Iron, 15f, 3);
            _inventory.AddItem(MineralType.Gold, 40f, 1);

            // Act
            var available = _crafting.GetAvailableRecipes();

            // Assert
            Assert.AreEqual(1, available.Count);
            Assert.AreEqual("steel_ingot", available[0].id);
        }

        [Test]
        public void Craft_WithOutputMineral_AddsToInventory()
        {
            // Arrange: create custom recipe with output
            var recipe = RecipeDef.Create(
                "test_recipe", "Test",
                new[] { MineralType.Iron }, new[] { 2 },
                output: MineralType.Crystal, outputQty: 1
            );
            _inventory.AddItem(MineralType.Iron, 15f, 5);

            // Act
            bool result = _crafting.Craft(recipe);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(3, _inventory.GetCount(MineralType.Iron)); // 5 - 2
            Assert.AreEqual(1, _inventory.GetCount(MineralType.Crystal)); // +1
        }

        [Test]
        public void RecipeDef_IsValid_WithInvalidData_ReturnsFalse()
        {
            // Arrange: mismatched arrays
            var invalid = new RecipeDef
            {
                id = "bad",
                inputs = new[] { MineralType.Iron },
                inputCounts = new[] { 1, 2 } // Length mismatch
            };

            // Assert
            Assert.IsFalse(invalid.IsValid());
        }
    }
}
