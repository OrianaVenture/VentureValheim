using System.Collections.Generic;
using BepInEx;
using UnityEngine;

namespace VentureValheim.NPCS;

public static class NPCUtils
{
    public static string SetupText(NPCQuest quest, string text)
    {
        if (quest == null)
        {
            return text;
        }

        if (quest.GiveItem != null)
        {
            string giveItem = quest.GiveItem.PrefabName;
            if (Utility.GetItemDrop(giveItem, out var requirement))
            {
                giveItem = requirement.m_itemData.m_shared.m_name;
            }

            string giveItemText = $"{quest.GiveItem.Amount} {giveItem}";
            if (quest.GiveItem.Quality > 1)
            {
                giveItemText += $" *{quest.GiveItem.Quality}";
            }

            text = text.Replace("{giveitem}", giveItemText);
        }

        if (quest.RewardItem != null)
        {
            string rewardItem = quest.RewardItem.PrefabName;
            if (Utility.GetItemDrop(rewardItem, out var reward))
            {
                rewardItem = reward.m_itemData.m_shared.m_name;
            }

            string rewardItemText = $"{quest.RewardItem.Amount} {rewardItem}";
            if (quest.RewardItem.Quality > 1)
            {
                rewardItemText += $" *{quest.RewardItem.Quality}";
            }

            text = text.Replace("{reward}", rewardItemText);
        }

        return Localization.instance.Localize(text);
    }

    public static void Talk(Character npc, string title, string text, NPCQuest quest)
    {
        if (Player.m_localPlayer != null && !text.IsNullOrWhiteSpace())
        {
            Chat.instance.SetNpcText(npc.gameObject, Vector3.up * 2f, 15f, 20f, title, SetupText(quest, text), true);
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

    public static void GiveReward(ref ZNetView nview, ref Character character, NPCQuest quest)
    {
        NPCSPlugin.NPCSLogger.LogDebug($"Trying giving reward item! {quest.RewardItem == null}");
        if (quest.RewardItem != null &&
            Utility.GetItemPrefab(quest.RewardItem.PrefabName.GetStableHashCode(), out GameObject reward))
        {
            NPCSPlugin.NPCSLogger.LogDebug("Giving reward item!");
            var go = GameObject.Instantiate(reward,
                character.transform.position + new Vector3(0, 0.75f, 0) + (character.transform.rotation * Vector3.forward),
                character.transform.rotation);

            var itemdrop = go.GetComponent<ItemDrop>();
            itemdrop.SetStack(quest.RewardItem.Amount.Value);
            itemdrop.SetQuality(quest.RewardItem.Quality.Value);
            itemdrop.GetComponent<Rigidbody>().velocity = (character.transform.forward + Vector3.up) * 1f;
            character.m_zanim.SetTrigger("interact");
        }

        int rewardLimit = quest.RewardLimit.Value;
        if (rewardLimit > 0)
        {
            // TODO evaluate if this cna be broken into a seperate zdo variable to minimize setting stuff
            nview.ClaimOwnership();
            quest.RewardLimit -= 1;
            NPCZDOUtils.SetNPCQuest(ref nview, quest.Index.Value, quest);
        }

        if (!quest.RewardKey.IsNullOrWhiteSpace())
        {
            Utility.SetKey(quest.RewardKey, quest.RewardKeyType);
        }
    }

    public static bool TryUseItem(Character npc, ItemDrop.ItemData item)
    {
        var baseAI = npc.GetComponent<BaseAI>();

        if (baseAI != null && baseAI.m_aggravated)
        {
            return false;
        }

        var zNetView = npc.GetComponent<ZNetView>();

        if (NPCZDOUtils.GetType(zNetView) == (int)NPCData.NPCType.Trader)
        {
            var trader = npc.GetComponent<NPCTrader>();
            if (trader == null)
            {
                return false;
            }

            trader.UseItem(Player.m_localPlayer, item);
            return true;
        }

        var npcComponent = npc.GetComponent<INPC>();

        NPCQuest quest = npcComponent.Data.GetCurrentQuest();
        if (quest == null)
        {
            NPCSPlugin.NPCSLogger.LogDebug("TryUseItem: Quest is null!");
            return false;
        }

        var name = NPCZDOUtils.GetTamedName(zNetView);
        string text = quest.Text;

        if (quest.GiveItem != null && !quest.GiveItem.PrefabName.IsNullOrWhiteSpace())
        {
            if (item != null && item.m_dropPrefab.name.Equals(quest.GiveItem.PrefabName) && Player.m_localPlayer != null)
            {
                var quality = quest.GiveItem.Quality.Value;
                var amount = quest.GiveItem.Amount.Value;
                var player = Player.m_localPlayer;
                var count = player.GetInventory().CountItems(item.m_shared.m_name, quality);
                if (count >= amount)
                {
                    player.UnequipItem(item);
                    player.GetInventory().RemoveItem(item.m_shared.m_name, amount, quality);
                }
                else
                {
                    Talk(npc, name, "Hmmm that's not enough... " + text, quest);
                    return false;
                }
            }
            else
            {
                Talk(npc, name, "Hmmm that's not right... " + text, quest);
                return false;
            }
        }

        if (TryGiveReward(ref zNetView, ref npc, quest))
        {
            if (!quest.RewardText.IsNullOrWhiteSpace())
            {
                text = quest.RewardText;
            }
        }

        Talk(npc, name, text, quest);

        npcComponent.Data.UpdateQuest(true);
        return true;
    }

    public static bool TryInteract(Character npc)
    {
        var baseAI = npc.GetComponent<BaseAI>();

        if (baseAI.m_aggravated)
        {
            return false;
        }

        var zNetView = npc.GetComponent<ZNetView>();

        if (NPCZDOUtils.GetType(zNetView) == (int)NPCData.NPCType.Trader)
        {
            var trader = npc.GetComponent<NPCTrader>();
            if (trader == null)
            {
                return false;
            }

            //TODO
            trader.Interact(null, false, false);
            return true;
        }

        var npcComponent = npc.GetComponent<INPC>();

        NPCQuest quest = npcComponent.Data.GetCurrentQuest();
        if (quest == null)
        {
            NPCSPlugin.NPCSLogger.LogDebug("TryUseItem: Quest is null!");
            return false;
        }

        var text = quest.Text;

        if (quest.GiveItem == null || quest.GiveItem.PrefabName.IsNullOrWhiteSpace())
        {
            // I give my reward on interact, not when giving an item
            if (TryGiveReward(ref zNetView, ref npc, quest))
            {
                if (!quest.RewardText.IsNullOrWhiteSpace())
                {
                    text = quest.RewardText;
                }
            }
        }

        if (!quest.InteractKey.IsNullOrWhiteSpace())
        {
            Utility.SetKey(quest.InteractKey, quest.InteractKeyType);
        }

        var name = NPCZDOUtils.GetTamedName(zNetView);
        Talk(npc, name, text, quest); // TODO disable talking as needed

        npcComponent.Data.UpdateQuest(true);
        return false;
    }

    private static bool TryGiveReward(ref ZNetView nview, ref Character character, NPCQuest quest)
    {
        if (HasCorrectReqiuredKeys(quest.RequiredKeysSet))
        {
            GiveReward(ref nview, ref character, quest);
            return true;
        }

        return false;
    }

    public static string GetHoverText(Character npc, BaseAI baseAI)
    {
        if (npc == null || baseAI == null || baseAI.m_aggravated)
        {
            return "";
        }

        var type = NPCZDOUtils.GetType(npc.m_nview);
        string text = "";

        if (type != (int)NPCData.NPCType.None)
        {
            var npcComponent = npc.GetComponent<INPC>();
            var quest = npcComponent.Data.GetCurrentQuest();

            if (quest != null || type == (int)NPCData.NPCType.Trader)
            {
                text = Localization.instance.Localize(
                    "[<color=yellow><b>$KEY_Use</b></color>] $raven_interact");

                if (quest != null && quest.GiveItem != null)
                {
                    text += Localization.instance.Localize(
                        "\n[<color=yellow><b>1-8</b></color>] $npc_giveitem");
                }
            }
        }

        return text;
    }

    public static void OnDeath(Character character)
    {
        if (character.m_nview == null || !character.m_nview.IsOwner())
        {
            return;
        }

        GameObject[] effects = character.m_deathEffects.Create(
        character.transform.position, character.transform.rotation, character.transform);
        for (int lcv = 0; lcv < effects.Length; lcv++)
        {
            Ragdoll ragdoll = effects[lcv].GetComponent<Ragdoll>();

            if (ragdoll != null)
            {
                Vector3 velocity = character.m_body.velocity;
                if (character.m_pushForce.magnitude * 0.5f > velocity.magnitude)
                {
                    velocity = character.m_pushForce * 0.5f;
                }

                ragdoll.Setup(velocity, 0f, 0f, 0f, null);

                VisEquipment visEquip = effects[lcv].GetComponent<VisEquipment>();
                if (visEquip != null)
                {
                    NPCZDOUtils.CopyVisEquipment(ref visEquip, (character as Humanoid).m_visEquipment);
                }

                NPCZDOUtils.SetTamedName(ref ragdoll.m_nview, character.m_name);
                NPCZDOUtils.SetTrueDeath(ref ragdoll.m_nview, NPCZDOUtils.GetTrueDeath(character.m_nview));
            }
        }

        Utility.SetKey(character.m_defeatSetGlobalKey, NPCData.NPCKeyType.Global); // TODO, support private here?

        if (character.m_onDeath != null)
        {
            character.m_onDeath();
        }

        if (!NPCZDOUtils.GetTrueDeath(character.m_nview))
        {
            NPCFactory.RespawnNPC(character.transform.root.gameObject);
        }

        ZNetScene.instance.Destroy(character.gameObject);
    }
}
