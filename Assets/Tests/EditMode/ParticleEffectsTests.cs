using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace Tests.EditMode
{
    /// <summary>
    /// EditMode tests for ParticleEffects object pool and lifecycle.
    /// Uses Init() directly — no Awake() dependency.
    /// </summary>
    [TestFixture]
    public class ParticleEffectsTests
    {
        private GameObject _go;
        private ParticleEffects _fx;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("[ParticleEffects_Test]");
            _fx = _go.AddComponent<ParticleEffects>();
            // Manual init — EditMode does not call Awake()
            _fx.Init(3, 10);
        }

        [TearDown]
        public void TearDown()
        {
            if (_fx != null)
                _fx.ClearAll();
            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        // ─── Pool Initialization ───────────────────────────────────────────

        [Test]
        public void Init_CreatesPoolsForAllEffectTypes()
        {
            Assert.AreEqual(3, _fx.GetPoolCount(ParticleEffects.EffectType.MiningSpark));
            Assert.AreEqual(3, _fx.GetPoolCount(ParticleEffects.EffectType.AttackSlash));
            Assert.AreEqual(3, _fx.GetPoolCount(ParticleEffects.EffectType.EnemyDeath));
        }

        [Test]
        public void Init_IsIdempotent_DoesNotDuplicatePools()
        {
            _fx.Init(3, 10); // call again
            Assert.AreEqual(3, _fx.GetPoolCount(ParticleEffects.EffectType.MiningSpark));
        }

        [Test]
        public void Init_ClampsMinimumPoolSize()
        {
            _fx.Init(0, 0);
            Assert.AreEqual(1, _fx.InitialPoolSize);
            Assert.GreaterOrEqual(_fx.MaxPoolSize, _fx.InitialPoolSize);
        }

        // ─── Play & Pool Dequeue ───────────────────────────────────────────

        [Test]
        public void Play_DequeuesFromPool_ActiveCountIncreases()
        {
            int poolBefore = _fx.GetPoolCount(ParticleEffects.EffectType.MiningSpark);
            var ps = ParticleEffects.Play(ParticleEffects.EffectType.MiningSpark, Vector3.zero);

            Assert.IsNotNull(ps);
            Assert.AreEqual(1, _fx.ActiveCount);
            Assert.AreEqual(poolBefore - 1, _fx.GetPoolCount(ParticleEffects.EffectType.MiningSpark));
        }

        [Test]
        public void Play_SetsPositionCorrectly()
        {
            var pos = new Vector3(5f, 3f, 0f);
            var ps = ParticleEffects.Play(ParticleEffects.EffectType.AttackSlash, pos);

            Assert.AreEqual(pos, ps.transform.position);
        }

        [Test]
        public void Play_ActivatesGameObject()
        {
            var ps = ParticleEffects.Play(ParticleEffects.EffectType.EnemyDeath, Vector3.zero);
            Assert.IsTrue(ps.gameObject.activeSelf);
        }

        [Test]
        public void Play_MultipleEffects_TracksAllActive()
        {
            ParticleEffects.Play(ParticleEffects.EffectType.MiningSpark, Vector3.zero);
            ParticleEffects.Play(ParticleEffects.EffectType.AttackSlash, Vector3.one);
            ParticleEffects.Play(ParticleEffects.EffectType.EnemyDeath, Vector3.up);

            Assert.AreEqual(3, _fx.ActiveCount);
        }

        // ─── Return to Pool ────────────────────────────────────────────────

        [Test]
        public void ReturnToPool_DeactivatesAndRequeues()
        {
            var ps = ParticleEffects.Play(ParticleEffects.EffectType.MiningSpark, Vector3.zero);
            Assert.AreEqual(1, _fx.ActiveCount);

            _fx.ReturnToPool(ps);

            Assert.AreEqual(0, _fx.ActiveCount);
            Assert.IsFalse(ps.gameObject.activeSelf);
            Assert.AreEqual(3, _fx.GetPoolCount(ParticleEffects.EffectType.MiningSpark)); // back to initial
        }

        [Test]
        public void ReturnToPool_NullSafe()
        {
            Assert.DoesNotThrow(() => _fx.ReturnToPool(null));
        }

        [Test]
        public void ReturnToPool_IgnoresUntrackedParticle()
        {
            var otherGo = new GameObject("Other");
            var otherPs = otherGo.AddComponent<ParticleSystem>();

            _fx.ReturnToPool(otherPs); // not tracked — should be no-op
            Assert.AreEqual(0, _fx.ActiveCount);

            Object.DestroyImmediate(otherGo);
        }

        [Test]
        public void ReturnAll_ClearsAllActive()
        {
            ParticleEffects.Play(ParticleEffects.EffectType.MiningSpark, Vector3.zero);
            ParticleEffects.Play(ParticleEffects.EffectType.AttackSlash, Vector3.one);
            ParticleEffects.Play(ParticleEffects.EffectType.EnemyDeath, Vector3.up);
            Assert.AreEqual(3, _fx.ActiveCount);

            _fx.ReturnAll();
            Assert.AreEqual(0, _fx.ActiveCount);
        }

        // ─── Pool Reuse ────────────────────────────────────────────────────

        [Test]
        public void Play_ReusesReturnedInstance()
        {
            var ps1 = ParticleEffects.Play(ParticleEffects.EffectType.MiningSpark, Vector3.zero);
            int id1 = ps1.GetInstanceID();
            _fx.ReturnToPool(ps1);

            var ps2 = ParticleEffects.Play(ParticleEffects.EffectType.MiningSpark, Vector3.one);
            int id2 = ps2.GetInstanceID();

            Assert.AreEqual(id1, id2, "Pool should reuse the same ParticleSystem instance");
        }

        [Test]
        public void Play_PoolExhausted_ExpandsPool()
        {
            // Drain pool of 3
            var a = ParticleEffects.Play(ParticleEffects.EffectType.MiningSpark, Vector3.zero);
            var b = ParticleEffects.Play(ParticleEffects.EffectType.MiningSpark, Vector3.one);
            var c = ParticleEffects.Play(ParticleEffects.EffectType.MiningSpark, Vector3.up);
            Assert.AreEqual(0, _fx.GetPoolCount(ParticleEffects.EffectType.MiningSpark));

            // 4th play should expand pool (under max cap)
            var d = ParticleEffects.Play(ParticleEffects.EffectType.MiningSpark, Vector3.right);
            Assert.IsNotNull(d);
            Assert.AreEqual(4, _fx.ActiveCount);
        }

        // ─── Lifecycle / ClearAll ──────────────────────────────────────────

        [Test]
        public void ClearAll_DestroysAllPooledAndActive()
        {
            ParticleEffects.Play(ParticleEffects.EffectType.MiningSpark, Vector3.zero);
            ParticleEffects.Play(ParticleEffects.EffectType.AttackSlash, Vector3.one);

            _fx.ClearAll();

            Assert.AreEqual(0, _fx.ActiveCount);
            Assert.AreEqual(0, _fx.GetPoolCount(ParticleEffects.EffectType.MiningSpark));
            Assert.AreEqual(0, _fx.GetPoolCount(ParticleEffects.EffectType.AttackSlash));
            Assert.AreEqual(0, _fx.GetPoolCount(ParticleEffects.EffectType.EnemyDeath));
        }

        // ─── Static API Convenience ────────────────────────────────────────

        [Test]
        public void StaticAPI_PlayMiningEffect_Works()
        {
            var ps = ParticleEffects.PlayMiningEffect(new Vector3(1f, 2f, 0f));
            Assert.IsNotNull(ps);
            Assert.AreEqual(1, _fx.ActiveCount);
        }

        [Test]
        public void StaticAPI_PlayAttackEffect_Works()
        {
            var ps = ParticleEffects.PlayAttackEffect(Vector3.zero);
            Assert.IsNotNull(ps);
        }

        [Test]
        public void StaticAPI_PlayDeathEffect_Works()
        {
            var ps = ParticleEffects.PlayDeathEffect(Vector3.zero);
            Assert.IsNotNull(ps);
        }

        // ─── ParticleSystem Configuration ──────────────────────────────────

        [Test]
        public void MiningSpark_IsNonLooping()
        {
            var ps = ParticleEffects.Play(ParticleEffects.EffectType.MiningSpark, Vector3.zero);
            Assert.IsFalse(ps.main.loop);
        }

        [Test]
        public void AttackSlash_IsNonLooping()
        {
            var ps = ParticleEffects.Play(ParticleEffects.EffectType.AttackSlash, Vector3.zero);
            Assert.IsFalse(ps.main.loop);
        }

        [Test]
        public void EnemyDeath_IsNonLooping()
        {
            var ps = ParticleEffects.Play(ParticleEffects.EffectType.EnemyDeath, Vector3.zero);
            Assert.IsFalse(ps.main.loop);
        }

        [Test]
        public void MiningSpark_HasGoldColor()
        {
            var ps = ParticleEffects.Play(ParticleEffects.EffectType.MiningSpark, Vector3.zero);
            Color c = ps.main.startColor.color;
            // Gold ≈ (1, 0.84, 0.2)
            Assert.Greater(c.r, 0.8f);
            Assert.Greater(c.g, 0.6f);
            Assert.Less(c.b, 0.4f);
        }
    }
}
