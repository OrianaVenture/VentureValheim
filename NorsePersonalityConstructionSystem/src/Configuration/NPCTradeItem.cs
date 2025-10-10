using System;

namespace VentureValheim.NPCS;

#nullable enable
[Serializable]
public class NPCTradeItem
{
    public string? PrefabName { get; set; }
    public int? Quality { get; set; }
    public int? Amount { get; set; }
    public int? Cost { get; set; }
    public string? RequiredKey { get; set; }

    public NPCTradeItem() { }
    public NPCTradeItem(string[] fields)
    {
        if (!int.TryParse(fields[1], out int quality))
        {
            quality = 1;
            // TODO: This is not easily supported at time of creation
        }

        if (!int.TryParse(fields[2], out int amount))
        {
            amount = 1;
        }

        if (!int.TryParse(fields[3], out int cost))
        {
            cost = 999;
        }

        PrefabName = fields[0];
        Quality = quality;
        Amount = amount;
        Cost = cost;
        RequiredKey = fields[4];
    }

    public void CleanData()
    {
        PrefabName ??= "";
        Quality ??= 1;
        Amount ??= 1;
        Cost ??= 999;
        RequiredKey ??= "";
    }

    public override string ToString()
    {
        return $"{PrefabName}`{Quality}`{Amount}`{Cost}`{RequiredKey}";
    }
}