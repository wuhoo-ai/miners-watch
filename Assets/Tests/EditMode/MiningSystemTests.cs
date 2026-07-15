using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    public class MiningSystemTests
    {
        [Test]
        public void CanCreateMiningComponent()
        {
            var player = new GameObject("Player");
            var ss = player.AddComponent<StaminaSystem>();
            ss.Init();
            var inv = player.AddComponent<InventorySystem>();
            inv.Init();
            var ms = player.AddComponent<MiningSystem>();
            ms.Init();
            ms.Stamina = ss;
            ms.Inventory = inv;
            Assert.IsNotNull(ms.Stamina);
            Assert.IsNotNull(ms.Inventory);
            Assert.AreEqual(100f, ss.currentStamina);
            Assert.AreEqual(10, inv.Capacity);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void TryMine_AddsToInventory()
        {
            var player = new GameObject("Player");
            var ss = player.AddComponent<StaminaSystem>();
            ss.Init();
            var inv = player.AddComponent<InventorySystem>();
            inv.Init();
            var ms = player.AddComponent<MiningSystem>();
            ms.Init();
            ms.Stamina = ss;
            ms.Inventory = inv;

            // Create a mineral node with Iron data
            var nodeObj = new GameObject("IronNode");
            var node = nodeObj.AddComponent<MineralNode>();
            var data = MineralData.Create(MineralType.Iron, "Iron", 2f, 15f, 
                new[] { DepthLevel.Shallow, DepthLevel.Medium });
            node.Init(data);
            // Place node close to player
            nodeObj.transform.position = player.transform.position;

            bool result = ms.TryMine(node);
            Assert.IsTrue(result, "TryMine should succeed");
            Assert.AreEqual(1, inv.GetCount(MineralType.Iron));
            Assert.AreEqual(98f, ss.currentStamina); // 100 - 2

            Object.DestroyImmediate(nodeObj);
            Object.DestroyImmediate(player);
        }
    }
}
