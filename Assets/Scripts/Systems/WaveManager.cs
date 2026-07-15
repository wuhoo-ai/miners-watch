using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MinersWatch
{
    /// <summary>Wave configuration for a single depth level.</summary>
    public class WaveConfig
    {
        public int waveNumber;
        public int enemyCount;
        public EnemyType enemyType;
    }

    /// <summary>
    /// Wave manager: tracks wave progression, decides when to spawn next wave.
    /// Pure logic — no spawning, just configuration. Testable via Init().
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("Wave Config")]
        [SerializeField] private float _waveInterval = 15f; // seconds between waves
        [SerializeField] private int _wavesPerNight = 3;

        private int _currentWave;
        private float _waveTimer;
        private DepthLevel _depth;
        private bool _nightActive;

        public int CurrentWave => _currentWave;
        public bool AllWavesComplete => _currentWave >= _wavesPerNight;
        public bool IsNightActive => _nightActive;

        /// <summary>Get the enemy config for the current wave.</summary>
        public WaveConfig GetCurrentWaveConfig()
        {
            if (_currentWave >= _wavesPerNight) return null;
            return GetWaveConfig(_depth, _currentWave);
        }

        public void Init(DepthLevel depth)
        {
            _depth = depth;
            _currentWave = 0;
            _waveTimer = 0f;
            _nightActive = false;
        }

        public void StartNight()
        {
            _nightActive = true;
            _currentWave = 0;
            _waveTimer = 0f;
        }

        public void EndNight()
        {
            _nightActive = false;
        }

        /// <summary>Advance by dt. Returns true when a new wave should spawn.</summary>
        public bool Tick(float dt)
        {
            if (!_nightActive || AllWavesComplete) return false;

            _waveTimer += dt;
            if (_waveTimer >= _waveInterval)
            {
                _waveTimer -= _waveInterval;
                _currentWave++;
                return true;
            }
            return false;
        }

        /// <summary>Static wave definitions — mirrors GDD §3.4.</summary>
        public static WaveConfig GetWaveConfig(DepthLevel depth, int waveIndex)
        {
            return depth switch
            {
                DepthLevel.Shallow => waveIndex switch
                {
                    0 => new WaveConfig { waveNumber = 1, enemyCount = 3, enemyType = EnemyType.Rockworm },
                    1 => new WaveConfig { waveNumber = 2, enemyCount = 5, enemyType = EnemyType.Rockworm },
                    _ => new WaveConfig { waveNumber = 3, enemyCount = 8, enemyType = EnemyType.Rockworm },
                },
                DepthLevel.Medium => waveIndex switch
                {
                    0 => new WaveConfig { waveNumber = 1, enemyCount = 3, enemyType = EnemyType.Rockworm },
                    1 => new WaveConfig { waveNumber = 2, enemyCount = 5, enemyType = EnemyType.Shadow },
                    _ => new WaveConfig { waveNumber = 3, enemyCount = 5, enemyType = EnemyType.Shadow },
                },
                DepthLevel.Deep => waveIndex switch
                {
                    0 => new WaveConfig { waveNumber = 1, enemyCount = 3, enemyType = EnemyType.Shadow },
                    1 => new WaveConfig { waveNumber = 2, enemyCount = 4, enemyType = EnemyType.Lavabeast },
                    3 => new WaveConfig { waveNumber = 3, enemyCount = 3, enemyType = EnemyType.Lavabeast },
                    4 => new WaveConfig { waveNumber = 5, enemyCount = 1, enemyType = EnemyType.Guardian },
                    _ => new WaveConfig { waveNumber = 3, enemyCount = 3, enemyType = EnemyType.Lavabeast },
                },
                _ => new WaveConfig { waveNumber = 1, enemyCount = 3, enemyType = EnemyType.Rockworm },
            };
        }
    }
}
