using NUnit.Framework;
using UnityEngine;

namespace MinersWatch.Tests
{
    /// <summary>
    /// EditMode tests for EnemyAI behavior variants.
    /// Uses explicit Init() — does NOT rely on Awake().
    /// </summary>
    public class EnemyAIVariantTests
    {
        private Enemy _enemy;
        private EnemyAI _ai;
        private Transform _coreTarget;

        [SetUp]
        public void SetUp()
        {
            var enemyGo = new GameObject("TestEnemy");
            _enemy = enemyGo.AddComponent<Enemy>();
            _enemy.Init(100, 10, 2f);

            _ai = enemyGo.AddComponent<EnemyAI>();

            var targetGo = new GameObject("CoreTarget");
            _coreTarget = targetGo.transform;
            _coreTarget.position = new Vector3(10f, 0f, 0f);
        }

        [TearDown]
        public void TearDown()
        {
            if (_enemy != null) Object.DestroyImmediate(_enemy.gameObject);
            if (_coreTarget != null) Object.DestroyImmediate(_coreTarget.gameObject);
        }

        [Test]
        public void Init_WithLinearBehavior_SetsCorrectBehavior()
        {
            _ai.Init(_enemy, _coreTarget, AIBehavior.Linear);
            Assert.AreEqual(AIBehavior.Linear, _ai.Behavior);
        }

        [Test]
        public void Init_WithBlinkBehavior_SetsCorrectBehavior()
        {
            _ai.Init(_enemy, _coreTarget, AIBehavior.Blink);
            Assert.AreEqual(AIBehavior.Blink, _ai.Behavior);
        }

        [Test]
        public void Blink_TimerAccumulatesUntilBlinkInterval()
        {
            _ai.Init(_enemy, _coreTarget, AIBehavior.Blink);

            _ai.Tick(1f);
            Assert.AreEqual(1f, _ai.GetBlinkTimer(), 0.01f);

            _ai.Tick(0.5f);
            Assert.AreEqual(1.5f, _ai.GetBlinkTimer(), 0.01f);
        }

        [Test]
        public void Blink_ResetsTimerAfterBlinkInterval()
        {
            _ai.Init(_enemy, _coreTarget, AIBehavior.Blink);
            float initialX = _enemy.transform.position.x;

            _ai.Tick(2f); // Exactly 2s blink interval

            // Timer should reset to 0 after blink
            Assert.AreEqual(0f, _ai.GetBlinkTimer(), 0.01f);
            // Position should have moved (blinked forward)
            Assert.Greater(_enemy.transform.position.x, initialX);
        }

        [Test]
        public void Ranged_StartsCooldownAfterAttack()
        {
            _ai.Init(_enemy, _coreTarget, AIBehavior.Ranged);
            _enemy.transform.position = new Vector3(7f, 0f, 0f); // Within 5f range
            _coreTarget.position = new Vector3(10f, 0f, 0f);

            bool attackFired = false;
            _ai.OnRangedAttack += () => attackFired = true;

            _ai.Tick(3f); // Ranged interval = 3s

            Assert.IsTrue(attackFired);
            Assert.AreEqual(3f, _ai.GetRangedCooldown(), 0.01f);
        }

        [Test]
        public void Ranged_DecreasesCooldownOverTime()
        {
            _ai.Init(_enemy, _coreTarget, AIBehavior.Ranged);
            _enemy.transform.position = new Vector3(7f, 0f, 0f);

            _ai.Tick(3f); // Fire attack, cooldown = 3s
            _ai.Tick(1f); // 1s passes, cooldown = 2s

            Assert.AreEqual(2f, _ai.GetRangedCooldown(), 0.01f);
        }

        [Test]
        public void Boss_AdvancesThroughPhases()
        {
            _ai.Init(_enemy, _coreTarget, AIBehavior.Boss);
            _enemy.transform.position = new Vector3(0f, 0f, 0f);

            Assert.AreEqual(0, _ai.GetBossPhase());

            // Phase 0: Rush (5s)
            _ai.Tick(5f);
            Assert.AreEqual(1, _ai.GetBossPhase());

            // Phase 1: Slam AOE (5s)
            _ai.Tick(5f);
            Assert.AreEqual(2, _ai.GetBossPhase());

            // Phase 2: Summon (1s)
            _ai.Tick(1f);
            Assert.AreEqual(3, _ai.GetBossPhase());

            // Phase 3: Enrage (stays)
            _ai.Tick(10f);
            Assert.AreEqual(3, _ai.GetBossPhase());
        }

        [Test]
        public void Boss_FiresSummonEvent()
        {
            _ai.Init(_enemy, _coreTarget, AIBehavior.Boss);
            bool summonFired = false;
            _ai.OnBossSummon += () => summonFired = true;

            _ai.Tick(5f); // Phase 0→1
            _ai.Tick(5f); // Phase 1→2
            _ai.Tick(1f); // Phase 2: summon fires

            Assert.IsTrue(summonFired);
        }

        [Test]
        public void Linear_MovesTowardCore()
        {
            _ai.Init(_enemy, _coreTarget, AIBehavior.Linear);
            _enemy.transform.position = new Vector3(0f, 0f, 0f);
            float initialX = _enemy.transform.position.x;

            _ai.Tick(1f);

            Assert.Greater(_enemy.transform.position.x, initialX);
        }
    }
}
