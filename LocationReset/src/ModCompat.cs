using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.LocationReset
{
    /// <summary>
    /// Mod compatibility checks.
    /// </summary>
    internal static class ModCompat
    {

        internal const string DungeonSplitterName = "dungeon_splitter";
        private static bool? _DungeonSplitterInstalled = null;

        /// <summary>
        /// Flag indicating if DungeonSplitter is installed.
        /// </summary>
        public static bool DungeonSplitterInstalled { 
            get {
                // Check for Dungeon Splitter if have not already checked
                _DungeonSplitterInstalled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(DungeonSplitterName);
                return _DungeonSplitterInstalled.Value;
            } 
        }

        internal const string MVBPName = "Searica.Valheim.MoreVanillaBuildPrefabs";
        private static bool? _MVBPInstalled = null;
        /// <summary>
        /// Flag indicating if MVBP is installed.
        /// </summary>
        public static bool MVBPInstalled
        {
            get
            {
                // Check for More Vanilla Build Prefabs
                _MVBPInstalled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(MVBPName);
                return _MVBPInstalled.Value;
            }
        }
    }
}
