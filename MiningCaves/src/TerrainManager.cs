using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.MiningCaves;

public class TerrainManager
{
    private const string SUFFIX = "_VVMC";
    private static bool _objectDBReady = false;

    protected struct OriginalTerrainComp
    {
        public string Name;
        public GameObject SpawnOnHit;
        public List<GameObject> Pieces;

        public OriginalTerrainComp(string name, GameObject spawnOnHit, List<GameObject> pieces)
        {
            Name = name;
            SpawnOnHit = spawnOnHit;
            Pieces = pieces;
        }
    }

    private static List<OriginalTerrainComp> _originalTerrainCompList = new List<OriginalTerrainComp>();

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

    /// <summary>
    /// Attempts to get the ItemDrop by the given name's hashcode, if not found searches by string.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="item"></param>
    /// <returns>True on sucessful find</returns>
    public static bool GetItemDrop(string name, out ItemDrop item)
    {
        item = null;

        if (!name.IsNullOrWhiteSpace())
        {
            // Try hash code
            var prefab = ObjectDB.instance.GetItemPrefab(name.GetStableHashCode());
            if (prefab == null)
            {
                // Failed, try slow search
                prefab = ObjectDB.instance.GetItemPrefab(name);
            }

            if (prefab != null)
            {
                item = prefab.GetComponent<ItemDrop>();
                if (item != null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Restores terrain modifying from original for all modified items.
    /// </summary>
    private static void RestoreOriginalValues()
    {
        foreach (var original in _originalTerrainCompList)
        {
            if (!GetItemDrop(original.Name, out ItemDrop item))
            {
                continue;
            }

            item.m_itemData.m_shared.m_spawnOnHitTerrain = original.SpawnOnHit;

            PieceTable pieceTable = item.m_itemData.m_shared.m_buildPieces;
            if (pieceTable != null)
            {
                item.m_itemData.m_shared.m_buildPieces.m_pieces = original.Pieces;
            }
        }

        _originalTerrainCompList = new List<OriginalTerrainComp>();
    }

    private static bool RemoveHitTerrainFromTable(ref ItemDrop itemDrop, out List<GameObject> OriginalPieces)
    {
        bool changed = false;
        OriginalPieces = null;

        PieceTable pieceTable = itemDrop.m_itemData.m_shared.m_buildPieces;
        if (pieceTable != null)
        {
            OriginalPieces = pieceTable.m_pieces;
            List<GameObject> newPieceTable = new List<GameObject>();

            foreach (GameObject piece in pieceTable.m_pieces)
            {
                TerrainOp terrainOp = piece.GetComponent<TerrainOp>();

                if (terrainOp != null)
                {
                    if (terrainOp.m_settings.m_level != false || terrainOp.m_settings.m_smooth != false)
                    {
                        // Replace pieces that flatten or level
                        var copy = GameObject.Instantiate(piece, MiningCavesPlugin.Root.transform, false);
                        copy.name = Utils.GetPrefabName(copy) + SUFFIX;
                        var terrainOpCopy = copy.GetComponent<TerrainOp>();
                        terrainOpCopy.m_settings.m_level = false;
                        terrainOpCopy.m_settings.m_smooth = false;

                        itemDrop.m_itemData.m_shared.m_spawnOnHitTerrain = copy;
                        newPieceTable.Add(copy);
                        changed = true;
                        continue;
                    }
                    else if (terrainOp.m_settings.m_raise == true)
                    {
                        // Remove pieces that raise the terrain
                        changed = true;
                        continue;
                    }
                }

                newPieceTable.Add(piece);
            }

            if (changed)
            {
                itemDrop.m_itemData.m_shared.m_buildPieces.m_pieces = newPieceTable;
            }
        }

        return changed;
    }

    private static bool RemoveHitTerrain(ref ItemDrop itemDrop, out GameObject original)
    {
        original = itemDrop.m_itemData.m_shared.m_spawnOnHitTerrain;
        if (original != null)
        {
            var terrainOp = original.GetComponent<TerrainOp>();
            if (terrainOp != null &&
                (terrainOp.m_settings.m_raise != false || terrainOp.m_spawnOnPlaced != null))
            {
                var copy = GameObject.Instantiate(original, MiningCavesPlugin.Root.transform, false);
                copy.name = Utils.GetPrefabName(copy) + SUFFIX;
                var terrainOpCopy = copy.GetComponent<TerrainOp>();
                terrainOpCopy.m_settings.m_raise = false;
                terrainOpCopy.m_spawnOnPlaced = null;
                itemDrop.m_itemData.m_shared.m_spawnOnHitTerrain = copy;

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes terrain modifying from all items in the game when enabled.
    /// </summary>
    public static void ApplyTerrainLocking()
    {
        if (!_objectDBReady)
        {
            return;
        }

        RestoreOriginalValues();

        if (MiningCavesPlugin.GetLockTerrain())
        {
            RemoveToolTerrainChanges();
        }

        if (MiningCavesPlugin.GetRemoveSilverWishbonePing() ||
            (MiningCavesPlugin.GetLockTerrain() && MiningCavesPlugin.GetLockTerrainIgnoreItems().IsNullOrWhiteSpace()))
        {
            RemoveWishbonePing();
        }
    }

    private static void RemoveToolTerrainChanges()
    {
        var ignoreItems = StringToSet(MiningCavesPlugin.GetLockTerrainIgnoreItems());

        foreach (GameObject item in ObjectDB.instance.m_items)
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

            GameObject spawnOnHit = null;
            List<GameObject> pieces = null;

            if (RemoveHitTerrainFromTable(ref itemDrop, out pieces) || RemoveHitTerrain(ref itemDrop, out spawnOnHit))
            {
                _originalTerrainCompList.Add(new OriginalTerrainComp(item.name, spawnOnHit, pieces));
            }
        }

        MiningCavesPlugin.MiningCavesLogger.LogInfo("Done removing terrain operations from tools.");
    }

    private static void RemoveWishbonePing()
    {
        GameObject silver = ZNetScene.instance.GetPrefab("silvervein");
        if (silver != null)
        {
            var beacon = silver.GetComponentInChildren<Beacon>();
            if (beacon != null)
            {
                GameObject.Destroy(beacon);
                MiningCavesPlugin.MiningCavesLogger.LogInfo("Done removing silver wishbone ping.");
            }
            else
            {
                MiningCavesPlugin.MiningCavesLogger.LogDebug("Issue finding silvervein beacon, could not remove.");
            }
        }
        else
        {
            MiningCavesPlugin.MiningCavesLogger.LogDebug("Issue finding silvervein. Could not remove beacon.");
        }
    }

    /// <summary>
    /// Perform changes.
    /// </summary>
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    public static class Patch_ObjectDB_Awake
    {
        private static void Postfix()
        {
            if (SceneManager.GetActiveScene().name.Equals("main"))
            {
                _objectDBReady = true;
                ApplyTerrainLocking();
            }
            else
            {
                _objectDBReady = false;
            }
        }
    }
}

