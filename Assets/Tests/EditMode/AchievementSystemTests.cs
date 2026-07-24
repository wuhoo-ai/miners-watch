using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for AchievementSystem.
    /// Uses explicit Init() — does NOT rely on Awake().
    /// </summary>
    public class AchievementSystemTests
    {
        private GameObject _go;
        private AchievementSystem _ach;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestAchievements");
            _ach = _go.AddComponent<AchievementSystem>();
            _ach.Init();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // ── definition tests ────────────────────────────────────

        [Test]
        public void AllAchievements_HasAtLeast15()
        {
            Assert.GreaterOrEqual(AchievementSystem.AllAchievements.Length, 15);
        }

        [Test]
        public void AllAchievements_HaveUniqueIds()
        {
            var ids = new System.Collections.Generic.HashSet<string>();
            foreach (var def in AchievementSystem.AllAchievements)
            {
                Assert.IsFalse(ids.Contains(def.id), $"Duplicate achievement id: {def.id}");
                ids.Add(def.id);
            }
        }

        [Test]
        public void GetDef_KnownId_ReturnsDef()
        {
            var def = AchievementSystem.GetDef("first_kill");
            Assert.IsNotNull(def);
            Assert.AreEqual("first_kill", def.id);
            Assert.AreEqual("First Blood", def.title);
        }

        [Test]
        public void GetDef_UnknownId_ReturnsNull()
        {
            Assert.IsNull(AchievementSystem.GetDef("nonexistent"));
        }

        // ── kill achievements ───────────────────────────────────

        [Test]
        public void NotifyEnemyKilled_FirstKill_UnlocksFirstKill()
        {
            _ach.NotifyEnemyKilled();
            Assert.IsTrue(_ach.IsUnlocked("first_kill"));
            Assert.AreEqual(1, _ach.KillCount);
        }

        [Test]
        public void NotifyEnemyKilled_10Kills_UnlocksKill10()
        {
            for (int i = 0; i < 10; i++) _ach.NotifyEnemyKilled();
            Assert.IsTrue(_ach.IsUnlocked("kill_10"));
            Assert.IsFalse(_ach.IsUnlocked("kill_50"));
        }

        [Test]
        public void NotifyEnemyKilled_50Kills_UnlocksKill50()
        {
            for (int i = 0; i < 50; i++) _ach.NotifyEnemyKilled();
            Assert.IsTrue(_ach.IsUnlocked("kill_50"));
            Assert.IsFalse(_ach.IsUnlocked("kill_100"));
        }

        [Test]
        public void NotifyEnemyKilled_100Kills_UnlocksKill100()
        {
            for (int i = 0; i < 100; i++) _ach.NotifyEnemyKilled();
            Assert.IsTrue(_ach.IsUnlocked("kill_100"));
        }

        // ── craft achievements ──────────────────────────────────

        [Test]
        public void NotifyCraftCompleted_First_UnlocksFirstCraft()
        {
            _ach.NotifyCraftCompleted();
            Assert.IsTrue(_ach.IsUnlocked("first_craft"));
        }

        [Test]
        public void NotifyCraftCompleted_5_UnlocksCraft5()
        {
            for (int i = 0; i < 5; i++) _ach.NotifyCraftCompleted();
            Assert.IsTrue(_ach.IsUnlocked("craft_5"));
            Assert.IsFalse(_ach.IsUnlocked("craft_20"));
        }

        [Test]
        public void NotifyCraftCompleted_20_UnlocksCraft20()
        {
            for (int i = 0; i < 20; i++) _ach.NotifyCraftCompleted();
            Assert.IsTrue(_ach.IsUnlocked("craft_20"));
        }

        // ── night survival achievements ─────────────────────────

        [Test]
        public void NotifyNightSurvived_NoDamage_UnlocksNoDamageNight()
        {
            _ach.NotifyNightSurvived(tookDamage: false);
            Assert.IsTrue(_ach.IsUnlocked("no_damage_night"));
        }

        [Test]
        public void NotifyNightSurvived_WithDamage_DoesNotUnlockNoDamageNight()
        {
            _ach.NotifyNightSurvived(tookDamage: true);
            Assert.IsFalse(_ach.IsUnlocked("no_damage_night"));
        }

        [Test]
        public void NotifyNightSurvived_3Nights_UnlocksSurvive3()
        {
            for (int i = 0; i < 3; i++) _ach.NotifyNightSurvived(tookDamage: true);
            Assert.IsTrue(_ach.IsUnlocked("survive_3_nights"));
            Assert.IsFalse(_ach.IsUnlocked("survive_10_nights"));
        }

        [Test]
        public void NotifyNightSurvived_10Nights_UnlocksSurvive10()
        {
            for (int i = 0; i < 10; i++) _ach.NotifyNightSurvived(tookDamage: true);
            Assert.IsTrue(_ach.IsUnlocked("survive_10_nights"));
        }

        // ── mineral collection ──────────────────────────────────

        [Test]
        public void NotifyMineralMined_AllTypes_UnlocksCollectAll()
        {
            foreach (MineralType t in System.Enum.GetValues(typeof(MineralType)))
                _ach.NotifyMineralMined(t);
            Assert.IsTrue(_ach.IsUnlocked("collect_all"));
        }

        [Test]
        public void NotifyMineralMined_PartialTypes_DoesNotUnlockCollectAll()
        {
            _ach.NotifyMineralMined(MineralType.Stone);
            _ach.NotifyMineralMined(MineralType.Iron);
            Assert.IsFalse(_ach.IsUnlocked("collect_all"));
        }

        // ── gold achievements ───────────────────────────────────

        [Test]
        public void NotifyGoldChanged_PositiveGold_UnlocksFirstGold()
        {
            _ach.NotifyGoldChanged(10);
            Assert.IsTrue(_ach.IsUnlocked("first_gold"));
        }

        [Test]
        public void NotifyGoldChanged_ZeroGold_DoesNotUnlockFirstGold()
        {
            _ach.NotifyGoldChanged(0);
            Assert.IsFalse(_ach.IsUnlocked("first_gold"));
        }

        [Test]
        public void NotifyGoldChanged_500Gold_UnlocksRich500()
        {
            _ach.NotifyGoldChanged(500);
            Assert.IsTrue(_ach.IsUnlocked("rich_500"));
        }

        [Test]
        public void NotifyGoldChanged_PeakTracking_UnlocksRich500EvenIfGoldDrops()
        {
            _ach.NotifyGoldChanged(600);
            _ach.NotifyGoldChanged(100); // gold dropped
            Assert.IsTrue(_ach.IsUnlocked("rich_500")); // peak was 600
        }

        // ── depth achievements ──────────────────────────────────

        [Test]
        public void NotifyDepthReached_Medium_UnlocksReachMedium()
        {
            _ach.NotifyDepthReached(DepthLevel.Medium);
            Assert.IsTrue(_ach.IsUnlocked("reach_medium"));
            Assert.IsFalse(_ach.IsUnlocked("reach_deep"));
        }

        [Test]
        public void NotifyDepthReached_Deep_UnlocksReachDeep()
        {
            _ach.NotifyDepthReached(DepthLevel.Deep);
            Assert.IsTrue(_ach.IsUnlocked("reach_deep"));
            Assert.IsTrue(_ach.IsUnlocked("reach_medium")); // Deep >= Medium
        }

        // ── upgrade achievement ─────────────────────────────────

        [Test]
        public void NotifyUpgradeBought_UnlocksFirstUpgrade()
        {
            _ach.NotifyUpgradeBought();
            Assert.IsTrue(_ach.IsUnlocked("first_upgrade"));
        }

        // ── inventory achievement ───────────────────────────────

        [Test]
        public void NotifyInventoryChanged_FullInventory_UnlocksFullInventory()
        {
            var invGo = new GameObject("TestInv");
            var inv = invGo.AddComponent<InventorySystem>();
            inv.Init();
            inv.SetCapacity(2);
            inv.AddItem(MineralType.Stone, 5f, 1);
            inv.AddItem(MineralType.Iron, 15f, 1);

            _ach.NotifyInventoryChanged(inv);
            Assert.IsTrue(_ach.IsUnlocked("full_inventory"));

            Object.DestroyImmediate(invGo);
        }

        [Test]
        public void NotifyInventoryChanged_NotFull_DoesNotUnlock()
        {
            var invGo = new GameObject("TestInv2");
            var inv = invGo.AddComponent<InventorySystem>();
            inv.Init();
            inv.SetCapacity(10);
            inv.AddItem(MineralType.Stone, 5f, 1);

            _ach.NotifyInventoryChanged(inv);
            Assert.IsFalse(_ach.IsUnlocked("full_inventory"));

            Object.DestroyImmediate(invGo);
        }

        // ── event notification ──────────────────────────────────

        [Test]
        public void OnAchievementUnlocked_FiresOnNewUnlock()
        {
            AchievementDef received = null;
            _ach.OnAchievementUnlocked += def => received = def;

            _ach.NotifyEnemyKilled();

            Assert.IsNotNull(received);
            Assert.AreEqual("first_kill", received.id);
        }

        [Test]
        public void OnAchievementUnlocked_DoesNotFireOnDuplicate()
        {
            int fireCount = 0;
            _ach.OnAchievementUnlocked += _ => fireCount++;

            _ach.NotifyEnemyKilled();
            _ach.NotifyEnemyKilled(); // second kill — first_kill already unlocked

            Assert.AreEqual(1, fireCount);
        }

        // ── duplicate unlock prevention ─────────────────────────

        [Test]
        public void IsUnlocked_DuplicateNotify_StillTrue()
        {
            _ach.NotifyEnemyKilled();
            _ach.NotifyEnemyKilled();
            Assert.IsTrue(_ach.IsUnlocked("first_kill"));
            Assert.AreEqual(1, _ach.UnlockedCount); // only counted once
        }

        // ── persistence ─────────────────────────────────────────

        [Test]
        public void SaveTo_WritesUnlockedIds()
        {
            _ach.NotifyEnemyKilled();
            _ach.NotifyCraftCompleted();

            var data = SaveData.CreateDefault();
            _ach.SaveTo(data);

            Assert.IsNotNull(data.unlockedAchievements);
            Assert.AreEqual(2, data.unlockedAchievements.Count);
            Assert.IsTrue(data.unlockedAchievements.Contains("first_kill"));
            Assert.IsTrue(data.unlockedAchievements.Contains("first_craft"));
        }

        [Test]
        public void LoadFrom_RestoresUnlockedIds()
        {
            var data = SaveData.CreateDefault();
            data.unlockedAchievements.Add("first_kill");
            data.unlockedAchievements.Add("rich_500");

            _ach.LoadFrom(data);

            Assert.IsTrue(_ach.IsUnlocked("first_kill"));
            Assert.IsTrue(_ach.IsUnlocked("rich_500"));
            Assert.AreEqual(2, _ach.UnlockedCount);
        }

        [Test]
        public void SaveLoad_RoundTrip()
        {
            _ach.NotifyEnemyKilled();
            _ach.NotifyNightSurvived(false);
            _ach.NotifyDepthReached(DepthLevel.Medium);

            var data = SaveData.CreateDefault();
            _ach.SaveTo(data);

            // Serialize + deserialize via SaveSystem
            string json = SaveSystem.Serialize(data);
            var restored = SaveSystem.Deserialize(json);

            Assert.IsNotNull(restored);
            Assert.IsNotNull(restored.unlockedAchievements);
            Assert.AreEqual(3, restored.unlockedAchievements.Count);

            // Load into fresh system
            var go2 = new GameObject("TestAch2");
            var ach2 = go2.AddComponent<AchievementSystem>();
            ach2.Init();
            ach2.LoadFrom(restored);

            Assert.IsTrue(ach2.IsUnlocked("first_kill"));
            Assert.IsTrue(ach2.IsUnlocked("no_damage_night"));
            Assert.IsTrue(ach2.IsUnlocked("reach_medium"));

            Object.DestroyImmediate(go2);
        }

        [Test]
        public void LoadFrom_NullData_DoesNotCrash()
        {
            _ach.LoadFrom(null);
            Assert.AreEqual(0, _ach.UnlockedCount);
        }

        [Test]
        public void LoadFrom_EmptyList_DoesNotCrash()
        {
            var data = SaveData.CreateDefault();
            data.unlockedAchievements.Clear();
            _ach.LoadFrom(data);
            Assert.AreEqual(0, _ach.UnlockedCount);
        }

        // ── init reset ──────────────────────────────────────────

        [Test]
        public void Init_ResetsAllState()
        {
            _ach.NotifyEnemyKilled();
            _ach.NotifyCraftCompleted();
            Assert.Greater(_ach.UnlockedCount, 0);

            _ach.Init();
            Assert.AreEqual(0, _ach.UnlockedCount);
            Assert.AreEqual(0, _ach.KillCount);
            Assert.AreEqual(0, _ach.CraftCount);
            Assert.AreEqual(0, _ach.NightsSurvived);
        }

        // ── SaveData default ────────────────────────────────────

        [Test]
        public void SaveData_CreateDefault_HasEmptyAchievements()
        {
            var data = SaveData.CreateDefault();
            Assert.IsNotNull(data.unlockedAchievements);
            Assert.AreEqual(0, data.unlockedAchievements.Count);
        }
    }
}
