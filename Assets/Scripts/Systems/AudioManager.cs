using UnityEngine;
using UnityEngine.Audio;

namespace MinersWatch
{
    /// <summary>
    /// Simple audio manager: plays SFX by name, loops BGM by context (day/night/boss).
    /// All audio loaded from Resources/Audio folder. Volume controlled via PlayerPrefs.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Volume (0-1)")]
        [SerializeField] private float _masterVolume = 0.8f;
        [SerializeField] private float _sfxVolume = 1f;
        [SerializeField] private float _bgmVolume = 0.6f;

        private AudioSource _sfxSource;
        private AudioSource _bgmSource;

        private static AudioManager _instance;
        public static AudioManager Instance => _instance;

        public float MasterVolume
        {
            get => _masterVolume;
            set { _masterVolume = Mathf.Clamp01(value); ApplyVolumes(); }
        }
        public float SfxVolume
        {
            get => _sfxVolume;
            set { _sfxVolume = Mathf.Clamp01(value); ApplyVolumes(); }
        }
        public float BgmVolume
        {
            get => _bgmVolume;
            set { _bgmVolume = Mathf.Clamp01(value); ApplyVolumes(); }
        }

        private void Awake()
        {
            if (_instance != null) { Destroy(gameObject); return; }
            _instance = this;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.loop = false;

            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.playOnAwake = false;
            _bgmSource.loop = true;
            _bgmSource.spatialBlend = 0f;

            LoadVolumes();
            ApplyVolumes();

            // Hook day/night cycle for BGM switching
            var cycle = GetComponent<DayNightCycle>();
            if (cycle != null)
                cycle.OnPhaseChanged += OnDayNightPhaseChanged;
        }

        private void OnDayNightPhaseChanged(DayNightPhase phase)
        {
            switch (phase)
            {
                case DayNightPhase.Day: PlayDayBGM(); break;
                case DayNightPhase.NightTransition: PlaySFX("sfx_day_night_transition"); break;
                case DayNightPhase.Night: PlayNightBGM(); break;
                case DayNightPhase.Settlement: PlayDayBGM(); break;
            }
        }

        private void LoadVolumes()
        {
            _masterVolume = PlayerPrefs.GetFloat("audio_master", 0.8f);
            _sfxVolume = PlayerPrefs.GetFloat("audio_sfx", 1f);
            _bgmVolume = PlayerPrefs.GetFloat("audio_bgm", 0.6f);
        }

        public void SaveVolumes()
        {
            PlayerPrefs.SetFloat("audio_master", _masterVolume);
            PlayerPrefs.SetFloat("audio_sfx", _sfxVolume);
            PlayerPrefs.SetFloat("audio_bgm", _bgmVolume);
            PlayerPrefs.Save();
        }

        private void ApplyVolumes()
        {
            if (_sfxSource != null) _sfxSource.volume = _masterVolume * _sfxVolume;
            if (_bgmSource != null) _bgmSource.volume = _masterVolume * _bgmVolume;
        }

        /// <summary>Play a one-shot SFX. Returns false if clip not found.</summary>
        public bool PlaySFX(string name)
        {
            var clip = Resources.Load<AudioClip>($"Audio/SFX/{name}");
            if (clip == null)
            {
                Debug.LogWarning($"[Audio] SFX '{name}' not found");
                return false;
            }
            _sfxSource.PlayOneShot(clip);
            return true;
        }

        /// <summary>Play/switch looping BGM. Pass null to stop.</summary>
        public void PlayBGM(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                _bgmSource.Stop();
                _bgmSource.clip = null;
                return;
            }
            var clip = Resources.Load<AudioClip>($"Audio/BGM/{name}");
            if (clip == null)
            {
                Debug.LogWarning($"[Audio] BGM '{name}' not found");
                return;
            }
            if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;
            _bgmSource.clip = clip;
            _bgmSource.Play();
        }

        /// <summary>Start day BGM.</summary>
        public void PlayDayBGM() => PlayBGM("bgm_day");
        /// <summary>Start night BGM.</summary>
        public void PlayNightBGM() => PlayBGM("bgm_night");
        /// <summary>Start boss BGM.</summary>
        public void PlayBossBGM() => PlayBGM("bgm_boss");

        /// <summary>Play mine SFX (random 1/2).</summary>
        public void PlayMine() => PlaySFX(Random.value < 0.5f ? "sfx_mine_01" : "sfx_mine_02");

        /// <summary>Play enemy hit SFX (random 1/2).</summary>
        public void PlayEnemyHit() => PlaySFX(Random.value < 0.5f ? "sfx_enemy_hit_01" : "sfx_enemy_hit_02");

        public void PlayEnemyDeath() => PlaySFX("sfx_enemy_death");
        public void PlayBuild() => PlaySFX("sfx_build");
        public void PlaySell() => PlaySFX("sfx_sell");
        public void PlayDayNightTransition() => PlaySFX("sfx_day_night_transition");
        public void PlayBossRoar() => PlaySFX("sfx_boss_roar");
        public void PlayVictory() => PlaySFX("sfx_victory");
        public void PlayGameOver() => PlaySFX("sfx_game_over");

        // --- Convenience static shortcuts ---
        public static void SFX(string name) => _instance?.PlaySFX(name);
        public static void BGM(string name) => _instance?.PlayBGM(name);

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
