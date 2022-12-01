namespace VentureValheim.Progression
{
    public enum ItemCategory
    {
        Undefined = -1,
        Weapon = 0,
        Armor = 1,
        Shield = 2
    }

    public enum ItemType
    {
        Undefined = -1,
        None = 0,
        Shield = 1,
        Helmet = 2,
        Chest = 3,
        Legs = 4,
        Shoulder = 5,
        Utility = 6,
        Tool = 7,
        PickAxe = 8,
        Axe = 9,
        Bow = 10,
        Ammo = 11,
        Sword = 20,
        Knife = 21,
        Mace = 22,
        Sledge = 23,
        Atgeir = 25,
        Battleaxe = 26,
        Primative = 27,
        Spear = 28,
        TowerShield = 29,
        BucklerShield = 30,
        PrimativeArmor = 31
    }

    public class ItemClassification
    {
        public string Name;
        public WorldConfiguration.Biome BiomeType;
        public ItemType ItemType;
        public ItemCategory ItemCategory;
        public float ItemValue;
        public int VanillaUpgradeLevels;
        public float VanillaValue;
        public float VanillaUpgradeValue;
        public HitData.DamageTypes? VanillaDamageValue;
        public HitData.DamageTypes? VanillaUpgradeDamageValue;

        public ItemClassification(string name, WorldConfiguration.Biome biomeType, ItemType itemType, ItemCategory itemCategory, float value)
        {
            Name = name;
            BiomeType = biomeType;
            ItemType = itemType;
            ItemCategory = itemCategory;
            ItemValue = value;
            VanillaUpgradeLevels = 0;
            VanillaValue = 0;
            VanillaUpgradeValue = 0;
            VanillaDamageValue = null;
            VanillaUpgradeDamageValue = null;
        }

        public void UpdateItem(WorldConfiguration.Biome biomeType, ItemType itemType, ItemCategory itemCategory, float value)
        {
            BiomeType = biomeType;
            ItemType = itemType;
            ItemCategory = itemCategory;
            ItemValue = value;
        }

        public static ItemCategory GetItemCategory(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.BucklerShield:
                case ItemType.Shield:
                case ItemType.TowerShield:
                    return ItemCategory.Shield;
                case ItemType.Shoulder:
                case ItemType.PrimativeArmor:
                case ItemType.Helmet:
                case ItemType.Chest:
                case ItemType.Legs:
                case ItemType.Utility:
                    return ItemCategory.Armor;
                case ItemType.Primative:
                case ItemType.Knife:
                case ItemType.Ammo:
                case ItemType.PickAxe:
                case ItemType.Sword:
                case ItemType.Mace:
                case ItemType.Spear:
                case ItemType.Axe:
                case ItemType.Sledge:
                case ItemType.Atgeir:
                case ItemType.Battleaxe:
                case ItemType.Bow:
                case ItemType.Tool:
                    return ItemCategory.Weapon;
                default:
                    return ItemCategory.Undefined;
            }
        }
    }
}