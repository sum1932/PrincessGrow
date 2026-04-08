using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Views
{
    /// <summary>
    /// 개별 아이템 슬롯 View
    /// 인벤토리와 상점에서 공통으로 사용
    /// </summary>
    public class ItemSlotView : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image rarityBorderImage;
        [SerializeField] private Image equippedOverlay;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Button button;

        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = Color.white;
        [SerializeField] private Color uncommonColor = new Color(0.7f, 1f, 0.7f);
        [SerializeField] private Color rareColor = new Color(0.3f, 0.7f, 1f);
        [SerializeField] private Color epicColor = new Color(0.8f, 0.3f, 1f);
        [SerializeField] private Color legendaryColor = new Color(1f, 0.6f, 0.2f);

        public string ItemId { get; private set; }
        public InventoryItem InventoryItem { get; private set; }
        public Item Item { get; private set; }

        public event Action<ItemSlotView> OnSlotClicked;

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(() => OnSlotClicked?.Invoke(this));
            }
        }

        /// <summary>
        /// 인벤토리 아이템으로 초기화
        /// </summary>
        public void Initialize(InventoryItem inventoryItem, Item itemData, Action<ItemSlotView> clickCallback)
        {
            InventoryItem = inventoryItem;
            Item = itemData;
            ItemId = inventoryItem?.ItemId ?? itemData?.Id;
            OnSlotClicked = clickCallback;

            UpdateVisuals();
        }

        /// <summary>
        /// 상점 아이템으로 초기화 (수량 표시 없음)
        /// </summary>
        public void Initialize(Item itemData, Action<ItemSlotView> clickCallback)
        {
            Item = itemData;
            ItemId = itemData?.Id;
            InventoryItem = null;
            OnSlotClicked = clickCallback;

            UpdateShopVisuals();
        }

        private void UpdateVisuals()
        {
            if (Item == null) return;

            // 아이콘 설정
            if (iconImage != null)
            {
                // TODO: IconPath에서 스프라이트 로드
            }

            // 희귀도 테두리 색상
            if (rarityBorderImage != null)
            {
                rarityBorderImage.color = GetRarityColor(Item.Rarity);
            }

            // 수량 표시 (인벤토리에서만)
            if (quantityText != null)
            {
                if (InventoryItem != null && InventoryItem.Quantity > 1)
                {
                    quantityText.text = $"x{InventoryItem.Quantity}";
                    quantityText.gameObject.SetActive(true);
                }
                else
                {
                    quantityText.gameObject.SetActive(false);
                }
            }

            // 장착 표시 (장비 아이템만)
            if (equippedOverlay != null && InventoryItem != null)
            {
                equippedOverlay.gameObject.SetActive(InventoryItem.IsEquipped);
            }

            // 이름
            if (nameText != null)
            {
                nameText.text = Item.Name;
            }
        }

        private void UpdateShopVisuals()
        {
            if (Item == null) return;

            // 아이콘 설정
            if (iconImage != null)
            {
                // TODO: IconPath에서 스프라이트 로드
            }

            // 희귀도 테두리 색상
            if (rarityBorderImage != null)
            {
                rarityBorderImage.color = GetRarityColor(Item.Rarity);
            }

            // 상점에서는 수량 숨김
            if (quantityText != null)
            {
                quantityText.gameObject.SetActive(false);
            }

            // 장착 표시 숨김
            if (equippedOverlay != null)
            {
                equippedOverlay.gameObject.SetActive(false);
            }

            // 이름
            if (nameText != null)
            {
                nameText.text = Item.Name;
            }
        }

        public void SetSelected(bool selected)
        {
            if (rarityBorderImage != null)
            {
                rarityBorderImage.color = selected ? Color.yellow : GetRarityColor(Item?.Rarity ?? ItemRarity.Common);
            }
        }

        public void SetEquipped(bool equipped)
        {
            if (equippedOverlay != null)
            {
                equippedOverlay.gameObject.SetActive(equipped);
            }
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

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }
}
