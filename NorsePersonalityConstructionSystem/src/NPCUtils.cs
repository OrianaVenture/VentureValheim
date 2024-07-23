using System;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;

namespace VentureValheim.NPCS;

public class NPCUtils
{
    public enum NPCType
    {
        None = 0,
        Information = 1,
        Reward = 2,
        Sellsword = 3,
        SlayTarget = 4,
        Trader = 5
    }

    public enum NPCKeyType
    {
        Player = 0,
        Global = 1
    }

    public const string NPCGROUP = "VV_NPC";
    public static readonly int NONE_HASH = "none".GetStableHashCode();

    public static readonly Color32 HairColorMin = new Color32(0xFF, 0xED, 0xB4, 0xFF);
    public static readonly Color32 HairColorMax = new Color32(0xFF, 0x7C, 0x47, 0xFF);
    public static readonly Color32 SkinColorMin = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    public static readonly Color32 SkinColorMax = new Color32(0x4C, 0x4C, 0x4C, 0xFF);

    #region ZDO Values

    public const string ZDOVar_INITIALIZED = "VV_Initialized";
    public static bool GetInitialized(ZNetView nview) => nview.GetZDO().GetBool(ZDOVar_INITIALIZED);
    public static void SetInitialized(ref ZNetView nview, bool init) => nview.GetZDO().Set(ZDOVar_INITIALIZED, init);

    public static string GetTamedName(ZNetView nview) => nview.GetZDO().GetString(ZDOVars.s_tamedName);
    public static void SetTamedName(ref ZNetView nview, string name) => nview.GetZDO().Set(ZDOVars.s_tamedName, name);

    public const string ZDOVar_NPCTYPE = "VV_NPCType";
    public static int GetType(ZNetView nview) => nview.GetZDO().GetInt(ZDOVar_NPCTYPE);
    public static void SetType(ref ZNetView nview, int type) => nview.GetZDO().Set(ZDOVar_NPCTYPE, type);

    // TODO support all kinds of attachment, probably by enum type
    public const string ZDOVar_SITTING = "VV_Sitting";
    public static bool GetSitting(ZNetView nview) => nview.GetZDO().GetBool(ZDOVar_SITTING);
    public static void SetSitting(ref ZNetView nview, bool sit) => nview.GetZDO().Set(ZDOVar_SITTING, sit);

    public const string ZDOVar_ATTACHED = "VV_Attached";
    public static bool GetAttached(ZNetView nview) => nview.GetZDO().GetBool(ZDOVar_ATTACHED);
    public static void SetAttached(ref ZNetView nview, bool attach) => nview.GetZDO().Set(ZDOVar_ATTACHED, attach);

    public const string ZDOVar_SPAWNPOINT = "VV_SpawnPoint";
    public static Vector3 GetSpawnPoint(ZNetView nview) => nview.GetZDO().GetVec3(ZDOVar_SPAWNPOINT, Vector3.zero);
    public static void SetSpawnPoint(ref ZNetView nview, Vector3 point) => nview.GetZDO().Set(ZDOVar_SPAWNPOINT, point);

    // Main Text used when all else fails
    public const string ZDOVar_DEFAULTTEXT = "VV_DefaultText";
    public static string GetNPCDefaultText(ZNetView nview) => nview.GetZDO().GetString(ZDOVar_DEFAULTTEXT);
    public static void SetNPCDefaultText(ref ZNetView nview, string text) => nview.GetZDO().Set(ZDOVar_DEFAULTTEXT, text);

    // Interact Text
    public const string ZDOVar_INTERACTTEXT = "VV_InteractText";
    public static string GetNPCInteractText(ZNetView nview) => nview.GetZDO().GetString(ZDOVar_INTERACTTEXT);
    public static void SetNPCInteractText(ref ZNetView nview, string text) => nview.GetZDO().Set(ZDOVar_INTERACTTEXT, text);

    // Use Item
    public const string ZDOVar_GIVEITEM = "VV_GiveItem";
    public static string GetNPCGiveItem(ZNetView nview) => nview.GetZDO().GetString(ZDOVar_GIVEITEM);
    public static void SetNPCGiveItem(ref ZNetView nview, string item) => nview.GetZDO().Set(ZDOVar_GIVEITEM, item);

    public const string ZDOVar_GIVEITEMQUALITY = "VV_UseItemQuality";
    public static int GetNPCGiveItemQuality(ZNetView nview) => nview.GetZDO().GetInt(ZDOVar_GIVEITEMQUALITY);
    public static void SetNPCGiveItemQuality(ref ZNetView nview, int quality) => nview.GetZDO().Set(ZDOVar_GIVEITEMQUALITY, quality);

    public const string ZDOVar_GIVEITEMAMOUNT = "VV_UseItemAmount";
    public static int GetNPCGiveItemAmount(ZNetView nview) => nview.GetZDO().GetInt(ZDOVar_GIVEITEMAMOUNT);
    public static void SetNPCGiveItemAmount(ref ZNetView nview, int amount) => nview.GetZDO().Set(ZDOVar_GIVEITEMAMOUNT, amount);

    // Reward
    public const string ZDOVar_REWARDTEXT = "VV_RewardText";
    public static string GetNPCRewardText(ZNetView nview) => nview.GetZDO().GetString(ZDOVar_REWARDTEXT);
    public static void SetNPCRewardText(ref ZNetView nview, string text) => nview.GetZDO().Set(ZDOVar_REWARDTEXT, text);

    public const string ZDOVar_REWARDITEM = "VV_RewardItem";
    public static string GetNPCRewardItem(ZNetView nview) => nview.GetZDO().GetString(ZDOVar_REWARDITEM);
    public static void SetNPCRewardItem(ref ZNetView nview, string item) => nview.GetZDO().Set(ZDOVar_REWARDITEM, item);

    public const string ZDOVar_REWARDITEMQUALITY = "VV_RewardItemQualtiy";
    public static int GetNPCRewardItemQuality(ZNetView nview) => nview.GetZDO().GetInt(ZDOVar_REWARDITEMQUALITY);
    public static void SetNPCRewardItemQuality(ref ZNetView nview, int quality) => nview.GetZDO().Set(ZDOVar_REWARDITEMQUALITY, quality);

    public const string ZDOVar_REWARDITEMAMOUNT = "VV_RewardItemAmount";
    public static int GetNPCRewardItemAmount(ZNetView nview) => nview.GetZDO().GetInt(ZDOVar_REWARDITEMAMOUNT);
    public static void SetNPCRewardItemAmount(ref ZNetView nview, int amount) => nview.GetZDO().Set(ZDOVar_REWARDITEMAMOUNT, amount);

    public const string ZDOVar_REWARDLIMIT = "VV_RewardLimit";
    public static int GetNPCRewardLimit(ZNetView nview) => nview.GetZDO().GetInt(ZDOVar_REWARDLIMIT, -1); // -1 is unlimited
    public static void SetNPCRewardLimit(ref ZNetView nview, int limit) => nview.GetZDO().Set(ZDOVar_REWARDLIMIT, limit);

    // TODO add cooldown option
    // Keys
    public const string ZDOVar_REQUIREDKEYS = "VV_RequiredKeys";
    public static string GetNPCRequiredKeys(ZNetView nview) => nview.GetZDO().GetString(ZDOVar_REQUIREDKEYS);
    public static void SetNPCRequiredKeys(ref ZNetView nview, string keys) => nview.GetZDO().Set(ZDOVar_REQUIREDKEYS, keys);

    public const string ZDOVar_NOTREQUIREDKEYS = "VV_NotRequiredKeys";
    public static string GetNPCNotRequiredKeys(ZNetView nview) => nview.GetZDO().GetString(ZDOVar_NOTREQUIREDKEYS);
    public static void SetNPCNotRequiredKeys(ref ZNetView nview, string keys) => nview.GetZDO().Set(ZDOVar_NOTREQUIREDKEYS, keys);

    public const string ZDOVar_INTERACTKEY = "VV_InteractKey";
    public static string GetNPCInteractKey(ZNetView nview) => nview.GetZDO().GetString(ZDOVar_INTERACTKEY);
    public static void SetNPCInteractKey(ref ZNetView nview, string key) => nview.GetZDO().Set(ZDOVar_INTERACTKEY, key);

    public const string ZDOVar_INTERACTKEYTYPE = "VV_InteractKeyType";
    public static bool GetNPCInteractKeyType(ZNetView nview) => nview.GetZDO().GetBool(ZDOVar_INTERACTKEYTYPE);
    public static void SetNPCInteractKeyType(ref ZNetView nview, bool type) => nview.GetZDO().Set(ZDOVar_INTERACTKEYTYPE, type);

    public const string ZDOVar_REWARDKEY = "VV_RewardKey";
    public static string GetNPCRewardKey(ZNetView nview) => nview.GetZDO().GetString(ZDOVar_REWARDKEY);
    public static void SetNPCRewardKey(ref ZNetView nview, string key) => nview.GetZDO().Set(ZDOVar_REWARDKEY, key);

    public const string ZDOVar_REWARDKEYTYPE = "VV_RewardKeyType";
    public static bool GetNPCRewardKeyType(ZNetView nview) => nview.GetZDO().GetBool(ZDOVar_REWARDKEYTYPE);
    public static void SetNPCRewardKeyType(ref ZNetView nview, bool type) => nview.GetZDO().Set(ZDOVar_REWARDKEYTYPE, type);

    public const string ZDOVar_DEFEATKEY = "VV_DefeatKey";
    public static string GetNPCDefeatKey(ZNetView nview) => nview.GetZDO().GetString(ZDOVar_DEFEATKEY);
    public static void SetNPCDefeatKey(ref ZNetView nview, string key) => nview.GetZDO().Set(ZDOVar_DEFEATKEY, key);

    public const string ZDOVar_TRUEDEATH = "VV_TrueDeath";
    public static bool GetTrueDeath(ZNetView nview) => nview.GetZDO().GetBool(ZDOVar_TRUEDEATH);
    public static void SetTrueDeath(ref ZNetView nview, bool death) => nview.GetZDO().Set(ZDOVar_TRUEDEATH, death);

    // Vanity
    public static Vector3 GetSkinColor(ZNetView nview) => nview.GetZDO().GetVec3(ZDOVars.s_skinColor, Vector3.zero);
    public static Vector3 GetHairColor(ZNetView nview) => nview.GetZDO().GetVec3(ZDOVars.s_hairColor, Vector3.zero);

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
        //ZDOVars.s_rightBackItem,
        //ZDOVars.s_leftBackItem,
        //ZDOVars.s_leftBackItemVariant
    };

    #endregion

    #region Helper Functions

    public static string SetupText(ZNetView zNetView, string text)
    {
        if (zNetView == null)
        {
            return text;
        }

        string giveItem = GetNPCGiveItem(zNetView);
        if (Utility.GetItemDrop(giveItem, out var requirement))
        {
            giveItem = requirement.m_itemData.m_shared.m_name;
        }

        string giveItemText = $"{GetNPCGiveItemAmount(zNetView)} {giveItem}";
        if (GetNPCGiveItemQuality(zNetView) > 1)
        {
            giveItemText += $" *{GetNPCGiveItemQuality(zNetView)}";
        }

        string rewardItem = GetNPCRewardItem(zNetView);
        if (Utility.GetItemDrop(rewardItem, out var reward))
        {
            rewardItem = reward.m_itemData.m_shared.m_name;
        }

        string rewardItemText = $"{GetNPCRewardItemAmount(zNetView)} {rewardItem}";
        if (GetNPCRewardItemQuality(zNetView) > 1)
        {
            rewardItemText += $" *{GetNPCRewardItemQuality(zNetView)}";
        }

        text = text.Replace("{giveitem}", giveItemText).Replace("{reward}", rewardItemText);
        return Localization.instance.Localize(text);
    }

    public static void CopyZDO(ref ZNetView copy, ZNetView original)
    {
        // TODO: copy.GetZDO().SetRotation(original.GetZDO().GetRotation());

        // TODO, investigate seed not working to spawn exact same creature
        copy.GetZDO().Set(ZDOVars.s_seed, original.GetZDO().GetInt(ZDOVars.s_seed));
        SetInitialized(ref copy, GetInitialized(original));
        SetTamedName(ref copy, GetTamedName(original));
        SetType(ref copy, GetType(original));
        SetSitting(ref copy, GetSitting(original));
        SetAttached(ref copy, GetAttached(original));
        SetSpawnPoint(ref copy, GetSpawnPoint(original));
        SetTrueDeath(ref copy, GetTrueDeath(original));

        SetNPCDefaultText(ref copy, GetNPCDefaultText(original));
        SetNPCInteractText(ref copy, GetNPCInteractText(original));

        SetNPCGiveItem(ref copy, GetNPCGiveItem(original));
        SetNPCGiveItemQuality(ref copy, GetNPCGiveItemQuality(original));
        SetNPCGiveItemAmount(ref copy, GetNPCGiveItemAmount(original));

        SetNPCRewardText(ref copy, GetNPCRewardText(original));
        SetNPCRewardItem(ref copy, GetNPCRewardItem(original));
        SetNPCRewardItemQuality(ref copy, GetNPCRewardItemQuality(original));
        SetNPCRewardItemAmount(ref copy, GetNPCRewardItemAmount(original));
        SetNPCRewardLimit(ref copy, GetNPCRewardLimit(original));

        SetNPCRequiredKeys(ref copy, GetNPCRequiredKeys(original));
        SetNPCNotRequiredKeys(ref copy, GetNPCNotRequiredKeys(original));
        SetNPCInteractKey(ref copy, GetNPCInteractKey(original));
        SetNPCInteractKeyType(ref copy, GetNPCInteractKeyType(original));
        SetNPCRewardKey(ref copy, GetNPCRewardKey(original));
        SetNPCRewardKeyType(ref copy, GetNPCRewardKeyType(original));
        SetNPCDefeatKey(ref copy, GetNPCDefeatKey(original));
    }

    public static void CopyVisEquipment(ref VisEquipment copy, VisEquipment original)
    {
        copy.SetModel(original.m_nview.GetZDO().GetInt(ZDOVars.s_modelIndex));
        copy.SetSkinColor(original.m_nview.GetZDO().GetVec3(ZDOVars.s_skinColor, Vector3.zero));
        copy.SetHairColor(original.m_nview.GetZDO().GetVec3(ZDOVars.s_hairColor, Vector3.zero));

        foreach (int item in ZdoVisEquipment)
        {
            copy.m_nview.GetZDO().Set(item, original.m_nview.GetZDO().GetInt(item));
        }
    }

    public static void SetZDOFromConfig(ref ZNetView nview, NPCConfiguration.NPCConfig config)
    {
        try
        {
            SetTamedName(ref nview, config.Name);
            SetType(ref nview, (int)config.Type);
            SetTrueDeath(ref nview, config.TrueDeath);

            SetInitialized(ref nview, !config.GiveDefaultItems);

            SetNPCDefaultText(ref nview, config.DefaultText);
            SetNPCInteractText(ref nview, config.InteractText);

            SetNPCGiveItem(ref nview, config.GiveItem);
            SetNPCGiveItemQuality(ref nview, config.GiveItemQuality.Value);
            SetNPCGiveItemAmount(ref nview, config.GiveItemAmount.Value);

            SetNPCRewardText(ref nview, config.RewardText);
            SetNPCRewardItem(ref nview, config.RewardItem);
            SetNPCRewardItemQuality(ref nview, config.RewardItemQuality.Value);
            SetNPCRewardItemAmount(ref nview, config.RewardItemAmount.Value);
            SetNPCRewardLimit(ref nview, config.RewardLimit.Value);

            SetNPCRequiredKeys(ref nview, config.RequiredKeys);
            SetNPCNotRequiredKeys(ref nview, config.NotRequiredKeys);
            SetNPCInteractKey(ref nview, config.InteractKey);
            SetNPCInteractKeyType(ref nview, config.InteractKeyType == NPCKeyType.Global);
            SetNPCRewardKey(ref nview, config.RewardKey);
            SetNPCRewardKeyType(ref nview, config.RewardKeyType == NPCKeyType.Global);
            SetNPCDefeatKey(ref nview, config.DefeatKey);
        }
        catch (Exception e)
        {
            NPCSPlugin.NPCSLogger.LogError("There was an issue spawing the npc from configurations, " +
                "did you forget to reload the file?");
            NPCSPlugin.NPCSLogger.LogWarning(e);
        }
    }

    public static void Talk(GameObject npc, string title, string text)
    {
        if (Player.m_localPlayer != null && !text.IsNullOrWhiteSpace())
        {
            var zNetView = npc.GetComponent<ZNetView>();
            Chat.instance.SetNpcText(npc.gameObject, Vector3.up * 2f, 15f, 20f, title, SetupText(zNetView, text), true);
        }
    }

    public static bool HasCorrectReqiuredKeys(HashSet<string> requiredKeys)
    {
        foreach (string key in requiredKeys)
        {
            if (!Utility.HasKey(key))
            {
                return false;
            }
        }

        return true;
    }

    public static bool HasCorrectNotReqiuredKeys(HashSet<string> notRequiredKeys)
    {
        foreach (string key in notRequiredKeys)
        {
            if (Utility.HasKey(key))
            {
                return false;
            }
        }

        return true;
    }

    public static bool HasCorrectKeys(HashSet<string> requiredKeys, HashSet<string> notRequiredKeys)
    {
        return HasCorrectReqiuredKeys(requiredKeys) && HasCorrectNotReqiuredKeys(notRequiredKeys);
    }

    public static void GiveReward(ref ZNetView nview, ref Character character)
    {
        if (!GetNPCRewardItem(nview).IsNullOrWhiteSpace())
        {
            var reward = ObjectDB.instance.GetItemPrefab(GetNPCRewardItem(nview));

            if (reward != null)
            {
                // TODO: test
                var go = GameObject.Instantiate(reward,
                    character.transform.position + new Vector3(0, 0.75f, 0) + (character.transform.rotation * Vector3.forward),
                    character.transform.rotation);

                var itemdrop = go.GetComponent<ItemDrop>();
                itemdrop.SetStack(GetNPCRewardItemAmount(nview));
                itemdrop.SetQuality(GetNPCRewardItemQuality(nview));
                itemdrop.GetComponent<Rigidbody>().velocity = (character.transform.forward + Vector3.up) * 1f;
                character.m_zanim.SetTrigger("interact");
            }
        }

        int rewardLimit = GetNPCRewardLimit(nview);
        if (rewardLimit > 0)
        {
            nview.ClaimOwnership();
            SetNPCRewardLimit(ref nview, rewardLimit - 1);
        }

        if (!GetNPCRewardKey(nview).IsNullOrWhiteSpace())
        {
            Utility.SetKey(GetNPCRewardKey(nview), GetNPCRewardKeyType(nview));
        }
    }

    public static bool TryUseItem(GameObject npc, ItemDrop.ItemData item)
    {
        var zNetView = npc.GetComponent<ZNetView>();
        var npcComponent = npc.GetComponent<INPC>();
        var baseAI = npc.GetComponent<BaseAI>();

        if (GetType(zNetView) != (int)NPCType.Reward ||
            !HasCorrectKeys(npcComponent.GetRequiredKeysSet(), npcComponent.GetNotRequiredKeysSet()) ||
            GetNPCRewardLimit(zNetView) == 0 ||
            baseAI.m_aggravated)
        {
            return false;
        }

        var giveItem = GetNPCGiveItem(zNetView);
        var name = GetTamedName(zNetView);

        if (!giveItem.IsNullOrWhiteSpace())
        {
            if (item != null && item.m_dropPrefab.name.Equals(giveItem) && Player.m_localPlayer != null)
            {
                var quality = GetNPCGiveItemQuality(zNetView);
                var amount = GetNPCGiveItemAmount(zNetView);
                var player = Player.m_localPlayer;
                var count = player.GetInventory().CountItems(item.m_shared.m_name, quality);
                if (count >= amount)
                {
                    player.UnequipItem(item);
                    player.GetInventory().RemoveItem(item.m_shared.m_name, amount, quality);
                }
                else
                {
                    Talk(npc, name, "Hmmm that's not enough... " + GetNPCInteractText(zNetView));
                    return false;
                }
            }
            else
            {
                Talk(npc, name, "Hmmm that's not right... " + GetNPCInteractText(zNetView));
                return false;
            }
        }

        var character = npc.GetComponent<Character>();
        GiveReward(ref zNetView, ref character);

        Talk(npc, name, GetNPCRewardText(zNetView));

        return true;
    }

    public static bool TryInteract(GameObject npc)
    {
        var zNetView = npc.GetComponent<ZNetView>();
        var npcComponent = npc.GetComponent<INPC>();
        var baseAI = npc.GetComponent<BaseAI>();
        var character = npc.GetComponent<Character>();

        if (baseAI.m_aggravated)
        {
            return false;
        }

        var text = GetNPCDefaultText(zNetView);

        if (GetType(zNetView) == (int)NPCType.Reward)
        {
            if (GetNPCRewardLimit(zNetView) != 0)
            {
                // This is unlocked
                if (GetNPCGiveItem(zNetView).IsNullOrWhiteSpace())
                {
                    // I give my reward on interact, change key requirement text unlocks
                    if (HasCorrectNotReqiuredKeys(npcComponent.GetNotRequiredKeysSet()))
                    {
                        if (HasCorrectReqiuredKeys(npcComponent.GetRequiredKeysSet()))
                        {
                            GiveReward(ref zNetView, ref character);
                            var rewardText = GetNPCRewardText(zNetView);
                            if (!rewardText.IsNullOrWhiteSpace())
                            {
                                text = rewardText;
                            }
                        }
                        else
                        {
                            text = GetNPCInteractText(zNetView);
                        }
                    }
                }
                else if (HasCorrectKeys(npcComponent.GetRequiredKeysSet(), npcComponent.GetNotRequiredKeysSet()))
                {
                    // I give my reward on give item
                    text = GetNPCInteractText(zNetView);
                }
            }
        }
        else if (HasCorrectKeys(npcComponent.GetRequiredKeysSet(), npcComponent.GetNotRequiredKeysSet()))
        {
            text = GetNPCInteractText(zNetView);
        }

        var key = GetNPCInteractKey(zNetView);
        if (!key.IsNullOrWhiteSpace())
        {
            Utility.SetKey(key, GetNPCInteractKeyType(zNetView));
        }

        var name = GetTamedName(zNetView);
        Talk(npc, name, text);

        return false;
    }

    public static string GetHoverText(ZNetView zNetView, BaseAI baseAI)
    {
        if (baseAI == null || baseAI.m_aggravated)
        {
            return "";
        }

        if (zNetView == null || zNetView.GetZDO() == null)
        {
            return "";
        }

        var type = GetType(zNetView);
        string text = "";

        if (type != (int)NPCUtils.NPCType.None)
        {
            text = Localization.instance.Localize(
                "[<color=yellow><b>$KEY_Use</b></color>] Talk");
        }

        if (type == (int)NPCUtils.NPCType.Reward && !GetNPCRewardItem(zNetView).IsNullOrWhiteSpace())
        {
            text += Localization.instance.Localize(
                "\n[<color=yellow><b>1-8</b></color>] Give");
        }

        return text;
    }

    public static GameObject CreateGameObject(GameObject original, string name)
    {
        GameObject go = GameObject.Instantiate(original, NPCSPlugin.Root.transform, false);
        go.name = NPCSPlugin.MOD_PREFIX + name;
        go.transform.SetParent(NPCSPlugin.Root.transform, false);

        return go;
    }

    public static void RegisterGameObject(GameObject obj)
    {
        ZNetScene.instance.m_prefabs.Add(obj);
        ZNetScene.instance.m_namedPrefabs.Add(obj.name.GetStableHashCode(), obj);
        NPCSPlugin.NPCSLogger.LogDebug($"Adding object to prefabs {obj.name}");
    }

    #endregion
}
