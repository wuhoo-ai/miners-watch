using System;
using System.Collections.Generic;
using UnityEngine;

namespace MinersWatch
{
    public class MiningSystem : MonoBehaviour
    {
        [Header("Mining Settings")]
        [SerializeField] private float mineCooldown = 0.5f;
        [SerializeField] private float mineDistance = 1.5f;

        public StaminaSystem Stamina;
        public InventorySystem Inventory;
        private MineralNode currentTarget;
        private float lastMineTime;

        public List<MineralType> MinedMinerals { get; private set; } = new List<MineralType>();
        public event Action<MineralType> OnMineralMined;

        private void Awake()
        {
            lastMineTime = -999f;
            if (mineCooldown <= 0f) mineCooldown = 0.5f;
            Stamina = GetComponent<StaminaSystem>() ?? GetComponentInParent<StaminaSystem>();
        }

        public void Init() { } // hook for test compatibility

        public bool TryMine(MineralNode node)
        {
            if (node == null) return false;
            if (Time.time - lastMineTime < mineCooldown) return false;
            if (Stamina == null) return false;

            float dist = Vector2.Distance(transform.position, node.transform.position);
            if (dist > mineDistance) return false;

            var data = node.MineralData;
            if (data == null) return false;

            if (!Stamina.Consume(data.staminaCost)) return false;

            lastMineTime = Time.time;
            MinedMinerals.Add(data.mineralType);
            Inventory?.AddItem(data.mineralType, data.sellPrice, 1);
            OnMineralMined?.Invoke(data.mineralType);
            Destroy(node.gameObject);
            return true;
        }

        // Trigger detection — runtime only, sets currentTarget for Update()
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("MineralNode"))
                currentTarget = other.GetComponent<MineralNode>();
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("MineralNode") && other.GetComponent<MineralNode>() == currentTarget)
                currentTarget = null;
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E) && currentTarget != null)
                TryMine(currentTarget);
        }
    }
}
