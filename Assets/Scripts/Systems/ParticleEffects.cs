using System.Collections.Generic;
using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Particle effect manager with object pooling.
    /// Provides code-generated ParticleSystem presets (no prefab dependency).
    /// Singleton — auto-created on first access in play mode; EditMode-safe via Init().
    /// </summary>
    public class ParticleEffects : MonoBehaviour
    {
        public enum EffectType { MiningSpark, AttackSlash, EnemyDeath }

        private static ParticleEffects _instance;
        public static ParticleEffects Instance => _instance;

        [Header("Pool Settings")]
        [SerializeField] private int _initialPoolSize = 5;
        [SerializeField] private int _maxPoolSize = 20;

        private readonly Dictionary<EffectType, Queue<ParticleSystem>> _pools = new();
        private readonly Dictionary<ParticleSystem, EffectType> _activeEffects = new();
        private readonly Dictionary<ParticleSystem, float> _returnTimers = new();

        public int InitialPoolSize => _initialPoolSize;
        public int MaxPoolSize => _maxPoolSize;

        /// <summary>Number of active (playing) effects.</summary>
        public int ActiveCount => _activeEffects.Count;

        /// <summary>Number of pooled (idle) effects for a given type.</summary>
        public int GetPoolCount(EffectType type) =>
            _pools.TryGetValue(type, out var q) ? q.Count : 0;

        // ─── Singleton Bootstrap ───────────────────────────────────────────

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap() => EnsureExists();

        public static ParticleEffects EnsureExists()
        {
            if (_instance != null) return _instance;
            _instance = FindAnyObjectByType<ParticleEffects>();
            if (_instance != null) return _instance;

            var go = new GameObject("[ParticleEffects]");
            _instance = go.AddComponent<ParticleEffects>();
            if (Application.isPlaying) DontDestroyOnLoad(go);
            return _instance;
        }

        /// <summary>
        /// Initialize pools. Called from Awake() in play mode, or manually in EditMode tests.
        /// Safe to call multiple times (idempotent).
        /// </summary>
        public void Init(int initialPoolSize = 5, int maxPoolSize = 20)
        {
            _initialPoolSize = Mathf.Max(1, initialPoolSize);
            _maxPoolSize = Mathf.Max(_initialPoolSize, maxPoolSize);

            foreach (EffectType type in System.Enum.GetValues(typeof(EffectType)))
            {
                if (!_pools.ContainsKey(type))
                    _pools[type] = new Queue<ParticleSystem>();

                // Pre-warm pool
                while (_pools[type].Count < _initialPoolSize)
                {
                    var ps = CreateEffect(type);
                    ps.gameObject.SetActive(false);
                    _pools[type].Enqueue(ps);
                }
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            Init(_initialPoolSize, _maxPoolSize);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        // ─── Public API ────────────────────────────────────────────────────

        /// <summary>Play mining spark effect at position (gold sparks on ore hit).</summary>
        public static void PlayMiningEffect(Vector3 pos) => Play(EffectType.MiningSpark, pos);

        /// <summary>Play attack slash effect at position (arc trail on swing).</summary>
        public static void PlayAttackEffect(Vector3 pos) => Play(EffectType.AttackSlash, pos);

        /// <summary>Play enemy death effect at position (burst + fade).</summary>
        public static void PlayDeathEffect(Vector3 pos) => Play(EffectType.EnemyDeath, pos);

        /// <summary>Generic play by type. Returns the ParticleSystem instance (for testing).</summary>
        public static ParticleSystem Play(EffectType type, Vector3 pos)
        {
            var mgr = EnsureExists();
            return mgr.PlayInternal(type, pos);
        }

        // ─── Pool Logic ────────────────────────────────────────────────────

        private ParticleSystem PlayInternal(EffectType type, Vector3 pos)
        {
            if (!_pools.ContainsKey(type))
                _pools[type] = new Queue<ParticleSystem>();

            ParticleSystem ps;

            if (_pools[type].Count > 0)
            {
                ps = _pools[type].Dequeue();
            }
            else if (_activeEffects.Count + TotalPooled() < _maxPoolSize * 3)
            {
                // Expand pool if under global cap
                ps = CreateEffect(type);
            }
            else
            {
                // Pool exhausted — steal oldest active of same type
                ps = StealOldest(type);
                if (ps == null) return null;
            }

            ps.gameObject.SetActive(true);
            ps.transform.position = pos;
            ps.Clear();
            ps.Play();

            _activeEffects[ps] = type;

            // Schedule return to pool after main duration
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            _returnTimers[ps] = duration;

            return ps;
        }

        private void Update()
        {
            // Check active effects for completion
            var toRemove = new List<ParticleSystem>();
            foreach (var kvp in _returnTimers)
            {
                _returnTimers[kvp.Key] = kvp.Value - Time.deltaTime;
                if (_returnTimers[kvp.Key] <= 0f || !kvp.Key.isPlaying)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var ps in toRemove)
                ReturnToPool(ps);
        }

        /// <summary>Force-return an effect to its pool (test-friendly).</summary>
        public void ReturnToPool(ParticleSystem ps)
        {
            if (ps == null) return;
            if (!_activeEffects.TryGetValue(ps, out var type)) return;

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.gameObject.SetActive(false);
            _activeEffects.Remove(ps);
            _returnTimers.Remove(ps);

            if (_pools.ContainsKey(type))
                _pools[type].Enqueue(ps);
        }

        /// <summary>Return all active effects to pool.</summary>
        public void ReturnAll()
        {
            var list = new List<ParticleSystem>(_activeEffects.Keys);
            foreach (var ps in list)
                ReturnToPool(ps);
        }

        private ParticleSystem StealOldest(EffectType type)
        {
            foreach (var kvp in _activeEffects)
            {
                if (kvp.Value == type)
                {
                    var ps = kvp.Key;
                    _activeEffects.Remove(ps);
                    _returnTimers.Remove(ps);
                    return ps;
                }
            }
            return null;
        }

        private int TotalPooled()
        {
            int total = 0;
            foreach (var q in _pools.Values) total += q.Count;
            return total;
        }

        // ─── Effect Factories (code-generated ParticleSystem) ──────────────

        private ParticleSystem CreateEffect(EffectType type)
        {
            var go = new GameObject($"FX_{type}");
            go.transform.SetParent(transform);
            var ps = go.AddComponent<ParticleSystem>();

            switch (type)
            {
                case EffectType.MiningSpark:
                    ConfigureMiningSpark(ps);
                    break;
                case EffectType.AttackSlash:
                    ConfigureAttackSlash(ps);
                    break;
                case EffectType.EnemyDeath:
                    ConfigureEnemyDeath(ps);
                    break;
            }

            // No renderer material needed for basic particles (default is fine)
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
                renderer.material = new Material(Shader.Find("Sprites/Default"));

            return ps;
        }

        /// <summary>MiningSpark: short gold sparks burst on ore hit.</summary>
        private static void ConfigureMiningSpark(ParticleSystem ps)
        {
            var main = ps.main;
            main.duration = 0.3f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
            main.startColor = new Color(1f, 0.84f, 0.2f); // gold
            main.maxParticles = 30;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.enabled = true;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 15, 25) });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 45f;
            shape.radius = 0.05f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.9f, 0.3f, 1f),
                new Color(1f, 0.6f, 0f, 0f));

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 0f);

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-3f, -1f); // gravity-like fall
        }

        /// <summary>AttackSlash: arc swing trail effect.</summary>
        private static void ConfigureAttackSlash(ParticleSystem ps)
        {
            var main = ps.main;
            main.duration = 0.25f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startColor = new Color(0.8f, 0.9f, 1f, 0.9f); // white-blue
            main.maxParticles = 20;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 80f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Donut;
            shape.radius = 0.6f;
            shape.donutRadius = 0.05f;
            shape.arc = 120f; // arc sweep

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(
                new Color(0.7f, 0.85f, 1f, 0.9f),
                new Color(0.5f, 0.7f, 1f, 0f));

            var trails = ps.trails;
            trails.enabled = true;
            trails.lifetime = 0.15f;
            trails.minVertexDistance = 0.02f;
            trails.widthOverTrail = new ParticleSystem.MinMaxCurve(0.5f, 0f);
            trails.colorOverLifetime = new ParticleSystem.MinMaxGradient(
                new Color(0.8f, 0.9f, 1f, 0.7f),
                new Color(0.6f, 0.8f, 1f, 0f));
        }

        /// <summary>EnemyDeath: burst scatter + fade out.</summary>
        private static void ConfigureEnemyDeath(ParticleSystem ps)
        {
            var main = ps.main;
            main.duration = 0.6f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 7f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.9f, 0.2f, 0.2f), new Color(0.6f, 0.1f, 0.4f));
            main.maxParticles = 50;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.enabled = true;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 25, 40) });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.3f, 0.3f, 1f),
                new Color(0.4f, 0f, 0.2f, 0f));

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 0f);

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(1f, 3f);
        }

        // ─── Cleanup ───────────────────────────────────────────────────────

        /// <summary>Destroy all pooled and active effects. For test teardown.</summary>
        public void ClearAll()
        {
            foreach (var pool in _pools.Values)
            {
                while (pool.Count > 0)
                {
                    var ps = pool.Dequeue();
                    if (ps != null) DestroyImmediate(ps.gameObject);
                }
            }
            foreach (var ps in new List<ParticleSystem>(_activeEffects.Keys))
            {
                if (ps != null) DestroyImmediate(ps.gameObject);
            }
            _activeEffects.Clear();
            _returnTimers.Clear();
        }
    }
}
