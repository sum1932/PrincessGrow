namespace DessertKingdom.Core.Domain
{
    /// <summary>
    /// 아이템 정보
    /// </summary>
    public class Item
    {
        public string Id { get; }
        public string Name { get; }
        public ItemType Type { get; }
        public ItemRarity Rarity { get; }
        public string Description { get; }
        public int Price { get; }
        public int EffectValue { get; }
        public string IconPath { get; }

        public Item(string id, string name, ItemType type, ItemRarity rarity, 
                   string description, int price, int effectValue, string iconPath)
        {
            Id = id;
            Name = name;
            Type = type;
            Rarity = rarity;
            Description = description;
            Price = price;
            EffectValue = effectValue;
            IconPath = iconPath;
        }
    }

    /// <summary>
    /// 아이템 타입
    /// </summary>
    public enum ItemType
    {
        Consumable,
        Equipment,
        Material,
        Quest
    }

    /// <summary>
    /// 아이템 희귀도
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}
