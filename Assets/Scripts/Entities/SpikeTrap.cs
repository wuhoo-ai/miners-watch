using UnityEngine;

namespace MinersWatch
{
    /// <summary>Spike trap: deals damage on collision, depletes after N uses.</summary>
    public class SpikeTrap : DefenseEntity
    {
        [SerializeField] private int _maxUses = 3;
        [SerializeField] private int _damage = 20;
        private int _remainingUses;

        public int Damage => _damage;
        public int RemainingUses => _remainingUses;
        public bool IsDepleted => _remainingUses <= 0;

        public override void Init(int hp)
        {
            base.Init(hp);
            if (_maxUses <= 0) _maxUses = 3;
            if (_damage <= 0) _damage = 20;
            _remainingUses = _maxUses;
        }

        protected override void Awake()
        {
            base.Awake();
            _remainingUses = _maxUses;
        }

        /// <summary>Called when enemy steps on trap. Returns damage dealt, 0 if depleted.</summary>
        public int Trigger()
        {
            if (IsDepleted || IsDestroyed) return 0;
            _remainingUses--;
            return _damage;
        }
    }
}
