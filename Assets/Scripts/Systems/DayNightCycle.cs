using System;
using UnityEngine;

namespace MinersWatch
{
    public enum DayNightPhase { Day, NightTransition, Night, Settlement }

    /// <summary>
    /// Day-night cycle state machine. Uses explicit Tick(dt) for EditMode testability.
    /// Runtime: Update() → Tick(Time.deltaTime).
    /// </summary>
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Durations")]
        [SerializeField] private float _dayDuration = 120f;
        [SerializeField] private float _nightDuration = 90f;
        [SerializeField] private float _transitionDuration = 3f;
        [SerializeField] private float _settlementDuration = 5f;
        [SerializeField] private float _warningTime = 10f;

        private DayNightPhase _phase = DayNightPhase.Day;
        private float _elapsed;
        private bool _warningFired;

        public DayNightPhase CurrentPhase => _phase;
        public float PhaseElapsed => _elapsed;
        public float PhaseDuration => GetPhaseDuration(_phase);
        public float TimeRemaining => Mathf.Max(0f, PhaseDuration - _elapsed);
        public bool IsWarningActive => _phase == DayNightPhase.Day && TimeRemaining <= _warningTime && TimeRemaining > 0f;

        public event Action<DayNightPhase> OnPhaseChanged;
        public event Action OnWarning;

        public void Init()
        {
            _phase = DayNightPhase.Day;
            _elapsed = 0f;
            _warningFired = false;
            GuardDurations();
        }

        private void Awake() => Init();

        private void GuardDurations()
        {
            if (_dayDuration <= 0f) _dayDuration = 120f;
            if (_nightDuration <= 0f) _nightDuration = 90f;
            if (_transitionDuration <= 0f) _transitionDuration = 3f;
            if (_settlementDuration <= 0f) _settlementDuration = 5f;
            if (_warningTime <= 0f) _warningTime = 10f;
        }

        private void Update() => Tick(Time.deltaTime);

        /// <summary>Advance the cycle by dt seconds. Testable entry point.</summary>
        public void ResetToDayStart() { _elapsed = 0f; _currentPhase = DayNightPhase.Day; }

        public void Tick(float dt)
        {
            if (dt <= 0f) return;

            _elapsed += dt;

            // Warning check (during Day phase only)
            if (_phase == DayNightPhase.Day && !_warningFired && TimeRemaining <= _warningTime)
            {
                _warningFired = true;
                OnWarning?.Invoke();
            }

            // Phase transition check
            while (_elapsed >= PhaseDuration)
            {
                _elapsed -= PhaseDuration;
                _phase = NextPhase(_phase);
                _warningFired = false;
                OnPhaseChanged?.Invoke(_phase);
            }
        }

        private float GetPhaseDuration(DayNightPhase phase) => phase switch
        {
            DayNightPhase.Day => _dayDuration,
            DayNightPhase.NightTransition => _transitionDuration,
            DayNightPhase.Night => _nightDuration,
            DayNightPhase.Settlement => _settlementDuration,
            _ => _dayDuration
        };

        private static DayNightPhase NextPhase(DayNightPhase current) => current switch
        {
            DayNightPhase.Day => DayNightPhase.NightTransition,
            DayNightPhase.NightTransition => DayNightPhase.Night,
            DayNightPhase.Night => DayNightPhase.Settlement,
            DayNightPhase.Settlement => DayNightPhase.Day,
            _ => DayNightPhase.Day
        };
    }
}
