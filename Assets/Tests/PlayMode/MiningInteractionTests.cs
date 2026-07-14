using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MinersWatch;

namespace MinersWatch.Tests.PlayMode
{
    public class MiningInteractionTests
    {
        [UnityTest]
        public IEnumerator MineralNode_Destroyed_AfterMining()
        {
            // Create player with MiningSystem + trigger collider
            var player = new GameObject("Player");
            player.tag = "Player";
            var stamina = player.AddComponent<StaminaSystem>();
            var mining = player.AddComponent<MiningSystem>();
            var playerCol = player.AddComponent<CircleCollider2D>();
            playerCol.isTrigger = true;
            playerCol.radius = 2f;
            player.transform.position = Vector3.zero;
            stamina.Init();

            // Create mineral node
            var nodeObj = new GameObject("MineralNode");
            nodeObj.tag = "MineralNode";
            var node = nodeObj.AddComponent<MineralNode>();
            var data = MineralData.Create(MineralType.Stone, "Stone", 1f, 5f, new[] { DepthLevel.Shallow });
            node.Init(data);
            nodeObj.transform.position = new Vector3(0.5f, 0f, 0f);

            yield return null; // Let physics/triggers settle

            // Mine the node directly
            bool result = mining.TryMine(node);
            Assert.IsTrue(result, "Mining should succeed");

            yield return null; // Let destruction process

            // Node should be destroyed after successful mining
            Assert.IsTrue(nodeObj == null, "MineralNode GameObject should be destroyed after mining");
        }
    }
}
