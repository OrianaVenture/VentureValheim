using BepInEx;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace VentureValheim.NPCS;

public class Utility
{
    public static void CopyFields<T1, T2>(T1 original, ref T2 clone) where T2 : T1
    {
        var fields = typeof(T1).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (FieldInfo field in fields)
        {
            try
            {
                var value = field.GetValue(original);
                field.SetValue(clone, value);
            }
            catch { }
        }
    }

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
        NPCSPlugin.NPCSLogger.LogDebug($"Creating new set...");
        var set = new HashSet<string>();

        if (!str.IsNullOrWhiteSpace())
        {
            var keys = str.Split(NPCZDOUtils.CommaSeparatorList, int.MaxValue, System.StringSplitOptions.RemoveEmptyEntries);
            for (var lcv = 0; lcv < keys.Length; lcv++)
            {
                NPCSPlugin.NPCSLogger.LogDebug($"Added to set: {keys[lcv].Trim().ToLower()}");
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

    public static void SetKey(string key, NPCData.NPCKeyType type)
    {
        if (!string.IsNullOrEmpty(key))
        {
            if (type == NPCData.NPCKeyType.Global)
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

    public static GameObject CreateGameObject(GameObject original, string name)
    {
        GameObject go = GameObject.Instantiate(original, NPCSPlugin.Root.transform, false);
        go.name = NPCSPlugin.MOD_PREFIX + name;
        go.transform.SetParent(NPCSPlugin.Root.transform, false);

        return go;
    }

    public static void RegisterGameObject(GameObject obj)
    {
        ZNetScene.instance.m_prefabs.Add(obj);
        ZNetScene.instance.m_namedPrefabs.Add(obj.name.GetStableHashCode(), obj);
        NPCSPlugin.NPCSLogger.LogDebug($"Adding object to prefabs {obj.name}");
    }

    public static string GetString<T>(T item)
    {
        string result = "";
        if (item != null)
        {
            result = item.ToString();
        }
        return result;
    }

    public static string GetStringFromList<T>(List<T> items)
    {
        string result = "";
        if (items != null)
        {
            foreach (T item in items)
            {
                result += $"{item.ToString()}{NPCZDOUtils.PipeSeparator}";
            }
        }
        return result;
    }
}
