using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Views
{
    /// <summary>
    /// 상점 패널 View
    /// 상점 아이템 목록 표시 및 구매 기능
    /// </summary>
    public class ShopPanelView : MonoBehaviour
    {
        [Header("Tab Buttons")]
        [SerializeField] private Button allTabButton;
        [SerializeField] private Button consumableTabButton;
        [SerializeField] private Button equipmentTabButton;
        [SerializeField] private Button materialTabButton;
        [SerializeField] private Button questTabButton;

        [Header("Item Grid")]
        [SerializeField] private Transform itemGridParent;
        [SerializeField] private GameObject itemSlotPrefab;
        [SerializeField] private GridLayoutGroup gridLayout;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Info Panel")]
        [SerializeField] private TextMeshProUGUI shopNameText;
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private ItemDetailView itemDetailView;

        [Header("Close Button")]
        [SerializeField] private Button closeButton;

        public event Action<string, int> OnItemPurchased;  // itemId, quantity
        public event Action OnPanelClosed;

        private List<Item> _availableItems;
        private Economy _economy;
        private List<ItemSlotView> _itemSlots = new();
        private ItemType? _currentFilter = null;
        private ItemSlotView _selectedSlot = null;

        private void Awake()
        {
            SetupTabButtons();
            
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => OnPanelClosed?.Invoke());
            }

            // ItemDetailView 이벤트 연결
            if (itemDetailView != null)
            {
                itemDetailView.OnBuyClicked += OnDetailBuyClicked;
                itemDetailView.OnCloseClicked += OnDetailCloseClicked;
            }
        }

        private void SetupTabButtons()
        {
            if (allTabButton != null)
                allTabButton.onClick.AddListener(() => SelectTab(null));
            if (consumableTabButton != null)
                consumableTabButton.onClick.AddListener(() => SelectTab(ItemType.Consumable));
            if (equipmentTabButton != null)
                equipmentTabButton.onClick.AddListener(() => SelectTab(ItemType.Equipment));
            if (materialTabButton != null)
                materialTabButton.onClick.AddListener(() => SelectTab(ItemType.Material));
            if (questTabButton != null)
                questTabButton.onClick.AddListener(() => SelectTab(ItemType.Quest));
        }

        /// <summary>
        /// 상점 초기화
        /// </summary>
        public void Initialize(List<Item> availableItems, Economy economy, string shopName = "상점")
        {
            _availableItems = availableItems ?? new List<Item>();
            _economy = economy;

            if (shopNameText != null)
            {
                shopNameText.text = shopName;
            }

            RefreshUI();
        }

        /// <summary>
        /// 특정 탭 선택
        /// </summary>
        public void SelectTab(ItemType? type)
        {
            _currentFilter = type;
            RefreshUI();
            UpdateTabVisuals();
        }

        private void RefreshUI()
        {
            RefreshItemGrid();
            UpdateMoneyDisplay();
        }

        private void RefreshItemGrid()
        {
            ClearItemSlots();

            if (_availableItems == null) return;

            var items = _currentFilter.HasValue 
                ? _availableItems.Where(i => i.Type == _currentFilter.Value).ToList()
                : _availableItems;
            
            foreach (var item in items)
            {
                CreateItemSlot(item);
            }

            // 스크롤 위치 리셋
            if (scrollRect != null)
            {
                scrollRect.normalizedPosition = new Vector2(0, 1);
            }
        }

        private void CreateItemSlot(Item item)
        {
            if (itemSlotPrefab == null || itemGridParent == null) return;

            var slotObj = Instantiate(itemSlotPrefab, itemGridParent);
            var slotView = slotObj.GetComponent<ItemSlotView>();

            if (slotView != null)
            {
                slotView.Initialize(item, OnItemSlotClicked);
                _itemSlots.Add(slotView);
            }
        }

        private void ClearItemSlots()
        {
            foreach (var slot in _itemSlots)
            {
                if (slot != null && slot.gameObject != null)
                    Destroy(slot.gameObject);
            }
            _itemSlots.Clear();
            _selectedSlot = null;
        }

        private void OnItemSlotClicked(ItemSlotView slot)
        {
            // 이전 선택 해제
            if (_selectedSlot != null)
            {
                _selectedSlot.SetSelected(false);
            }

            // 새 선택
            _selectedSlot = slot;
            _selectedSlot.SetSelected(true);

            // 상세 정보 표시
            if (itemDetailView != null && slot.Item != null)
            {
                int maxAffordable = _economy != null ? _economy.CurrentMoney / slot.Item.Price : 1;
                int maxBuyable = Mathf.Min(maxAffordable, 99);
                itemDetailView.ShowShopItem(slot.Item, maxBuyable);
            }
        }

        private void UpdateMoneyDisplay()
        {
            if (moneyText != null && _economy != null)
            {
                moneyText.text = $"보유 골드: {_economy.CurrentMoney}G";
            }
        }

        private void UpdateTabVisuals()
        {
            // TODO: 선택된 탭 버튼 강조 표시
        }

        // ItemDetailView 이벤트 핸들러
        private void OnDetailBuyClicked(int quantity)
        {
            if (_selectedSlot?.Item != null)
            {
                string itemId = _selectedSlot.Item.Id;
                
                if (_economy != null && _economy.CanAfford(_selectedSlot.Item.Price * quantity))
                {
                    OnItemPurchased?.Invoke(itemId, quantity);
                }
            }
        }

        private void OnDetailCloseClicked()
        {
            if (_selectedSlot != null)
            {
                _selectedSlot.SetSelected(false);
                _selectedSlot = null;
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            RefreshUI();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            itemDetailView?.Hide();
        }

        private void OnDestroy()
        {
            if (itemDetailView != null)
            {
                itemDetailView.OnBuyClicked -= OnDetailBuyClicked;
                itemDetailView.OnCloseClicked -= OnDetailCloseClicked;
            }
        }
    }
}
