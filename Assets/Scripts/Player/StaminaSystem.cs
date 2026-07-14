using System;
using UnityEngine;

namespace MinersWatch
{
    public class StaminaSystem : MonoBehaviour
    {
        public float maxStamina { get; private set; }
        public float currentStamina { get; private set; }

        public event Action<float, float> OnChanged;

        public StaminaSystem(float maxStamina = 100f)
        {
            this.maxStamina = maxStamina;
            this.currentStamina = maxStamina;
        }

        public bool Consume(float amount)
        {
            if (amount < 0f) return false;
            if (amount == 0f) return true;

            if (currentStamina < amount)
                return false;

            currentStamina -= amount;
            currentStamina = ClampStamina(currentStamina);
            OnChanged?.Invoke(currentStamina, maxStamina);
            return true;
        }

        public void Restore(float amount)
        {
            if (amount < 0f) return;
            if (amount == 0f) return;

            currentStamina += amount;
            currentStamina = ClampStamina(currentStamina);
            OnChanged?.Invoke(currentStamina, maxStamina);
        }

        public void RestoreFull()
        {
            if (currentStamina >= maxStamina) return;
            currentStamina = maxStamina;
            OnChanged?.Invoke(currentStamina, maxStamina);
        }

        private float ClampStamina(float value)
        {
            return UnityEngine.Mathf.Clamp(value, 0f, maxStamina);
        }
    }
}
