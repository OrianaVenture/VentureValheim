using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.PathsideAssistance
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class PathsideAssistancePlugin : BaseUnityPlugin
    {
        private const string ModName = "PathsideAssistance";
        private const string ModVersion = "0.1.0";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource PathsideAssistanceLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static GameObject Root;
        private static bool Configured = false;

        public void Awake()
        {
            PathsideAssistanceLogger.LogInfo("Thank you for your call, help is on the way.");

            // Create a dummy root object to reference
            Root = new GameObject("PathsideRoot");
            Root.SetActive(false);
            DontDestroyOnLoad(Root);

            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);
        }

        /// <summary>
        /// Adds new pieces to the given piece based off existing entries in the piece table
        /// </summary>
        /// <param name="piece"></param>
        private static void UpdatePieceTable(GameObject piece)
        {
            if (piece != null)
            {
                var itemDrop = piece.GetComponent<ItemDrop>();
                if (itemDrop != null)
                {
                    var pieceTable = itemDrop.m_itemData.m_shared.m_buildPieces;
                    if (pieceTable != null)
                    {
                        List<GameObject> newPieceTable = new List<GameObject>();
                        foreach (var item in pieceTable.m_pieces)
                        {
                            newPieceTable.Add(item); // Add original
                            var copy = GetPaintOnlyCopy(item);
                            if (copy != null)
                            {
                                newPieceTable.Add(copy); // Add modified
                            }
                        }

                        itemDrop.m_itemData.m_shared.m_buildPieces.m_pieces = newPieceTable;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a copy of a piece with no terrain height changes and a modified name to identify it
        /// given the original has a terrain height change. If doesn't have a height changes returns
        /// a copy that has a radius twice as large.
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        private static GameObject GetPaintOnlyCopy(GameObject piece)
        {
            if (piece.GetComponent<TerrainOp>() != null)
            {
                var copy = GameObject.Instantiate(piece, Root.transform, false);
                copy.name = Utils.GetPrefabName(copy) + "_ALT";

                // Change name
                var pieceComp = copy.GetComponent<Piece>();
                if (pieceComp != null)
                {
                    pieceComp.m_name = Localization.instance.Localize($"{pieceComp.m_name}") + " ALT";
                }

                // Change TerrainOp
                var terrainComp = copy.GetComponent<TerrainOp>();
                if (terrainComp.m_settings.m_raise != true &&
                    (terrainComp.m_settings.m_level == true || terrainComp.m_settings.m_smooth == true))
                {
                    terrainComp.m_settings.m_raise = false;
                    terrainComp.m_settings.m_level = false;
                    terrainComp.m_settings.m_smooth = false;
                }
                else
                {
                    terrainComp.m_settings.m_raiseRadius *= 2;
                    terrainComp.m_settings.m_levelRadius *= 2;
                    terrainComp.m_settings.m_smoothRadius *= 2;
                    terrainComp.m_settings.m_paintRadius *= 2;
                }

                return copy;
            }

            return null;
        }

        /// <summary>
        /// Apply changes, low priority to run after other mod patches.
        /// </summary>
        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
        public static class Patch_ObjectDB_Awake
        {
            [HarmonyPriority(Priority.Low)]
            private static void Postfix(ObjectDB __instance)
            {
                if (!Configured && SceneManager.GetActiveScene().name.Equals("main"))
                {
                    var hoe = __instance.GetItemPrefab("Hoe".GetStableHashCode());
                    UpdatePieceTable(hoe);

                    var cultivator = __instance.GetItemPrefab("Cultivator".GetStableHashCode());
                    UpdatePieceTable(cultivator);

                    PathsideAssistanceLogger.LogInfo("Done adding additional options.");
                    Configured = true;
                }
            }
        }
    }
}