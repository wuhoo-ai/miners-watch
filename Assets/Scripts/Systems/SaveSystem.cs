using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// JSON save/load system with version check and backup rotation.
    /// All file I/O through Save/Load methods — testable without real filesystem
    /// via Serialize/Deserialize helpers.
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        [Header("Save Config")]
        [SerializeField] private int _maxBackups = 3;
        [SerializeField] private string _saveFileName = "minerswatch_save.json";

        private string SaveDir => Application.persistentDataPath;
        private string SavePath(int slot) => Path.Combine(SaveDir, slot == 0
            ? _saveFileName
            : $"{Path.GetFileNameWithoutExtension(_saveFileName)}_backup{slot}.json");

        public void Init()
        {
            if (_maxBackups <= 0) _maxBackups = 3;
            if (string.IsNullOrEmpty(_saveFileName)) _saveFileName = "minerswatch_save.json";
        }

        private void Awake() => Init();

        /// <summary>Serialize SaveData to JSON string.</summary>
        public static string Serialize(SaveData data)
        {
            return JsonUtility.ToJson(data, prettyPrint: true);
        }

        /// <summary>Deserialize JSON string to SaveData. Returns null if invalid or version mismatch.</summary>
        public static SaveData Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;

            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return null;
            if (data.version != SaveData.CurrentVersion) return null;
            return data;
        }

        /// <summary>Save to disk with backup rotation.</summary>
        public bool Save(SaveData data)
        {
            if (data == null) return false;

            try
            {
                string json = Serialize(data);

                // Rotate backups: shift backup2→backup3, backup1→backup2, main→backup1
                for (int i = _maxBackups; i >= 1; i--)
                {
                    string src = SavePath(i - 1);
                    string dst = SavePath(i);
                    if (File.Exists(src))
                    {
                        if (File.Exists(dst)) File.Delete(dst);
                        File.Move(src, dst);
                    }
                }

                // Write new main save
                File.WriteAllText(SavePath(0), json);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SaveSystem.Save failed: {e.Message}");
                return false;
            }
        }

        /// <summary>Load from disk. Returns null if no save file or version mismatch.</summary>
        public SaveData Load()
        {
            try
            {
                string path = SavePath(0);
                if (!File.Exists(path)) return null;

                string json = File.ReadAllText(path);
                return Deserialize(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SaveSystem.Load failed: {e.Message}");
                return null;
            }
        }

        /// <summary>Check if a save file exists.</summary>
        public bool HasSave()
        {
            return File.Exists(SavePath(0));
        }

        /// <summary>Delete all save files.</summary>
        public void DeleteAll()
        {
            for (int i = 0; i <= _maxBackups; i++)
            {
                string path = SavePath(i);
                if (File.Exists(path)) File.Delete(path);
            }
        }
    }
}
