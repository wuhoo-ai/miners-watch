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
                _shop = GetComponent<ShopSystem>() ?? GetComponentInParent<ShopSystem>();

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
            _shop?.SellAllMinerals();
        }

        private void OnBuyUpgrade(UpgradeType type)
        {
            _shop?.BuyUpgrade(type);
        }

        private void OnBuyDefense(DefenseType type)
        {
            _shop?.BuyDefense(type);
        }

        public void RefreshDisplay()
        {
            if (_shop == null) return;
            if (_goldText != null)
                _goldText.text = $"Gold: {_shop.Upgrades?.Gold ?? 0}";
        }
    }
}
