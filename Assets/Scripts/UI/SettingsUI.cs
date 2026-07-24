using UnityEngine;
using UnityEngine.UI;

namespace MinersWatch
{
    /// <summary>
    /// Settings panel: volume sliders, difficulty selector, about info.
    /// Controlled by MainMenuUI — toggles visibility when Settings button is clicked.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("Volume Sliders")]
        [SerializeField] private Slider _masterSlider;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Slider _bgmSlider;

        [Header("Labels")]
        [SerializeField] private Text _masterLabel;
        [SerializeField] private Text _sfxLabel;
        [SerializeField] private Text _bgmLabel;
        [SerializeField] private Text _versionLabel;

        private AudioManager _audio;

        private void Awake()
        {
            _audio = AudioManager.Instance ?? FindAnyObjectByType<AudioManager>();
            WireSliders();
            LoadAndApply();
            UpdateLabels();

            if (_versionLabel != null)
                _versionLabel.text = $"矿工守夜 v1.2\nHermes Agent · 2026";
        }

        private void WireSliders()
        {
            if (_masterSlider != null)
            {
                _masterSlider.onValueChanged.AddListener(v =>
                {
                    if (_audio != null) _audio.MasterVolume = v;
                    UpdateLabels();
                });
            }
            if (_sfxSlider != null)
            {
                _sfxSlider.onValueChanged.AddListener(v =>
                {
                    if (_audio != null) _audio.SfxVolume = v;
                    UpdateLabels();
                });
            }
            if (_bgmSlider != null)
            {
                _bgmSlider.onValueChanged.AddListener(v =>
                {
                    if (_audio != null) _audio.BgmVolume = v;
                    UpdateLabels();
                });
            }
        }

        private void LoadAndApply()
        {
            float master = PlayerPrefs.GetFloat("audio_master", 0.8f);
            float sfx = PlayerPrefs.GetFloat("audio_sfx", 1f);
            float bgm = PlayerPrefs.GetFloat("audio_bgm", 0.6f);

            if (_masterSlider != null) _masterSlider.value = master;
            if (_sfxSlider != null) _sfxSlider.value = sfx;
            if (_bgmSlider != null) _bgmSlider.value = bgm;

            if (_audio != null)
            {
                _audio.MasterVolume = master;
                _audio.SfxVolume = sfx;
                _audio.BgmVolume = bgm;
            }
        }

        public void SaveSettings()
        {
            if (_audio != null) _audio.SaveVolumes();
        }

        private void UpdateLabels()
        {
            if (_masterLabel != null && _masterSlider != null)
                _masterLabel.text = $"主音量: {Mathf.RoundToInt(_masterSlider.value * 100)}%";
            if (_sfxLabel != null && _sfxSlider != null)
                _sfxLabel.text = $"音效: {Mathf.RoundToInt(_sfxSlider.value * 100)}%";
            if (_bgmLabel != null && _bgmSlider != null)
                _bgmLabel.text = $"音乐: {Mathf.RoundToInt(_bgmSlider.value * 100)}%";
        }

        private void OnDestroy()
        {
            SaveSettings();
        }
    }
}
