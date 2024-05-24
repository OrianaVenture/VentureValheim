using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.VentureQuest;

public class Patches
{
    /// <summary>
    /// Attempts to get the ItemDrop by the given name's hashcode, if not found searches by string.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="item"></param>
    /// <returns>True on sucessful find</returns>
    public static bool GetItemDrop(string name, out ItemDrop item)
    {
        item = null;

        if (!name.IsNullOrWhiteSpace())
        {
            // Try hash code
            var prefab = ObjectDB.instance.GetItemPrefab(name.GetStableHashCode());
            if (prefab == null)
            {
                // Failed, try slow search
                prefab = ObjectDB.instance.GetItemPrefab(name);
            }

            if (prefab != null)
            {
                item = prefab.GetComponent<ItemDrop>();
                if (item != null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static NPC GetClosestNPC(Vector3 position)
    {
        Collider[] hits = Physics.OverlapBox(position, Vector3.one, Quaternion.identity);
        NPC closestnpc = null;

        foreach (var hit in hits)
        {
            var npcs = hit.transform.root.gameObject.GetComponentsInChildren<NPC>();
            if (npcs != null)
            {
                for (int lcv = 0; lcv < npcs.Length; lcv++)
                {
                    var npc = npcs[lcv];
                    if (closestnpc == null || (Vector3.Distance(position, npc.transform.position) <
                        Vector3.Distance(position, closestnpc.transform.position)))
                    {
                        closestnpc = npc;
                    }
                }
            }
        }

        return closestnpc;
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
    private static class Patch_Terminal_InitTerminal
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(out bool __state)
        {
            __state = Terminal.m_terminalInitialized;
        }

        private static void Postfix(bool __state)
        {
            if (__state)
            {
                return;
            }

            VentureQuestPlugin.VentureQuestLogger.LogInfo("Adding Terminal Commands for npc management.");
            // TODO: Migrate to a format that can accept long string inputs

            new Terminal.ConsoleCommand("vq_spawnNPC", "[name] [model]", delegate (Terminal.ConsoleEventArgs args)
            {
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;
                var playerRotation = Player.m_localPlayer.gameObject.transform.rotation;
                var position = playerPosition + (playerRotation * Vector3.forward);

                if (args.Length > 2)
                {
                    NPCFactory.SpawnNPC(position, playerRotation, args[1], args[2]);
                }
                else if (args.Length > 1)
                {
                    NPCFactory.SpawnNPC(position, playerRotation,  args[1]);
                }
                else
                {
                    NPCFactory.SpawnNPC(position, playerRotation);
                }
            }, isCheat: false, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("vq_setnpc_info", "[text] [(optional) key]", delegate (Terminal.ConsoleEventArgs args)
            {
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;
                var npc = GetClosestNPC(playerPosition);
                if (npc == null)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                if (args.Length >= 3)
                {
                    npc.SetTypeInformation(args[1], args[2]);
                }
                else if (args.Length >= 2)
                {
                    npc.SetTypeInformation(args[1]);
                }
                else
                {
                    args.Context.AddString("Wrong Syntax!");
                }
            }, isCheat: false, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("vq_setnpc_rewarditem", "[default text] [item text] [item] [amount] " +
                "[reward text] [reward] [amount] [(optional) requires key] [(optional) rewards key] [(optional) repeat]",
                delegate (Terminal.ConsoleEventArgs args)
            {
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;
                var npc = GetClosestNPC(playerPosition);
                if (npc == null)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                if (args.Length >= 11)
                {
                    npc.SetTypeRewardItem(args[1], args[2], args[3], int.Parse(args[4]), args[5], args[6], int.Parse(args[7]),
                        args[8], args[9], int.Parse(args[10]));
                }
                else if (args.Length >= 8)
                {
                    npc.SetTypeRewardItem(args[1], args[2], args[3], int.Parse(args[4]), args[5], args[6], int.Parse(args[7]));
                }
                else
                {
                    args.Context.AddString("Wrong Syntax!");
                }
            }, isCheat: false, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("vq_setnpc_rewardkey", "[default text] [item text] [item] [amount] " +
                "[reward text] [reward key] [(optional) requires key] [(optional) repeat]", delegate (Terminal.ConsoleEventArgs args)
            {
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;
                var npc = GetClosestNPC(playerPosition);
                if (npc == null)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                if (args.Length >= 9)
                {
                    npc.SetTypeRewardKey(args[1], args[2], args[3], int.Parse(args[4]), args[5], args[6], args[7], int.Parse(args[8]));
                }
                else if (args.Length >= 5)
                {
                    npc.SetTypeRewardKey(args[1], args[2], args[3], int.Parse(args[4]), args[5], args[6]);
                }
                else
                {
                    args.Context.AddString("Wrong Syntax!");
                }
            }, isCheat: false, isNetwork: false, onlyServer: false);
        }
    }

    /// <summary>
    /// Spawn npc when interacting with bed
    /// </summary>
    [HarmonyPatch(typeof(Bed), nameof(Bed.Interact))]
    private static class Patch_Bed_Interact
    {
        private static bool Prefix(Bed __instance)
        {
            if (ZInput.GetKey(KeyCode.RightControl))
            {
                var gameobject = NPCFactory.SpawnNPC(__instance.transform.position, __instance.transform.rotation);
                var npc = gameobject.GetComponent<NPC>();

                if (npc != null)
                {
                    npc.SetSpawnPoint(__instance.transform.position);
                    __instance.SetOwner(0L, npc.m_name);
                }

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Chair), nameof(Chair.Interact))]
    private static class Patch_Chair_Interact
    {
        private static bool Prefix(Chair __instance, bool hold, ref bool __result)
        {
            if (!hold && ZInput.GetKey(KeyCode.RightAlt))
            {
                // Add npc to chair
                var npc = NPCFactory.SpawnNPC(__instance.m_attachPoint.position, __instance.m_attachPoint.rotation, "Sitter");

                //npc.transform.position = __instance.m_attachPoint.position;
                npc.transform.rotation = __instance.m_attachPoint.rotation;
                var npcComponent = npc.GetComponent<NPC>();
                npcComponent.SetRandom();

                if (npcComponent != null)
                {
                    npcComponent.AttachStart(__instance);
                }

                __result = false;
                return false;
            }

            return true;
        }
    }
}