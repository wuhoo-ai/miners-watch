using UnityEngine;
using UnityEngine.UI;

namespace MinersWatch
{
    public class StaminaBarUI : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private Image fillImage;
        [SerializeField] private TMPro.TextMeshProUGUI staminaText;

        private StaminaSystem stamina;

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                stamina = player.GetComponent<StaminaSystem>();
            }

            if (stamina == null)
            {
                enabled = false;
                return;
            }

            if (slider == null)
                slider = GetComponentInChildren<Slider>();

            if (slider != null)
            {
                slider.minValue = 0f;
                slider.maxValue = stamina.maxStamina;
                slider.value = stamina.currentStamina;
            }
        }

        private void OnEnable()
        {
            if (stamina != null)
                stamina.OnChanged += OnStaminaChanged;
        }

        private void OnDisable()
        {
            if (stamina != null)
                stamina.OnChanged -= OnStaminaChanged;
        }

        private void OnStaminaChanged(float current, float max)
        {
            if (slider != null)
            {
                slider.value = current;
            }

            float ratio = current / max;

            if (fillImage != null)
            {
                if (ratio > 0.6f)
                    fillImage.color = Color.green;
                else if (ratio > 0.3f)
                    fillImage.color = Color.yellow;
                else
                    fillImage.color = Color.red;
            }

            if (staminaText != null)
            {
                staminaText.text = $"{(int)current} / {(int)max}";
            }
        }
    }
}
