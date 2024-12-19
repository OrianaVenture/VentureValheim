using System;

namespace VentureValheim.NPCS;

[Serializable]
public class NPCItem
{
    public string PrefabName { get; set; }
    public int? Quality { get; set; }
    public int? Amount { get; set; }
    public bool? RemoveItem { get; set; }

    public NPCItem() { }
    public NPCItem(string[] fields)
    {
        if (!int.TryParse(fields[1], out int quality))
        {
            quality = 1;
        }

        if (!int.TryParse(fields[2], out int amount))
        {
            amount = 1;
        }

        if (!bool.TryParse(fields[3], out bool remove))
        {
            remove = true;
        }

        PrefabName = fields[0];
        Quality = quality;
        Amount = amount;
        RemoveItem = remove;
    }

    public void CleanData()
    {
        PrefabName ??= "";
        Quality ??= 1;
        Amount ??= 1;
        RemoveItem ??= true;
    }

    public override string ToString()
    {
        return $"{PrefabName}`" +
            $"{Quality.Value}`" +
            $"{Amount.Value}`" +
            $"{RemoveItem}";
    }
}