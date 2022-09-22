using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;

namespace VentureValheim.Progression
{
    public class ProgressionManager
    {
        public static string BlockedGlobalKeys { get; private set; }
        public static string AllowedGlobalKeys { get; private set; }
        public static List<string>? BlockedGlobalKeysList  { get; private set; }
        public static List<string>? AllowedGlobalKeysList  { get; private set; }

        private ProgressionManager()
        {
            BlockedGlobalKeys = "";
            AllowedGlobalKeys = "";
            BlockedGlobalKeysList = null;
            AllowedGlobalKeysList = null;
        }

        private static readonly ProgressionManager _instance = new ProgressionManager();

        public static ProgressionManager Instance
        {
            get => _instance;
        }

        public void Initialize(string blockedGlobalKeys, string allowedGlobalKeys)
        {
            if (!BlockedGlobalKeys.Equals(blockedGlobalKeys))
            {
                BlockedGlobalKeysList = null;

                if (!blockedGlobalKeys.IsNullOrWhiteSpace())
                {
                    BlockedGlobalKeysList = blockedGlobalKeys.Split(',').ToList();
                    for (var lcv = 0; lcv < BlockedGlobalKeysList.Count; lcv++)
                    {
                        BlockedGlobalKeysList[lcv] = BlockedGlobalKeysList[lcv].Trim();
                    }
                }
            }

            if (!AllowedGlobalKeys.Equals(allowedGlobalKeys))
            {
                AllowedGlobalKeysList = null;

                if (!allowedGlobalKeys.IsNullOrWhiteSpace())
                {
                    AllowedGlobalKeysList = allowedGlobalKeys.Split(',').ToList();
                    for (var lcv = 0; lcv < AllowedGlobalKeysList.Count; lcv++)
                    {
                        AllowedGlobalKeysList[lcv] = AllowedGlobalKeysList[lcv].Trim();
                    }
                }
            }
        }

        /// <summary>
        /// Skips the original ZoneSystem.SetGlobalKey method if a key is blocked.
        /// </summary>
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SetGlobalKey))]
        public static class Patch_ZoneSystem_SetGlobalKey
        {
            private static bool Prefix(string name)
            {
                // TODO initialize on config changes only
                Instance.Initialize(ProgressionPlugin.Instance.GetBlockedGlobalKeys(), ProgressionPlugin.Instance.GetAllowedGlobalKeys());
                if (Instance.BlockGlobalKey(name))
                {
                    ProgressionPlugin.GetProgressionLogger().LogDebug($"Skipping adding global key: {name}.");
                    return false; // Skip adding the global key
                }

                ProgressionPlugin.GetProgressionLogger().LogDebug($"Adding global key: {name}.");
                return true; // Continue adding the global key
            }
        }

        /// <summary>
        /// Whether to block a Global Key based on configuration settings.
        /// </summary>
        /// <param name="globalKey"></param>
        /// <returns>True when default blocked and does not exist in the allowed list,
        /// or when default unblocked and key is in the blocked list.</returns>
        public bool BlockGlobalKey(string globalKey)
        {
            if (ProgressionPlugin.Instance.GetBlockAllGlobalKeys())
            {
                return !AllowedGlobalKeysList?.Contains(globalKey) ?? true;
            }

            return BlockedGlobalKeysList?.Contains(globalKey) ?? false;
        }
    }
}