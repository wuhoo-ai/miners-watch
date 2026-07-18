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
        public UpgradeSystem Upgrades;
        private MineralNode currentTarget;
        private float lastMineTime;

        public List<MineralType> MinedMinerals { get; private set; } = new List<MineralType>();
        public event Action<MineralType> OnMineralMined;

        private void Awake() => Init();

        public void Init()
        {
            lastMineTime = -999f;
            if (mineCooldown <= 0f) mineCooldown = 0.5f;
            if (Stamina == null) Stamina = GetComponent<StaminaSystem>() ?? GetComponentInParent<StaminaSystem>();
            if (Inventory == null) Inventory = GetComponentInParent<InventorySystem>() ?? GameRoot.Get<InventorySystem>();
            if (Upgrades == null) Upgrades = GetComponentInParent<UpgradeSystem>() ?? GameRoot.Get<UpgradeSystem>();
        }

        public bool TryMine(MineralNode node)
        {
            if (node == null) return false;
            if (Time.time - lastMineTime < mineCooldown) return false;
            if (Stamina == null) return false;

            float dist = Vector2.Distance(transform.position, node.transform.position);
            if (dist > mineDistance) return false;

            var data = node.MineralData;
            if (data == null) return false;

            if (!Stamina.Consume(GetEffectiveCost(data.staminaCost))) return false;

            lastMineTime = Time.time;
            MinedMinerals.Add(data.mineralType);
            Inventory?.AddItem(data.mineralType, data.sellPrice, 1);
            OnMineralMined?.Invoke(data.mineralType);
#if UNITY_EDITOR
            DestroyImmediate(node.gameObject);
#else
            Destroy(node.gameObject);
#endif
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
            if ((Input.GetKeyDown(KeyCode.E) || TouchInput.ConsumeMine()) && currentTarget != null)
                TryMine(currentTarget);
        }
        /// <summary>Apply pickaxe upgrade multiplier to stamina cost.</summary>
        private float GetEffectiveCost(float baseCost)
        {
            if (Upgrades == null) return baseCost;
            int level = Upgrades.GetLevel(UpgradeType.Pickaxe);
            return level switch
            {
                2 => baseCost * 0.7f,
                3 => baseCost * 0.5f,
                _ => baseCost,
            };
        }
    }
}
