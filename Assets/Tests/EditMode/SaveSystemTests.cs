using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    public class SaveDataTests
    {
        [Test]
        public void CreateDefault_HasCorrectDefaults()
        {
            var data = SaveData.CreateDefault();
            Assert.AreEqual(SaveData.CurrentVersion, data.version);
            Assert.AreEqual(0, data.depthLevel);
            Assert.AreEqual(0, data.gold);
            Assert.AreEqual(1, data.upgradeLevels["Pickaxe"]);
            Assert.AreEqual(1, data.upgradeLevels["Armor"]);
            Assert.AreEqual(1, data.upgradeLevels["Backpack"]);
            Assert.AreEqual(0, data.inventory.Count);
            Assert.AreEqual(15, data.defenseGrid.Length);
            Assert.AreEqual(0, data.waveProgress);
        }

        [Test]
        public void Serialize_ProducesValidJson()
        {
            var data = SaveData.CreateDefault();
            data.gold = 500;
            data.upgradeLevels["Pickaxe"] = 2;

            string json = SaveSystem.Serialize(data);
            Assert.IsTrue(json.Contains("\"gold\": 500"));
            Assert.IsTrue(json.Contains("\"Pickaxe\": 2"));
        }

        [Test]
        public void Deserialize_RoundTrip()
        {
            var original = SaveData.CreateDefault();
            original.gold = 999;
            original.depthLevel = 2;
            original.upgradeLevels["Armor"] = 3;

            string json = SaveSystem.Serialize(original);
            var restored = SaveSystem.Deserialize(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual(original.version, restored.version);
            Assert.AreEqual(original.gold, restored.gold);
            Assert.AreEqual(original.depthLevel, restored.depthLevel);
            Assert.AreEqual(original.upgradeLevels["Armor"], restored.upgradeLevels["Armor"]);
        }

        [Test]
        public void Deserialize_Null_ReturnsNull()
        {
            Assert.IsNull(SaveSystem.Deserialize(null));
        }

        [Test]
        public void Deserialize_Empty_ReturnsNull()
        {
            Assert.IsNull(SaveSystem.Deserialize(""));
        }

        [Test]
        public void Deserialize_WrongVersion_ReturnsNull()
        {
            var data = SaveData.CreateDefault();
            data.version = 99; // invalid
            string json = SaveSystem.Serialize(data);
            Assert.IsNull(SaveSystem.Deserialize(json));
        }

        [Test]
        public void InventoryEntry_FromItem_RoundTrip()
        {
            var item = new InventoryItem(MineralType.Gold, 40f, 3);
            var entry = InventoryEntry.FromItem(item);

            Assert.AreEqual("Gold", entry.mineralType);
            Assert.AreEqual(3, entry.count);
            Assert.AreEqual(40f, entry.sellPrice, 0.01f);
            Assert.AreEqual(MineralType.Gold, entry.GetMineralType());
        }

        [Test]
        public void InventoryEntry_GetMineralType_Invalid_DefaultsToStone()
        {
            var entry = new InventoryEntry { mineralType = "NotAMineral", count = 1, sellPrice = 0f };
            Assert.AreEqual(MineralType.Stone, entry.GetMineralType());
        }
    }

    public class SaveSystemTests
    {
        private GameObject _go;
        private SaveSystem _save;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("TestSave");
            _save = _go.AddComponent<SaveSystem>();
            _save.Init();
        }

        [TearDown]
        public void TearDown()
        {
            _save.DeleteAll();
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void Save_Load_RoundTrip()
        {
            var data = SaveData.CreateDefault();
            data.gold = 420;
            data.depthLevel = 1;
            data.upgradeLevels["Pickaxe"] = 2;

            bool saved = _save.Save(data);
            Assert.IsTrue(saved);

            var loaded = _save.Load();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(420, loaded.gold);
            Assert.AreEqual(1, loaded.depthLevel);
            Assert.AreEqual(2, loaded.upgradeLevels["Pickaxe"]);
        }

        [Test]
        public void Load_NoSaveFile_ReturnsNull()
        {
            Assert.IsNull(_save.Load());
            Assert.IsFalse(_save.HasSave());
        }

        [Test]
        public void HasSave_TrueAfterSave()
        {
            Assert.IsFalse(_save.HasSave());
            _save.Save(SaveData.CreateDefault());
            Assert.IsTrue(_save.HasSave());
        }

        [Test]
        public void Save_BackupRotation_CreatesBackups()
        {
            // Save 4 times — should have main + 3 backups
            for (int i = 0; i < 4; i++)
            {
                var data = SaveData.CreateDefault();
                data.gold = i * 100;
                _save.Save(data);
            }

            // Most recent save should still load
            var loaded = _save.Load();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(300, loaded.gold);
        }

        [Test]
        public void Save_Null_ReturnsFalse()
        {
            Assert.IsFalse(_save.Save(null));
        }

        [Test]
        public void DeleteAll_RemovesSave()
        {
            _save.Save(SaveData.CreateDefault());
            Assert.IsTrue(_save.HasSave());

            _save.DeleteAll();
            Assert.IsFalse(_save.HasSave());
        }
    }
}
