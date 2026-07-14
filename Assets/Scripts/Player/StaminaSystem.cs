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

        private void Awake()
        {
            // Guard against Unity serialization zeroing in EditMode
            if (_maxStamina <= 0f) _maxStamina = DefaultMax;
            if (_currentStamina <= 0f) _currentStamina = _maxStamina;
        }

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
