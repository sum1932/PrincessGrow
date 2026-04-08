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
    /// 인벤토리 패널 View
    /// 아이템 목록 표시
    /// </summary>
    public class InventoryPanelView : MonoBehaviour
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
        [SerializeField] private TextMeshProUGUI itemCountText;
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private ItemDetailView itemDetailView;

        [Header("Close Button")]
        [SerializeField] private Button closeButton;

        public event Action<string> OnItemUsed;  // itemId
        public event Action<string> OnItemEquipped;  // itemId
        public event Action<string> OnItemUnequipped;  // itemId
        public event Action<string, int> OnItemSold;  // itemId, quantity
        public event Action OnPanelClosed;

        private Inventory _inventory;
        private Dictionary<string, Item> _itemDatabase;
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
                itemDetailView.OnUseClicked += OnDetailUseClicked;
                itemDetailView.OnEquipClicked += OnDetailEquipClicked;
                itemDetailView.OnUnequipClicked += OnDetailUnequipClicked;
                itemDetailView.OnSellClicked += OnDetailSellClicked;
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
        /// 인벤토리 데이터 설정
        /// </summary>
        public void Initialize(Inventory inventory, Dictionary<string, Item> itemDatabase, Economy economy)
        {
            _inventory = inventory;
            _itemDatabase = itemDatabase;
            _economy = economy;

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
            UpdateInfoPanel();
        }

        private void RefreshItemGrid()
        {
            ClearItemSlots();

            if (_inventory == null) return;

            var allItems = _inventory.GetAllItems();
            
            foreach (var inventoryItem in allItems)
            {
                if (_itemDatabase.TryGetValue(inventoryItem.ItemId, out var item))
                {
                    // 필터링 적용
                    if (_currentFilter == null || item.Type == _currentFilter.Value)
                    {
                        CreateItemSlot(inventoryItem, item);
                    }
                }
            }

            // 스크롤 위치 리셋
            if (scrollRect != null)
            {
                scrollRect.normalizedPosition = new Vector2(0, 1);
            }
        }

        private void CreateItemSlot(InventoryItem inventoryItem, Item item)
        {
            if (itemSlotPrefab == null || itemGridParent == null) return;

            var slotObj = Instantiate(itemSlotPrefab, itemGridParent);
            var slotView = slotObj.GetComponent<ItemSlotView>();

            if (slotView != null)
            {
                slotView.Initialize(inventoryItem, item, OnItemSlotClicked);
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
            if (itemDetailView != null && slot.InventoryItem != null)
            {
                itemDetailView.ShowItem(slot.InventoryItem, slot.Item);
            }
        }

        private void UpdateInfoPanel()
        {
            if (itemCountText != null && _inventory != null)
            {
                int count = _inventory.GetAllItems().Count;
                itemCountText.text = $"{count}";
            }

            if (moneyText != null && _economy != null)
            {
                moneyText.text = $"{_economy.CurrentMoney}G";
            }
        }

        private void UpdateTabVisuals()
        {
            // 탭 버튼 시각적 업데이트 (선택된 탭 강조)
            // TODO: Image 컴포넌트 색상 변경 등
        }

        // ItemDetailView 이벤트 핸들러
        private void OnDetailUseClicked()
        {
            if (_selectedSlot?.InventoryItem != null)
            {
                OnItemUsed?.Invoke(_selectedSlot.InventoryItem.ItemId);
            }
        }

        private void OnDetailEquipClicked()
        {
            if (_selectedSlot?.InventoryItem != null)
            {
                OnItemEquipped?.Invoke(_selectedSlot.InventoryItem.ItemId);
            }
        }

        private void OnDetailUnequipClicked()
        {
            if (_selectedSlot?.InventoryItem != null)
            {
                OnItemUnequipped?.Invoke(_selectedSlot.InventoryItem.ItemId);
            }
        }

        private void OnDetailSellClicked(int quantity)
        {
            if (_selectedSlot?.InventoryItem != null)
            {
                OnItemSold?.Invoke(_selectedSlot.InventoryItem.ItemId, quantity);
                itemDetailView?.Hide();
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
                itemDetailView.OnUseClicked -= OnDetailUseClicked;
                itemDetailView.OnEquipClicked -= OnDetailEquipClicked;
                itemDetailView.OnUnequipClicked -= OnDetailUnequipClicked;
                itemDetailView.OnSellClicked -= OnDetailSellClicked;
                itemDetailView.OnCloseClicked -= OnDetailCloseClicked;
            }
        }
    }
}
