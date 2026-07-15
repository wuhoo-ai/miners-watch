using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    public class DepthProgressionTests
    {
        private GameObject _go;
        private DepthProgression _dp;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("TestDepth");
            _dp = _go.AddComponent<DepthProgression>();
            _dp.Init();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_go);

        [Test]
        public void StartsAtShallow()
        {
            Assert.AreEqual(DepthLevel.Shallow, _dp.CurrentDepth);
            Assert.AreEqual(0, _dp.AccumulatedGold);
        }

        [Test]
        public void Shallow_AlwaysAccessible()
        {
            Assert.IsTrue(_dp.CanEnterDepth(DepthLevel.Shallow));
        }

        [Test]
        public void Mid_Locked_Initially()
        {
            Assert.IsFalse(_dp.IsMidUnlocked);
            Assert.IsFalse(_dp.CanEnterDepth(DepthLevel.Medium));
        }

        [Test]
        public void Deep_Locked_Initially()
        {
            Assert.IsFalse(_dp.IsDeepUnlocked);
            Assert.IsFalse(_dp.CanEnterDepth(DepthLevel.Deep));
        }

        [Test]
        public void Mid_UnlocksAt500()
        {
            _dp.AddEarnings(500);
            Assert.IsTrue(_dp.IsMidUnlocked);
            Assert.IsTrue(_dp.CanEnterDepth(DepthLevel.Medium));
            Assert.IsFalse(_dp.IsDeepUnlocked); // deep still locked
        }

        [Test]
        public void Deep_UnlocksAt2000()
        {
            _dp.AddEarnings(2000);
            Assert.IsTrue(_dp.IsMidUnlocked);
            Assert.IsTrue(_dp.IsDeepUnlocked);
            Assert.IsTrue(_dp.CanEnterDepth(DepthLevel.Deep));
        }

        [Test]
        public void AccumulatedGold_Increments()
        {
            _dp.AddEarnings(300);
            _dp.AddEarnings(250);
            Assert.AreEqual(550, _dp.AccumulatedGold);
            Assert.IsTrue(_dp.IsMidUnlocked);
        }

        [Test]
        public void UnlockEvent_FiresOnce()
        {
            int events = 0;
            DepthLevel lastUnlock = DepthLevel.Shallow;
            _dp.OnDepthUnlocked += d => { events++; lastUnlock = d; };

            _dp.AddEarnings(500);
            Assert.AreEqual(1, events);
            Assert.AreEqual(DepthLevel.Medium, lastUnlock);

            // Add more — no duplicate event for Mid
            _dp.AddEarnings(100);
            Assert.AreEqual(1, events);
        }

        [Test]
        public void UnlockEvent_FiresForBoth()
        {
            int events = 0;
            _dp.OnDepthUnlocked += _ => events++;

            _dp.AddEarnings(2000); // unlocks both in one shot
            Assert.AreEqual(2, events);
        }

        [Test]
        public void AddEarnings_Negative_Ignored()
        {
            _dp.AddEarnings(-100);
            Assert.AreEqual(0, _dp.AccumulatedGold);
        }

        [Test]
        public void SetDepth_RespectsUnlock()
        {
            _dp.SetDepth(DepthLevel.Medium); // locked — should not change
            Assert.AreEqual(DepthLevel.Shallow, _dp.CurrentDepth);

            _dp.AddEarnings(500);
            _dp.SetDepth(DepthLevel.Medium);
            Assert.AreEqual(DepthLevel.Medium, _dp.CurrentDepth);
        }
    }

    public class SceneControllerTests
    {
        [Test]
        public void GetSceneNameForDepth_ReturnsCorrectNames()
        {
            var go = new GameObject("SC");
            var sc = go.AddComponent<SceneController>();
            var dp = go.AddComponent<DepthProgression>();
            dp.Init();
            sc.Init(dp);

            Assert.AreEqual("ShallowCave", sc.GetSceneNameForDepth(DepthLevel.Shallow));
            Assert.AreEqual("MidCave", sc.GetSceneNameForDepth(DepthLevel.Medium));
            Assert.AreEqual("DeepCave", sc.GetSceneNameForDepth(DepthLevel.Deep));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Init_GuardsFadeDuration()
        {
            var go = new GameObject("SC");
            var sc = go.AddComponent<SceneController>();
            sc.Init(null);
            // Should not crash — just verifies no exception
            Assert.IsNull(sc.CurrentScene);
            Object.DestroyImmediate(go);
        }
    }
}
