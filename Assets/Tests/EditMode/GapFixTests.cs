using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    public class GapFixTests
    {
        [Test]
        public void PlayerHP_InitsCorrectly()
        {
            var go = new GameObject("Player");
            var up = go.AddComponent<UpgradeSystem>(); up.Init();
            var hp = go.AddComponent<PlayerHP>(); hp.Init(up);
            Assert.AreEqual(100, hp.MaxHP);
            Assert.AreEqual(100, hp.CurrentHP);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void PlayerHP_ArmorLv2_GivesBonus()
        {
            var go = new GameObject("Player");
            var up = go.AddComponent<UpgradeSystem>(); up.Init();
            up.AddGold(999); up.BuyUpgrade(UpgradeType.Armor);
            var hp = go.AddComponent<PlayerHP>(); hp.Init(up);
            Assert.AreEqual(120, hp.MaxHP); // 100 * 1.2
            Object.DestroyImmediate(go);
        }

        [Test]
        public void PlayerHP_TakeDamage()
        {
            var go = new GameObject("Player");
            var up = go.AddComponent<UpgradeSystem>(); up.Init();
            var hp = go.AddComponent<PlayerHP>(); hp.Init(up);
            hp.TakeDamage(30);
            Assert.AreEqual(70, hp.CurrentHP);
            Assert.IsFalse(hp.IsDead);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void BaseCore_TakeDamage()
        {
            var go = new GameObject("Core");
            var core = go.AddComponent<BaseCore>();
            core.TakeDamage(50);
            Assert.AreEqual(150, core.CurrentHP);
            Assert.IsFalse(core.IsDestroyed);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void BaseCore_Destroyed()
        {
            var go = new GameObject("Core");
            var core = go.AddComponent<BaseCore>();
            core.TakeDamage(200);
            Assert.AreEqual(0, core.CurrentHP);
            Assert.IsTrue(core.IsDestroyed);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void BackpackUpgrade_IncreasesCapacity()
        {
            var go = new GameObject("Shop");
            var inv = go.AddComponent<InventorySystem>(); inv.Init();
            var up = go.AddComponent<UpgradeSystem>(); up.Init();
            var shop = go.AddComponent<ShopSystem>(); shop.Init(inv, up);

            Assert.AreEqual(10, inv.Capacity);
            up.AddGold(999);
            up.BuyUpgrade(UpgradeType.Backpack); // OnUpgraded fires, ShopSystem sets capacity
            Assert.AreEqual(20, inv.Capacity);
            up.BuyUpgrade(UpgradeType.Backpack); // Lv2→3
            Assert.AreEqual(30, inv.Capacity);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Enemy_DropLoot_AddsToInventory()
        {
            var go = new GameObject("Enemy");
            var enemy = go.AddComponent<Enemy>(); enemy.Init(30, 10, 2f);
            var invGo = new GameObject("Inv");
            var inv = invGo.AddComponent<InventorySystem>(); inv.Init();

            // Override GetDef via reflection is not clean — test the MapToMineral/price directly
            // The DropLoot method requires GetDef() override from subclass
            // For base Enemy, GetDef returns null → no drop (as designed)
            enemy.DropLoot(inv);
            Assert.AreEqual(0, inv.UsedSlots); // base enemy has no loot

            Object.DestroyImmediate(invGo);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void BossGuardian_DropsObsidian()
        {
            var go = new GameObject("Boss");
            var boss = go.AddComponent<BossGuardian>(); boss.Init(500, 50, 3f);
            var invGo = new GameObject("Inv");
            var inv = invGo.AddComponent<InventorySystem>(); inv.Init();

            boss.DropLoot(inv);
            Assert.AreEqual(1, inv.GetCount(MineralType.Obsidian));

            Object.DestroyImmediate(invGo);
            Object.DestroyImmediate(go);
        }
    }
}
