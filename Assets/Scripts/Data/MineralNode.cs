using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Component attached to mineral node GameObjects in the scene.
    /// Holds a reference to the MineralData ScriptableObject that defines this mineral's properties.
    /// </summary>
    public class MineralNode : MonoBehaviour
    {
        [SerializeField] private MineralData mineralData;

        public MineralData MineralData => mineralData;

        public void Init(MineralData data)
        {
            mineralData = data;
        }
    }
}
