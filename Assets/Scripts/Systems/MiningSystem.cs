using System;
using System.Collections.Generic;
using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Handles mining interactions: detects nearby MineralNode via trigger,
    /// responds to Interact input (E key), consumes stamina, and records mined minerals.
    /// Attach to the Player GameObject alongside StaminaSystem.
    /// </summary>
    public class MiningSystem : MonoBehaviour
    {
        [Header("Mining Settings")]
        [SerializeField] private float mineCooldown = 0.5f;
        [SerializeField] private float mineDistance = 1.5f;

        private StaminaSystem stamina;
        private MineralNode currentTarget;
        private float lastMineTime;

        /// <summary>Temporary list of mined minerals — will be integrated with T004 backpack system.</summary>
        public List<MineralType> MinedMinerals { get; private set; } = new List<MineralType>();

        /// <summary>Fired when a mineral is successfully mined, for UI updates.</summary>
        public event Action<MineralType> OnMineralMined;

        private void Awake()
        {
            lastMineTime = -mineCooldown; // allow first mine immediately
            stamina = GetComponent<StaminaSystem>() ?? GetComponentInParent<StaminaSystem>();
            if (stamina == null)
            {
                stamina = gameObject.AddComponent<StaminaSystem>();
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("MineralNode"))
            {
                var node = other.GetComponent<MineralNode>();
                if (node != null)
                    currentTarget = node;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("MineralNode"))
            {
                var node = other.GetComponent<MineralNode>();
                if (node != null && node == currentTarget)
                    currentTarget = null;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E) && currentTarget != null)
            {
                TryMine(currentTarget);
            }
        }

        /// <summary>
        /// Attempt to mine the given mineral node.
        /// Validates distance, cooldown, stamina availability, then destroys the node.
        /// Returns true on success.
        /// </summary>
        public bool TryMine(MineralNode node)
        {
            if (node == null) return false;
            if (Time.time - lastMineTime < mineCooldown) return false;
            if (stamina == null) return false;

            float dist = Vector2.Distance(transform.position, node.transform.position);
            if (dist > mineDistance) return false;

            var data = node.MineralData;
            if (data == null) return false;

            if (!stamina.Consume(data.staminaCost)) return false;

            lastMineTime = Time.time;
            MinedMinerals.Add(data.mineralType);
            OnMineralMined?.Invoke(data.mineralType);

            currentTarget = null;
            Destroy(node.gameObject);
            return true;
        }
    }
}
