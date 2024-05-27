using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VentureValheim.VentureQuest;

public class Utility
{
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

    public static NPC GetClosestNPC(Vector3 position)
    {
        Collider[] hits = Physics.OverlapBox(position, Vector3.one * 2, Quaternion.identity);
        NPC closestnpc = null;

        foreach (var hit in hits)
        {
            var npcs = hit.transform.root.gameObject.GetComponentsInChildren<NPC>();
            if (npcs != null)
            {
                for (int lcv = 0; lcv < npcs.Length; lcv++)
                {
                    var npc = npcs[lcv];
                    if (closestnpc == null || (Vector3.Distance(position, npc.transform.position) <
                        Vector3.Distance(position, closestnpc.transform.position)))
                    {
                        closestnpc = npc;
                    }
                }
            }
        }

        return closestnpc;
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
}
