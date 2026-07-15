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
            var ss = player.AddComponent<StaminaSystem>(); ss.Init();
            var inv = player.AddComponent<InventorySystem>(); inv.Init();
            var up = player.AddComponent<UpgradeSystem>(); up.Init();
            var ms = player.AddComponent<MiningSystem>();
            ms.Init(); ms.Stamina = ss; ms.Inventory = inv; ms.Upgrades = up;
            Assert.IsNotNull(ms.Stamina);
            Assert.IsNotNull(ms.Inventory);
            Assert.AreEqual(100f, ss.currentStamina);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void PickaxeLv2_ReducesStaminaCost()
        {
            var player = new GameObject("Player");
            var ss = player.AddComponent<StaminaSystem>(); ss.Init();
            var inv = player.AddComponent<InventorySystem>(); inv.Init();
            var up = player.AddComponent<UpgradeSystem>(); up.Init();
            up.AddGold(200); up.BuyUpgrade(UpgradeType.Pickaxe);
            var ms = player.AddComponent<MiningSystem>();
            ms.Init(); ms.Stamina = ss; ms.Inventory = inv; ms.Upgrades = up;

            var nodeObj = new GameObject("IronNode");
            var node = nodeObj.AddComponent<MineralNode>();
            var data = MineralData.Create(MineralType.Iron, "Iron", 2f, 15f,
                new[] { DepthLevel.Shallow, DepthLevel.Medium });
            node.Init(data);

            bool result = ms.TryMine(node);
            Assert.IsTrue(result);
            // Iron costs 2 stamina, Lv2 pickaxe = 2 * 0.7 = 1.4 → Consume uses float
            Assert.AreEqual(98.6f, ss.currentStamina, 0.01f);

            Object.DestroyImmediate(nodeObj);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void TryMine_AddsToInventory()
        {
            var player = new GameObject("Player");
            var ss = player.AddComponent<StaminaSystem>(); ss.Init();
            var inv = player.AddComponent<InventorySystem>(); inv.Init();
            var up = player.AddComponent<UpgradeSystem>(); up.Init();
            var ms = player.AddComponent<MiningSystem>();
            ms.Init(); ms.Stamina = ss; ms.Inventory = inv; ms.Upgrades = up;

            var nodeObj = new GameObject("IronNode");
            var node = nodeObj.AddComponent<MineralNode>();
            var data = MineralData.Create(MineralType.Iron, "Iron", 2f, 15f,
                new[] { DepthLevel.Shallow, DepthLevel.Medium });
            node.Init(data);
            nodeObj.transform.position = player.transform.position;

            bool result = ms.TryMine(node);
            Assert.IsTrue(result, "TryMine should succeed");
            Assert.AreEqual(1, inv.GetCount(MineralType.Iron));
            Assert.AreEqual(98f, ss.currentStamina);

            Object.DestroyImmediate(nodeObj);
            Object.DestroyImmediate(player);
        }
    }
}
