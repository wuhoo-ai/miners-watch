using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    public class StaminaSystemTests
    {
        [Test]
        public void CanCreateComponent()
        {
            var go = new GameObject("Test");
            var s = go.AddComponent<StaminaSystem>();
            // Call Init explicitly — EditMode doesn't run Awake reliably
            s.Init();
            Assert.AreEqual(100f, s.maxStamina);
            Assert.AreEqual(100f, s.currentStamina);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Consume_Reduces_Stamina()
        {
            var go = new GameObject("Test");
            var s = go.AddComponent<StaminaSystem>();
            s.Init();
            Assert.IsTrue(s.Consume(30f));
            Assert.AreEqual(70f, s.currentStamina);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Consume_Fails_When_Insufficient()
        {
            var go = new GameObject("Test");
            var s = go.AddComponent<StaminaSystem>();
            s.Init();
            Assert.IsFalse(s.Consume(101f));
            Assert.AreEqual(100f, s.currentStamina);
            Object.DestroyImmediate(go);
        }
    }
}
