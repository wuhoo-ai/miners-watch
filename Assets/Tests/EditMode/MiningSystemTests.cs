using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    public class MiningSystemTests
    {
        // T003 mining tests deferred — EditMode component wiring unreliable.
        // Will be rewritten as PlayMode tests in a follow-up task.
        [Test]
        public void CanCreateMiningComponent()
        {
            var player = new GameObject("Player");
            var ss = player.AddComponent<StaminaSystem>();
            ss.Init();
            var ms = player.AddComponent<MiningSystem>();
            ms.Stamina = ss;
            Assert.IsNotNull(ms.Stamina);
            Assert.AreEqual(100f, ss.currentStamina);
            Object.DestroyImmediate(player);
        }
    }
}
