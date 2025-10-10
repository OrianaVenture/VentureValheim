using System;

namespace VentureValheim.NPCS;

#nullable enable
[Serializable]
public class NPCTraderUseItem
{
    public string? PrefabName { get; set; }
    public string? RewardKey { get; set; }
    public bool? RemoveItem { get; set; }
    public string? Text { get; set; }

    public NPCTraderUseItem() { }
    public NPCTraderUseItem(string[] fields)
    {
        if (!bool.TryParse(fields[2], out bool remove))
        {
            remove = true;
        }

        PrefabName = fields[0];
        RewardKey = fields[1];
        RemoveItem = remove;
        Text = fields[3];
    }

    public void CleanData()
    {
        PrefabName ??= "";
        RewardKey ??= "";
        RemoveItem ??= false;
        Text ??= "";
        Text = NPCConfiguration.ReplaceReservedCharacters(Text);
    }

    public override string ToString()
    {
        return $"{PrefabName}`{RewardKey}`{RemoveItem}`{Text}";
    }
}
