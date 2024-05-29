using BepInEx;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VentureValheim.NPCS;

public class Utility
{
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
            var npc = hit.transform.root.gameObject.GetComponentInChildren<NPC>();
            if (npc != null)
            {
                if (closestnpc == null || (Vector3.Distance(position, npc.transform.position) <
                        Vector3.Distance(position, closestnpc.transform.position)))
                {
                    closestnpc = npc;
                }
            }
        }

        return closestnpc;
    }

    public static List<NPC> GetAllNPCS(Vector3 position, float range)
    {
        Collider[] hits = Physics.OverlapBox(position, Vector3.one * range, Quaternion.identity);
        List<NPC> npcs = new List<NPC>();

        foreach (var hit in hits)
        {
            var npc = hit.transform.root.gameObject.GetComponentInChildren<NPC>();
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
}
