using UnityEngine;
using UnityEngine.UI;

namespace MinersWatch
{
    /// <summary>
    /// Day-night HUD: countdown timer, phase label, warning flash.
    /// Subscribes to DayNightCycle events for reactive updates.
    /// </summary>
    public class DayNightUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DayNightCycle _cycle;
        [SerializeField] private Text _phaseText;
        [SerializeField] private Text _timerText;
        [SerializeField] private GameObject _warningPanel;

        public void Init(DayNightCycle cycle)
        {
            _cycle = cycle;
            if (_cycle != null)
            {
                _cycle.OnPhaseChanged += _ => RefreshDisplay();
                _cycle.OnWarning += OnWarningTriggered;
            }
            RefreshDisplay();
        }

        private void Awake()
        {
            if (_cycle == null)
                _cycle = GetComponent<DayNightCycle>() ?? GetComponentInParent<DayNightCycle>() ?? GameRoot.Get<DayNightCycle>();

            if (_cycle != null)
            {
                _cycle.OnPhaseChanged += _ => RefreshDisplay();
                _cycle.OnWarning += OnWarningTriggered;
                RefreshDisplay();
            }
        }

        private void OnDestroy()
        {
            if (_cycle != null)
            {
                _cycle.OnPhaseChanged -= _ => RefreshDisplay();
                _cycle.OnWarning -= OnWarningTriggered;
            }
        }

        private void Update()
        {
            // Per-frame timer refresh (no event for every second)
            if (_cycle != null && _timerText != null)
                _timerText.text = FormatTime(_cycle.TimeRemaining);
        }

        private void OnWarningTriggered()
        {
            if (_warningPanel != null)
                _warningPanel.SetActive(true);
        }

        public void RefreshDisplay()
        {
            if (_cycle == null) return;

            if (_phaseText != null)
                _phaseText.text = PhaseLabel(_cycle.CurrentPhase);

            if (_warningPanel != null && _cycle.CurrentPhase != DayNightPhase.Day)
                _warningPanel.SetActive(false);
        }

        private static string PhaseLabel(DayNightPhase phase) => phase switch
        {
            DayNightPhase.Day => "☀ Day",
            DayNightPhase.NightTransition => "⚠ Night Falls",
            DayNightPhase.Night => "🌙 Night",
            DayNightPhase.Settlement => "📊 Settlement",
            _ => "???"
        };

        private static string FormatTime(float seconds)
        {
            int m = Mathf.FloorToInt(seconds / 60f);
            int s = Mathf.FloorToInt(seconds % 60f);
            return $"{m:00}:{s:00}";
        }
    }
}
