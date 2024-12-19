using System;
using System.Collections.Generic;

namespace VentureValheim.NPCS;

[Serializable]
public class NPCConfig
{
    public string Id { get; set; }
    public string Name { get; set; }
    public NPCData.NPCType Type { get; set; }
    public string DefeatKey { get; set; }
    public bool TrueDeath { get; set; }
    public bool StandStill { get; set; }
    public bool GiveDefaultItems { get; set; }
    public List<NPCQuest> Quests { get; set; }
    // Trader
    public List<NPCTradeItem> TradeItems { get; set; }
    public List<NPCTraderUseItem> TraderUseItems { get; set; }
    // Texts
    public List<string> TalkTexts { get; set; }
    public List<string> GreetTexts { get; set; }
    public List<string> GoodbyeTexts { get; set; }
    public List<string> AggravatedTexts { get; set; }
    public List<string> StartTradeTexts { get; set; }
    public List<string> BuyTexts { get; set; }
    public List<string> SellTexts { get; set; }
    public List<string> NotCorrectTexts { get; set; }
    public List<string> NotAvailableTexts { get; set; }
    // Style
    public string Model { get; set; }
    public float? HairColorR { get; set; }
    public float? HairColorG { get; set; }
    public float? HairColorB { get; set; }
    public float? SkinColorR { get; set; }
    public float? SkinColorG { get; set; }
    public float? SkinColorB { get; set; }
    public int? ModelIndex { get; set; }
    public string Hair { get; set; }
    public string Beard { get; set; }
    public string Helmet { get; set; }
    public string Chest { get; set; }
    public string Legs { get; set; }
    public string Shoulder { get; set; }
    public int? ShoulderVariant { get; set; }
    public string Utility { get; set; }
    public string RightHand { get; set; }
    public string LeftHand { get; set; }
    public int? LeftHandVariant { get; set; }
    // OLD, TODO: migrate old data
    /*public string DefaultText { get; set; }
    public string RequiredKeys { get; set; }
    public string NotRequiredKeys { get; set; }
    public string InteractKey { get; set; }
    public NPCUtils.NPCKeyType InteractKeyType { get; set; }
    public string InteractText { get; set; }
    public string GiveItem { get; set; }
    public int? GiveItemQuality { get; set; }
    public int? GiveItemAmount { get; set; }
    public string RewardText { get; set; }
    public string RewardItem { get; set; }
    public int? RewardItemQuality { get; set; }
    public int? RewardItemAmount { get; set; }
    public string RewardKey { get; set; }
    public NPCUtils.NPCKeyType RewardKeyType { get; set; }
    public int? RewardLimit { get; set; }*/

    public void CleanData()
    {
        Name ??= "Ragnar";
        DefeatKey ??= "";
        Quests ??= new List<NPCQuest>();
        foreach (var quest in Quests)
        {
            quest.CleanData();
        }

        TradeItems ??= new List<NPCTradeItem>();
        foreach (var trade in TradeItems)
        {
            trade.CleanData();
        }

        TraderUseItems ??= new List<NPCTraderUseItem>();
        foreach (var item in TraderUseItems)
        {
            item.CleanData();
        }

        // TODO
        TalkTexts ??= new List<string>();
        NPCConfiguration.ReplaceReservedCharacters(TalkTexts);
        GreetTexts ??= new List<string>();
        NPCConfiguration.ReplaceReservedCharacters(GreetTexts);
        GoodbyeTexts ??= new List<string>();
        NPCConfiguration.ReplaceReservedCharacters(GoodbyeTexts);
        AggravatedTexts ??= new List<string>();
        NPCConfiguration.ReplaceReservedCharacters(AggravatedTexts);
        StartTradeTexts ??= new List<string>();
        NPCConfiguration.ReplaceReservedCharacters(StartTradeTexts);
        BuyTexts ??= new List<string>();
        NPCConfiguration.ReplaceReservedCharacters(BuyTexts);
        SellTexts ??= new List<string>();
        NPCConfiguration.ReplaceReservedCharacters(SellTexts);
        NotCorrectTexts ??= new List<string>();
        NPCConfiguration.ReplaceReservedCharacters(NotCorrectTexts);
        NotAvailableTexts ??= new List<string>();
        NPCConfiguration.ReplaceReservedCharacters(NotAvailableTexts);

        Model ??= "Player";
        Hair ??= "";
        Beard ??= "";
        Helmet ??= "";
        Chest ??= "";
        Legs ??= "";
        Shoulder ??= "";
        ShoulderVariant ??= 0;
        Utility ??= "";
        RightHand ??= "";
        LeftHand ??= "";
        LeftHandVariant ??= 0;
    }
}
