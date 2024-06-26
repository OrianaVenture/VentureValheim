using HarmonyLib;
using System;
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
                    npc.SetFromConfig(NPCConfiguration.GetConfig(args[1]), false);
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
                    npc.SetTrueDeath(bool.Parse(args[1]));
                }
                else
                {
                    npc.SetTrueDeath(true);
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

                npc.Attach(false);

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

                npc.Attach(true);

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

                npc.Attach(true, chair);

            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_set_calm", "[range]", delegate (Terminal.ConsoleEventArgs args)
            {
                var playerPosition = Player.m_localPlayer.gameObject.transform.position;

                if (args.Length >= 2)
                {
                    var npcs = Utility.GetAllNPCS(playerPosition, float.Parse(args[1]));
                    foreach (var npc in npcs)
                    {
                        var ai = npc.GetComponent<BaseAI>();
                        if (ai != null)
                        {
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

                npc.SetRotation(playerRotation * new Quaternion(0, 1, 0, 0));
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

                args.Context.AddString($"{(npc as NPCHumanoid).m_name}: {(npc as NPCHumanoid).GetSkinColor()}");
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

                args.Context.AddString($"{(npc as NPCHumanoid).m_name}: {(npc as NPCHumanoid).GetHairColor()}");

            }, isCheat: true, isNetwork: false, onlyServer: false);

            new Terminal.ConsoleCommand("npcs_remove", "", delegate (Terminal.ConsoleEventArgs args)
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
                    (npc as NPCHumanoid).m_nview.ClaimOwnership();
                    ZNetScene.instance.Destroy((npc as NPCHumanoid).gameObject);
                }
                else if (npc is NPCCharacter)
                {
                    (npc as NPCCharacter).m_nview.ClaimOwnership();
                    ZNetScene.instance.Destroy((npc as NPCCharacter).gameObject);
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

                npc.SetRandom();
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
            if (!hold && ZInput.GetKey(KeyCode.RightControl))
            {
                // Add npc to chair
                var npc = NPCFactory.SpawnNPC(__instance.m_attachPoint.position, __instance.m_attachPoint.rotation, "Sitter");

                var npcComponent = npc.GetComponent<NPCHumanoid>();

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