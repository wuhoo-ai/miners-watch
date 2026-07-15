using UnityEngine;

namespace MinersWatch
{
    /// <summary>Base enemy: HP, damage, speed. Testable via Init().</summary>
    public class Enemy : MonoBehaviour
    {
        [SerializeField] protected int _maxHP = 30;
        [SerializeField] protected int _damage = 10;
        [SerializeField] protected float _speed = 2f;
        protected int _currentHP;

        public int MaxHP => _maxHP;
        public int CurrentHP => _currentHP;
        public int Damage => _damage;
        public float Speed => _speed;
        public bool IsDead => _currentHP <= 0;

        public virtual void Init(int hp, int damage, float speed)
        {
            _maxHP = hp;
            _damage = damage;
            _speed = speed;
            _currentHP = hp;
            GuardDefaults();
        }

        protected virtual void Awake()
        {
            _currentHP = _maxHP;
            GuardDefaults();
        }

        private void GuardDefaults()
        {
            if (_maxHP <= 0) _maxHP = 30;
            if (_damage <= 0) _damage = 10;
            if (_speed <= 0f) _speed = 2f;
        }

        public virtual void TakeDamage(int damage)
        {
            if (damage <= 0 || IsDead) return;
            _currentHP = Mathf.Max(0, _currentHP - damage);
        }

        /// <summary>Drop loot when killed. Caller provides inventory to add to.</summary>
        public virtual void DropLoot(InventorySystem inventory)
        {
            if (inventory == null) return;
            var def = GetDef();
            if (def == null) return;
            // Simple drop: 1 item per kill (expandable)
            var mineral = MapToMineral(def.type);
            inventory.AddItem(mineral, GetSellPrice(mineral), 1);
        }

        private static EnemyDef GetDefForType(EnemyType type) => type switch
        {
            EnemyType.Rockworm => EnemyPresets.Rockworm,
            EnemyType.Shadow => EnemyPresets.Shadow,
            EnemyType.Lavabeast => EnemyPresets.Lavabeast,
            EnemyType.Guardian => EnemyPresets.Guardian,
            _ => null,
        };

        // Override per subclass
        protected virtual EnemyDef GetDef() => null;

        protected static MineralType MapToMineral(EnemyType type) => type switch
        {
            EnemyType.Rockworm => MineralType.Iron,
            EnemyType.Shadow => MineralType.Gold,
            EnemyType.Lavabeast => MineralType.Crystal,
            EnemyType.Guardian => MineralType.Obsidian,
            _ => MineralType.Stone,
        };

        protected static float GetSellPrice(MineralType type) => type switch
        {
            MineralType.Iron => 15f,
            MineralType.Gold => 40f,
            MineralType.Crystal => 100f,
            MineralType.Obsidian => 300f,
            _ => 5f,
        };
    }
}
