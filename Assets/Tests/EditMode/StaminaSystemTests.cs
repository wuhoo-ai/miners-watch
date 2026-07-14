using NUnit.Framework;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    public class StaminaSystemTests
    {
        private StaminaSystem stamina;

        [SetUp]
        public void SetUp()
        {
            // StaminaSystem needs a GameObject since it inherits MonoBehaviour
            var go = new UnityEngine.GameObject("TestStamina");
            stamina = go.AddComponent<StaminaSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (stamina != null && stamina.gameObject != null)
                UnityEngine.Object.DestroyImmediate(stamina.gameObject);
        }

        [Test]
        public void Consume_ReturnsFalse_WhenInsufficient()
        {
            Assert.IsFalse(stamina.Consume(101f), "Should return false when consuming more than max");
            Assert.AreEqual(100f, stamina.currentStamina, "Stamina should be unchanged on failed consume");
        }

        [Test]
        public void Consume_ReturnsTrue_WhenSufficient()
        {
            Assert.IsTrue(stamina.Consume(30f), "Should return true when consuming valid amount");
            Assert.AreEqual(70f, stamina.currentStamina, "Stamina should be reduced by 30");
        }

        [Test]
        public void CurrentStamina_NeverNegative()
        {
            stamina.Consume(50f);
            Assert.AreEqual(50f, stamina.currentStamina);

            stamina.Consume(60f); // Should fail: 50 < 60
            Assert.AreEqual(50f, stamina.currentStamina, "Stamina should not go negative");

            stamina.Consume(50f); // Exactly the remaining
            Assert.AreEqual(0f, stamina.currentStamina, "Should be zero");

            stamina.Consume(1f); // Try to go below zero
            Assert.AreEqual(0f, stamina.currentStamina, "Should remain at zero");
        }

        [Test]
        public void Restore_DoesNotExceedMax()
        {
            stamina.Consume(90f);
            Assert.AreEqual(10f, stamina.currentStamina);

            stamina.Restore(50f);
            Assert.AreEqual(60f, stamina.currentStamina);

            stamina.Restore(100f);
            Assert.AreEqual(100f, stamina.currentStamina, "Should cap at maxStamina");
        }

        [Test]
        public void Consume_Zero_ReturnsTrue_NoEvent()
        {
            bool eventFired = false;
            stamina.OnChanged += (c, m) => eventFired = true;

            Assert.IsTrue(stamina.Consume(0f));
            Assert.AreEqual(100f, stamina.currentStamina, "Stamina unchanged");
            Assert.IsFalse(eventFired, "No event should fire for zero consume");
        }

        [Test]
        public void RestoreFull_SetsToMax()
        {
            stamina.Consume(40f);
            stamina.RestoreFull();
            Assert.AreEqual(100f, stamina.currentStamina);
        }

        [Test]
        public void OnChanged_Fires_OnConsume()
        {
            float? reportedCurrent = null;
            float? reportedMax = null;
            stamina.OnChanged += (c, m) => { reportedCurrent = c; reportedMax = m; };

            stamina.Consume(25f);

            Assert.AreEqual(75f, reportedCurrent);
            Assert.AreEqual(100f, reportedMax);
        }

        [Test]
        public void OnChanged_Fires_OnRestore()
        {
            stamina.Consume(50f);

            float? reportedCurrent = null;
            stamina.OnChanged += (c, m) => reportedCurrent = c;

            stamina.Restore(20f);

            Assert.AreEqual(70f, reportedCurrent);
        }
    }
}
