using System;
using UnityEngine;

namespace MinersWatch
{
    public class StaminaSystem : MonoBehaviour
    {
        [SerializeField] private float _maxStamina = 100f;
        public float maxStamina => _maxStamina;
        [System.NonSerialized] private float _currentStamina;
        public float currentStamina => _currentStamina;

        public event Action<float, float> OnChanged;

        private void Awake()
        {
            _currentStamina = _maxStamina;
        }

        public bool Consume(float amount)
        {
            if (amount < 0f) return false;
            if (amount == 0f) return true;

            if (_currentStamina < amount)
                return false;

            _currentStamina -= amount;
            _currentStamina = ClampStamina(_currentStamina);
            OnChanged?.Invoke(_currentStamina, _maxStamina);
            return true;
        }

        public void Restore(float amount)
        {
            if (amount < 0f) return;
            if (amount == 0f) return;

            _currentStamina += amount;
            _currentStamina = ClampStamina(_currentStamina);
            OnChanged?.Invoke(_currentStamina, _maxStamina);
        }

        public void RestoreFull()
        {
            if (_currentStamina >= _maxStamina) return;
            _currentStamina = _maxStamina;
            OnChanged?.Invoke(_currentStamina, _maxStamina);
        }

        private float ClampStamina(float value)
        {
            return UnityEngine.Mathf.Clamp(value, 0f, _maxStamina);
        }
    }
}
