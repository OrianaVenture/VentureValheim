using HarmonyLib;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace VentureValheim.PathsideAssistance
{
    public class PathsideAssistance
    {
        static PathsideAssistance() { }
        private PathsideAssistance() { }
        private static readonly PathsideAssistance _instance = new PathsideAssistance();

        public static PathsideAssistance Instance
        {
            get => _instance;
        }

        private static bool Configured = false;

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
                                IconMerge.AddSpriteOverlay(ref copy);
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
                var copy = GameObject.Instantiate(piece, PathsideAssistancePlugin.Root.transform, false);
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

                    if (pieceComp.m_resources != null)
                    {
                        for (int lcv = 0; lcv < pieceComp.m_resources.Length; lcv++)
                        {
                            pieceComp.m_resources[lcv].m_amount *= 4;
                        }
                    }
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

                    PathsideAssistancePlugin.PathsideAssistanceLogger.LogInfo("Done adding additional options.");
                    Configured = true;
                }
            }
        }
    }
}
