using UnityEngine;
using UnityEngine.UI;

namespace MinersWatch
{
    /// <summary>
    /// Surface cave-entry panel: three depth buttons gated by DepthProgression unlocks
    /// (mid ≥ $500 accumulated, deep ≥ $2000). Locked buttons are non-interactable and
    /// show the unlock price. Refreshes on enable and on depth unlock events.
    /// </summary>
    public class CaveEntryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button _shallowButton;
        [SerializeField] private Button _midButton;
        [SerializeField] private Button _deepButton;
        [SerializeField] private Text _midLabel;
        [SerializeField] private Text _deepLabel;

        private DepthProgression _progression;
        private SceneController _scenes;

        private void Awake()
        {
            _progression = GameRoot.Get<DepthProgression>() ?? FindObjectOfType<DepthProgression>();
            _scenes = GameRoot.Get<SceneController>() ?? FindObjectOfType<SceneController>();

            if (_shallowButton != null) _shallowButton.onClick.AddListener(() => Enter(DepthLevel.Shallow));
            if (_midButton != null) _midButton.onClick.AddListener(() => Enter(DepthLevel.Medium));
            if (_deepButton != null) _deepButton.onClick.AddListener(() => Enter(DepthLevel.Deep));

            if (_progression != null) _progression.OnDepthUnlocked += _ => Refresh();
            Refresh();
        }

        private void OnEnable() => Refresh();

        private void Enter(DepthLevel depth)
        {
            if (_progression != null && !_progression.CanEnterDepth(depth)) return;
            _scenes?.LoadCave(depth);
        }

        public void Refresh()
        {
            bool mid = _progression == null || _progression.IsMidUnlocked;
            bool deep = _progression == null || _progression.IsDeepUnlocked;
            if (_midButton != null) _midButton.interactable = mid;
            if (_deepButton != null) _deepButton.interactable = deep;
            if (_midLabel != null) _midLabel.text = mid ? "中层洞穴" : "中层 🔒$500";
            if (_deepLabel != null) _deepLabel.text = deep ? "深层洞穴" : "深层 🔒$2000";
        }
    }
}
