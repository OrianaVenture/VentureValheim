using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace VentureValheim.NPCS;

public class Patches
{
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

            NPCSPlugin.NPCSLogger.LogInfo("Adding Terminal Commands for npc management.");

            new Terminal.ConsoleCommand("npcs_reloadconfig", "", delegate (Terminal.ConsoleEventArgs args)
            {
                NPCConfiguration.ReloadFile();
            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_spawnrandom", "[name] [model]", delegate (Terminal.ConsoleEventArgs args)
            {
                Vector3 playerPosition = Player.m_localPlayer.gameObject.transform.position;
                Quaternion playerRotation = Player.m_localPlayer.gameObject.transform.rotation;
                Vector3 position = playerPosition + (playerRotation * Vector3.forward);

                if (args.Length > 2)
                {
                    NPCFactory.SpawnNPC(position, playerRotation, args[1], args[2]);
                }
                else if (args.Length > 1)
                {
                    NPCFactory.SpawnNPC(position, playerRotation, args[1]);
                }
                else
                {
                    NPCFactory.SpawnNPC(position, playerRotation);
                }
            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_spawnsaved", "[id]", delegate (Terminal.ConsoleEventArgs args)
            {
                Vector3 playerPosition = Player.m_localPlayer.gameObject.transform.position;
                Quaternion playerRotation = Player.m_localPlayer.gameObject.transform.rotation;
                Vector3 position = playerPosition + (playerRotation * Vector3.forward);

                if (args.Length >= 2)
                {
                    GameObject go = NPCFactory.SpawnSavedNPC(position, playerRotation, args[1]);
                    if (go == null)
                    {
                        args.Context.AddString("Could not spawn NPC!");
                    }
                }
                else
                {
                    args.Context.AddString("Wrong Syntax!");
                }
            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_set", "[id]", delegate (Terminal.ConsoleEventArgs args)
            {
                Vector3 playerPosition = Player.m_localPlayer.gameObject.transform.position;
                INPC npc = Utility.GetClosestNPC(playerPosition, out float _);
                if (npc == null)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                if (args.Length >= 2)
                {
                    npc.Data.SetFromConfig(NPCConfiguration.GetConfig(args[1]), false);
                }
                else
                {
                    args.Context.AddString("Wrong Syntax!");
                }
            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_set_truedeath", "[boolean]", delegate (Terminal.ConsoleEventArgs args)
            {
                Vector3 playerPosition = Player.m_localPlayer.gameObject.transform.position;
                INPC npc = Utility.GetClosestNPC(playerPosition, out float _);
                if (npc == null)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                if (args.Length >= 2)
                {
                    npc.Data.SetTrueDeath(bool.Parse(args[1]));
                }
                else
                {
                    npc.Data.SetTrueDeath(true);
                }
            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_set_move", "", delegate (Terminal.ConsoleEventArgs args)
            {
                Vector3 playerPosition = Player.m_localPlayer.gameObject.transform.position;
                INPC npc = Utility.GetClosestNPC(playerPosition, out float _);
                if (npc == null)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                string animation = "";
                if (args.Length >= 2)
                {
                    animation = args[1];
                }

                npc.Data.Attach(false, animation);

            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_set_still", "", delegate (Terminal.ConsoleEventArgs args)
            {
                Vector3 playerPosition = Player.m_localPlayer.gameObject.transform.position;
                INPC npc = Utility.GetClosestNPC(playerPosition, out float _);
                if (npc == null)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                string animation = "";
                if (args.Length >= 2)
                {
                    animation = args[1];
                }

                npc.Data.Attach(true, animation);

            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_set_sit", "", delegate (Terminal.ConsoleEventArgs args)
            {
                Vector3 playerPosition = Player.m_localPlayer.gameObject.transform.position;
                INPC npc = Utility.GetClosestNPC(playerPosition, out float _);
                if (npc == null)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                Chair chair = Utility.GetClosestChair(playerPosition, Vector3.one * 2);

                npc.Data.Attach(true, "", chair);

            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_set_calm", "[range]", delegate (Terminal.ConsoleEventArgs args)
            {
                Vector3 playerPosition = Player.m_localPlayer.gameObject.transform.position;

                float range = 5f;
                if (args.Length >= 2 && float.TryParse(args[1], out float newRange))
                {
                    range = newRange;
                }

                List<INPC> npcs = Utility.GetAllNPCS(playerPosition, range);

                foreach (INPC npc in npcs)
                {
                    BaseAI ai = null;
                    (npc as Character).m_nview.ClaimOwnership();
                    ai = (npc as Character).GetComponent<BaseAI>();

                    if (ai != null)
                    {
                        ai.SetAlerted(false);
                        ai.SetAggravated(false, BaseAI.AggravatedReason.Damage);
                    }
                }
            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_set_faceme", "", delegate (Terminal.ConsoleEventArgs args)
            {
                Vector3 playerPosition = Player.m_localPlayer.gameObject.transform.position;
                Quaternion playerRotation = Player.m_localPlayer.gameObject.transform.rotation;

                INPC npc = Utility.GetClosestNPC(playerPosition, out float _);
                if (npc == null)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                npc.Data.SetRotation(playerRotation * new Quaternion(0, 1, 0, 0));
            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_get_skincolor", "", delegate (Terminal.ConsoleEventArgs args)
            {
                Vector3 playerPosition = Player.m_localPlayer.gameObject.transform.position;
                INPC npc = Utility.GetClosestNPC(playerPosition, out float _);
                if (npc == null || npc is not NPCHumanoid)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                args.Context.AddString($"{(npc as NPCHumanoid).m_name}: {(npc as NPCHumanoid).Data.GetSkinColor()}");
            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_get_haircolor", "", delegate (Terminal.ConsoleEventArgs args)
            {
                Vector3 playerPosition = Player.m_localPlayer.gameObject.transform.position;
                INPC npc = Utility.GetClosestNPC(playerPosition, out float _);
                if (npc == null || npc is not NPCHumanoid)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                args.Context.AddString($"{(npc as NPCHumanoid).m_name}: {(npc as NPCHumanoid).Data.GetHairColor()}");

            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_remove", "", delegate (Terminal.ConsoleEventArgs args)
            {
                Vector3 playerPosition = Player.m_localPlayer.gameObject.transform.position;

                List<INPC> npcs;

                if (args.Length >= 2 && int.TryParse(args[1], out int range))
                {
                    npcs = Utility.GetAllNPCS(playerPosition, range);
                }
                else
                {
                    INPC npc = Utility.GetClosestNPC(playerPosition, out float _);
                    npcs = new List<INPC>()
                    {
                        npc
                    };
                }

                for (int lcv = 0; lcv < npcs.Count; lcv++)
                {
                    INPC npc = npcs[lcv];
                    if (npc == null)
                    {
                        args.Context.AddString("No npc found.");
                        return;
                    }

                    (npc as Character).m_nview.ClaimOwnership();
                    ZNetScene.instance.Destroy((npc as Character).gameObject);
                }

            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_randomize", "", delegate (Terminal.ConsoleEventArgs args)
            {
                Vector3 playerPosition = Player.m_localPlayer.gameObject.transform.position;
                INPC npc = Utility.GetClosestNPC(playerPosition, out float _);
                if (npc == null)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                npc.Data.SetRandom();
            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_info", "", delegate (Terminal.ConsoleEventArgs args)
            {
                Vector3 playerPosition = Player.m_localPlayer.gameObject.transform.position;
                INPC npc = Utility.GetClosestNPC(playerPosition, out float _);
                if (npc == null)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                if (npc is NPCHumanoid)
                {
                    NPCHumanoid npcHuman = (NPCHumanoid)npc;
                    args.Context.AddString($"{npcHuman.m_name}: " +
                    $"{npcHuman.m_hairItem}, " +
                    $"{npcHuman.m_beardItem}, " +
                    $"{npcHuman.m_helmetItem?.m_dropPrefab?.name}, " +
                    $"{npcHuman.m_chestItem?.m_dropPrefab?.name}, " +
                    $"{npcHuman.m_legItem?.m_dropPrefab?.name}, " +
                    $"{npcHuman.m_shoulderItem?.m_dropPrefab?.name}, " +
                    $"{npcHuman.m_utilityItem?.m_dropPrefab?.name}, " +
                    $"{npcHuman.m_rightItem?.m_dropPrefab?.name}, " +
                    $"{npcHuman.m_leftItem?.m_dropPrefab?.name}");
                }
                else if (npc is NPCCharacter)
                {
                    NPCCharacter npcCharacter = (NPCCharacter)npc;
                    args.Context.AddString($"{npcCharacter.m_name}");
                }
            }, isCheat: true, isNetwork: false, onlyServer: false);
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
                GameObject gameobject = NPCFactory.SpawnNPC(__instance.transform.position,
                    __instance.transform.rotation);
                NPCHumanoid npc = gameobject.GetComponent<NPCHumanoid>();

                if (npc != null)
                {
                    npc.Data.SetSpawnPoint(__instance.transform.position);
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
            if (!hold && ZInput.GetKey(KeyCode.RightControl))
            {
                // Add npc to chair
                GameObject npc = NPCFactory.SpawnNPC(__instance.m_attachPoint.position,
                    __instance.m_attachPoint.rotation, "Sitter");

                NPCHumanoid npcComponent = npc.GetComponent<NPCHumanoid>();

                if (npcComponent != null)
                {
                    npcComponent.Attach(true, __instance);
                }

                __result = false;
                return false;
            }

            return true;
        }
    }

    private static class Patch_StoreGui
    {
        private static bool originalAttach = false;

        /// <summary>
        /// Set the trader to attach and stand still for interaction.
        /// </summary>
        [HarmonyPatch(typeof(StoreGui), nameof(StoreGui.Show))]
        private static class Patch_StoreGui_Show
        {
            private static void Postfix(StoreGui __instance, Trader trader)
            {
                if (trader != __instance.m_trader)
                {
                    return;
                }

                originalAttach = false;
                INPC npc = __instance.m_trader.GetComponent<INPC>();

                if (npc != null)
                {
                    originalAttach = npc.Data.HasAttach;
                    if (!originalAttach)
                    {
                        npc.Data.Attach(true, "");
                    }
                }
            }
        }

        /// <summary>
        /// Set the trader to continue the original animation.
        /// </summary>
        [HarmonyPatch(typeof(StoreGui), nameof(StoreGui.Hide))]
        private static class Patch_StoreGui_Hide
        {
            private static void Prefix(StoreGui __instance)
            {
                if (!__instance.m_trader || originalAttach)
                {
                    return;
                }

                INPC npc = __instance.m_trader.GetComponent<INPC>();

                if (npc != null)
                {
                    string animation = "";
                    if ((npc as Character).m_nview.GetZDO() != null)
                    {
                        animation = NPCZDOUtils.GetAnimation((npc as Character).m_nview.GetZDO());
                    }

                    npc.Data.Attach(false, animation);
                }
            }
        }
    }

    /// <summary>
    /// Visual bug fix patch for Trader script not unequipping items before removing.
    /// </summary>
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem),
        new Type[] { typeof(ItemDrop.ItemData) })]
    private static class Patch_Inventory_RemoveItem
    {
        private static void Prefix(Inventory __instance, ItemDrop.ItemData item)
        {
            if (__instance == Player.m_localPlayer.m_inventory)
            {
                Player.m_localPlayer.UnequipItem(item);
            }
        }
    }

    /// <summary>
    /// Do not let players sit on NPCs.
    /// </summary>
    [HarmonyPatch(typeof(Chair))]
    static class ChairPatch
    {
        // Thank you Redseiko for the beautiful transpiler
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Chair.Interact))]
        static IEnumerable<CodeInstruction> InputTextTranspiler(
            IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions, generator)
                .Start()
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(Player), nameof(Player.m_localPlayer))),
                    new CodeMatch(OpCodes.Ldc_I4_2),
                    new CodeMatch(OpCodes.Ldstr, "$msg_blocked"))
                .ThrowIfInvalid($"Could not patch Chair.Interact()! (msg-blocked)")
                .CreateLabel(out Label msgBlockedLabel)
                .Start()
                .MatchStartForward(
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Player), nameof(Player.GetClosestPlayer))),
                    new CodeMatch(OpCodes.Stloc_1))
                .ThrowIfInvalid("Could not patch Chair.Interact()! (get-closest-player)")
                .Advance(offset: 2)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ChairPatch), nameof(IsChairOccupied))),
                    new CodeInstruction(OpCodes.Brtrue, msgBlockedLabel))
                .InstructionEnumeration();
        }

        static bool IsChairOccupied(Chair chair)
        {
            Vector3 position = chair.m_attachPoint.position;
            INPC npc = Utility.GetClosestNPC(position, out float distance);
            if (npc != null && distance <= 0.1f)
            {
                return true;
            }

            return false;
        }
    }
}