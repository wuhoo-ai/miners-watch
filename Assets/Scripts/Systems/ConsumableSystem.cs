using System.Collections.Generic;
using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Consumable item system: use items from inventory to apply temporary or instant effects.
    /// Core logic is pure C# callable from EditMode tests via Init().
    /// </summary>
    public class ConsumableSystem : MonoBehaviour
    {
        private InventorySystem _inventory;
        private ConsumableDef[] _consumables;
        private Dictionary<ConsumableType, float> _activeEffects = new Dictionary<ConsumableType, float>();

        public IReadOnlyDictionary<ConsumableType, float> ActiveEffects => _activeEffects;
        public InventorySystem Inventory => _inventory;

        /// <summary>Explicit init for EditMode tests where Awake may not fire.</summary>
        public void Init(InventorySystem inventory, ConsumableDef[] consumables = null)
        {
            _inventory = inventory;
            _consumables = consumables ?? ConsumablePresets.All;
            _activeEffects.Clear();
        }

        private void Awake()
        {
            if (_inventory == null)
                _inventory = GameRoot.Get<InventorySystem>();
            if (_consumables == null || _consumables.Length == 0)
                _consumables = ConsumablePresets.All;
        }

        /// <summary>
        /// Check if player has materials to use this consumable.
        /// Does NOT modify inventory.
        /// </summary>
        public bool CanUse(ConsumableType type)
        {
            if (_inventory == null) return false;

            var def = GetDef(type);
            if (def == null || !def.IsValid()) return false;

            return _inventory.HasItem(def.material, def.materialCount);
        }

        /// <summary>
        /// Use consumable: deduct materials and apply effect.
        /// Returns false if materials insufficient or consumable invalid.
        /// For instant effects (duration=0), applies immediately.
        /// For timed effects (duration>0), activates/replaces existing timer.
        /// </summary>
        public bool Use(ConsumableType type)
        {
            if (!CanUse(type)) return false;

            var def = GetDef(type);
            if (def == null) return false;

            // Deduct materials
            if (!_inventory.RemoveItem(def.material, def.materialCount))
            {
                Debug.LogError($"[ConsumableSystem] Failed to deduct {def.material} x{def.materialCount} after CanUse succeeded");
                return false;
            }

            // Apply effect
            if (def.duration <= 0f)
            {
                // Instant effect (Bomb, HealPotion)
                // Effect application is handled by external systems reading this event
                OnInstantEffect?.Invoke(type, def.effectValue);
            }
            else
            {
                // Timed effect (Torch, SpeedScroll)
                // Replace existing timer if already active
                _activeEffects[type] = def.duration;
            }

            return true;
        }

        /// <summary>
        /// Advance effect timers by dt seconds.
        /// Removes expired effects.
        /// Call this from Update() or Tick(dt) in tests.
        /// </summary>
        public void Tick(float dt)
        {
            if (dt <= 0f) return;

            // Snapshot keys to avoid collection-modified-during-enumeration
            var keys = new List<ConsumableType>(_activeEffects.Keys);
            var expired = new List<ConsumableType>();
            foreach (var key in keys)
            {
                float remaining = _activeEffects[key] - dt;
                if (remaining <= 0f)
                {
                    expired.Add(key);
                }
                else
                {
                    _activeEffects[key] = remaining;
                }
            }

            foreach (var type in expired)
            {
                _activeEffects.Remove(type);
                OnEffectExpired?.Invoke(type);
            }
        }

        /// <summary>Check if a timed effect is currently active.</summary>
        public bool IsEffectActive(ConsumableType type)
        {
            return _activeEffects.ContainsKey(type);
        }

        /// <summary>Get remaining duration of an active effect.</summary>
        public float GetEffectDuration(ConsumableType type)
        {
            return _activeEffects.TryGetValue(type, out float duration) ? duration : 0f;
        }

        /// <summary>Get effect value (magnitude) for a consumable type.</summary>
        public float GetEffectValue(ConsumableType type)
        {
            var def = GetDef(type);
            return def != null ? def.effectValue : 0f;
        }

        /// <summary>Force-expire an active effect (for testing or special cases).</summary>
        public void ExpireEffect(ConsumableType type)
        {
            if (_activeEffects.Remove(type))
            {
                OnEffectExpired?.Invoke(type);
            }
        }

        /// <summary>Clear all active effects (for reset/new game).</summary>
        public void ClearAllEffects()
        {
            _activeEffects.Clear();
        }

        private ConsumableDef GetDef(ConsumableType type)
        {
            if (_consumables == null) return null;
            foreach (var def in _consumables)
            {
                if (def.type == type) return def;
            }
            return null;
        }

        /// <summary>Fired when an instant effect is applied (Bomb, HealPotion).</summary>
        public System.Action<ConsumableType, float> OnInstantEffect;

        /// <summary>Fired when a timed effect expires.</summary>
        public System.Action<ConsumableType> OnEffectExpired;
    }
}
