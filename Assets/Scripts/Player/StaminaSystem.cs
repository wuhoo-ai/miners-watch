using System;
using UnityEngine;

namespace MinersWatch
{
    public class StaminaSystem : MonoBehaviour
    {
        private const float DefaultMax = 100f;

        [SerializeField] private float _maxStamina = DefaultMax;
        [SerializeField] private float _currentStamina = DefaultMax;

        public float maxStamina => _maxStamina;
        public float currentStamina => _currentStamina;

        public event Action<float, float> OnChanged;

        /// <summary>Explicit init for EditMode tests where Awake may not fire.</summary>
        public void Init()
        {
            if (_maxStamina <= 0f) _maxStamina = DefaultMax;
            _currentStamina = _maxStamina;
        }

        private void Awake() => Init();

        public bool Consume(float amount)
        {
            if (amount < 0f) return false;
            if (amount == 0f) return true;

            if (_currentStamina < amount)
                return false;

            _currentStamina -= amount;
            _currentStamina = Mathf.Clamp(_currentStamina, 0f, _maxStamina);
            OnChanged?.Invoke(_currentStamina, _maxStamina);
            return true;
        }

        public void Restore(float amount)
        {
            if (amount < 0f) return;
            if (amount == 0f) return;

            _currentStamina += amount;
            _currentStamina = Mathf.Clamp(_currentStamina, 0f, _maxStamina);
            OnChanged?.Invoke(_currentStamina, _maxStamina);
        }

        public void RestoreFull()
        {
            if (_currentStamina >= _maxStamina) return;
            _currentStamina = _maxStamina;
            OnChanged?.Invoke(_currentStamina, _maxStamina);
        }
    }
}
