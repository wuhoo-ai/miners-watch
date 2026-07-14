using UnityEngine;

namespace MinersWatch
{
    public enum MineralType
    {
        Stone,
        Iron,
        Gold,
        Crystal,
        Obsidian
    }

    public enum DepthLevel
    {
        Shallow,
        Medium,
        Deep
    }

    [CreateAssetMenu(fileName = "New Mineral", menuName = "MinersWatch/Mineral Data")]
    public class MineralData : ScriptableObject
    {
        public MineralType mineralType;
        public string mineralName;
        public float staminaCost;
        public float sellPrice;
        public DepthLevel[] depthLevels;

        /// <summary>
        /// Static factory method to create ScriptableObject instances at runtime without AssetDatabase.
        /// </summary>
        public static MineralData Create(MineralType type, string name, float staminaCost, float sellPrice, DepthLevel[] depthLevels)
        {
            var data = CreateInstance<MineralData>();
            data.mineralType = type;
            data.mineralName = name;
            data.staminaCost = staminaCost;
            data.sellPrice = sellPrice;
            data.depthLevels = depthLevels;
            return data;
        }
    }
}
