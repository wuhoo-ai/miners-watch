using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    public class MiningSystemTests
    {
        private GameObject playerObj;
        private StaminaSystem stamina;
        private MiningSystem mining;

        [SetUp]
        public void SetUp()
        {
            playerObj = new GameObject("Player");
            stamina = playerObj.AddComponent<StaminaSystem>();
            mining = playerObj.AddComponent<MiningSystem>();
            stamina.Init();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(playerObj);
        }

        [Test]
        public void ConsumeStamina_OnMine()
        {
            // Arrange
            var nodeObj = new GameObject("MineralNode");
            nodeObj.tag = "MineralNode";
            var node = nodeObj.AddComponent<MineralNode>();
            var data = MineralData.Create(MineralType.Stone, "Stone", 1f, 5f, new[] { DepthLevel.Shallow });
            node.Init(data);
            nodeObj.transform.position = Vector3.zero;
            playerObj.transform.position = Vector3.zero;

            float initialStamina = stamina.currentStamina;

            // Act
            bool result = mining.TryMine(node);

            // Assert
            Assert.IsTrue(result, "Mining should succeed");
            Assert.AreEqual(initialStamina - 1f, stamina.currentStamina, 0.01f, "Stamina should decrease by stone's cost (1)");
            Assert.Contains(MineralType.Stone, mining.MinedMinerals, "Stone should be added to mined list");
        }

        [Test]
        public void CannotMine_WhenNoStamina()
        {
            // Arrange — drain all stamina
            stamina.Consume(stamina.maxStamina);
            Assert.AreEqual(0f, stamina.currentStamina, 0.01f);

            var nodeObj = new GameObject("MineralNode");
            nodeObj.tag = "MineralNode";
            var node = nodeObj.AddComponent<MineralNode>();
            var data = MineralData.Create(MineralType.Stone, "Stone", 1f, 5f, new[] { DepthLevel.Shallow });
            node.Init(data);
            nodeObj.transform.position = Vector3.zero;
            playerObj.transform.position = Vector3.zero;

            // Act
            bool result = mining.TryMine(node);

            // Assert
            Assert.IsFalse(result, "Mining should fail when stamina is depleted");
            Assert.AreEqual(0f, stamina.currentStamina, 0.01f, "Stamina should remain at 0");
            Assert.IsEmpty(mining.MinedMinerals, "No minerals should be mined");
        }

        [Test]
        public void MineralType_CorrectStaminaCost(
            [Values(MineralType.Stone, MineralType.Iron, MineralType.Gold, MineralType.Crystal, MineralType.Obsidian)]
            MineralType type)
        {
            // Act
            var data = CreateTestData(type);

            // Assert
            Assert.IsNotNull(data, $"MineralData should be creatable for {type}");
            float expectedCost = GetExpectedStaminaCost(type);
            Assert.AreEqual(expectedCost, data.staminaCost, 0.01f,
                $"Stamina cost for {type} should be {expectedCost}");
        }

        private MineralData CreateTestData(MineralType type)
        {
            switch (type)
            {
                case MineralType.Stone:    return MineralData.Create(type, "Stone",    1f,   5f,  new[] { DepthLevel.Shallow });
                case MineralType.Iron:     return MineralData.Create(type, "Iron",     2f,  15f,  new[] { DepthLevel.Shallow, DepthLevel.Medium });
                case MineralType.Gold:     return MineralData.Create(type, "Gold",     5f,  40f,  new[] { DepthLevel.Medium, DepthLevel.Deep });
                case MineralType.Crystal:  return MineralData.Create(type, "Crystal", 10f, 100f,  new[] { DepthLevel.Medium, DepthLevel.Deep });
                case MineralType.Obsidian: return MineralData.Create(type, "Obsidian", 20f, 300f,  new[] { DepthLevel.Deep });
                default:                   return null;
            }
        }

        private float GetExpectedStaminaCost(MineralType type)
        {
            switch (type)
            {
                case MineralType.Stone:    return 1f;
                case MineralType.Iron:     return 2f;
                case MineralType.Gold:     return 5f;
                case MineralType.Crystal:  return 10f;
                case MineralType.Obsidian: return 20f;
                default:                   return 0f;
            }
        }
    }
}
