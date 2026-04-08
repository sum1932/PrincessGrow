using System.Collections.Generic;
using System.Linq;

namespace DessertKingdom.Core.Domain
{
    /// <summary>
    /// 플레이어 인벤토리
    /// </summary>
    public class Inventory
    {
        private Dictionary<string, InventoryItem> _items;

        public Inventory()
        {
            _items = new Dictionary<string, InventoryItem>();
        }

        /// <summary>
        /// 아이템 추가
        /// </summary>
        public void AddItem(string itemId, int quantity = 1)
        {
            if (_items.ContainsKey(itemId))
            {
                _items[itemId].Quantity += quantity;
            }
            else
            {
                _items[itemId] = new InventoryItem(itemId, quantity);
            }
        }

        /// <summary>
        /// 아이템 제거
        /// </summary>
        public bool RemoveItem(string itemId, int quantity = 1)
        {
            if (!_items.ContainsKey(itemId))
                return false;

            var item = _items[itemId];
            if (item.Quantity < quantity)
                return false;

            item.Quantity -= quantity;
            if (item.Quantity <= 0)
            {
                _items.Remove(itemId);
            }

            return true;
        }

        /// <summary>
        /// 아이템 보유 여부 확인
        /// </summary>
        public bool HasItem(string itemId, int quantity = 1)
        {
            return _items.ContainsKey(itemId) && _items[itemId].Quantity >= quantity;
        }

        /// <summary>
        /// 아이템 수량 확인
        /// </summary>
        public int GetItemQuantity(string itemId)
        {
            return _items.ContainsKey(itemId) ? _items[itemId].Quantity : 0;
        }

        /// <summary>
        /// 모든 아이템 조회
        /// </summary>
        public List<InventoryItem> GetAllItems()
        {
            return _items.Values.ToList();
        }

        /// <summary>
        /// 특정 아이템 조회
        /// </summary>
        public InventoryItem GetItem(string itemId)
        {
            return _items.ContainsKey(itemId) ? _items[itemId] : null;
        }

        /// <summary>
        /// 아이템 장착
        /// </summary>
        public void EquipItem(string itemId)
        {
            if (_items.ContainsKey(itemId))
            {
                _items[itemId].IsEquipped = true;
            }
        }

        /// <summary>
        /// 아이템 장착 해제
        /// </summary>
        public void UnequipItem(string itemId)
        {
            if (_items.ContainsKey(itemId))
            {
                _items[itemId].IsEquipped = false;
            }
        }

        /// <summary>
        /// 인벤토리 비우기
        /// </summary>
        public void Clear()
        {
            _items.Clear();
        }
    }
}
