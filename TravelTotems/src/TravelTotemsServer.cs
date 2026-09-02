using HarmonyLib;
using Jotunn.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace VentureValheim.TravelTotems;

public static partial class TravelTotems
{
    private static void StartTravelTotemsServer()
    {
        bool trackerFound = false;
        Reset();

        foreach (KeyValuePair<ZDOID, ZDO> zdo in ZDOMan.instance.m_objectsByID)
        {
            int prefab = zdo.Value.GetPrefab();
            if (AssetManager.IsTravelTotemTracker(prefab))
            {
                trackerFound = true;
                TravelTotemTrackerZDO = zdo.Value;
                continue;
            }
            else if (!AssetManager.IsTravelTotem(prefab))
            {
                continue;
            }

            TravelTotemZDOs.Add(zdo.Value);

            int bin = zdo.Value.GetInt(TravelTotemPortal.TOTEM_ID_BIN, -1);
            int id = zdo.Value.GetInt(TravelTotemPortal.TOTEM_ID, -1);

            if (bin == -1 || id == -1)
            {
                UnassignedTravelTotemZDOs.Enqueue(zdo.Value);
            }
        }

        if (!trackerFound)
        {
            GameObject tracker = PrefabManager.Cache.GetPrefab<GameObject>(AssetManager.TravelTotemTrackerPrefabName);
            GameObject.Instantiate(tracker, Vector3.zero, Quaternion.identity);
        }
        else
        {
            InitializeTracker();
        }

        TravelTotemsPlugin.TravelTotemsLogger.LogDebug($"Tracking {TravelTotemZDOs.Count} travel totems. " +
                $"{UnassignedTravelTotemZDOs.Count} need assignment.");

        ProcessUnassignedTotems();
    }

    public static void InitializeTracker()
    {
        if (TravelTotemTrackerZDO == null)
        {
            TravelTotemsPlugin.TravelTotemsLogger.LogError("Tracker is null! Will not be able to assign new totem ids.");
            return;
        }

        int nextTotemIDBin = TravelTotemTrackerZDO.GetInt(TOTEM_MAX_ID_BIN, -1);
        int nextTotemID = TravelTotemTrackerZDO.GetInt(TOTEM_MAX_ID, -1);

        if (nextTotemIDBin == -1 || nextTotemID == -1)
        {
            bool hasTotems = FindMaxTotemID(out int bin, out int id);

            UpdateMaxTotemIndexes(bin, id);
        }
    }

    public static bool FindMaxTotemID(out int maxBin, out int maxID)
    {
        bool found = false;
        maxBin = 0;
        maxID = 0;
        foreach (KeyValuePair<ZDOID, ZDO> zdo in ZDOMan.instance.m_objectsByID)
        {
            int prefab = zdo.Value.GetPrefab();
            if (!AssetManager.IsTravelTotem(prefab))
            {
                continue;
            }

            int bin = zdo.Value.GetInt(TravelTotemPortal.TOTEM_ID_BIN, -1);
            int id = zdo.Value.GetInt(TravelTotemPortal.TOTEM_ID, -1);

            if (bin > maxBin)
            {
                found = true;
                maxBin = bin;
                maxID = id;
            }
            else if (bin == maxBin && id > maxID)
            {
                found = true;
                maxID = id;
            }
        }

        return found;
    }

    /// <summary>
    /// Counts up by one bit, resetting after the 31th digit, to be stored in a 32-bit integer.
    /// </summary>
    private static void IncrementIDMax()
    {
        int bin = GetMaxTotemIDBin();
        int id = GetMaxTotemID();

        if (id == 0x40000000)
        {
            bin++;
            id = 1;
        }
        else if (id <= 0)
        {
            id = 1;
        }
        else
        {
            id <<= 1;
        }

        UpdateMaxTotemIndexes(bin, id);
    }

    public static void RPC_ConsumeTotemID(long sender)
    {
        if (!ZNet.instance.IsServer() || TravelTotemTrackerZDO == null)
        {
            return;
        }

        IncrementIDMax();

        ZRoutedRpc.instance.InvokeRoutedRPC(
            sender, RPCNAME_ConsumeTotemIDResponse, GetMaxTotemIDBin(), GetMaxTotemID());
    }

    private static void UpdateMaxTotemIndexes(int bin, int id)
    {
        if (TravelTotemTrackerZDO == null || !TravelTotemTrackerZDO.IsValid())
        {
            return;
        }

        TravelTotemTrackerZDO.SetOwner(ZDOMan.GetSessionID());
        TravelTotemTrackerZDO.Set(TOTEM_MAX_ID_BIN, bin);
        TravelTotemTrackerZDO.Set(TOTEM_MAX_ID, id);
    }

    public static int GetMaxTotemIDBin()
    {
        return TravelTotemTrackerZDO.GetInt(TOTEM_MAX_ID_BIN);
    }

    public static int GetMaxTotemID()
    {
        return TravelTotemTrackerZDO.GetInt(TOTEM_MAX_ID);
    }
}