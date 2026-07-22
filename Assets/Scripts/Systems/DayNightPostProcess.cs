using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MinersWatch
{
    /// <summary>
    /// Controls URP post-processing based on day/night cycle.
    /// Hooks DayNightCycle.OnPhaseChanged to lerp ColorAdjustments and Bloom
    /// for smooth day→night transitions.
    /// 
    /// Requires: a global Volume with a VolumeProfile containing:
    ///   - Bloom (for crystal glow at night)
    ///   - Vignette (for cave atmosphere)
    ///   - ColorAdjustments (for color filter + exposure)
    /// </summary>
    public class DayNightPostProcess : MonoBehaviour
    {
        [Header("Bloom")]
        [SerializeField] private float _bloomIntensity = 0.15f;
        [SerializeField] private float _bloomThreshold = 0.9f;

        [Header("Vignette")]
        [SerializeField] private float _vignetteIntensity = 0.3f;
        [SerializeField] private Color _vignetteColor = new Color(0.05f, 0.05f, 0.1f);

        [Header("Day/Night Color")]
        [SerializeField] private Color _dayFilter = Color.white;
        [SerializeField] private Color _nightFilter = new Color(0.7f, 0.75f, 1f);
        [SerializeField] private float _dayExposure = 0f;
        [SerializeField] private float _nightExposure = -0.4f;
        [SerializeField] private float _transitionSpeed = 2f;

        [Header("Volume Reference")]
        [SerializeField] private Volume _globalVolume;

        private Bloom _bloom;
        private Vignette _vignette;
        private ColorAdjustments _colorAdjustments;
        private DayNightCycle _cycle;

        private Color _targetFilter;
        private float _targetExposure;
        private bool _isNight;

        private void Awake()
        {
            _cycle = GetComponent<DayNightCycle>();
            if (_cycle != null)
                _cycle.OnPhaseChanged += OnPhaseChanged;

            if (_globalVolume == null)
                _globalVolume = FindObjectOfType<Volume>();

            if (_globalVolume != null && _globalVolume.profile != null)
            {
                _globalVolume.profile.TryGet(out _bloom);
                _globalVolume.profile.TryGet(out _vignette);
                _globalVolume.profile.TryGet(out _colorAdjustments);
            }

            _targetFilter = _dayFilter;
            _targetExposure = _dayExposure;
            ApplyImmediate();
        }

        private void OnDestroy()
        {
            if (_cycle != null)
                _cycle.OnPhaseChanged -= OnPhaseChanged;
        }

        private void OnPhaseChanged(DayNightPhase phase)
        {
            _isNight = phase == DayNightPhase.Night || phase == DayNightPhase.NightTransition;
            _targetFilter = _isNight ? _nightFilter : _dayFilter;
            _targetExposure = _isNight ? _nightExposure : _dayExposure;
        }

        private void Update()
        {
            // Lazy-find global Volume (it lives in Surface/Cave scenes, loaded additively)
            if (_globalVolume == null)
            {
                _globalVolume = FindObjectOfType<Volume>();
                if (_globalVolume != null && _globalVolume.profile != null)
                {
                    _globalVolume.profile.TryGet(out _bloom);
                    _globalVolume.profile.TryGet(out _vignette);
                    _globalVolume.profile.TryGet(out _colorAdjustments);
                    ApplyImmediate();
                }
                else return; // no volume yet, try again next frame
            }

            float t = Time.deltaTime * _transitionSpeed;

            // Smooth color filter transition (day=white, night=cool blue)
            if (_colorAdjustments.colorFilter.overrideState)
                _colorAdjustments.colorFilter.value = Color.Lerp(
                    _colorAdjustments.colorFilter.value, _targetFilter, t);

            // Smooth exposure transition (night = slightly darker)
            if (_colorAdjustments.postExposure.overrideState)
                _colorAdjustments.postExposure.value = Mathf.Lerp(
                    _colorAdjustments.postExposure.value, _targetExposure, t);

            // Boost bloom at night (crystals/campfire glow)
            if (_bloom != null && _bloom.intensity.overrideState)
            {
                float targetBloom = _isNight ? _bloomIntensity * 1.5f : _bloomIntensity;
                _bloom.intensity.value = Mathf.Lerp(_bloom.intensity.value, targetBloom, t);
            }
        }

        private void ApplyImmediate()
        {
            if (_bloom != null)
            {
                _bloom.intensity.value = _bloomIntensity;
                _bloom.threshold.value = _bloomThreshold;
                _bloom.active = true;
            }
            if (_vignette != null)
            {
                _vignette.intensity.value = _vignetteIntensity;
                _vignette.color.value = _vignetteColor;
                _vignette.active = true;
            }
            if (_colorAdjustments != null)
            {
                _colorAdjustments.colorFilter.value = _dayFilter;
                _colorAdjustments.postExposure.value = _dayExposure;
                _colorAdjustments.active = true;
            }
        }

        /// <summary>Called by SceneAuthor to inject the global volume reference.</summary>
        public void SetVolume(Volume volume)
        {
            _globalVolume = volume;
            if (volume != null && volume.profile != null)
            {
                volume.profile.TryGet(out _bloom);
                volume.profile.TryGet(out _vignette);
                volume.profile.TryGet(out _colorAdjustments);
                ApplyImmediate();
            }
        }
    }
}
