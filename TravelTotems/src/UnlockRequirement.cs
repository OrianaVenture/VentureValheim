using UnityEngine;

namespace VentureValheim.TravelTotems;

public struct UnlockRequirement
{
    public string PrefabName;
    public int Amount;
    private string ItemDropName;

    public UnlockRequirement(string prefab, int amount)
    {
        PrefabName = prefab;
        Amount = amount;
        ItemDropName = null;
    }

    public string GetItemDropName()
    {
        if (ItemDropName == null)
        {
        GameObject item = ObjectDB.instance.GetItemPrefab(PrefabName);

        if (item == null )
        {
            return string.Empty;
        }

        ItemDrop itemDrop = item.GetComponent<ItemDrop>();

        if (itemDrop == null)
        {
            return string.Empty;
        }

        ItemDropName = itemDrop.m_itemData.m_shared.m_name;
        }

        return ItemDropName;
    }
}