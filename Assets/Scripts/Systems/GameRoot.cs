using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// 方案A: persistent system host. Survives Surface⇄Cave scene swaps via DontDestroyOnLoad.
    /// Auto-created before the first scene loads; scene-local components resolve shared
    /// systems through GameRoot.Get&lt;T&gt;() as the last link of their Awake fallback chains.
    /// EditMode-safe: never auto-created outside play mode, Get() returns null when absent.
    /// </summary>
    public class GameRoot : MonoBehaviour
    {
        private static GameRoot _instance;
        public static GameRoot Instance => _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap() => EnsureExists();

        public static GameRoot EnsureExists()
        {
            if (_instance != null) return _instance;
            _instance = FindObjectOfType<GameRoot>();
            if (_instance != null) return _instance;

            var go = new GameObject("GameRoot");
            _instance = go.AddComponent<GameRoot>();
            go.AddComponent<InventorySystem>();
            go.AddComponent<UpgradeSystem>();
            go.AddComponent<ShopSystem>();
            go.AddComponent<DayNightCycle>();
            go.AddComponent<DepthProgression>();
            go.AddComponent<SceneController>();
            go.AddComponent<SaveSystem>();
            go.AddComponent<NightCurfew>();
            return _instance;
        }

        /// <summary>Resolve a shared system from the persistent root. Null-safe when no root exists.</summary>
        public static T Get<T>() where T : Component =>
            _instance != null ? _instance.GetComponent<T>() : null;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }

    /// <summary>
    /// Core-loop enforcer: when night falls while the player is inside a cave,
    /// force the return to the surface defense zone (GDD: 夜晚固定在地面防御区).
    /// </summary>
    public class NightCurfew : MonoBehaviour
    {
        private DayNightCycle _cycle;
        private SceneController _scenes;

        private void Start()
        {
            _cycle = GetComponent<DayNightCycle>();
            _scenes = GetComponent<SceneController>();
            if (_cycle != null) _cycle.OnPhaseChanged += OnPhase;
        }

        private void OnDestroy()
        {
            if (_cycle != null) _cycle.OnPhaseChanged -= OnPhase;
        }

        private void OnPhase(DayNightPhase phase)
        {
            if (phase == DayNightPhase.NightTransition && _scenes != null && _scenes.IsInCave)
                _scenes.LoadSurface();
        }
    }
}
