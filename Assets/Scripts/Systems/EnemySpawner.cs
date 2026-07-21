using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// W9: Enemy spawner — takes WaveManager tick signals and actually spawns
    /// Enemy GameObjects around the BaseCore at night. Cleans up all enemies
    /// when dawn breaks.
    /// 
    /// Spawns are staggered (one per frame) to avoid frame spikes.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Zone")]
        [SerializeField] private float _spawnRadiusMin = 5f;
        [SerializeField] private float _spawnRadiusMax = 9f;
        [SerializeField] private Transform _coreTarget; // BaseCore transform (enemies walk toward this)

        private WaveManager _waves;
        private DayNightCycle _cycle;
        private int _enemiesAlive;
        private bool _spawning;
        private List<Enemy> _activeEnemies = new List<Enemy>();

        public int EnemiesAlive => _enemiesAlive;
        public bool HasActiveEnemies => _enemiesAlive > 0;

        // ── lifecycle ────────────────────────────────────────

        private void Start()
        {
            _waves = GetComponent<WaveManager>();
            _cycle = GetComponent<DayNightCycle>();
            if (_cycle != null) _cycle.OnPhaseChanged += OnPhase;
            if (_coreTarget == null)
            {
                var core = GameObject.Find("BaseCore");
                if (core != null) _coreTarget = core.transform;
            }
        }

        private void OnDestroy()
        {
            if (_cycle != null) _cycle.OnPhaseChanged -= OnPhase;
        }

        private void Update()
        {
            if (_waves == null || !_waves.IsNightActive || _spawning) return;

            // Tick wave manager. If a new wave should spawn, start the spawn coroutine.
            if (_waves.Tick(Time.deltaTime))
            {
                var cfg = _waves.GetCurrentWaveConfig();
                if (cfg != null)
                    StartCoroutine(SpawnWave(cfg));
            }
        }

        // ── phase callbacks ──────────────────────────────────

        private void OnPhase(DayNightPhase phase)
        {
            if (phase == DayNightPhase.Night && _waves != null)
            {
                _waves.StartNight();
            }
            else if (phase == DayNightPhase.Day)
            {
                DespawnAll();
                if (_waves != null) _waves.EndNight();
            }
        }

        // ── spawning ─────────────────────────────────────────

        private IEnumerator SpawnWave(WaveConfig cfg)
        {
            _spawning = true;
            for (int i = 0; i < cfg.enemyCount; i++)
            {
                SpawnOne(cfg.enemyType);
                yield return null; // one per frame
            }
            _spawning = false;
        }

        private void SpawnOne(EnemyType type)
        {
            // Spawn at a random point around the core
            Vector2 dir = Random.insideUnitCircle.normalized;
            float dist = Random.Range(_spawnRadiusMin, _spawnRadiusMax);
            Vector3 pos = (_coreTarget != null ? _coreTarget.position : Vector3.zero)
                          + new Vector3(dir.x * dist, dir.y * dist, 0);

            var go = new GameObject($"Enemy_{type}_{_enemiesAlive}");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.5f;

            // Visual: procedural pixel-art sprite per enemy type
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.Get(type);
            sr.color = Color.white;
            sr.sortingOrder = 3;

            // Collider for Turret detection + AI movement
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;

            var enemy = go.AddComponent<Enemy>();
            enemy.Init(type);

            var ai = go.AddComponent<EnemyAI>();
            ai.Init(enemy, _coreTarget);

            enemy.OnKilled += () =>
            {
                _enemiesAlive--;
                _activeEnemies.Remove(enemy);
            };

            _enemiesAlive++;
            _activeEnemies.Add(enemy);
        }

        private void DespawnAll()
        {
            StopAllCoroutines();
            _spawning = false;
            foreach (var e in _activeEnemies)
            {
                if (e != null) Destroy(e.gameObject);
            }
            _activeEnemies.Clear();
            _enemiesAlive = 0;
        }

    }
}
