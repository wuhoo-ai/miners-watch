using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Enemy AI state machine. Manages movement direction, attack target, and death.
    /// Call Tick(dt) from Update() — testable with explicit time.
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _coreTarget; // base core to attack

        private Enemy _enemy;
        private EnemyState _state = EnemyState.Moving;
        private float _attackTimer;
        private float _attackInterval = 1f;

        public EnemyState CurrentState => _state;
        public Transform CoreTarget { get => _coreTarget; set => _coreTarget = value; }

        public void Init(Enemy enemy, Transform coreTarget)
        {
            _enemy = enemy;
            _coreTarget = coreTarget;
            _state = EnemyState.Moving;
            _attackTimer = 0f;
        }

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }

        private void Update() => Tick(Time.deltaTime);

        /// <summary>Advance AI by dt seconds. Testable entry point.</summary>
        public void Tick(float dt)
        {
            if (_enemy == null || _enemy.IsDead)
            {
                _state = EnemyState.Dead;
                return;
            }

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
    }
}
