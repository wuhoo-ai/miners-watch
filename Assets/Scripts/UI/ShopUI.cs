using UnityEngine;
using UnityEngine.UI;

namespace MinersWatch
{
    /// <summary>
    /// Shop UI: sell button, upgrade buttons, defense buttons. Reactive to gold changes.
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ShopSystem _shop;
        [SerializeField] private Text _goldText;

        [Header("Upgrade Buttons")]
        [SerializeField] private Button _sellAllButton;
        [SerializeField] private Button _buyPickaxeButton;
        [SerializeField] private Button _buyArmorButton;
        [SerializeField] private Button _buyBackpackButton;

        [Header("Defense Buttons")]
        [SerializeField] private Button _buyWallButton;
        [SerializeField] private Button _buySpikeTrapButton;
        [SerializeField] private Button _buyTurretButton;

        [Header("Button Labels (for level updates)")]
        [SerializeField] private Text _sellAllLabel;
        [SerializeField] private Text _pickaxeLabel;
        [SerializeField] private Text _armorLabel;
        [SerializeField] private Text _backpackLabel;

        public void Init(ShopSystem shop)
        {
            _shop = shop;
            if (_shop?.Upgrades != null)
                _shop.Upgrades.OnGoldChanged += _ => RefreshDisplay();
            WireButtons();
            RefreshDisplay();
        }

        private void Awake()
        {
            if (_shop == null)
                _shop = GetComponent<ShopSystem>() ?? GetComponentInParent<ShopSystem>() ?? GameRoot.Get<ShopSystem>();

            if (_shop?.Upgrades != null)
                _shop.Upgrades.OnGoldChanged += _ => RefreshDisplay();

            WireButtons();
            RefreshDisplay();
        }

        private void OnDestroy()
        {
            if (_shop?.Upgrades != null)
                _shop.Upgrades.OnGoldChanged -= _ => RefreshDisplay();
        }

        private void WireButtons()
        {
            if (_sellAllButton != null) _sellAllButton.onClick.AddListener(OnSellAll);
            if (_buyPickaxeButton != null) _buyPickaxeButton.onClick.AddListener(() => OnBuyUpgrade(UpgradeType.Pickaxe));
            if (_buyArmorButton != null) _buyArmorButton.onClick.AddListener(() => OnBuyUpgrade(UpgradeType.Armor));
            if (_buyBackpackButton != null) _buyBackpackButton.onClick.AddListener(() => OnBuyUpgrade(UpgradeType.Backpack));
            if (_buyWallButton != null) _buyWallButton.onClick.AddListener(() => OnBuyDefense(DefenseType.Wall));
            if (_buySpikeTrapButton != null) _buySpikeTrapButton.onClick.AddListener(() => OnBuyDefense(DefenseType.SpikeTrap));
            if (_buyTurretButton != null) _buyTurretButton.onClick.AddListener(() => OnBuyDefense(DefenseType.Turret));
        }

        private void OnSellAll()
        {
            int earned = _shop?.SellAllMinerals() ?? 0;
            Debug.Log($"[ShopUI] Sold all minerals — earned ${earned}");
            RefreshDisplay();
        }

        private void OnBuyUpgrade(UpgradeType type)
        {
            bool ok = _shop?.BuyUpgrade(type) ?? false;
            Debug.Log($"[ShopUI] Buy {type}: {(ok ? "OK" : "FAILED (gold/level)")}");
            RefreshDisplay();
        }

        private void OnBuyDefense(DefenseType type)
        {
            bool ok = _shop?.BuyDefense(type) ?? false;
            Debug.Log($"[ShopUI] Buy {type}: {(ok ? "OK" : "FAILED (gold/iron)")}");
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            if (_shop == null) return;

            if (_goldText != null)
            {
                int gold = _shop.Upgrades?.Gold ?? 0;
                _goldText.text = $"${gold}";
            }

            // Update upgrade button labels with levels + costs
            if (_pickaxeLabel != null && _shop.Upgrades != null)
            {
                int level = _shop.Upgrades.GetLevel(UpgradeType.Pickaxe);
                int cost = _shop.Upgrades.GetUpgradeCost(UpgradeType.Pickaxe);
                _pickaxeLabel.text = level >= 3 ? "镐 Lv.MAX" : $"镐 Lv.{level}→{level+1} ${cost}";
                if (_buyPickaxeButton != null)
                    _buyPickaxeButton.interactable = cost > 0 && _shop.Upgrades.Gold >= cost;
            }

            if (_armorLabel != null && _shop.Upgrades != null)
            {
                int level = _shop.Upgrades.GetLevel(UpgradeType.Armor);
                int cost = _shop.Upgrades.GetUpgradeCost(UpgradeType.Armor);
                _armorLabel.text = level >= 3 ? "甲 Lv.MAX" : $"甲 Lv.{level}→{level+1} ${cost}";
                if (_buyArmorButton != null)
                    _buyArmorButton.interactable = cost > 0 && _shop.Upgrades.Gold >= cost;
            }

            if (_backpackLabel != null && _shop.Upgrades != null)
            {
                int level = _shop.Upgrades.GetLevel(UpgradeType.Backpack);
                int cost = _shop.Upgrades.GetUpgradeCost(UpgradeType.Backpack);
                _backpackLabel.text = level >= 3 ? "包 Lv.MAX" : $"包 Lv.{level}→{level+1} ${cost}";
                if (_buyBackpackButton != null)
                    _buyBackpackButton.interactable = cost > 0 && _shop.Upgrades.Gold >= cost;
            }

            // Update defense button affordability
            UpdateDefenseButton(_buyWallButton, DefenseType.Wall);
            UpdateDefenseButton(_buySpikeTrapButton, DefenseType.SpikeTrap);
            UpdateDefenseButton(_buyTurretButton, DefenseType.Turret);
        }

        private void UpdateDefenseButton(Button btn, DefenseType type)
        {
            if (btn == null || _shop == null) return;
            btn.interactable = _shop.CanAffordDefense(type);
        }
    }
}
