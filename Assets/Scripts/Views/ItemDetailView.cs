using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Views
{
    /// <summary>
    /// 아이템 상세 정보 패널 View
    /// 선택된 아이템의 정보를 표시하고 액션 버튼 제공
    /// </summary>
    public class ItemDetailView : MonoBehaviour
    {
        [Header("Item Info")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI effectText;
        [SerializeField] private Image rarityBackground;

        [Header("Action Buttons")]
        [SerializeField] private Button useButton;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button unequipButton;
        [SerializeField] private Button sellButton;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button closeButton;

        [Header("Quantity Controls")]
        [SerializeField] private GameObject quantitySelector;
        [SerializeField] private Slider quantitySlider;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private TextMeshProUGUI totalPriceText;

        [Header("Price Display")]
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI sellPriceText;

        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = Color.white;
        [SerializeField] private Color uncommonColor = new Color(0.7f, 1f, 0.7f);
        [SerializeField] private Color rareColor = new Color(0.3f, 0.7f, 1f);
        [SerializeField] private Color epicColor = new Color(0.8f, 0.3f, 1f);
        [SerializeField] private Color legendaryColor = new Color(1f, 0.6f, 0.2f);

        public event Action OnUseClicked;
        public event Action OnEquipClicked;
        public event Action OnUnequipClicked;
        public event Action<int> OnSellClicked; // quantity
        public event Action<int> OnBuyClicked;  // quantity
        public event Action OnCloseClicked;

        private InventoryItem _currentInventoryItem;
        private Item _currentItem;
        private bool _isShopMode;
        private int _maxQuantity = 1;
        private int _currentQuantity = 1;

        private void Awake()
        {
            SetupButtons();
            SetupQuantitySlider();
        }

        private void SetupButtons()
        {
            if (useButton != null)
                useButton.onClick.AddListener(() => OnUseClicked?.Invoke());
            if (equipButton != null)
                equipButton.onClick.AddListener(() => OnEquipClicked?.Invoke());
            if (unequipButton != null)
                unequipButton.onClick.AddListener(() => OnUnequipClicked?.Invoke());
            if (sellButton != null)
                sellButton.onClick.AddListener(() => OnSellClicked?.Invoke(_currentQuantity));
            if (buyButton != null)
                buyButton.onClick.AddListener(() => OnBuyClicked?.Invoke(_currentQuantity));
            if (closeButton != null)
                closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        }

        private void SetupQuantitySlider()
        {
            if (quantitySlider != null)
            {
                quantitySlider.onValueChanged.AddListener(OnQuantityChanged);
            }
        }

        private void OnQuantityChanged(float value)
        {
            _currentQuantity = Mathf.RoundToInt(value);
            UpdateQuantityDisplay();
        }

        /// <summary>
        /// 인벤토리 아이템 표시
        /// </summary>
        public void ShowItem(InventoryItem inventoryItem, Item item)
        {
            _isShopMode = false;
            _currentInventoryItem = inventoryItem;
            _currentItem = item;
            _maxQuantity = inventoryItem?.Quantity ?? 1;
            _currentQuantity = 1;

            UpdateUI();
            SetupInventoryButtons();
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 상점 아이템 표시
        /// </summary>
        public void ShowShopItem(Item item, int maxBuyable)
        {
            _isShopMode = true;
            _currentInventoryItem = null;
            _currentItem = item;
            _maxQuantity = maxBuyable;
            _currentQuantity = 1;

            UpdateUI();
            SetupShopButtons();
            gameObject.SetActive(true);
        }

        private void UpdateUI()
        {
            if (_currentItem == null) return;

            // 기본 정보
            if (nameText != null)
                nameText.text = _currentItem.Name;

            if (typeText != null)
                typeText.text = GetTypeDisplayText(_currentItem.Type);

            if (descriptionText != null)
                descriptionText.text = _currentItem.Description;

            // 효과 텍스트
            if (effectText != null)
            {
                effectText.text = _currentItem.EffectValue != 0 ? $"효과: {_currentItem.EffectValue}" : "효과 없음";
            }

            // 희귀도 배경
            if (rarityBackground != null)
            {
                rarityBackground.color = GetRarityColor(_currentItem.Rarity);
            }

            // 가격 표시
            if (priceText != null)
            {
                priceText.text = $"{_currentItem.Price}G";
                priceText.gameObject.SetActive(_isShopMode);
            }

            if (sellPriceText != null)
            {
                int sellPrice = Mathf.RoundToInt(_currentItem.Price * 0.6f); // 60% 판매가
                sellPriceText.text = $"{sellPrice}G";
                sellPriceText.gameObject.SetActive(!_isShopMode);
            }

            // 수량 선택기
            if (quantitySelector != null)
            {
                quantitySelector.SetActive(_maxQuantity > 1);
            }

            if (quantitySlider != null)
            {
                quantitySlider.maxValue = _maxQuantity;
                quantitySlider.value = 1;
            }

            UpdateQuantityDisplay();
        }

        private void UpdateQuantityDisplay()
        {
            if (quantityText != null)
            {
                quantityText.text = $"x{_currentQuantity}";
            }

            if (totalPriceText != null)
            {
                int totalPrice = _isShopMode 
                    ? _currentItem.Price * _currentQuantity 
                    : Mathf.RoundToInt(_currentItem.Price * 0.6f) * _currentQuantity;
                totalPriceText.text = $"합계: {totalPrice}G";
            }
        }

        private void SetupInventoryButtons()
        {
            if (_currentItem == null) return;

            // 사용 버튼 (소모품만)
            if (useButton != null)
            {
                bool canUse = _currentItem.Type == ItemType.Consumable;
                useButton.gameObject.SetActive(canUse);
            }

            // 장착 버튼 (장비만)
            if (equipButton != null)
            {
                bool isEquipped = _currentInventoryItem?.IsEquipped ?? false;
                bool canEquip = _currentItem.Type == ItemType.Equipment && !isEquipped;
                equipButton.gameObject.SetActive(canEquip);
            }

            // 해제 버튼 (장비만)
            if (unequipButton != null)
            {
                bool isEquipped = _currentInventoryItem?.IsEquipped ?? false;
                unequipButton.gameObject.SetActive(isEquipped);
            }

            // 판매 버튼
            if (sellButton != null)
            {
                sellButton.gameObject.SetActive(true);
            }

            // 구매 버튼 숨김
            if (buyButton != null)
            {
                buyButton.gameObject.SetActive(false);
            }
        }

        private void SetupShopButtons()
        {
            // 사용/장착/해제/판매 버튼 숨김
            if (useButton != null) useButton.gameObject.SetActive(false);
            if (equipButton != null) equipButton.gameObject.SetActive(false);
            if (unequipButton != null) unequipButton.gameObject.SetActive(false);
            if (sellButton != null) sellButton.gameObject.SetActive(false);

            // 구매 버튼 표시
            if (buyButton != null)
            {
                buyButton.gameObject.SetActive(true);
            }
        }

        private string GetTypeDisplayText(ItemType type)
        {
            return type switch
            {
                ItemType.Consumable => "소모품",
                ItemType.Equipment => "장비",
                ItemType.Material => "재료",
                ItemType.Quest => "퀘스트",
                _ => "기타"
            };
        }

        private Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Legendary => legendaryColor,
                ItemRarity.Epic => epicColor,
                ItemRarity.Rare => rareColor,
                ItemRarity.Uncommon => uncommonColor,
                _ => commonColor
            };
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _currentInventoryItem = null;
            _currentItem = null;
        }

        private void OnDestroy()
        {
            if (useButton != null) useButton.onClick.RemoveAllListeners();
            if (equipButton != null) equipButton.onClick.RemoveAllListeners();
            if (unequipButton != null) unequipButton.onClick.RemoveAllListeners();
            if (sellButton != null) sellButton.onClick.RemoveAllListeners();
            if (buyButton != null) buyButton.onClick.RemoveAllListeners();
            if (closeButton != null) closeButton.onClick.RemoveAllListeners();
            if (quantitySlider != null) quantitySlider.onValueChanged.RemoveAllListeners();
        }
    }
}
