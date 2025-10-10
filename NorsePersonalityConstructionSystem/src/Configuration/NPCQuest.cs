using System;
using System.Collections.Generic;

namespace VentureValheim.NPCS;

#nullable enable
[Serializable]
public class NPCQuest
{
    public int? Index { get; set; }
    public string? Text { get; set; }
    public string? RewardText { get; set; }
    public string? RequiredKeys { get; set; }
    public HashSet<string>? RequiredKeysSet { get; private set; }
    public string? NotRequiredKeys { get; set; }
    public HashSet<string>? NotRequiredKeysSet { get; private set; }
    public string? InteractKey { get; set; }
    public NPCData.NPCKeyType InteractKeyType { get; set; }
    public NPCItem? GiveItem { get; set; }
    public List<NPCItem>? RewardItems { get; set; }
    public string? RewardKey { get; set; }
    public NPCData.NPCKeyType RewardKeyType { get; set; }
    public int? RewardLimit { get; set; }

    public NPCQuest() { }
    public NPCQuest(string[] fields)
    {
        if (!int.TryParse(fields[4], out int interactType))
        {
            interactType = 0;
        }

        if (!int.TryParse(fields[6], out int rewardType))
        {
            rewardType = 0;
        }

        if (!int.TryParse(fields[8], out int limit))
        {
            limit = -1;
        }

        Text = fields[0];
        RequiredKeys = fields[1];
        RequiredKeysSet = Utility.StringToSet(RequiredKeys);
        NotRequiredKeys = fields[2];
        NotRequiredKeysSet = Utility.StringToSet(NotRequiredKeys);
        InteractKey = fields[3];
        InteractKeyType = (NPCData.NPCKeyType)interactType;
        RewardKey = fields[5];
        RewardKeyType = (NPCData.NPCKeyType)rewardType;
        RewardText = fields[7];
        RewardLimit = limit;
    }

    public void CleanData()
    {
        Text ??= "";
        Text = NPCConfiguration.ReplaceReservedCharacters(Text);
        RequiredKeys ??= "";
        RequiredKeysSet = Utility.StringToSet(RequiredKeys);
        NotRequiredKeys ??= "";
        NotRequiredKeysSet = Utility.StringToSet(NotRequiredKeys);
        InteractKey ??= "";
        RewardKey ??= "";
        RewardLimit ??= -1; // Unlimited

        GiveItem?.CleanData();

        if (RewardItems != null)
        {
            foreach (var item in RewardItems)
            {
                item?.CleanData();
            }
        }
    }

    public override string ToString()
    {
        return $"{Text}`"
        + $"{RequiredKeys}`"
        + $"{NotRequiredKeys}`"
        + $"{InteractKey}`"
        + $"{InteractKeyType}`"
        + $"{RewardKey}`"
        + $"{RewardKeyType}`"
        + $"{RewardText}`"
        + $"{RewardLimit}";
    }
}
