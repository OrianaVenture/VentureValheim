using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VentureValheim.Progression
{
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
                return Instance.SkipAddKeyMethod(name);
            }

            private static void Postfix(string name)
            {
                if (Player.m_localPlayer != null && !Instance.BlockPrivateKey(name))
                {
                    List<Player> nearbyPlayers = new List<Player>();
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
            }
        }

        // Server side global key cleanup logic, used for servers with vanilla players
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.RPC_SetGlobalKey))]
        public static class Patch_ZoneSystem_RPC_SetGlobalKey
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(string name)
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"RPC_SetGlobalKey called for: {name}.");
                bool runOriginal = Instance.SkipAddKeyMethod(name);
                if (!runOriginal)
                {
                    ZoneSystem.instance.SendGlobalKeys(ZRoutedRpc.Everybody);
                }

                return runOriginal;
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
        /// If using private keys, returns true if the key is in the global list when
        /// the instance is a dedicated server, or true if the local player has the private key.
        /// If not using private keys uses default behavior.
        /// </summary>
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GetGlobalKey), new Type[] { typeof(string) })]
        public static class Patch_ZoneSystem_GetGlobalKey
        {
            private static void Postfix(string name, ref bool __result)
            {
                if (ProgressionConfiguration.Instance.GetUsePrivateKeys() &&
                    !ZNet.instance.IsDedicated() &&
                    !Instance.IsWorldModifier(name))
                {
                    __result = Instance.HasPrivateKey(name);
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
                Instance.UpdateConfigs();

                HashSet<string> loadedKeys = new HashSet<string>();

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
            }
        }

        /// <summary>
        /// Add the config updater watcher to the player.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.SetLocalPlayer))]
        public static class Patch_Player_SetLocalPlayer
        {
            private static void Postfix(Player __instance)
            {
                if (!ProgressionAPI.IsInTheMainScene())
                {
                    return;
                }

                if (__instance.GetComponent<KeyManagerUpdater> == null)
                {
                    __instance.gameObject.AddComponent<KeyManagerUpdater>();
                }
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
                if (ZNet.instance.IsServer())
                {
                    ProgressionPlugin.VentureProgressionLogger.LogInfo("Starting Server Key Management. Cleaning up public keys!");

                    Instance.Reset();
                    Instance.UpdateConfigs();

                    var keys = ZoneSystem.instance.GetGlobalKeys();
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
                        ZRoutedRpc.instance.Register(RPCNAME_ServerSetPrivateKeys, new Action<long, string, string>(Instance.RPC_ServerSetPrivateKeys));
                        ZRoutedRpc.instance.Register(RPCNAME_ServerSetPrivateKey, new Action<long, string, string>(Instance.RPC_ServerSetPrivateKey));
                        ZRoutedRpc.instance.Register(RPCNAME_ServerRemovePrivateKey, new Action<long, string, string>(Instance.RPC_ServerRemovePrivateKey));

                        ZRoutedRpc.instance.Register(RPCNAME_SetPrivateKey, new Action<long, string>(Instance.RPC_SetPrivateKey));
                        ZRoutedRpc.instance.Register(RPCNAME_RemovePrivateKey, new Action<long, string>(Instance.RPC_RemovePrivateKey));
                        ZRoutedRpc.instance.Register(RPCNAME_ResetPrivateKeys, new Action<long>(Instance.RPC_ResetPrivateKeys));
                    }
                    catch
                    {
                        ProgressionPlugin.VentureProgressionLogger.LogDebug("Server RPCs have already been registered. Skipping.");
                    }


                    if (Instance._keyManagerUpdater == null)
                    {
                        var obj = GameObject.Instantiate(new GameObject());
                        Instance._keyManagerUpdater = obj.AddComponent<KeyManagerUpdater>();
                    }
                }
            }
        }

        /// <summary>
        /// Fix my mistake of adding GlobalKeys.PlayerEvents to the list multiple times
        /// </summary>
        [HarmonyPatch(typeof(ZPlayFabMatchmaking), nameof(ZPlayFabMatchmaking.CreateLobby))]
        public static class Patch_ZPlayFabMatchmaking_CreateLobby
        {
            private static void Prefix()
            {
                RemoveDuplicates(ref ZPlayFabMatchmaking.m_instance.m_serverData.modifiers);
            }
        }

        /// <summary>
        /// Fix my mistake of adding GlobalKeys.PlayerEvents to the list multiple times (server patch)
        /// </summary>
        [HarmonyPatch(typeof(ZSteamMatchmaking), nameof(ZSteamMatchmaking.RegisterServer))]
        public static class Patch_ZSteamMatchmaking_RegisterServer
        {
            private static void Prefix(ref List<string> modifiers)
            {
                RemoveDuplicates(ref modifiers);
            }
        }

        private static void RemoveDuplicates(ref List<string> keys)
        {
            if (keys != null)
            {
                var fixedKeys = new HashSet<string>();
                foreach (string key in keys)
                {
                    if (!fixedKeys.Contains(key))
                    {
                        fixedKeys.Add(key);
                    }
                    else
                    {
                        ProgressionPlugin.VentureProgressionLogger.LogWarning($"Found duplicate world modifier key {key}, fixing.");
                    }
                }

                keys = fixedKeys.ToList();
            }
        }

        /// <summary>
        /// Enables all of Haldor's items by bypassing key checking.
        /// </summary>
        [HarmonyPatch(typeof(Trader), nameof(Trader.GetAvailableItems))]
        public static class Patch_Trader_GetAvailableItems
        {
            [HarmonyPriority(Priority.First)]
            private static void Postfix(Trader __instance, ref List<Trader.TradeItem> __result)
            {
                var name = Utils.GetPrefabName(__instance.gameObject);

                if ((name.Equals("Haldor") && ProgressionConfiguration.Instance.GetUnlockAllHaldorItems()) ||
                    ((name.Equals("Hildir") && ProgressionConfiguration.Instance.GetUnlockAllHildirItems())))
                {
                    __result = new List<Trader.TradeItem>(__instance.m_items);
                }
            }
        }

        /// <summary>
        /// Fix error thrown when index is 0 and no items exist.
        /// </summary>
        [HarmonyPatch(typeof(StoreGui), nameof(StoreGui.SelectItem))]
        public static class Patch_StoreGui_SelectItem
        {
            private static void Prefix(StoreGui __instance, ref int index)
            {
                if (__instance.m_itemList.Count == 0)
                {
                    index = -1;
                }
            }
        }

        /// <summary>
        /// Set up custom keys for Trader items.
        /// </summary>
        [HarmonyPatch(typeof(Trader), nameof(Trader.Start))]
        public static class Patch_Trader_Start
        {
            [HarmonyPriority(Priority.First)]
            private static void Postfix(Trader __instance)
            {
                var traderName = Utils.GetPrefabName(__instance.gameObject);
                Dictionary<string, string> items = null;

                if (traderName.Equals("Haldor"))
                {
                    items = Instance.GetTraderConfiguration();
                }
                else if (traderName.Equals("Hildir"))
                {
                    items = Instance.GetHildirConfiguration(__instance.m_items);
                }

                if (items != null)
                {
                    foreach (var item in __instance.m_items)
                    {
                        if (item.m_prefab != null)
                        {
                            var name = Utils.GetPrefabName(item.m_prefab.gameObject);
                            if (items.ContainsKey(name))
                            {
                                item.m_requiredGlobalKey = items[name];
                            }
                        }
                    }
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
                }, isCheat: true, isNetwork: false, onlyServer: true);
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
                }, isCheat: true, isNetwork: false, onlyServer: true);
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
                }, isCheat: true, isNetwork: false, onlyServer: true);
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
                }, isCheat: true, isNetwork: false, onlyServer: true);
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
                }, isCheat: true, isNetwork: false, onlyServer: true);
            }
        }

        /// <summary>
        /// Only increase taming if the player has the private key.
        /// </summary>
        [HarmonyPatch(typeof(Tameable), nameof(Tameable.DecreaseRemainingTime))]
        public static class Patch_Tameable_DecreaseRemainingTime
        {
            [HarmonyPriority(Priority.Low)]
            private static void Prefix(Tameable __instance, ref float time)
            {
                if (ProgressionConfiguration.Instance.GetLockTaming())
                {
                    if (__instance.m_character == null ||
                        !Instance.HasTamingKey(Utils.GetPrefabName(__instance.m_character.gameObject)))
                    {
                        time = 0f;
                    }
                }
            }
        }

        /// <summary>
        /// Block getting guardian powers without the key.
        /// </summary>
        [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.DelayedPowerActivation))]
        public static class Patch_ItemStand_DelayedPowerActivation
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(ItemStand __instance)
            {
                if (ProgressionConfiguration.Instance.GetLockGuardianPower())
                {
                    if (!Instance.HasGuardianKey(__instance.m_guardianPower?.name))
                    {
                        Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                        return false; // Skip giving power
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Block activating guardian powers without the key if a Player already has one.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.ActivateGuardianPower))]
        public static class Patch_Player_ActivateGuardianPower
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(Player __instance, ref bool __result)
            {
                if (!__instance.m_guardianPower.IsNullOrWhiteSpace() && ProgressionConfiguration.Instance.GetLockGuardianPower())
                {
                    if (!Instance.HasGuardianKey(__instance.m_guardianPower))
                    {
                        Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                        __result = false; // Not sure why they have a return type on this, watch for game changes
                        return false; // Skip giving power
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Block the boss spawn when the player has not defeated the previous boss
        /// </summary>
        [HarmonyPatch(typeof(OfferingBowl), nameof(OfferingBowl.SpawnBoss))]
        public static class Patch_OfferingBowl_SpawnBoss
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(OfferingBowl __instance, ref bool __result)
            {
                if (ProgressionConfiguration.Instance.GetLockBossSummons() && __instance.m_bossPrefab != null)
                {
                    if (!Instance.HasSummoningKey(Utils.GetPrefabName(__instance.m_bossPrefab.gameObject)))
                    {
                        Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                        __result = false;
                        return false; // Skip summoning
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Block equipping items without the proper keys.
        /// </summary>
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
        public static class Patch_Humanoid_EquipItem
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(Humanoid __instance, ref bool __result, ItemDrop.ItemData item)
            {
                if (__instance != Player.m_localPlayer)
                {
                    return true;
                }

                if (ProgressionConfiguration.Instance.GetLockEquipment())
                {
                    if (Instance.IsActionBlocked(item, item.m_quality, true, true, false))
                    {
                        Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                        __result = false;
                        return false; // Skip equipping item
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Block opening doors without the proper keys.
        /// </summary>
        [HarmonyPatch(typeof(Door), nameof(Door.HaveKey))]
        public static class Patch_Door_HaveKey
        {
            [HarmonyPriority(Priority.Low)]
            private static void Postfix(Door __instance, ref bool __result)
            {
                if (__result && ProgressionConfiguration.Instance.GetLockEquipment() && __instance.m_keyItem != null &&
                    !Instance.HasItemKey(Utils.GetPrefabName(__instance.m_keyItem.gameObject), true, false, false))
                {
                    Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                    __result = false;
                }
            }
        }

        /// <summary>
        /// Block crafting items without the proper keys.
        /// </summary>
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.DoCrafting))]
        public static class Patch_InventoryGui_DoCrafting
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(InventoryGui __instance)
            {
                var lockCrafting = ProgressionConfiguration.Instance.GetLockCrafting();
                var lockCooking = ProgressionConfiguration.Instance.GetLockCooking();

                int quality = ProgressionAPI.GetQualityLevel(__instance.m_craftUpgradeItem);

                if ((lockCrafting || lockCooking) && Instance.IsActionBlocked(__instance.m_craftRecipe, quality, lockCrafting, lockCrafting, lockCooking))
                {
                    Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                    return false; // Skip crafting or cooking
                }

                return true;
            }
        }

        /// <summary>
        /// Block placing items without the proper keys.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
        public static class Patch_Player_PlacePiece
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(ref bool __result, Piece piece)
            {
                if (ProgressionConfiguration.Instance.GetLockBuilding() && piece?.m_resources != null)
                {
                    for (int lcv = 0; lcv < piece.m_resources.Length; lcv++)
                    {
                        if (piece.m_resources[lcv]?.m_resItem != null &&
                            !Instance.HasItemKey(Utils.GetPrefabName(piece.m_resources[lcv].m_resItem.gameObject), true, true, false))
                        {
                            Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                            __result = false;
                            return false; // Skip placing
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Block cooking items without the proper keys.
        /// </summary>
        [HarmonyPatch(typeof(CookingStation), nameof(CookingStation.OnUseItem))]
        public static class Patch_CookingStation_OnUseItem
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(ItemDrop.ItemData item, ref bool __result)
            {
                if (ProgressionConfiguration.Instance.GetLockCooking() && Instance.IsActionBlocked(item, item.m_quality, false, false, true))
                {
                    Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                    __result = false;
                    return false; // Skip cooking
                }

                return true;
            }
        }

        /// <summary>
        /// Block portal usage without the proper keys.
        /// </summary>
        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Teleport))]
        public static class Patch_TeleportWorld_Teleport
        {
            private static bool Prefix(Player player)
            {
                if (player == Player.m_localPlayer && !Instance.HasKey(ProgressionConfiguration.Instance.GetLockPortalsKey()))
                {
                    Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                    return false; // Skip portaling
                }

                return true;
            }
        }
    }
}
