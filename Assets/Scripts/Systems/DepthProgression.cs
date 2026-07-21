using System;
using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Depth progression: tracks accumulated gold, unlocks deeper levels.
    /// Pure logic — no scene dependency. Testable via Init().
    /// </summary>
    public class DepthProgression : MonoBehaviour
    {
        [Header("Unlock Thresholds")]
        [SerializeField] private int _midUnlockGold = 50;
        [SerializeField] private int _deepUnlockGold = 100;

        private int _accumulatedGold;
        private DepthLevel _currentDepth = DepthLevel.Shallow;

        public DepthLevel CurrentDepth => _currentDepth;
        public int AccumulatedGold => _accumulatedGold;
        public bool IsMidUnlocked => _accumulatedGold >= _midUnlockGold;
        public bool IsDeepUnlocked => _accumulatedGold >= _deepUnlockGold;

        public event Action<DepthLevel> OnDepthUnlocked;

        public void Init()
        {
            _accumulatedGold = 0;
            _currentDepth = DepthLevel.Shallow;
            if (_midUnlockGold <= 0) _midUnlockGold = 50;
            if (_deepUnlockGold <= 0) _deepUnlockGold = 100;
        }

        private void Awake() => Init();

        /// <summary>Reset for new game session.</summary>
        public void Reset() => Init();

        /// <summary>Add earnings to accumulated total. Returns newly unlocked depths.</summary>
        public void AddEarnings(int gold)
        {
            if (gold <= 0) return;

            bool hadMid = IsMidUnlocked;
            bool hadDeep = IsDeepUnlocked;

            _accumulatedGold += gold;

            if (!hadMid && IsMidUnlocked)
            {
                OnDepthUnlocked?.Invoke(DepthLevel.Medium);
            }
            if (!hadDeep && IsDeepUnlocked)
            {
                OnDepthUnlocked?.Invoke(DepthLevel.Deep);
            }
        }

        /// <summary>Check if player can enter a specific depth.</summary>
        public bool CanEnterDepth(DepthLevel depth) => depth switch
        {
            DepthLevel.Shallow => true,
            DepthLevel.Medium => IsMidUnlocked,
            DepthLevel.Deep => IsDeepUnlocked,
            _ => false,
        };

        /// <summary>Set current depth (called when entering a scene).</summary>
        public void SetDepth(DepthLevel depth)
        {
            if (CanEnterDepth(depth))
                _currentDepth = depth;
        }
    }
}
