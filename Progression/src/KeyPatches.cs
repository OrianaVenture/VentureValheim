using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VentureValheim.Progression;

public partial class KeyManager
{
    /// <summary>
    /// Skips the original ZoneSystem.SetGlobalKey method if a key is blocked.
    /// </summary>
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SetGlobalKey), new Type[] { typeof(string) })]
    public static class Patch_ZoneSystem_SetGlobalKey
    {
        [HarmonyPriority(Priority.Low)]
        private static bool Prefix(string name)
        {
            return Instance.SkipAddKeyMethod(name.ToLower());
        }

        private static void Postfix(string name, bool __runOriginal)
        {
            bool update = __runOriginal;
            name = name.ToLower();
            if (Player.m_localPlayer != null && !Instance.BlockPrivateKey(name))
            {
                update = true;
                List<Player> nearbyPlayers = new();
                Player.GetPlayersInRange(Player.m_localPlayer.transform.position, 100, nearbyPlayers);

                if (nearbyPlayers != null && nearbyPlayers.Count == 0)
                {
                    ProgressionPlugin.VentureProgressionLogger.LogDebug($"No players in range to send key!");
                }
                else
                {
                    for (int lcv = 0; lcv < nearbyPlayers.Count; lcv++)
                    {
                        var player = nearbyPlayers[lcv].GetPlayerName();
                        ProgressionPlugin.VentureProgressionLogger.LogDebug(
                                $"Attempting to send private key: {name} to \"{player}\".");
                        Instance.SendPrivateKey(player, name);
                    }
                }
            }
            else
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"Skipping adding private key: {name}.");
            }

            if (update)
            {
                Instance.UpdateSkillConfigurations();
            }
        }
    }

    /// <summary>
    /// Server side global key cleanup logic, used for servers with vanilla players.
    /// This rpc should never get sent by private key users when the global key is blocked.
    /// </summary>
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.RPC_SetGlobalKey))]
    public static class Patch_ZoneSystem_RPC_SetGlobalKey
    {
        [HarmonyPriority(Priority.Low)]
        private static bool Prefix(string name)
        {
            ProgressionPlugin.VentureProgressionLogger.LogDebug($"RPC_SetGlobalKey called for: {name}.");
            bool runOriginal = Instance.SkipAddKeyMethod(name.ToLower());
            if (!runOriginal)
            {
                ZoneSystem.instance.SendGlobalKeys(ZRoutedRpc.Everybody);
            }

            return runOriginal;
        }
    }

    /// <summary>
    /// When keys change update skill configurations.
    /// </summary>
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.RPC_GlobalKeys))]
    public static class Patch_ZoneSystem_RPC_GlobalKeys
    {
        private static void Postfix()
        {
            Instance.UpdateSkillConfigurations();
        }
    }

    /// <summary>
    /// Returns false if the global key is blocked. Used to determine if the add global key game methods
    /// should be skipped or not.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private bool SkipAddKeyMethod(string key)
    {
        if (Instance.BlockGlobalKey(ProgressionConfiguration.Instance.GetBlockAllGlobalKeys(), key))
        {
            ProgressionPlugin.VentureProgressionLogger.LogDebug($"Skipping adding global key: {key}.");
            return false; // Skip adding the global key
        }

        ProgressionPlugin.VentureProgressionLogger.LogDebug($"Adding global key: {key}.");
        return true; // Continue adding the global key
    }

    /// <summary>
    /// If using private keys, returns true if the key is in the global list
    /// or if the local player has the private key.
    /// If not using private keys uses default behavior.
    /// </summary>
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GetGlobalKey), new Type[] { typeof(string) })]
    public static class Patch_ZoneSystem_GetGlobalKey
    {
        private static bool Prefix(string name, ref bool __result)
        {
            name = name.ToLower();
            if (ProgressionConfiguration.Instance.GetUsePrivateKeys() &&
                !ZNet.instance.IsDedicated() &&
                !Instance.IsWorldModifier(name))
            {
                __result = Instance.HasPrivateKey(name);

                return !__result; // If found skip search for global key
            }

            return true;
        }
    }

    /// <summary>
    /// If using private keys, returns the player private key list merged with the global list.
    /// If not using private keys or is a dedicated server uses default behavior.
    /// </summary>
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GetGlobalKeys))]
    public static class Patch_ZoneSystem_GetGlobalKeys
    {
        private static void Postfix(ref List<string> __result)
        {
            if (ProgressionConfiguration.Instance.GetUsePrivateKeys() &&
                !ZNet.instance.IsDedicated())
            {
                var privateKeys = new List<string>(Instance.PrivateKeysList);
                __result = ProgressionAPI.MergeLists(__result, privateKeys);
            }
        }
    }

    /// <summary>
    /// If using private keys, removes the key from the private player keys list
    /// in addition to the default behavior of removing from the global list.
    /// </summary>
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.RPC_RemoveGlobalKey))]
    public static class Patch_ZoneSystem_RPC_RemoveGlobalKey
    {
        private static void Postfix(string name)
        {
            ProgressionPlugin.VentureProgressionLogger.LogDebug($"RPC_RemoveGlobalKey called for: {name}.");

            if (ProgressionConfiguration.Instance.GetUsePrivateKeys() &&
                !ZNet.instance.IsDedicated())
            {
                Instance.RemovePrivateKey(name);
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Save))]
    public static class Patch_Player_Save
    {
        private static void Prefix(Player __instance)
        {
            if (!ProgressionAPI.IsInTheMainScene())
            {
                // Prevent keys from last game session saving to the wrong player file when using logout
                Instance.ResetPlayer();
            }
            else
            {
                if (__instance.m_customData.ContainsKey(PLAYER_SAVE_KEY))
                {
                    __instance.m_customData[PLAYER_SAVE_KEY] = Instance.GetPrivateKeysString();
                }
                else
                {
                    __instance.m_customData.Add(PLAYER_SAVE_KEY, Instance.GetPrivateKeysString());
                }
            }
        }
    }

    /// <summary>
    /// Load private keys from the player file if the data exists.
    /// Cleans up private keys based off configurations then syncs the data with the server.
    ///
    /// Patches before EquipInventoryItems since that is the first method
    /// that needs access to the player private keys, and only happens
    /// during the Player.Load method.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.EquipInventoryItems))]
    public static class Patch_Player_EquipInventoryItems
    {
        private static void Prefix(Player __instance)
        {
            if (!ProgressionAPI.IsInTheMainScene())
            {
                return;
            }

            ProgressionPlugin.VentureProgressionLogger.LogInfo("Starting Player Key Management. Cleaning up private keys!");

            Instance.ResetPlayer();
            Instance.UpdateConfigurations();

            HashSet<string> loadedKeys = new();

            if (__instance.m_customData.ContainsKey(PLAYER_SAVE_KEY))
            {
                loadedKeys = ProgressionAPI.StringToSet(__instance.m_customData[PLAYER_SAVE_KEY]);
            }

            // Add loaded private keys if not blocked
            foreach (var key in loadedKeys)
            {
                if (!Instance.BlockPrivateKey(key))
                {
                    Instance.PrivateKeysList.Add(key);
                }
            }

            // Add enforced private keys regardless of settings
            foreach (var key in Instance.EnforcedPrivateKeysList)
            {
                Instance.PrivateKeysList.Add(key);
            }

            try
            {
                ZRoutedRpc.instance.Register(RPCNAME_SetPrivateKey, new Action<long, string>(Instance.RPC_SetPrivateKey));
                ZRoutedRpc.instance.Register(RPCNAME_RemovePrivateKey, new Action<long, string>(Instance.RPC_RemovePrivateKey));
                ZRoutedRpc.instance.Register(RPCNAME_ResetPrivateKeys, new Action<long>(Instance.RPC_ResetPrivateKeys));
            }
            catch
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug("Player RPCs have already been registered. Skipping.");
            }

            // Sync data on connect
            Instance.SendPrivateKeysToServer(Instance.PrivateKeysList);
            Instance.UpdateSkillConfigurations();
        }
    }

    /// <summary>
    /// Register RPCs and perform a server key cleanup when starting up.
    /// </summary>
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Load))]
    public static class Patch_ZoneSystem_Load
    {
        private static void Postfix()
        {
            if (!ZNet.instance.IsServer())
            {
                return;
            }

            ProgressionPlugin.VentureProgressionLogger.LogInfo("Starting Server Key Management. Cleaning up public keys!");

            Instance.ResetConfigurations();
            Instance.ResetServer();
            Instance.UpdateConfigurations();

            var keys = ProgressionAPI.GetGlobalKeys().ToList();
            var blockAll = ProgressionConfiguration.Instance.GetBlockAllGlobalKeys();

            // Remove any blocked global keys from the list
            for (int lcv = 0; lcv < keys.Count; lcv++)
            {
                if (Instance.BlockGlobalKey(blockAll, keys[lcv]))
                {
                    ZoneSystem.instance.m_globalKeys.Remove(keys[lcv]);
                }
            }

            // Add enforced global keys regardless of settings
            foreach (var key in Instance.EnforcedGlobalKeysList)
            {
                ZoneSystem.instance.m_globalKeys.Add(key);
            }

            if (ProgressionConfiguration.Instance.GetUsePrivateKeys())
            {
                // Add player based raids setting
                ZoneSystem.instance.m_globalKeysEnums.Add(GlobalKeys.PlayerEvents);
            }

            // Register Server RPCs
            try
            {
                ZRoutedRpc.instance.Register(RPCNAME_ServerListKeys, new Action<long>(Instance.RPC_ServerListKeys));
                ZRoutedRpc.instance.Register(RPCNAME_ServerSetPrivateKeys, new Action<long, string, long>(Instance.RPC_ServerSetPrivateKeys));
                ZRoutedRpc.instance.Register(RPCNAME_ServerSetPrivateKey, new Action<long, string, long>(Instance.RPC_ServerSetPrivateKey));
                ZRoutedRpc.instance.Register(RPCNAME_ServerRemovePrivateKey, new Action<long, string, long>(Instance.RPC_ServerRemovePrivateKey));

                ZRoutedRpc.instance.Register(RPCNAME_SetPrivateKey, new Action<long, string>(Instance.RPC_SetPrivateKey));
                ZRoutedRpc.instance.Register(RPCNAME_RemovePrivateKey, new Action<long, string>(Instance.RPC_RemovePrivateKey));
                ZRoutedRpc.instance.Register(RPCNAME_ResetPrivateKeys, new Action<long>(Instance.RPC_ResetPrivateKeys));
            }
            catch
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug("Server RPCs have already been registered. Skipping.");
            }
        }
    }

    /// <summary>
    /// Adds commands for managing player keys.
    /// </summary>
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

            ProgressionPlugin.VentureProgressionLogger.LogInfo("Adding Terminal Commands for key management.");

            new Terminal.ConsoleCommand("setglobalkey", "[name]", delegate (Terminal.ConsoleEventArgs args)
            {
                if (args.Length >= 2)
                {
                    ProgressionAPI.AddGlobalKey(args[1]);
                    args.Context.AddString($"Setting global key {args[1]}.");
                }
                else
                {
                    args.Context.AddString("Syntax: setglobalkey [key]");
                }
            }, isCheat: true, isNetwork: false, onlyServer: false);
            new Terminal.ConsoleCommand("removeglobalkey", "[name]", delegate (Terminal.ConsoleEventArgs args)
            {
                if (args.Length >= 2)
                {
                    ProgressionAPI.RemoveGlobalKey(args[1]);
                    args.Context.AddString($"Removing global key {args[1]}.");
                }
                else
                {
                    args.Context.AddString("Syntax: removeglobalkey [key]");
                }
            }, isCheat: true, isNetwork: false, onlyServer: false);
            new Terminal.ConsoleCommand("listglobalkeys", "", delegate (Terminal.ConsoleEventArgs args)
            {
                var keys = ProgressionAPI.GetGlobalKeys();
                args.Context.AddString($"Total Keys {keys.Count}");
                foreach (string key in keys)
                {
                    args.Context.AddString(key);
                }
            }, isCheat: true, isNetwork: false, onlyServer: false);
            new Terminal.ConsoleCommand("resetglobalkeys", "", delegate (Terminal.ConsoleEventArgs args)
            {
                ZoneSystem.instance.ResetGlobalKeys();
            }, isCheat: true, isNetwork: false, onlyServer: false);
            new Terminal.ConsoleCommand("setprivatekey", "[name] [optional: player name]", delegate (Terminal.ConsoleEventArgs args)
            {
                if (args.Length >= 3)
                {
                    var name = args[2];
                    for (int lcv = 3; lcv < args.Length; lcv++)
                    {
                        name += " " + args[lcv];
                    }
                    Instance.AddPrivateKey(args[1], name);
                    args.Context.AddString($"Setting private key {args[1]} for player {name}.");
                }
                else if (args.Length == 2)
                {
                    Instance.AddPrivateKey(args[1]);
                    args.Context.AddString($"Setting private key {args[1]}.");
                }
                else
                {
                    args.Context.AddString("Syntax: setprivatekey [key]");
                }
            }, isCheat: true, isNetwork: false, onlyServer: false);
            new Terminal.ConsoleCommand("removeprivatekey", "[name] [optional: player name]", delegate (Terminal.ConsoleEventArgs args)
            {
                if (args.Length >= 3)
                {
                    var name = args[2];
                    for (int lcv = 3; lcv < args.Length; lcv++)
                    {
                        name += " " + args[lcv];
                    }
                    Instance.RemovePrivateKey(args[1], name);
                    args.Context.AddString($"Removing private key {args[1]} for player {name}.");
                }
                else if (args.Length == 2)
                {
                    Instance.RemovePrivateKey(args[1]);
                    args.Context.AddString($"Removing private key {args[1]}.");
                }
                else
                {
                    args.Context.AddString("Syntax: removeprivatekey [key] [optional: player name]");
                }
            }, isCheat: true, isNetwork: false, onlyServer: false);
            new Terminal.ConsoleCommand("resetprivatekeys", "[optional: player name]", delegate (Terminal.ConsoleEventArgs args)
            {
                if (args.Length >= 2)
                {

                    var name = args[1];
                    for (int lcv = 2; lcv < args.Length; lcv++)
                    {
                        name += " " + args[lcv];
                    }
                    Instance.ResetPrivateKeys(args[1]);
                    args.Context.AddString($"Private keys cleared for player {name}.");
                }
                else if (args.Length == 1)
                {
                    Instance.ResetPrivateKeys();
                    args.Context.AddString("Private keys cleared");
                }
                else
                {
                    args.Context.AddString("Syntax: resetprivatekeys [optional: player name]");
                }
            }, isCheat: true, isNetwork: false, onlyServer: false);
            new Terminal.ConsoleCommand("listprivatekeys", "", delegate (Terminal.ConsoleEventArgs args)
            {
                args.Context.AddString($"Total Keys {Instance.PrivateKeysList.Count}");
                foreach (string key in Instance.PrivateKeysList)
                {
                    args.Context.AddString(key);
                }
            }, isCheat: false, isNetwork: false, onlyServer: false);
            new Terminal.ConsoleCommand("listserverkeys", "", delegate (Terminal.ConsoleEventArgs args)
            {
                if (ZNet.instance.IsServer())
                {
                    args.Context.AddString($"Total Players Recorded This Session: {Instance.ServerPrivateKeysList.Count}");

                    foreach (var set in Instance.ServerPrivateKeysList)
                    {
                        var numKeys = set.Value?.Count ?? 0;

                        args.Context.AddString($"Player {set.Key} has {numKeys} recorded keys:");

                        if (set.Value != null)
                        {
                            foreach (string key in set.Value)
                            {
                                args.Context.AddString(key);
                            }
                        }
                    }
                }
                else
                {
                    args.Context.AddString($"You are not the server, no data available client side. Printing key information to server logoutput.log file.");
                    Instance.SendServerListKeys();
                }
            }, isCheat: true, isNetwork: false, onlyServer: false);
        }
    }
}
