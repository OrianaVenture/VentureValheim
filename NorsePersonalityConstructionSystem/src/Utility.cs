using BepInEx;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VentureValheim.NPCS;

public class Utility
{
    /// <summary>
    /// Attempts to get the ItemDrop by the given name's hashcode, if not found searches by string.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="item"></param>
    /// <returns>True on successful find</returns>
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

    public static bool GetItemPrefab(int hash, out GameObject item)
    {
        item = ObjectDB.instance.GetItemPrefab(hash);

        if (item != null)
        {
            return true;
        }

        return false;
    }

    public static HashSet<string> StringToSet(string str)
    {
        var set = new HashSet<string>();

        if (!str.IsNullOrWhiteSpace())
        {
            List<string> keys = str.Split(',').ToList();
            for (var lcv = 0; lcv < keys.Count; lcv++)
            {
                set.Add(keys[lcv].Trim().ToLower());
            }
        }

        return set;
    }

    public static INPC GetClosestNPC(Vector3 position)
    {
        Collider[] hits = Physics.OverlapBox(position, Vector3.one * 3, Quaternion.identity);
        GameObject closestnpc = null;

        foreach (var hit in hits)
        {
            var go = hit.transform.root.gameObject;
            if (go != null && go.GetComponentInChildren<INPC>() != null)
            {
                if (closestnpc == null || (Vector3.Distance(position, go.transform.position) <
                        Vector3.Distance(position, closestnpc.transform.position)))
                {
                    closestnpc = go;
                }
            }
        }

        if (closestnpc != null)
        {
            return closestnpc.GetComponentInChildren<INPC>();
        }

        return null;
    }

    public static List<NPCHumanoid> GetAllNPCS(Vector3 position, float range)
    {
        Collider[] hits = Physics.OverlapBox(position, Vector3.one * range, Quaternion.identity);
        List<NPCHumanoid> npcs = new List<NPCHumanoid>();

        foreach (var hit in hits)
        {
            var npc = hit.transform.root.gameObject.GetComponentInChildren<NPCHumanoid>();
            if (npc != null)
            {
                npcs.Add(npc);
            }
        }

        return npcs;
    }

    public static Chair GetClosestChair(Vector3 position, Vector3 scale)
    {
        Collider[] hits = Physics.OverlapBox(position, scale, Quaternion.identity);
        Chair closestChair = null;

        foreach (var hit in hits)
        {
            var chairs = hit.transform.root.gameObject.GetComponentsInChildren<Chair>();
            if (chairs != null)
            {
                for (int lcv = 0; lcv < chairs.Length; lcv++)
                {
                    var chair = chairs[lcv];
                    if (closestChair == null || (Vector3.Distance(position, chair.transform.position) <
                        Vector3.Distance(position, closestChair.transform.position)))
                    {
                        closestChair = chair;
                    }
                }
            }
        }

        return closestChair;
    }

    public static void SetKey(string key, bool global)
    {
        if (!string.IsNullOrEmpty(key))
        {
            if (global)
            {
                ZoneSystem.instance.SetGlobalKey(key);
            }
            else
            {
                Player.m_localPlayer.AddUniqueKey(key);
            }
        }
    }

    public static bool HasKey(string key)
    {
        if (key.IsNullOrWhiteSpace())
        {
            return true;
        }

        key = key.ToLower();

        return ZoneSystem.instance.GetGlobalKey(key) || Player.m_localPlayer.HaveUniqueKey(key);
    }
}
