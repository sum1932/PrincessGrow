namespace DessertKingdom.Core.Domain
{
    /// <summary>
    /// 인벤토리 아이템 (플레이어가 소유한 아이템 인스턴스)
    /// </summary>
    public class InventoryItem
    {
        public string ItemId { get; }
        public int Quantity { get; set; }
        public bool IsEquipped { get; set; }
        public string InstanceId { get; }

        public InventoryItem(string itemId, int quantity = 1, string instanceId = null)
        {
            ItemId = itemId;
            Quantity = quantity;
            InstanceId = instanceId ?? System.Guid.NewGuid().ToString();
            IsEquipped = false;
        }
    }

    /// <summary>
    /// 아이템 카테고리 (상세 분류)
    /// </summary>
    public enum ItemCategory
    {
        // Consumable
        Consumable_Recovery,
        Consumable_Buff,
        Consumable_Special,

        // Gift
        Gift_Food,
        Gift_Cosmetic,
        Gift_Lifestyle,
        Gift_Special,

        // Equipment
        Equipment_Outfit,
        Equipment_Accessory,

        // Special
        Special_Quest,
        Special_Growth
    }
}
