using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    public class EnemyTests
    {
        [Test]
        public void Rockworm_HasCorrectStats()
        {
            var go = new GameObject("Enemy");
            var enemy = go.AddComponent<Enemy>();
            enemy.Init(30, 10, 2f);
            Assert.AreEqual(30, enemy.MaxHP);
            Assert.AreEqual(10, enemy.Damage);
            Assert.AreEqual(2f, enemy.Speed, 0.01f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Enemy_TakeDamage_ReducesHP()
        {
            var go = new GameObject("Enemy");
            var enemy = go.AddComponent<Enemy>();
            enemy.Init(80, 20, 3.5f);
            enemy.TakeDamage(30);
            Assert.AreEqual(50, enemy.CurrentHP);
            Assert.IsFalse(enemy.IsDead);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Enemy_TakeDamage_Kills()
        {
            var go = new GameObject("Enemy");
            var enemy = go.AddComponent<Enemy>();
            enemy.Init(30, 10, 2f);
            enemy.TakeDamage(30);
            Assert.AreEqual(0, enemy.CurrentHP);
            Assert.IsTrue(enemy.IsDead);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Enemy_TakeDamage_DeadEnemy_Ignored()
        {
            var go = new GameObject("Enemy");
            var enemy = go.AddComponent<Enemy>();
            enemy.Init(30, 10, 2f);
            enemy.TakeDamage(30);
            enemy.TakeDamage(99);
            Assert.AreEqual(0, enemy.CurrentHP);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void EnemyPresets_HaveCorrectData()
        {
            var r = EnemyPresets.Rockworm;
            Assert.AreEqual(EnemyType.Rockworm, r.type);
            Assert.AreEqual(30, r.hp);
            Assert.AreEqual(10, r.damage);

            var s = EnemyPresets.Shadow;
            Assert.AreEqual(EnemyType.Shadow, s.type);
            Assert.AreEqual(80, s.hp);
            Assert.AreEqual(20, s.damage);

            var l = EnemyPresets.Lavabeast;
            Assert.AreEqual(EnemyType.Lavabeast, l.type);
            Assert.AreEqual(200, l.hp);

            var g = EnemyPresets.Guardian;
            Assert.AreEqual(EnemyType.Guardian, g.type);
            Assert.AreEqual(500, g.hp);
            Assert.AreEqual(50, g.damage);
        }
    }

    public class EnemyAITests
    {
        [Test]
        public void StartsInMovingState()
        {
            var go = new GameObject("AI");
            var enemy = go.AddComponent<Enemy>();
            enemy.Init(30, 10, 2f);
            var ai = go.AddComponent<EnemyAI>();
            var core = new GameObject("Core");
            ai.Init(enemy, core.transform);

            Assert.AreEqual(EnemyState.Moving, ai.CurrentState);

            Object.DestroyImmediate(core);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DiesWhenEnemyDead()
        {
            var go = new GameObject("AI");
            var enemy = go.AddComponent<Enemy>();
            enemy.Init(30, 10, 2f);
            var ai = go.AddComponent<EnemyAI>();
            ai.Init(enemy, null);

            enemy.TakeDamage(30);
            ai.Tick(0.1f);
            Assert.AreEqual(EnemyState.Dead, ai.CurrentState);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void OnBlocked_SwitchesToAttacking()
        {
            var go = new GameObject("AI");
            var enemy = go.AddComponent<Enemy>();
            enemy.Init(30, 10, 2f);
            var ai = go.AddComponent<EnemyAI>();
            ai.Init(enemy, null);

            ai.OnBlocked();
            Assert.AreEqual(EnemyState.Attacking, ai.CurrentState);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void OnPathCleared_SwitchesToMoving()
        {
            var go = new GameObject("AI");
            var enemy = go.AddComponent<Enemy>();
            enemy.Init(30, 10, 2f);
            var ai = go.AddComponent<EnemyAI>();
            ai.Init(enemy, null);

            ai.OnBlocked();
            ai.OnPathCleared();
            Assert.AreEqual(EnemyState.Moving, ai.CurrentState);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Die_ForcesDeadState()
        {
            var go = new GameObject("AI");
            var enemy = go.AddComponent<Enemy>();
            enemy.Init(30, 10, 2f);
            var ai = go.AddComponent<EnemyAI>();
            ai.Init(enemy, null);

            ai.Die();
            Assert.AreEqual(EnemyState.Dead, ai.CurrentState);

            Object.DestroyImmediate(go);
        }
    }

    public class WaveManagerTests
    {
        [Test]
        public void ShallowWave1_3Rockworms()
        {
            var cfg = WaveManager.GetWaveConfig(DepthLevel.Shallow, 0);
            Assert.AreEqual(1, cfg.waveNumber);
            Assert.AreEqual(3, cfg.enemyCount);
            Assert.AreEqual(EnemyType.Rockworm, cfg.enemyType);
        }

        [Test]
        public void ShallowWave3_8Rockworms()
        {
            var cfg = WaveManager.GetWaveConfig(DepthLevel.Shallow, 2);
            Assert.AreEqual(3, cfg.waveNumber);
            Assert.AreEqual(8, cfg.enemyCount);
        }

        [Test]
        public void MediumWave2_5Shadows()
        {
            var cfg = WaveManager.GetWaveConfig(DepthLevel.Medium, 1);
            Assert.AreEqual(2, cfg.waveNumber);
            Assert.AreEqual(5, cfg.enemyCount);
            Assert.AreEqual(EnemyType.Shadow, cfg.enemyType);
        }

        [Test]
        public void DeepWave3_3Lavabeasts()
        {
            var cfg = WaveManager.GetWaveConfig(DepthLevel.Deep, 2);
            Assert.AreEqual(3, cfg.waveNumber);
            Assert.AreEqual(EnemyType.Lavabeast, cfg.enemyType);
        }

        [Test]
        public void Tick_SpawnsWavesAtInterval()
        {
            var go = new GameObject("WM");
            var wm = go.AddComponent<WaveManager>();
            wm.Init(DepthLevel.Shallow);
            wm.StartNight();

            Assert.IsFalse(wm.Tick(10f)); // not yet
            Assert.IsTrue(wm.Tick(6f));    // 16s → spawn wave 1
            Assert.AreEqual(1, wm.CurrentWave);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void AllWavesComplete_After3Waves()
        {
            var go = new GameObject("WM");
            var wm = go.AddComponent<WaveManager>();
            wm.Init(DepthLevel.Shallow);
            wm.StartNight();

            wm.Tick(15f); // wave 1
            wm.Tick(15f); // wave 2
            wm.Tick(15f); // wave 3
            Assert.IsTrue(wm.AllWavesComplete);
            Assert.IsFalse(wm.Tick(15f)); // no more waves

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Tick_DoesNothing_WhenNightInactive()
        {
            var go = new GameObject("WM");
            var wm = go.AddComponent<WaveManager>();
            wm.Init(DepthLevel.Shallow);
            // Night not started
            Assert.IsFalse(wm.Tick(999f));
            Assert.AreEqual(0, wm.CurrentWave);

            Object.DestroyImmediate(go);
        }
    }

    public class BossGuardianTests
    {
        [Test]
        public void InheritsEnemyStats()
        {
            var go = new GameObject("Boss");
            var boss = go.AddComponent<BossGuardian>();
            boss.Init(500, 50, 3f);
            Assert.AreEqual(500, boss.MaxHP);
            Assert.AreEqual(50, boss.Damage);
            Assert.AreEqual(3f, boss.GetEffectiveSpeed(), 0.01f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void RageActivates_AtHalfHP()
        {
            var go = new GameObject("Boss");
            var boss = go.AddComponent<BossGuardian>();
            boss.Init(500, 50, 3f);
            Assert.IsFalse(boss.IsRaging);

            boss.TakeDamage(250); // 50% HP
            Assert.IsTrue(boss.IsRaging);
            Assert.AreEqual(3f * 1.5f, boss.GetEffectiveSpeed(), 0.01f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Rage_DoesNotActivate_AboveHalfHP()
        {
            var go = new GameObject("Boss");
            var boss = go.AddComponent<BossGuardian>();
            boss.Init(500, 50, 3f);

            boss.TakeDamage(200); // 60% HP
            Assert.IsFalse(boss.IsRaging);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Rage_OnlyActivatesOnce()
        {
            var go = new GameObject("Boss");
            var boss = go.AddComponent<BossGuardian>();
            boss.Init(500, 50, 3f);

            boss.TakeDamage(250); // first rage trigger
            Assert.IsTrue(boss.IsRaging);
            float speedAfterRage = boss.GetEffectiveSpeed();

            boss.TakeDamage(100); // more damage — no second rage
            Assert.AreEqual(speedAfterRage, boss.GetEffectiveSpeed(), 0.01f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Slam_At8sInterval()
        {
            var go = new GameObject("Boss");
            var boss = go.AddComponent<BossGuardian>();
            boss.Init(500, 50, 3f);

            Assert.AreEqual(0, boss.TickSlam(4f));  // not yet
            Assert.AreEqual(0, boss.TickSlam(3.9f)); // 7.9s — still not
            Assert.AreEqual(75, boss.TickSlam(0.2f)); // 8.1s → triggers
            Assert.AreEqual(0, boss.TickSlam(1f));    // cooldown

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Slam_DoesNotFire_WhenDead()
        {
            var go = new GameObject("Boss");
            var boss = go.AddComponent<BossGuardian>();
            boss.Init(500, 50, 3f);

            boss.TakeDamage(500);
            Assert.AreEqual(0, boss.TickSlam(999f));

            Object.DestroyImmediate(go);
        }
    }
}
