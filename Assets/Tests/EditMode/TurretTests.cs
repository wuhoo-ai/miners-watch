using NUnit.Framework;
using UnityEngine;

namespace MinersWatch.Tests.EditMode
{
    /// <summary>Turret tests: targeting, damage, cooldown, dead-filtering, night activation.</summary>
    public class TurretTests
    {
        [Test]
        public void Init_SetsDefaults()
        {
            var go = new GameObject("Turret", typeof(Turret), typeof(CircleCollider2D));
            var t = go.GetComponent<Turret>();
            t.Init(50); // hp via DefenseEntity
            Assert.AreEqual(50, t.CurrentHP);
            Assert.AreEqual(15, t.Damage);
            Assert.AreEqual(5f, t.Range);
        }

        [Test]
        public void FireAtTime_DamagesNearestEnemy()
        {
            var t = NewTurret();
            t.transform.position = Vector3.zero;

            var e1 = CreateMockEnemy(new Vector3(3, 0, 0), 50);
            var e2 = CreateMockEnemy(new Vector3(1, 0, 0), 50);
            t.EnemiesInRange.Add(e1);
            t.EnemiesInRange.Add(e2);

            // FireAtTime at time=10 — should pick nearest (e2 at dist=1)
            int dmg = t.FireAtTime(e2.transform, 10f);
            Assert.Greater(dmg, 0, "should deal damage to nearest");
            Assert.AreEqual(35, e2.CurrentHP);
            Assert.AreEqual(50, e1.CurrentHP, "farther untouched");
        }

        [Test]
        public void FireAtTime_RespectsCooldown()
        {
            var t = NewTurret();
            var e = CreateMockEnemy(new Vector3(1, 0, 0), 50);
            t.EnemiesInRange.Add(e);

            t.FireAtTime(e.transform, 10f); // first shot at t=10
            Assert.AreEqual(35, e.CurrentHP);

            int dmg2 = t.FireAtTime(e.transform, 11f); // t=11 < interval (2.0)
            Assert.AreEqual(0, dmg2, "still on cooldown");
            Assert.AreEqual(35, e.CurrentHP);

            int dmg3 = t.FireAtTime(e.transform, 12.1f); // t=12.1 → cooldown expired
            Assert.AreEqual(15, dmg3);
            Assert.AreEqual(20, e.CurrentHP);
        }

        [Test]
        public void FireAtTime_IgnoresOutOfRange()
        {
            var t = NewTurret();
            t.transform.position = Vector3.zero;
            var e = CreateMockEnemy(new Vector3(10, 0, 0), 50); // 10 > range=5
            t.EnemiesInRange.Add(e);

            int dmg = t.FireAtTime(e.transform, 10f);
            Assert.AreEqual(0, dmg, "out of range → no damage");
            Assert.AreEqual(50, e.CurrentHP);
        }

        [Test]
        public void FireAtTime_SkipsDestroyed()
        {
            var t = NewTurret();
            t.TakeDamage(999); // destroy it
            var e = CreateMockEnemy(new Vector3(1, 0, 0), 50);
            int dmg = t.FireAtTime(e.transform, 10f);
            Assert.AreEqual(0, dmg, "destroyed turret can't fire");
        }

        [Test]
        public void EnemiesInRange_FiltersDeadEnemies()
        {
            var t = NewTurret();
            var alive = CreateMockEnemy(new Vector3(1, 0, 0), 30);
            var dead = CreateMockEnemy(new Vector3(2, 0, 0), 0);
            t.EnemiesInRange.Add(alive);
            t.EnemiesInRange.Add(dead);

            // FireAtTime should skip dead enemy
            int dmg = t.FireAtTime(alive.transform, 10f);
            Assert.Greater(dmg, 0, "should hit alive enemy");
            Assert.AreEqual(15, alive.CurrentHP);
        }

        // ── helpers ───────────────────────────────────────────

        static Turret NewTurret()
        {
            var go = new GameObject("Turret", typeof(Turret), typeof(CircleCollider2D));
            var t = go.GetComponent<Turret>();
            t.Init(50);
            return t;
        }

        static Enemy CreateMockEnemy(Vector3 pos, int hp)
        {
            var go = new GameObject("Enemy", typeof(Enemy));
            go.transform.position = pos;
            var e = go.GetComponent<Enemy>();
            e.Init(hp, 10, 2f);
            return e;
        }
    }
}
