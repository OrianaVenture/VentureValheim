using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.LocationReset
{
    /// <summary>
    ///     Mod compatiability checks.
    /// </summary>
    internal static class ModCompat
    {

        internal const string DungeonSplitterName = "dungeon_splitter";
        private static bool? _DungeonSplitterInstalled = null;
        public static bool DungeonSplitterInstalled { 
            get {
                // Check for Dungeon Splitter if have not already checked
                _DungeonSplitterInstalled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(DungeonSplitterName);
                return _DungeonSplitterInstalled.Value;
            } 
        }

        internal const string MVBPName = "Searica.Valheim.MoreVanillaBuildPrefabs";
        private const string MVBPCheckPieceAddedMethodName = "IsPieceAddedByMVBP";
        private static bool? _MVBPInstalled = null;
        private static BaseUnityPlugin MVBPPlugin;
        private static MethodInfo IsPieceAddedByMVBP;

        /// <summary>
        ///     Flag indicating if MVBP is installed.
        /// </summary>
        public static bool MVBPInstalled
        {
            get
            {
                // Check for More Vanilla Build Prefabs
                _MVBPInstalled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(MVBPName);
                if (_MVBPInstalled.Value)
                {
                    MVBPPlugin ??= BepInEx.Bootstrap.Chainloader.PluginInfos[MVBPName].Instance;
                    IsPieceAddedByMVBP ??= AccessTools.Method(MVBPName.GetType(), MVBPCheckPieceAddedMethodName);
                }
                return _MVBPInstalled.Value;
            }
        }

        /// <summary>
        ///     Safely invoke MVBP's public API method IsPieceAddedByMVBP 
        ///     via reflection if and only if MVBP is installed.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="piece"></param>
        /// <returns>
        /// True if MVBP is installed and the prefab (or root prefab this GameObject is a clone of) 
        /// has had a Piece component added by MVBP, False otherwise.
        /// </returns>
        public static bool InvokeIsPieceAddedByMVBP(GameObject prefab, Piece piece = null)
        {
            if (!MVBPInstalled || MVBPPlugin is null || IsPieceAddedByMVBP is null)
            {
                return false;
            }

            try
            {
                return (bool)IsPieceAddedByMVBP.Invoke(MVBPPlugin, new object[] { prefab, piece });
            }
            catch (Exception ex)
            {
                LocationResetPlugin.LocationResetLogger.LogError(ex);
                LocationResetPlugin.LocationResetLogger.LogWarning("Failed to invoke IsPieceAddedByMVBP");
            }

            return false;
        }
    }
}
