using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    public class DefenseEntityTests
    {
        [Test]
        public void Wall_TakeDamage_ReducesHP()
        {
            var go = new GameObject("Wall");
            var wall = go.AddComponent<Wall>();
            wall.Init(50);
            wall.TakeDamage(20);
            Assert.AreEqual(30, wall.CurrentHP);
            Assert.IsFalse(wall.IsDestroyed);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Wall_TakeDamage_Destroys()
        {
            var go = new GameObject("Wall");
            var wall = go.AddComponent<Wall>();
            wall.Init(50);
            wall.TakeDamage(50);
            Assert.AreEqual(0, wall.CurrentHP);
            Assert.IsTrue(wall.IsDestroyed);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Wall_TakeDamage_Overkill()
        {
            var go = new GameObject("Wall");
            var wall = go.AddComponent<Wall>();
            wall.Init(50);
            wall.TakeDamage(999);
            Assert.AreEqual(0, wall.CurrentHP);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Wall_TakeDamage_Negative_Ignored()
        {
            var go = new GameObject("Wall");
            var wall = go.AddComponent<Wall>();
            wall.Init(50);
            wall.TakeDamage(-10);
            Assert.AreEqual(50, wall.CurrentHP);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SpikeTrap_Trigger_DealsDamage_AndDecrementsUses()
        {
            var go = new GameObject("Trap");
            var trap = go.AddComponent<SpikeTrap>();
            trap.Init(30);
            Assert.AreEqual(3, trap.RemainingUses);
            int dmg = trap.Trigger();
            Assert.AreEqual(20, dmg);
            Assert.AreEqual(2, trap.RemainingUses);
            Assert.IsFalse(trap.IsDepleted);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SpikeTrap_Depletes_AfterAllUses()
        {
            var go = new GameObject("Trap");
            var trap = go.AddComponent<SpikeTrap>();
            trap.Init(30);
            trap.Trigger(); // 2
            trap.Trigger(); // 1
            trap.Trigger(); // 0 → depleted
            Assert.IsTrue(trap.IsDepleted);
            int dmg = trap.Trigger(); // should return 0
            Assert.AreEqual(0, dmg);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SpikeTrap_Destroyed_TriggerReturns0()
        {
            var go = new GameObject("Trap");
            var trap = go.AddComponent<SpikeTrap>();
            trap.Init(30);
            trap.TakeDamage(30); // destroyed
            int dmg = trap.Trigger();
            Assert.AreEqual(0, dmg);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Turret_FireAtTime_DealsDamage()
        {
            var turretGo = new GameObject("Turret");
            var turret = turretGo.AddComponent<Turret>();
            turret.Init(40);

            var targetGo = new GameObject("Target");
            targetGo.transform.position = new Vector3(2f, 0f, 0f);
            turretGo.transform.position = Vector3.zero;

            int dmg = turret.FireAtTime(targetGo.transform, 999f);
            Assert.AreEqual(15, dmg);

            Object.DestroyImmediate(targetGo);
            Object.DestroyImmediate(turretGo);
        }

        [Test]
        public void Turret_FireAtTime_OutOfRange_Returns0()
        {
            var turretGo = new GameObject("Turret");
            var turret = turretGo.AddComponent<Turret>();
            turret.Init(40);

            var targetGo = new GameObject("Target");
            targetGo.transform.position = new Vector3(10f, 0f, 0f); // > 5 range
            turretGo.transform.position = Vector3.zero;

            int dmg = turret.FireAtTime(targetGo.transform, 999f);
            Assert.AreEqual(0, dmg);

            Object.DestroyImmediate(targetGo);
            Object.DestroyImmediate(turretGo);
        }

        [Test]
        public void Turret_FireAtTime_Cooldown()
        {
            var turretGo = new GameObject("Turret");
            var turret = turretGo.AddComponent<Turret>();
            turret.Init(40);

            var targetGo = new GameObject("Target");
            targetGo.transform.position = Vector3.zero;

            int dmg1 = turret.FireAtTime(targetGo.transform, 0f);
            Assert.AreEqual(15, dmg1);

            int dmg2 = turret.FireAtTime(targetGo.transform, 1f); // < 2s cooldown
            Assert.AreEqual(0, dmg2);

            int dmg3 = turret.FireAtTime(targetGo.transform, 3f); // > 2s
            Assert.AreEqual(15, dmg3);

            Object.DestroyImmediate(targetGo);
            Object.DestroyImmediate(turretGo);
        }

        [Test]
        public void Turret_FireAtTime_Destroyed_Returns0()
        {
            var turretGo = new GameObject("Turret");
            var turret = turretGo.AddComponent<Turret>();
            turret.Init(40);

            var targetGo = new GameObject("Target");
            targetGo.transform.position = Vector3.zero;

            turret.TakeDamage(40);
            int dmg = turret.FireAtTime(targetGo.transform, 999f);
            Assert.AreEqual(0, dmg);

            Object.DestroyImmediate(targetGo);
            Object.DestroyImmediate(turretGo);
        }
    }

    public class BuildSystemTests
    {
        private GameObject _go;
        private InventorySystem _inv;
        private UpgradeSystem _up;
        private ShopSystem _shop;
        private BuildSystem _build;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("TestBuild");
            _inv = _go.AddComponent<InventorySystem>();
            _inv.Init();
            _up = _go.AddComponent<UpgradeSystem>();
            _up.Init();
            _shop = _go.AddComponent<ShopSystem>();
            _shop.Init(_inv, _up);
            _build = _go.AddComponent<BuildSystem>();
            _build.Init(_shop);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_go);

        [Test]
        public void Grid_StartsEmpty()
        {
            Assert.IsTrue(_build.IsCellValid(0));
            Assert.IsTrue(_build.IsCellValid(7));
            Assert.IsTrue(_build.IsCellValid(14));
            Assert.IsFalse(_build.IsCellValid(-1));
            Assert.IsFalse(_build.IsCellValid(15));
        }

        [Test]
        public void PlaceDefense_Wall_Success()
        {
            _up.AddGold(50); // wall costs 50
            Assert.IsTrue(_build.CanPlaceDefense(DefenseType.Wall));
            bool result = _build.PlaceDefense(DefenseType.Wall, 3);
            Assert.IsTrue(result);
            Assert.IsFalse(_build.IsCellValid(3)); // cell now occupied
        }

        [Test]
        public void PlaceDefense_InsufficientGold_Fails()
        {
            Assert.IsFalse(_build.CanPlaceDefense(DefenseType.Wall));
            bool result = _build.PlaceDefense(DefenseType.Wall, 5);
            Assert.IsFalse(result);
            Assert.IsTrue(_build.IsCellValid(5)); // cell still free
        }

        [Test]
        public void PlaceDefense_SpikeTrap_NeedsIron()
        {
            _up.AddGold(80);
            Assert.IsFalse(_build.CanPlaceDefense(DefenseType.SpikeTrap)); // no iron

            _inv.AddItem(MineralType.Iron, 15f, 5);
            Assert.IsTrue(_build.CanPlaceDefense(DefenseType.SpikeTrap));

            bool result = _build.PlaceDefense(DefenseType.SpikeTrap, 7);
            Assert.IsTrue(result);
            Assert.IsFalse(_build.IsCellValid(7));
        }

        [Test]
        public void PlaceDefense_OccupiedCell_Fails()
        {
            _up.AddGold(100);
            _build.PlaceDefense(DefenseType.Wall, 5);
            // Try placing again at same cell
            bool result = _build.PlaceDefense(DefenseType.Wall, 5);
            Assert.IsFalse(result);
        }

        [Test]
        public void ClearCell_FreesGrid()
        {
            _up.AddGold(50);
            _build.PlaceDefense(DefenseType.Wall, 2);
            Assert.IsFalse(_build.IsCellValid(2));
            _build.ClearCell(2);
            Assert.IsTrue(_build.IsCellValid(2));
        }
    }
}
