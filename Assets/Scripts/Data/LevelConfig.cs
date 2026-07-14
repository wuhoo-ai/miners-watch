using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// ScriptableObject that defines the current level's depth.
    /// Referenced by MineralSpawner to determine which minerals to spawn.
    /// Defaults to Shallow if not assigned.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "MinersWatch/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        public DepthLevel levelDepth = DepthLevel.Shallow;
    }
}
