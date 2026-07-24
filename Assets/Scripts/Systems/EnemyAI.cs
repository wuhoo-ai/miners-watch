using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// AI behavior variants for different enemy types.
    /// </summary>
    public enum AIBehavior
    {
        Linear,   // Rockworm: straight movement (default)
        Blink,    // Shadow: teleport every 2s
        Ranged,   // Lavabeast: ranged attack
        Boss      // Guardian: phase-based
    }

    /// <summary>
    /// Enemy AI state machine. Manages movement direction, attack target, and death.
    /// Call Tick(dt) from Update() — testable with explicit time.
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _coreTarget; // base core to attack

        [Header("AI Behavior")]
        [SerializeField] private AIBehavior _behavior = AIBehavior.Linear;

        private Enemy _enemy;
        private EnemyState _state = EnemyState.Moving;
        private float _attackTimer;
        private float _attackInterval = 1f;

        // Blink behavior (Shadow)
        private float _blinkTimer;
        private float _blinkInterval = 2f;
        private float _blinkDistance = 3f;

        // Ranged behavior (Lavabeast)
        private float _rangedCooldown;
        private float _rangedInterval = 3f;
        private float _rangedRange = 5f;

        // Boss behavior (Guardian)
        private int _bossPhase = 0;
        private float _bossPhaseTimer;
        private float _bossPhaseDuration = 5f;

        public EnemyState CurrentState => _state;
        public AIBehavior Behavior => _behavior;
        public Transform CoreTarget { get => _coreTarget; set => _coreTarget = value; }

        public void Init(Enemy enemy, Transform coreTarget, AIBehavior behavior = AIBehavior.Linear)
        {
            _enemy = enemy;
            _coreTarget = coreTarget;
            _behavior = behavior;
            _state = EnemyState.Moving;
            _attackTimer = 0f;
            _blinkTimer = 0f;
            _rangedCooldown = 0f;
            _bossPhase = 0;
            _bossPhaseTimer = 0f;
        }

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }

        private void Update() => Tick(Time.deltaTime);

        private bool _deathEffectPlayed;

        /// <summary>Advance AI by dt seconds. Testable entry point.</summary>
        public void Tick(float dt)
        {
            if (_enemy == null || _enemy.IsDead)
            {
                if (_state != EnemyState.Dead && _enemy != null)
                {
                    // Death transition — play death particle effect once
                    ParticleEffects.PlayDeathEffect(transform.position);
                    _deathEffectPlayed = true;
                }
                _state = EnemyState.Dead;
                return;
            }

            // Behavior-specific logic
            switch (_behavior)
            {
                case AIBehavior.Linear:
                    TickLinear(dt);
                    break;
                case AIBehavior.Blink:
                    TickBlink(dt);
                    break;
                case AIBehavior.Ranged:
                    TickRanged(dt);
                    break;
                case AIBehavior.Boss:
                    TickBoss(dt);
                    break;
            }
        }

        private void TickLinear(float dt)
        {
            switch (_state)
            {
                case EnemyState.Moving:
                    MoveTowardCore(dt);
                    break;
                case EnemyState.Attacking:
                    AttackCore(dt);
                    break;
            }
        }

        private void TickBlink(float dt)
        {
            if (_state == EnemyState.Moving)
            {
                _blinkTimer += dt;
                if (_blinkTimer >= _blinkInterval)
                {
                    Blink();
                    _blinkTimer = 0f;
                }
                else
                {
                    MoveTowardCore(dt);
                }
            }
            else if (_state == EnemyState.Attacking)
            {
                AttackCore(dt);
            }
        }

        private void TickRanged(float dt)
        {
            if (_coreTarget == null) return;

            float distance = Vector2.Distance(transform.position, _coreTarget.position);

            // Move if out of range
            if (distance > _rangedRange)
            {
                MoveTowardCore(dt);
            }
            else
            {
                // In range: stop and attack
                _state = EnemyState.Attacking;
                _rangedCooldown -= dt;
                if (_rangedCooldown <= 0f)
                {
                    // Fire ranged attack (event for external system to handle)
                    OnRangedAttack?.Invoke();
                    _rangedCooldown = _rangedInterval;
                }
            }
        }

        private void TickBoss(float dt)
        {
            _bossPhaseTimer += dt;

            switch (_bossPhase)
            {
                case 0: // Phase 0: Rush toward core
                    MoveTowardCore(dt * 2f); // Double speed
                    if (_bossPhaseTimer >= _bossPhaseDuration)
                    {
                        _bossPhase = 1;
                        _bossPhaseTimer = 0f;
                    }
                    break;

                case 1: // Phase 1: Slam AOE
                    _state = EnemyState.Attacking;
                    if (_bossPhaseTimer >= _bossPhaseDuration)
                    {
                        OnBossSlam?.Invoke(); // AOE damage event
                        _bossPhase = 2;
                        _bossPhaseTimer = 0f;
                    }
                    break;

                case 2: // Phase 2: Summon minions
                    if (_bossPhaseTimer >= 1f) // Summon once per phase
                    {
                        OnBossSummon?.Invoke();
                        _bossPhase = 3;
                        _bossPhaseTimer = 0f;
                    }
                    break;

                case 3: // Phase 3: Enrage (faster attack)
                    _state = EnemyState.Attacking;
                    _attackTimer += dt;
                    if (_attackTimer >= _attackInterval * 0.5f) // 50% faster
                    {
                        _attackTimer -= _attackInterval * 0.5f;
                    }
                    break;
            }
        }

        private void MoveTowardCore(float dt)
        {
            if (_coreTarget == null) return;

            float dir = Mathf.Sign(_coreTarget.position.x - transform.position.x);
            transform.Translate(Vector2.right * (dir * _enemy.Speed * dt));

            // Check if reached attack range
            if (Vector2.Distance(transform.position, _coreTarget.position) < 0.5f)
                _state = EnemyState.Attacking;
        }

        private void AttackCore(float dt)
        {
            _attackTimer += dt;
            if (_attackTimer >= _attackInterval)
            {
                _attackTimer -= _attackInterval;
                // Damage is applied by external system (defense callback)
            }
        }

        private void Blink()
        {
            if (_coreTarget == null) return;

            Vector2 direction = (_coreTarget.position - transform.position).normalized;
            transform.Translate(direction * _blinkDistance);

            // Check if reached attack range after blink
            if (Vector2.Distance(transform.position, _coreTarget.position) < 0.5f)
                _state = EnemyState.Attacking;
        }

        /// <summary>Called when blocked by wall — switch to attacking obstruction.</summary>
        public void OnBlocked()
        {
            if (_state == EnemyState.Moving)
                _state = EnemyState.Attacking;
        }

        /// <summary>Called when obstruction destroyed — resume moving.</summary>
        public void OnPathCleared()
        {
            if (_state == EnemyState.Attacking)
                _state = EnemyState.Moving;
        }

        /// <summary>Force death state.</summary>
        public void Die()
        {
            _state = EnemyState.Dead;
        }

        /// <summary>Get current boss phase (0-3). Only valid for Boss behavior.</summary>
        public int GetBossPhase() => _bossPhase;

        /// <summary>Get blink timer remaining (for Shadow). Returns 0 if not Blink behavior.</summary>
        public float GetBlinkTimer() => _behavior == AIBehavior.Blink ? _blinkTimer : 0f;

        /// <summary>Get ranged cooldown remaining (for Lavabeast). Returns 0 if not Ranged behavior.</summary>
        public float GetRangedCooldown() => _behavior == AIBehavior.Ranged ? _rangedCooldown : 0f;

        /// <summary>Fired when Lavabeast fires ranged attack.</summary>
        public System.Action OnRangedAttack;

        /// <summary>Fired when Guardian performs slam AOE.</summary>
        public System.Action OnBossSlam;

        /// <summary>Fired when Guardian summons minions.</summary>
        public System.Action OnBossSummon;
    }
}
