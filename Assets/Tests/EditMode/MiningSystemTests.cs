using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    public class MiningSystemTests
    {
        [Test]
        public void TryMine_Succeeds_WithValidTarget()
        {
            var player = new GameObject("Player");
            var ss = player.AddComponent<StaminaSystem>();
            var ms = player.AddComponent<MiningSystem>();
            
            // Explicit dependency injection
            ms.Stamina = ss;
            ss.Init();
            
            // Verify stamina is initialized
            Assert.AreEqual(100f, ss.currentStamina, "Stamina should start at 100");
            
            var nodeObj = new GameObject("Node");
            var node = nodeObj.AddComponent<MineralNode>();
            node.Init(MineralData.Create(MineralType.Stone, "S", 1f, 0f, null));
            
            bool result = ms.TryMine(node);
            Assert.IsTrue(result, "TryMine should succeed");
            Assert.AreEqual(99f, ss.currentStamina, "Should consume 1 stamina");
            
            Object.DestroyImmediate(player);
        }
    }
}
