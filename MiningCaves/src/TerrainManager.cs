using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VentureValheim.MiningCaves
{
    public class TerrainManager
    {
        /// <summary>
        /// Converts a comma separated string to a HashSet of strings.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static HashSet<string> StringToSet(string str)
        {
            var set = new HashSet<string>();

            if (!str.IsNullOrWhiteSpace())
            {
                List<string> keys = str.Split(',').ToList();
                for (var lcv = 0; lcv < keys.Count; lcv++)
                {
                    set.Add(keys[lcv].Trim());
                }
            }

            return set;
        }

        [HarmonyPatch(typeof(ObjectDB), "Awake")]
        public static class Patch_ObjectDB_Awake
        {
            [HarmonyPriority(Priority.LowerThanNormal)]
            private static void Postfix(ObjectDB __instance)
            {
                if (!MiningCavesPlugin.GetLockTerrain())
                {
                    return;
                }

                var ignoreItems = StringToSet(MiningCavesPlugin.GetLockTerrainIgnoreItems());

                foreach (GameObject item in __instance.m_items)
                {
                    if (ignoreItems.Contains(item.name)) 
                    { 
                        continue;
                    }

                    ItemDrop itemDrop = item.GetComponent<ItemDrop>();
                    if (itemDrop == null)
                    {
                        continue;
                    }

                    // Remove terrain operation on object
                    GameObject hitTerrain = itemDrop.m_itemData.m_shared.m_spawnOnHitTerrain;
                    if (hitTerrain != null)
                    {
                        TerrainOp terrainOp = hitTerrain.GetComponent<TerrainOp>();
                        if (terrainOp != null)
                        {
                            terrainOp.m_settings.m_raise = false;
                            terrainOp.m_spawnOnPlaced = null;
                        }
                    }

                    // Remove terrain operations from piece table
                    PieceTable pieceTable = itemDrop.m_itemData.m_shared.m_buildPieces;
                    if (pieceTable != null)
                    {
                        List<GameObject> newPieceTable = new List<GameObject>();
                        foreach (GameObject piece in pieceTable.m_pieces)
                        {
                            TerrainOp terrainOp = piece.GetComponent<TerrainOp>();
                            if (terrainOp != null && terrainOp.m_settings.m_raise == false)
                            {
                                terrainOp.m_settings.m_level = false;
                                terrainOp.m_settings.m_smooth = false;
                                newPieceTable.Add(piece);
                            }
                        }

                        itemDrop.m_itemData.m_shared.m_buildPieces.m_pieces = newPieceTable;
                    }
                }

                MiningCavesPlugin.MiningCavesLogger.LogInfo("Done removing terrain operations from tools.");
            }
        }
    }
}

