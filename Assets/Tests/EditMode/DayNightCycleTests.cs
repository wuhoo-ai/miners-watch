using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    public class DayNightCycleTests
    {
        private GameObject _go;
        private DayNightCycle _cycle;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("TestCycle");
            _cycle = _go.AddComponent<DayNightCycle>();
            _cycle.Init();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_go);

        [Test]
        public void StartsInDayPhase()
        {
            Assert.AreEqual(DayNightPhase.Day, _cycle.CurrentPhase);
            Assert.Greater(_cycle.TimeRemaining, 0f);
        }

        [Test]
        public void Tick_AdvancesTimer()
        {
            float before = _cycle.TimeRemaining;
            _cycle.Tick(10f);
            Assert.AreEqual(before - 10f, _cycle.TimeRemaining, 0.01f);
        }

        [Test]
        public void Tick_Zero_DoesNothing()
        {
            float before = _cycle.TimeRemaining;
            _cycle.Tick(0f);
            Assert.AreEqual(before, _cycle.TimeRemaining, 0.01f);
        }

        [Test]
        public void Day_TransitionsToNightTransition_After120s()
        {
            int phaseChanges = 0;
            _cycle.OnPhaseChanged += _ => phaseChanges++;

            _cycle.Tick(120f);
            Assert.AreEqual(DayNightPhase.NightTransition, _cycle.CurrentPhase);
            Assert.AreEqual(1, phaseChanges);
        }

        [Test]
        public void NightTransition_TransitionsToNight_After3s()
        {
            _cycle.Tick(120f); // Day → NightTransition
            _cycle.Tick(3f);   // NightTransition → Night
            Assert.AreEqual(DayNightPhase.Night, _cycle.CurrentPhase);
        }

        [Test]
        public void Night_TransitionsToSettlement_After90s()
        {
            _cycle.Tick(120f);  // Day → NightTransition
            _cycle.Tick(3f);    // NightTransition → Night
            _cycle.Tick(90f);   // Night → Settlement
            Assert.AreEqual(DayNightPhase.Settlement, _cycle.CurrentPhase);
        }

        [Test]
        public void Settlement_TransitionsToDay_After5s()
        {
            _cycle.Tick(120f);  // Day → NightTransition
            _cycle.Tick(3f);    // NightTransition → Night
            _cycle.Tick(90f);   // Night → Settlement
            _cycle.Tick(5f);    // Settlement → Day
            Assert.AreEqual(DayNightPhase.Day, _cycle.CurrentPhase);
        }

        [Test]
        public void FullCycle_ReturnsToDay()
        {
            int phaseChanges = 0;
            _cycle.OnPhaseChanged += _ => phaseChanges++;

            // 120 + 3 + 90 + 5 = 218s for full cycle
            _cycle.Tick(218f);

            Assert.AreEqual(DayNightPhase.Day, _cycle.CurrentPhase);
            Assert.AreEqual(4, phaseChanges); // Day→NightTransition→Night→Settlement→Day
        }

        [Test]
        public void Warning_FiresAt10sRemaining()
        {
            bool warned = false;
            _cycle.OnWarning += () => warned = true;

            _cycle.Tick(110f); // 10s remaining
            Assert.IsTrue(warned, "Warning should fire at 10s remaining");
            Assert.IsTrue(_cycle.IsWarningActive);
        }

        [Test]
        public void Warning_DoesNotFire_BeforeThreshold()
        {
            bool warned = false;
            _cycle.OnWarning += () => warned = true;

            _cycle.Tick(100f); // 20s remaining
            Assert.IsFalse(warned, "Warning should NOT fire at 20s remaining");
            Assert.IsFalse(_cycle.IsWarningActive);
        }

        [Test]
        public void Warning_OnlyFiresOnce()
        {
            int warnCount = 0;
            _cycle.OnWarning += () => warnCount++;

            _cycle.Tick(111f); // past warning threshold, transition
            Assert.AreEqual(1, warnCount, "Warning should fire exactly once");
        }

        [Test]
        public void TimeRemaining_ResetsOnPhaseChange()
        {
            _cycle.Tick(120f); // → NightTransition
            Assert.AreEqual(3f, _cycle.TimeRemaining, 0.01f);
        }

        [Test]
        public void PhaseElapsed_ResetsOnPhaseChange()
        {
            _cycle.Tick(120f); // → NightTransition
            Assert.Less(_cycle.PhaseElapsed, 1f); // near 0 after reset
        }

        [Test]
        public void Tick_ExcessTime_CarriesOver()
        {
            // Tick 125s in one call: 120s finishes Day + 3s finishes NightTransition + 2s into Night
            _cycle.Tick(125f);
            Assert.AreEqual(DayNightPhase.Night, _cycle.CurrentPhase);
            Assert.Greater(_cycle.PhaseElapsed, 1.5f); // ~2s into Night
            Assert.Less(_cycle.PhaseElapsed, 2.5f);
        }

        [Test]
        public void PhaseDurations_FromInit()
        {
            // Day starts at 120, advance 1s and check
            _cycle.Tick(1f);
            Assert.Greater(_cycle.PhaseDuration, 100f);
            Assert.Less(_cycle.PhaseDuration, 130f);
            Assert.AreEqual(119f, _cycle.TimeRemaining, 0.01f);
        }
    }
}
