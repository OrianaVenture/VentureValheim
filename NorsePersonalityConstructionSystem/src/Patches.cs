using HarmonyLib;
using System.Collections.Generic;
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
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;
                var playerRotation = Player.m_localPlayer.gameObject.transform.rotation;
                var position = playerPosition + (playerRotation * Vector3.forward);

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
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;
                var playerRotation = Player.m_localPlayer.gameObject.transform.rotation;
                var position = playerPosition + (playerRotation * Vector3.forward);

                if (args.Length >= 2)
                {
                    var go = NPCFactory.SpawnSavedNPC(position, playerRotation, args[1]);
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
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;
                var npc = Utility.GetClosestNPC(playerPosition);
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
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;
                var npc = Utility.GetClosestNPC(playerPosition);
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
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;
                var npc = Utility.GetClosestNPC(playerPosition);
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
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;
                var npc = Utility.GetClosestNPC(playerPosition);
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
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;
                var npc = Utility.GetClosestNPC(playerPosition);
                if (npc == null)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                var chair = Utility.GetClosestChair(playerPosition, Vector3.one * 2);

                npc.Data.Attach(true, "", chair);

            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_set_calm", "[range]", delegate (Terminal.ConsoleEventArgs args)
            {
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;

                if (args.Length >= 2)
                {
                    var npcs = Utility.GetAllNPCS(playerPosition, float.Parse(args[1]));
                    foreach (var npc in npcs)
                    {
                        BaseAI ai = null;
                        if (npc is NPCHumanoid)
                        {
                            (npc as NPCHumanoid).m_nview.ClaimOwnership();
                            ai = (npc as NPCHumanoid).GetComponent<BaseAI>();
                        }
                        else if (npc is NPCCharacter)
                        {
                            (npc as NPCCharacter).m_nview.ClaimOwnership();
                            ai = (npc as NPCCharacter).GetComponent<BaseAI>();
                        }

                        if (ai != null)
                        {
                            ai.SetAlerted(false);
                            ai.SetAggravated(false, BaseAI.AggravatedReason.Damage);
                        }
                    }
                }
                else
                {
                    var npcs = Utility.GetAllNPCS(playerPosition, 5f);
                }
            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_set_faceme", "", delegate (Terminal.ConsoleEventArgs args)
            {
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;
                var playerRotation = Player.m_localPlayer.gameObject.transform.rotation;

                var npc = Utility.GetClosestNPC(playerPosition);
                if (npc == null)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                npc.Data.SetRotation(playerRotation * new Quaternion(0, 1, 0, 0));
            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_get_skincolor", "", delegate (Terminal.ConsoleEventArgs args)
            {
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;
                var npc = Utility.GetClosestNPC(playerPosition);
                if (npc == null || npc is not NPCHumanoid)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                args.Context.AddString($"{(npc as NPCHumanoid).m_name}: {(npc as NPCHumanoid).Data.GetSkinColor()}");
            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_get_haircolor", "", delegate (Terminal.ConsoleEventArgs args)
            {
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;
                var npc = Utility.GetClosestNPC(playerPosition);
                if (npc == null || npc is not NPCHumanoid)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                args.Context.AddString($"{(npc as NPCHumanoid).m_name}: {(npc as NPCHumanoid).Data.GetHairColor()}");

            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_remove", "", delegate (Terminal.ConsoleEventArgs args)
            {
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;

                List<INPC> npcs;

                if (args.Length >= 2 && int.TryParse(args[1], out int range))
                {
                    npcs = Utility.GetAllNPCS(playerPosition, range);
                }
                else
                {
                    var npc = Utility.GetClosestNPC(playerPosition);
                    npcs = new List<INPC>()
                    {
                        npc
                    };
                }

                for (int lcv = 0; lcv < npcs.Count; lcv++)
                {
                    var npc = npcs[lcv];
                    if (npc == null)
                    {
                        args.Context.AddString("No npc found.");
                        return;
                    }

                    if (npc is NPCHumanoid)
                    {
                        (npc as NPCHumanoid).m_nview.ClaimOwnership();
                        ZNetScene.instance.Destroy((npc as NPCHumanoid).gameObject);
                    }
                    else if (npc is NPCCharacter)
                    {
                        (npc as NPCCharacter).m_nview.ClaimOwnership();
                        ZNetScene.instance.Destroy((npc as NPCCharacter).gameObject);
                    }
                }

            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_randomize", "", delegate (Terminal.ConsoleEventArgs args)
            {
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;
                var npc = Utility.GetClosestNPC(playerPosition);
                if (npc == null)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                npc.Data.SetRandom();
            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_info", "", delegate (Terminal.ConsoleEventArgs args)
            {
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;
                var npc = Utility.GetClosestNPC(playerPosition);
                if (npc == null)
                {
                    args.Context.AddString("No npc found.");
                    return;
                }

                if (npc is NPCHumanoid)
                {
                    var npcHuman = (NPCHumanoid)npc;
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
                    var npcHuman = (NPCHumanoid)npc;
                    args.Context.AddString($"{npcHuman.m_name}");
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
                var gameobject = NPCFactory.SpawnNPC(__instance.transform.position, __instance.transform.rotation);
                var npc = gameobject.GetComponent<NPCHumanoid>();

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
                var npc = NPCFactory.SpawnNPC(__instance.m_attachPoint.position, __instance.m_attachPoint.rotation, "Sitter");

                var npcComponent = npc.GetComponent<NPCHumanoid>();

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

    [HarmonyPatch(typeof(StoreGui), nameof(StoreGui.Show))]
    private static class Patch_StoreGui_Show
    {
        private static void Postfix(StoreGui __instance, Trader trader)
        {
            if (trader == __instance.m_trader)
            {
                // todo set npcs to stop walking
            }
        }
    }

    [HarmonyPatch(typeof(StoreGui), nameof(StoreGui.Hide))]
    private static class Patch_StoreGui_Hide
    {
        private static void Prefix(StoreGui __instance)
        {
            if (__instance.m_trader && __instance.GetComponent<NPCTrader>())
            {
                // todo set npcs to continue walking
            }
        }
    }

    [HarmonyPatch(typeof(Chair), nameof(Chair.Interact))]
    private static class Patch_Chair_Interact2
    {
        private static void Prefix()
        {
            //TODO: don't let players sit on npcs
        }
    }
}