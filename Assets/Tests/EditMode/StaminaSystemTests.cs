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
            Assert.IsNotNull(s);
            Assert.AreEqual(100f, s.maxStamina);
            Assert.AreEqual(100f, s.currentStamina);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Consume_Reduces_Stamina()
        {
            var go = new GameObject("Test");
            var s = go.AddComponent<StaminaSystem>();
            bool ok = s.Consume(30f);
            Assert.IsTrue(ok);
            Assert.AreEqual(70f, s.currentStamina);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Consume_Fails_When_Insufficient()
        {
            var go = new GameObject("Test");
            var s = go.AddComponent<StaminaSystem>();
            bool ok = s.Consume(101f);
            Assert.IsFalse(ok);
            Assert.AreEqual(100f, s.currentStamina);
            Object.DestroyImmediate(go);
        }
    }
}
