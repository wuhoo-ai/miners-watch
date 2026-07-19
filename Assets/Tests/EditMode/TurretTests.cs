using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace MinersWatch.Tests.EditMode
{
    /// <summary>Turret tests: targeting, damage, cooldown, dead-filtering.</summary>
    public class TurretTests
    {
        static readonly MethodInfo FireMethod = typeof(Turret).GetMethod("Fire",
            BindingFlags.NonPublic | BindingFlags.Instance);

        static void FirePublic(Turret t) => FireMethod?.Invoke(t, null);

        [Test]
        public void Init_SetsRange_CooldownStartsAtZero()
        {
            var go = new GameObject("Turret", typeof(Turret), typeof(CircleCollider2D));
            var t = go.GetComponent<Turret>();
            t.Init(8f, 2f, 25);
            Assert.AreEqual(8f, t.Range);
            Assert.AreEqual(0f, t.CooldownProgress);
        }

        [Test]
        public void Fire_HitsNearest_SparesFarther()
        {
            var t = new GameObject("T", typeof(Turret), typeof(CircleCollider2D)).GetComponent<Turret>();
            t.transform.position = Vector3.zero;

            var e1 = CreateMockEnemy(new Vector3(3, 0, 0), 50);
            var e2 = CreateMockEnemy(new Vector3(1, 0, 0), 50);
            t.EnemiesInRange.Add(e1);
            t.EnemiesInRange.Add(e2);

            FirePublic(t);
            Assert.AreEqual(35, e2.CurrentHP, "nearest (dist=1) should take 15 dmg");
            Assert.AreEqual(50, e1.CurrentHP, "farther (dist=3) untouched");
        }

        [Test]
        public void Fire_SkipsDeadEnemies()
        {
            var t = new GameObject("T", typeof(Turret), typeof(CircleCollider2D)).GetComponent<Turret>();
            t.transform.position = Vector3.zero;

            var dead = CreateMockEnemy(new Vector3(1, 0, 0), 0);
            var alive = CreateMockEnemy(new Vector3(4, 0, 0), 30);
            t.EnemiesInRange.Add(dead);
            t.EnemiesInRange.Add(alive);

            FirePublic(t);
            Assert.AreEqual(15, alive.CurrentHP, "alive enemy takes 15");
            Assert.AreEqual(0, dead.CurrentHP, "dead stays dead");
        }

        [Test]
        public void Fire_NoEnemies_DoesNotThrow()
        {
            var t = new GameObject("T", typeof(Turret), typeof(CircleCollider2D)).GetComponent<Turret>();
            FirePublic(t);
            Assert.Pass();
        }

        [Test]
        public void ManualTick_FiresWhenCooldownElapses()
        {
            var t = new GameObject("T", typeof(Turret), typeof(CircleCollider2D)).GetComponent<Turret>();
            t.Init(5f, 2f, 10);

            var e = CreateMockEnemy(new Vector3(1, 0, 0), 30);
            t.EnemiesInRange.Add(e);

            bool fired1 = t.ManualTick(1.9f);
            Assert.IsFalse(fired1, "should not fire at 1.9/2.0");
            Assert.AreEqual(30, e.CurrentHP);

            bool fired2 = t.ManualTick(0.1f);
            Assert.IsTrue(fired2, "should fire when timer crosses 2.0");
            Assert.AreEqual(20, e.CurrentHP, "10 damage applied");
            Assert.Less(t.CooldownProgress, 0.1f, "timer reset after fire");
        }

        // ── helper ────────────────────────────────────────────

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
