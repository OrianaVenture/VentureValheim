using System.Collections;
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

        if (quest.GiveItem != null && text.Contains("{giveitem}"))
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

        if (quest.RewardItems != null && text.Contains("{reward}"))
        {
            string rewardItemText = "";
            for (int lcv = 0; lcv < quest.RewardItems.Count; lcv++)
            {
                var item = quest.RewardItems[lcv];
                string rewardItem = item.PrefabName;
                if (Utility.GetItemDrop(rewardItem, out var reward))
                {
                    rewardItem = reward.m_itemData.m_shared.m_name;
                }

                rewardItemText += $"{item.Amount} {rewardItem}";
                if (item.Quality > 1)
                {
                    rewardItemText += $" *{item.Quality}";
                }

                if (lcv < quest.RewardItems.Count -1)
                {
                    rewardItemText += ", ";
                }
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
        if (quest.RewardItems != null)
        {
            foreach (var item in quest.RewardItems)
            {
                if (Utility.GetItemPrefab(item.PrefabName.GetStableHashCode(), out GameObject reward))
                {
                    var go = GameObject.Instantiate(reward,
                        character.transform.position + new Vector3(0, 0.75f, 0) + (character.transform.rotation * Vector3.forward),
                        character.transform.rotation);

                    var itemdrop = go.GetComponent<ItemDrop>();
                    itemdrop.SetStack(item.Amount.Value);
                    itemdrop.SetQuality(item.Quality.Value);
                    itemdrop.GetComponent<Rigidbody>().linearVelocity = (character.transform.forward + Vector3.up) * 1f;
                }
            }

            character.m_zanim.SetTrigger("interact");
        }

        int rewardLimit = quest.RewardLimit.Value;
        if (rewardLimit > 0)
        {
            nview.ClaimOwnership();
            quest.RewardLimit -= 1;
            ZDO zdo = nview.GetZDO();
            NPCZDOUtils.SetNPCQuest(ref zdo, quest.Index.Value, quest);
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

        ZNetView zNetView = npc.GetComponent<ZNetView>();
        ZDO zdo = zNetView.GetZDO();

        if (NPCZDOUtils.GetType(zdo) == (int)NPCData.NPCType.Trader)
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
            return false;
        }

        var name = NPCZDOUtils.GetTamedName(zdo);
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
                    if (quest.GiveItem.RemoveItem.Value)
                    {
                        player.UnequipItem(item);
                        player.GetInventory().RemoveItem(item.m_shared.m_name, amount, quality);
                        player.m_zanim.SetTrigger("interact");
                    }
                }
                else
                {
                    // TODO: Localize these.
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

        if (NPCZDOUtils.GetType(zNetView.GetZDO()) == (int)NPCData.NPCType.Trader)
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

        var name = NPCZDOUtils.GetTamedName(zNetView.GetZDO());
        Talk(npc, name, text, quest); // TODO disable talking as needed
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

    // TODO: This is run on a continuous loop, make more efficient
    public static string GetHoverText(Character npc, BaseAI baseAI)
    {
        if (npc == null || npc.m_nview.GetZDO() == null || (baseAI != null && baseAI.m_aggravated))
        {
            return "";
        }

        var type = NPCZDOUtils.GetType(npc.m_nview.GetZDO());
        string text = "";

        if (type != (int)NPCData.NPCType.None)
        {
            var npcComponent = npc.GetComponent<INPC>();
            var quest = npcComponent.Data.GetCurrentQuest(false);

            if (type == (int)NPCData.NPCType.Trader || quest != null)
            {
                text = Localization.instance.Localize(
                    "[<color=yellow><b>$KEY_Use</b></color>] $raven_interact");

                if (type == (int)NPCData.NPCType.Trader ||
                    (quest != null && quest.GiveItem != null))
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

        ZDO zdo = character.m_nview.GetZDO();

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
                    NPCZDOUtils.CopyVisEquipment(ref visEquip, zdo);
                }

                ZDO ragdollZdo = ragdoll.m_nview.GetZDO();
                NPCZDOUtils.SetTamedName(ref ragdollZdo, character.m_name);
                NPCZDOUtils.SetTrueDeath(ref ragdollZdo, NPCZDOUtils.GetTrueDeath(zdo));
            }
        }

        Utility.SetKey(character.m_defeatSetGlobalKey, NPCData.NPCKeyType.Global); // TODO, support private here?

        if (character.m_onDeath != null)
        {
            character.m_onDeath();
        }

        if (!NPCZDOUtils.GetTrueDeath(zdo))
        {
            NPCSPlugin.NPCSLogger.LogWarning($"Adding new {character.name} to respawner!");
            NPCRespawner.Instance.AddZdo(Utils.GetPrefabName(character.name), zdo);
        }

        ZNetScene.instance.Destroy(character.gameObject);
    }
}
