using BepInEx;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace VentureValheim.NPCS;

public static class NPCZDOUtils
{
    public static readonly char BackTickSeparator = '`';
    public static readonly char[] BackTickSeparatorList = new char[] { '`' };
    public static readonly char PipeSeparator = '|';
    public static readonly char[] PipeSeparatorList = new char[] { '|' };
    public static readonly char[] CommaSeparatorList = new char[] { ',' };

    public static readonly int Version = 2;

    public const string ZDOVar_NPCVERSION = "VV_NPCVersion";
    public static int GetVersion(ZDO zdo) => zdo.GetInt(ZDOVar_NPCVERSION);
    public static void SetVersion(ref ZDO zdo) => zdo.Set(ZDOVar_NPCVERSION, Version);

    public const string ZDOVar_INITIALIZED = "VV_Initialized";
    public static bool GetInitialized(ZDO zdo) => zdo.GetBool(ZDOVar_INITIALIZED);
    public static void SetInitialized(ref ZDO zdo, bool init) => zdo.Set(ZDOVar_INITIALIZED, init);

    public static string GetTamedName(ZDO zdo) => zdo.GetString(ZDOVars.s_tamedName);
    public static void SetTamedName(ref ZDO zdo, string name) => zdo.Set(ZDOVars.s_tamedName, name);

    public const string ZDOVar_NPCTYPE = "VV_NPCType";
    public static int GetType(ZDO zdo) => zdo.GetInt(ZDOVar_NPCTYPE);
    public static void SetType(ref ZDO zdo, int type) => zdo.Set(ZDOVar_NPCTYPE, type);

    public const string ZDOVar_ANIMATION = "VV_Animation";
    public static string GetAnimation(ZDO zdo) => zdo.GetString(ZDOVar_ANIMATION);
    public static void SetAnimation(ref ZDO zdo, string animation) => zdo.Set(ZDOVar_ANIMATION, animation);

    public const string ZDOVar_ATTACHED = "VV_Attached";
    public static bool GetAttached(ZDO zdo) => zdo.GetBool(ZDOVar_ATTACHED);
    public static void SetAttached(ref ZDO zdo, bool attach) => zdo.Set(ZDOVar_ATTACHED, attach);

    public const string ZDOVar_ROTATION = "VV_Rotation";
    public static Quaternion GetRotation(ZDO zdo) => zdo.GetQuaternion(ZDOVar_ROTATION, Quaternion.identity);
    public static void SetRotation(ref ZDO zdo, Quaternion rotation) => zdo.Set(ZDOVar_ROTATION, rotation);

    public const string ZDOVar_SPAWNPOINT = "VV_SpawnPoint";
    public static Vector3 GetSpawnPoint(ZDO zdo) => zdo.GetVec3(ZDOVar_SPAWNPOINT, Vector3.zero);
    public static void SetSpawnPoint(ref ZDO zdo, Vector3 point) => zdo.Set(ZDOVar_SPAWNPOINT, point);

    public const string ZDOVar_DEFEATKEY = "VV_DefeatKey";
    public static string GetNPCDefeatKey(ZDO zdo) => zdo.GetString(ZDOVar_DEFEATKEY);
    public static void SetNPCDefeatKey(ref ZDO zdo, string key) => zdo.Set(ZDOVar_DEFEATKEY, key);

    public const string ZDOVar_TRUEDEATH = "VV_TrueDeath";
    public static bool GetTrueDeath(ZDO zdo) => zdo.GetBool(ZDOVar_TRUEDEATH);
    public static void SetTrueDeath(ref ZDO zdo, bool death) => zdo.Set(ZDOVar_TRUEDEATH, death);

    public const string ZDOVar_QUESTCOUNT = "VV_QuestCount";
    public static int GetNPCQuestCount(ZDO zdo) => zdo.GetInt(ZDOVar_QUESTCOUNT);
    public static void SetNPCQuestCount(ref ZDO zdo, int count) => zdo.Set(ZDOVar_QUESTCOUNT, count);

    public const string ZDOVar_QUESTS = "VV_Quests";
    public static bool GetNPCQuest(ZDO zdo, int index, out NPCQuest quest)
    {
        string zdoVar = zdo.GetString($"{ZDOVar_QUESTS}{index}");

        if (zdoVar.IsNullOrWhiteSpace())
        {
            quest = null;
            return false;
        }

        string[] fields = zdoVar.Split(BackTickSeparatorList, int.MaxValue, StringSplitOptions.None);

        NPCSPlugin.NPCSLogger.LogDebug($"GetNPCQuest {index} has length {fields.Length}: {zdoVar}");

        if (fields.Length >= 9)
        {
            quest = new NPCQuest(fields);

            var give = GetNPCQuestGive(zdo, index);

            NPCSPlugin.NPCSLogger.LogDebug($"GetNPCQuest give: {give}");
            if (!give.IsNullOrWhiteSpace())
            {
                var gives = give.Split(BackTickSeparatorList, int.MaxValue, StringSplitOptions.None);
                if (gives.Length >= 4)
                {
                    quest.GiveItem = new NPCItem(gives);
                }
            }

            var reward = GetNPCQuestReward(zdo, index);
            NPCSPlugin.NPCSLogger.LogDebug($"GetNPCQuest reward: {reward}");
            if (!reward.IsNullOrWhiteSpace())
            {
                var rewards = reward.Split(PipeSeparatorList, int.MaxValue, StringSplitOptions.None);
                if (rewards.Length > 0)
                {
                    var rewardIndexed = reward.Split(BackTickSeparatorList, int.MaxValue, StringSplitOptions.None);
                    if (rewardIndexed.Length >= 4)
                    {
                        quest.RewardItems = new List<NPCItem>
                        {
                            new NPCItem(rewardIndexed)
                        };
                    }
                }
            }

            return true;
        }

        NPCSPlugin.NPCSLogger.LogDebug($"GetNPCQuest failed!!");

        quest = null;
        return false;
    }

    public static string GetNPCQuest(ZDO zdo, int index) =>
        zdo.GetString($"{ZDOVar_QUESTS}{index}");
    public static string GetNPCQuestGive(ZDO zdo, int index) =>
        zdo.GetString($"{ZDOVar_QUESTS}{index}GIVE");
    public static string GetNPCQuestReward(ZDO zdo, int index) =>
        zdo.GetString($"{ZDOVar_QUESTS}{index}REWARD");

    public static void SetNPCQuest(ref ZDO zdo, int index, NPCQuest quest)
    {
        SetNPCQuestString(ref zdo, index, Utility.GetString(quest));
        if (quest == null)
        {
            NPCSPlugin.NPCSLogger.LogDebug($"SetNPCQuest, quest null! Setting blank data.");
            SetNPCQuestGive(ref zdo, index, "");
            SetNPCQuestReward(ref zdo, index, "");
        }
        else
        {
            SetNPCQuestGive(ref zdo, index, Utility.GetString(quest.GiveItem));
            SetNPCQuestReward(ref zdo, index, Utility.GetStringFromList(quest.RewardItems));
        }
    }

    public static void SetNPCQuestString(ref ZDO zdo, int index, string quest)
    {
        NPCSPlugin.NPCSLogger.LogDebug($"Setting Quest String: {quest}");
        zdo.Set($"{ZDOVar_QUESTS}{index}", quest);
    }
    public static void SetNPCQuestGive(ref ZDO zdo, int index, string give)
    {
        NPCSPlugin.NPCSLogger.LogDebug($"Setting Quest Give: {give}");
        zdo.Set($"{ZDOVar_QUESTS}{index}GIVE", give);
    }
    public static void SetNPCQuestReward(ref ZDO zdo, int index, string reward)
    {
        NPCSPlugin.NPCSLogger.LogDebug($"Setting Quest Reward: {reward}");
        zdo.Set($"{ZDOVar_QUESTS}{index}REWARD", reward);
    }

    // Trader
    public const string ZDOVar_TRADEITEMS = "VV_TradeItems";
    public static List<Trader.TradeItem> GetNPCTradeItems(ZDO zdo) =>
        GetTradeItems(zdo.GetString(ZDOVar_TRADEITEMS));
    public static void SetNPCTradeItems(ref ZDO zdo, List<NPCTradeItem> items) =>
        zdo.Set(ZDOVar_TRADEITEMS, Utility.GetStringFromList(items));

    public const string ZDOVar_TRADERUSEITEMS = "VV_TraderUseItems";
    public static List<Trader.TraderUseItem> GetNPCTraderUseItems(ZDO zdo) =>
        GetTraderUseItems(zdo.GetString(ZDOVar_TRADERUSEITEMS));
    public static void SetNPCTraderUseItems(ref ZDO zdo, List<NPCTraderUseItem> items) =>
        zdo.Set(ZDOVar_TRADERUSEITEMS, Utility.GetStringFromList(items));

    private static List<Trader.TradeItem> GetTradeItems(string config)
    {
        List<Trader.TradeItem> list = new List<Trader.TradeItem>();
        string[] fields = config.Split(PipeSeparatorList, int.MaxValue, StringSplitOptions.RemoveEmptyEntries);

        foreach (var field in fields)
        {
            var item = GetTradeItem(field);

            if (item != null)
            {
                list.Add(item);
            }
        }
        NPCSPlugin.NPCSLogger.LogDebug($"GetTradeItems: {list.Count}, from fields {fields.Length}: {config}");

        return list;
    }

    private static Trader.TradeItem GetTradeItem(string item)
    {
        string[] fields = item.Split(BackTickSeparatorList, int.MaxValue, StringSplitOptions.None);

        if (fields.Length >= 5)
        {
            var npcTradeItem = new NPCTradeItem(fields);
            Utility.GetItemPrefab(npcTradeItem.PrefabName.GetStableHashCode(), out var prefab);
            if (prefab == null || !prefab.TryGetComponent<ItemDrop>(out var itemdrop))
            {
                return null;
            }

            Trader.TradeItem tradeItem = new Trader.TradeItem();
            tradeItem.m_prefab = itemdrop;
            tradeItem.m_stack = npcTradeItem.Amount.Value;
            tradeItem.m_price = npcTradeItem.Cost.Value;
            tradeItem.m_requiredGlobalKey = npcTradeItem.RequiredKey;
            return tradeItem;
        }

        return null;
    }

    private static List<Trader.TraderUseItem> GetTraderUseItems(string config)
    {
        List<Trader.TraderUseItem> list = new List<Trader.TraderUseItem>();
        string[] items = config.Split(PipeSeparatorList, int.MaxValue, StringSplitOptions.RemoveEmptyEntries);

        foreach (var fieldString in items)
        {
            string[] fields = fieldString.Split(BackTickSeparatorList, int.MaxValue, StringSplitOptions.None);
            if (fields.Length >= 4)
            {
                var traderUseItem = new NPCTraderUseItem(fields);

                if (traderUseItem != null)
                {
                    var prefab = ZNetScene.instance.GetPrefab(traderUseItem.PrefabName);
                    if (prefab == null || !prefab.TryGetComponent<ItemDrop>(out var itemdrop))
                    {
                        return null;
                    }

                    Trader.TraderUseItem useItem = new Trader.TraderUseItem();
                    useItem.m_prefab = itemdrop;
                    useItem.m_setsGlobalKey = traderUseItem.RewardKey;
                    useItem.m_removesItem = traderUseItem.RemoveItem.Value;
                    useItem.m_dialog = traderUseItem.Text;

                    list.Add(useItem);
                }
            }
        }
        NPCSPlugin.NPCSLogger.LogDebug($"GetTraderUseItems: {list.Count}");

        return list;
    }

    // Random Talking
    private static string JoinStrings(List<string> strings)
    {
        string result = "";
        foreach (string s in strings)
        {
            result += $"{s}{BackTickSeparator}";
        }
        return result;
    }

    public const string ZDOVar_AGGROTEXTS = "VV_AggroTexts";
    public const string ZDOVar_TALKTEXTS = "VV_TalkTexts";
    public const string ZDOVar_GREETTEXTS = "VV_GreetTexts";
    public const string ZDOVar_GOODBYETEXTS = "VV_ByeTexts";
    public const string ZDOVar_STARTTRADETEXTS = "VV_StartTradeTexts";
    public const string ZDOVar_BUYTEXTS = "VV_BuyTexts";
    public const string ZDOVar_SELLTEXTS = "VV_SellTexts";
    public const string ZDOVar_NOTCORRECTTEXTS = "VV_NoCorrTexts";
    public const string ZDOVar_NOTAVAILABLETEXTS = "VV_NoAvailTexts";

    public static List<string> GetNPCTexts(string zdoVar, ZDO zdo, bool isTrader = false)
    {
        string text = zdo.GetString(zdoVar);
        var thing = text.Split(BackTickSeparatorList, int.MaxValue, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (thing.Count == 0 && isTrader)
        {
            thing.Add("...");
        }
        return thing;
    }

    public static void SetNPCTexts(string zdoVar, ref ZDO zdo, List<string> texts) =>
        zdo.Set(zdoVar, JoinStrings(texts));

    // TODO add cooldown option
    #region Legacy Fields
    public const string ZDOVar_SITTING = "VV_Sitting";
    public static bool GetLegacyNPCSitting(ZDO zdo) => zdo.GetBool(ZDOVar_SITTING);

    // Main Text used when all else fails
    private const string ZDOVar_DEFAULTTEXT = "VV_DefaultText";
    private static string GetLegacyNPCDefaultText(ZDO zdo) => zdo.GetString(ZDOVar_DEFAULTTEXT);

    // Interact Text
    private const string ZDOVar_INTERACTTEXT = "VV_InteractText";
    private static string GetLegacyNPCInteractText(ZDO zdo) => zdo.GetString(ZDOVar_INTERACTTEXT);

    // Use Item
    private const string ZDOVar_GIVEITEM = "VV_GiveItem";
    private static string GetLegacyNPCGiveItem(ZDO zdo) => zdo.GetString(ZDOVar_GIVEITEM);

    private const string ZDOVar_GIVEITEMQUALITY = "VV_UseItemQuality";
    private static int GetLegacyNPCGiveItemQuality(ZDO zdo) => zdo.GetInt(ZDOVar_GIVEITEMQUALITY);

    private const string ZDOVar_GIVEITEMAMOUNT = "VV_UseItemAmount";
    private static int GetLegacyNPCGiveItemAmount(ZDO zdo) => zdo.GetInt(ZDOVar_GIVEITEMAMOUNT);

    // Reward
    private const string ZDOVar_REWARDTEXT = "VV_RewardText";
    private static string GetLegacyNPCRewardText(ZDO zdo) => zdo.GetString(ZDOVar_REWARDTEXT);

    private const string ZDOVar_REWARDITEM = "VV_RewardItem";
    private static string GetLegacyNPCRewardItem(ZDO zdo) => zdo.GetString(ZDOVar_REWARDITEM);

    private const string ZDOVar_REWARDITEMQUALITY = "VV_RewardItemQualtiy";
    private static int GetLegacyNPCRewardItemQuality(ZDO zdo) => zdo.GetInt(ZDOVar_REWARDITEMQUALITY);

    private const string ZDOVar_REWARDITEMAMOUNT = "VV_RewardItemAmount";
    private static int GetLegacyNPCRewardItemAmount(ZDO zdo) => zdo.GetInt(ZDOVar_REWARDITEMAMOUNT);

    private const string ZDOVar_REWARDLIMIT = "VV_RewardLimit";
    private static int GetLegacyNPCRewardLimit(ZDO zdo) => zdo.GetInt(ZDOVar_REWARDLIMIT, -1); // -1 is unlimited

    // Keys
    private const string ZDOVar_REQUIREDKEYS = "VV_RequiredKeys";
    private static string GetLegacyNPCRequiredKeys(ZDO zdo) => zdo.GetString(ZDOVar_REQUIREDKEYS);

    private const string ZDOVar_NOTREQUIREDKEYS = "VV_NotRequiredKeys";
    private static string GetLegacyNPCNotRequiredKeys(ZDO zdo) => zdo.GetString(ZDOVar_NOTREQUIREDKEYS);

    private const string ZDOVar_INTERACTKEY = "VV_InteractKey";
    private static string GetLegacyNPCInteractKey(ZDO zdo) => zdo.GetString(ZDOVar_INTERACTKEY);

    private const string ZDOVar_INTERACTKEYTYPE = "VV_InteractKeyType";
    private static bool GetLegacyNPCInteractKeyType(ZDO zdo) => zdo.GetBool(ZDOVar_INTERACTKEYTYPE);

    private const string ZDOVar_REWARDKEY = "VV_RewardKey";
    private static string GetLegacyNPCRewardKey(ZDO zdo) => zdo.GetString(ZDOVar_REWARDKEY);

    private const string ZDOVar_REWARDKEYTYPE = "VV_RewardKeyType";
    private static bool GetLegacyNPCRewardKeyType(ZDO zdo) => zdo.GetBool(ZDOVar_REWARDKEYTYPE);

    private static void ResetLegacyNPCData(ref ZDO zdo)
    {
        // TODO validate
        zdo.Set(ZDOVar_DEFAULTTEXT, "");
        zdo.Set(ZDOVar_INTERACTTEXT, "");
        zdo.Set(ZDOVar_GIVEITEM, "");
        zdo.RemoveInt(ZDOVar_GIVEITEMQUALITY);
        zdo.RemoveInt(ZDOVar_GIVEITEMAMOUNT);
        zdo.Set(ZDOVar_REWARDTEXT, "");
        zdo.Set(ZDOVar_REWARDITEM, "");
        zdo.RemoveInt(ZDOVar_REWARDITEMQUALITY);
        zdo.RemoveInt(ZDOVar_REWARDITEMAMOUNT);
        zdo.RemoveInt(ZDOVar_REWARDLIMIT);
        zdo.Set(ZDOVar_REQUIREDKEYS, ""); 
        zdo.Set(ZDOVar_NOTREQUIREDKEYS, "");
        zdo.Set(ZDOVar_INTERACTKEY, "");
        zdo.RemoveInt(ZDOVar_INTERACTKEYTYPE);
        zdo.Set(ZDOVar_REWARDKEY, "");
        zdo.RemoveInt(ZDOVar_REWARDKEYTYPE);

        zdo.RemoveInt(ZDOVar_SITTING);
    }

    # endregion

    // Vanity
    public static Vector3 GetSkinColor(ZDO zdo) => zdo.GetVec3(ZDOVars.s_skinColor, Vector3.zero);
    public static Vector3 GetHairColor(ZDO zdo) => zdo.GetVec3(ZDOVars.s_hairColor, Vector3.zero);

    public static List<int> ZdoVisEquipment = new List<int>
    {
        ZDOVars.s_beardItem,
        ZDOVars.s_hairItem,
        ZDOVars.s_helmetItem,
        ZDOVars.s_chestItem,
        ZDOVars.s_legItem,
        ZDOVars.s_shoulderItem,
        ZDOVars.s_shoulderItemVariant,
        ZDOVars.s_utilityItem,
        ZDOVars.s_rightItem,
        ZDOVars.s_leftItem,
        ZDOVars.s_leftItemVariant
    };

    public static void CopyVisEquipment(ref VisEquipment copy, ZDO original)
    {
        copy.SetModel(original.GetInt(ZDOVars.s_modelIndex));
        copy.SetSkinColor(original.GetVec3(ZDOVars.s_skinColor, Vector3.zero));
        copy.SetHairColor(original.GetVec3(ZDOVars.s_hairColor, Vector3.zero));

        foreach (int item in ZdoVisEquipment)
        {
            copy.m_nview.GetZDO().Set(item, original.GetInt(item));
        }
    }

    // TODO remove
    /*public static void CopyZDO(ref ZDO copy, ZDO original)
    {
        SetVersion(ref copy);
        // TODO, what rotation needs to be copied when?
        //copy.SetRotation(original.GetRotation());
        SetRotation(ref copy, GetRotation(original));

        // TODO, investigate seed not working to spawn exact same creature
        copy.Set(ZDOVars.s_seed, original.GetInt(ZDOVars.s_seed));
        SetInitialized(ref copy, GetInitialized(original));
        SetTamedName(ref copy, GetTamedName(original));
        SetType(ref copy, GetType(original));
        SetAnimation(ref copy, GetAnimation(original));
        SetAttached(ref copy, GetAttached(original));
        SetSpawnPoint(ref copy, GetSpawnPoint(original));
        SetTrueDeath(ref copy, GetTrueDeath(original));
        SetNPCDefeatKey(ref copy, GetNPCDefeatKey(original));

        int oldCount = GetNPCQuestCount(copy);
        int count = GetNPCQuestCount(original);

        if (oldCount > count)
        {
            // Clean up old data
            for (int lcv = count; lcv < oldCount; lcv++)
            {
                SetNPCQuestString(ref copy, lcv, "");
                SetNPCQuestGive(ref copy, lcv, "");
                SetNPCQuestReward(ref copy, lcv, "");
            }
        }

        SetNPCQuestCount(ref copy, count);
        for (int lcv = 0; lcv < count; lcv++)
        {
            SetNPCQuestString(ref copy, lcv, GetNPCQuest(original, lcv));
            SetNPCQuestGive(ref copy, lcv, GetNPCQuestGive(original, lcv));
            SetNPCQuestReward(ref copy, lcv, GetNPCQuestReward(original, lcv));
        }
    }*/

    public static void SetZDOFromConfig(ref ZDO zdo, NPCConfig config)
    {
        try
        {
            SetTamedName(ref zdo, config.Name);
            SetType(ref zdo, (int)config.Type);
            SetTrueDeath(ref zdo, config.TrueDeath);
            SetNPCDefeatKey(ref zdo, config.DefeatKey);

            SetInitialized(ref zdo, !config.GiveDefaultItems);

            if (config.Quests != null)
            {
                SetQuestData(ref zdo, config.Quests);
            }

            SetNPCTradeItems(ref zdo, config.TradeItems);
            SetNPCTraderUseItems(ref zdo, config.TraderUseItems);

            SetNPCTexts(ZDOVar_AGGROTEXTS, ref zdo, config.AggravatedTexts);
            SetNPCTexts(ZDOVar_TALKTEXTS, ref zdo, config.TalkTexts);
            SetNPCTexts(ZDOVar_GREETTEXTS, ref zdo, config.GreetTexts);
            SetNPCTexts(ZDOVar_GOODBYETEXTS, ref zdo, config.GoodbyeTexts);
            SetNPCTexts(ZDOVar_STARTTRADETEXTS, ref zdo, config.StartTradeTexts);
            SetNPCTexts(ZDOVar_BUYTEXTS, ref zdo, config.BuyTexts);
            SetNPCTexts(ZDOVar_SELLTEXTS, ref zdo, config.SellTexts);
            SetNPCTexts(ZDOVar_NOTAVAILABLETEXTS, ref zdo, config.NotAvailableTexts);
            SetNPCTexts(ZDOVar_NOTCORRECTTEXTS, ref zdo, config.NotCorrectTexts);

            SetVersion(ref zdo);
        }
        catch (Exception e)
        {
            NPCSPlugin.NPCSLogger.LogError("There was an issue spawing the npc from configurations, " +
                "did you forget to reload the file?");
            NPCSPlugin.NPCSLogger.LogWarning(e);
        }
    }

    private static void SetQuestData(ref ZDO zdo, List<NPCQuest> quests)
    {
        int oldCount = GetNPCQuestCount(zdo);
        NPCSPlugin.NPCSLogger.LogDebug($"Setting quest data, {oldCount} old quests found");
        if (oldCount > 0)
        {
            // Clean up old data
            for (int lcv = 0; lcv < oldCount; lcv++)
            {
                SetNPCQuest(ref zdo, lcv, null);
            }
        }

        for (int lcv = 0; lcv < quests.Count; lcv++)
        {
            SetNPCQuest(ref zdo, lcv, quests[lcv]);
        }

        SetNPCQuestCount(ref zdo, quests.Count);
    }

    public static void UpgradeVersion(ref ZDO zdo)
    {
        if (GetVersion(zdo) < 2)
        {
            NPCSPlugin.NPCSLogger.LogDebug($"Upgrading version....");
            // Upgrade from unversioned
            NPCData.NPCType type = (NPCData.NPCType)GetType(zdo);

            if (type == NPCData.NPCType.Information || type == NPCData.NPCType.Reward)
            {
                NPCQuest firstQuest = new NPCQuest();
                // TODO text cases for InteractText
                firstQuest.Text = GetLegacyNPCDefaultText(zdo); //GetLegacyNPCInteractText(zdo);
                firstQuest.RewardText = GetLegacyNPCRewardText(zdo);

                firstQuest.NotRequiredKeys = GetLegacyNPCNotRequiredKeys(zdo);
                firstQuest.RequiredKeys = GetLegacyNPCRequiredKeys(zdo);
                firstQuest.InteractKey = GetLegacyNPCInteractKey(zdo);
                firstQuest.InteractKeyType = GetLegacyNPCInteractKeyType(zdo) ? NPCData.NPCKeyType.Player : NPCData.NPCKeyType.Global; // TODO check
                firstQuest.RewardKey = GetLegacyNPCRewardKey(zdo);
                firstQuest.RewardKeyType = GetLegacyNPCRewardKeyType(zdo) ? NPCData.NPCKeyType.Player : NPCData.NPCKeyType.Global; // TODO check
                firstQuest.RewardLimit = GetLegacyNPCRewardLimit(zdo);

                var reward = GetLegacyNPCRewardItem(zdo);
                if (!reward.IsNullOrWhiteSpace())
                {
                    firstQuest.RewardItems = new List<NPCItem>();
                    var item = new NPCItem();
                    item.PrefabName = reward;
                    item.Amount = GetLegacyNPCRewardItemAmount(zdo);
                    item.Quality = GetLegacyNPCRewardItemQuality(zdo);
                    item.RemoveItem = true;
                    firstQuest.RewardItems.Add(item);
                }

                var give = GetLegacyNPCGiveItem(zdo);
                if (!give.IsNullOrWhiteSpace())
                {
                    firstQuest.GiveItem = new NPCItem();
                    firstQuest.GiveItem.PrefabName = give;
                    firstQuest.GiveItem.Amount = GetLegacyNPCGiveItemAmount(zdo);
                    firstQuest.GiveItem.Quality = GetLegacyNPCGiveItemQuality(zdo);
                    firstQuest.GiveItem.RemoveItem = true;
                }

                SetQuestData(ref zdo, new List<NPCQuest> { firstQuest });

                if (GetLegacyNPCSitting(zdo))
                {
                    SetAttached(ref zdo, true);
                    SetAnimation(ref zdo, "attach_chair");
                }

                ResetLegacyNPCData(ref zdo);
            }

            SetVersion(ref zdo);
        }
    }
}
