using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace VentureValheim.TravelTotems;

public static partial class TravelTotems
{
    public const string TOTEM_MAX_ID_BIN = "VV_MaxTotemIDBin";
    public const string TOTEM_MAX_ID = "VV_MaxTotemID";

    public const string RPCNAME_RequestTotems = "VV_RequestTotems";
    public const string RPCNAME_SendTotems = "VV_SendTotems";

    public const string RPCNAME_ConsumeTotemID = "VV_ConsumeTotemID";
    public const string RPCNAME_ConsumeTotemIDResponse = "VV_ConsumeTotemIDResponse";

    private static List<ZDO> TravelTotemZDOs = new List<ZDO>();
    private static Queue<ZDO> UnassignedTravelTotemZDOs = new Queue<ZDO>();
    public static ZDO TravelTotemTrackerZDO;

    private static List<UnlockRequirement> TotemUnlockCost = new List<UnlockRequirement>();

    private static void Reset()
    {
        TravelTotemZDOs.Clear();
        TravelTotemTrackerZDO = null;
        UnassignedTravelTotemZDOs.Clear();
    }

    public static void SetRecipeFromConfig(string configString)
    {
        TotemUnlockCost.Clear();

        string[] entries = configString.Replace(" ", "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string entry in entries)
        {
            string[] parts = entry.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                AssetManager.PrintRecipeErrorMessage(configString);
                continue;
            }

            string item = parts[0];
            string amountString = parts[1];
            if (!int.TryParse(amountString, out int amount))
            {
                AssetManager.PrintRecipeErrorMessage(configString);
                continue;
            }

            TotemUnlockCost.Add(new UnlockRequirement
            {
                PrefabName = item,
                Amount = amount
            });
        }
    }

    public static void UpdateSettings()
    {
        SetRecipeFromConfig(TravelTotemsPlugin.GetTotemUnlockRequirementsString());

        if (TravelTotemsPlugin.GetTravelTotemType() == TravelTotemType.Undefined)
        {
            TravelTotemsPlugin.TravelTotemsLogger.LogWarning("Default totem type is undefined. " +
                "Please check your configuration for spelling errors!" +
                "New totems may behave strangely until fixed.");
        }
    }

    public static bool HasUnlockRequirements()
    {
        return TotemUnlockCost.Count > 0;
    }

    public static bool TryConsumeUnlockRequirements()
    {
        if (TotemUnlockCost.Count > 0)
        {
            foreach (UnlockRequirement cost in TotemUnlockCost)
            {
                if (Player.m_localPlayer.m_inventory.CountItems(cost.GetItemDropName()) < cost.Amount)
                {
                    return false;
                }
            }

            foreach (UnlockRequirement cost in TotemUnlockCost)
            {
                Player.m_localPlayer.m_inventory.RemoveItem(cost.GetItemDropName(), cost.Amount);
            }

            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_offerdone");
        }

        return true;
    }

    public static void AddTotem(ZDO totem)
    {
        if (totem != null && !TravelTotemZDOs.Contains(totem))
        {
            TravelTotemZDOs.Add(totem);

            if (ZNet.instance.IsServer())
            {
                // Update totem for all client connections
                ZDOMan.instance.ForceSendZDO(totem.m_uid);
            }
        }
    }

    public static void RemoveTotem(ZDO totem)
    {
        if (totem != null)
        {
            TravelTotemZDOs.Remove(totem);
        }

        TravelTotemMap.RefreshMapPins();
    }

    public static List<ZDO> GetTravelTotems()
    {
        return TravelTotemZDOs;
    }

    private static void StartTravelTotems()
    {
        TravelTotemsPlugin.TravelTotemsLogger.LogDebug($"Loading travel totems...");
        if (ZNet.instance.IsServer())
        {
            StartTravelTotemsServer();
            if (!ZNet.instance.IsDedicated())
            {
                // Update configurations for hosting player
                UpdateSettings();
            }
        }
        else
        {
            StartTravelTotemsClient();
        }
    }

    private static void StartTravelTotemsClient()
    {
        Reset();
    }

    /// <summary>
    /// Server response to the client sending the next available totem bin and ID.
    /// </summary>
    public static void RPC_ConsumeTotemIDResponse(long sender, int bin, int id)
    {
        ZDO zdo = UnassignedTravelTotemZDOs.Dequeue();

        if (zdo == null)
        {
            return;
        }

        if (!zdo.IsOwner())
        {
            zdo.SetOwner(ZDOMan.GetSessionID());
        }

        zdo.Set(TravelTotemPortal.TOTEM_ID_BIN, bin);
        zdo.Set(TravelTotemPortal.TOTEM_ID, id);
        zdo.Set(TravelTotemPortal.TOTEM_TYPE, (int)TravelTotemType.ServerDefault);
    }

    public static void ProcessUnassignedTotems()
    {
        int count = UnassignedTravelTotemZDOs.Count;

        for (int lcv = 0; lcv < count; lcv++)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(RPCNAME_ConsumeTotemID);
        }
    }

    public static void AssignTotemID(ZDO zdo)
    {
        if (zdo == null)
        {
            return;
        }

        UnassignedTravelTotemZDOs.Enqueue(zdo);
        ZRoutedRpc.instance.InvokeRoutedRPC(RPCNAME_ConsumeTotemID);
    }

    /// <summary>
    /// Recieved by the server to send the totem data to the requesting player.
    /// </summary>
    public static void RPC_RequestTotems(ZRpc rpc)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(rpc);

        if (peer == null)
        {
            TravelTotemsPlugin.TravelTotemsLogger.LogWarning("Recieved a request for totem data but peer was not found!");
            return;
        }

        ZPackage package = new ZPackage();
        package.Write(TravelTotemZDOs.Count);

        foreach (ZDO totem in TravelTotemZDOs)
        {
            package.Write(totem.m_uid);
            ZDOMan.instance.ForceSendZDO(peer.m_uid, totem.m_uid);
        }

        rpc.Invoke(RPCNAME_SendTotems, package);
    }

    /// <summary>
    /// Recieve sent totems.
    /// </summary>
    public static void RPC_SendTotems(ZRpc rpc, ZPackage package)
    {
        int count = package.ReadInt();
        int validCount = 0;

        for (int lcv = 0; lcv < count; lcv++)
        {
            ZDOID id = package.ReadZDOID();
            ZDO totem = ZDOMan.instance.GetZDO(id);

            if (totem == null)
            {
                ZDOMan.instance.RequestZDO(id);
                continue;
            }

            validCount++;
            TravelTotemZDOs.Add(totem);
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Start))]
    private static class Patch_ZNet_Start
    {
        private static void Postfix()
        {
            StartTravelTotems();
        }
    }

    /// <summary>
    /// Extract totems as they are created and track both client and server side.
    /// </summary>
    [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.RPC_ZDOData))]
    private static class Patch_ZDOMan_RPC_ZDOData
    {
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> ZDOManRPC_ZDODataTranspiler(
            IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions, generator)
                .Start()
                .MatchStartForward(
                    // zDO.Deserialize(pkg2);
                    new CodeMatch(OpCodes.Ldloc_3),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ZDO), nameof(ZDO.Deserialize))))
                .MatchStartBackwards(
                    new CodeMatch(OpCodes.Ldloc_S))
                .ExtractOperand(out object operand)
                .ThrowIfInvalid($"Could not patch ZDOMan.RPC_ZDOData!")
                .Advance(offset: 3)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloc_S, operand),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_ZDOMan_RPC_ZDOData), nameof(TryAddTotem))))
                .InstructionEnumeration();
        }

        static void TryAddTotem(ZDO zdoObject)
        {
            if (AssetManager.IsTravelTotem(zdoObject.m_prefab))
            {
                AddTotem(zdoObject);
            }
        }
    }

    [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.ShutDown))]
    private static class Patch_ZDOMan_ShutDown
    {
        private static void Postfix()
        {
            Reset();
        }
    }

    // Always send Totems
    [HarmonyPatch(typeof(ZDOMan.ZDOPeer), nameof(ZDOMan.ZDOPeer.ShouldSend))]
    private static class Patch_ZDOMan_ZDOPeer_ShouldSend
    {
        private static bool Prefix(ZDO zdo, ref bool __result)
        {
            if (AssetManager.IsTravelTotem(zdo.m_prefab))
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    // Never invalidate a totem
    [HarmonyPatch(typeof(ZDOMan.ZDOPeer), nameof(ZDOMan.ZDOPeer.ZDOSectorInvalidated))]
    private static class Patch_ZDOMan_ZDOPeer_ZDOSectorInvalidated
    {
        private static bool Prefix(ZDO zdo)
        {
            if (AssetManager.IsTravelTotem(zdo.m_prefab))
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Remove totems as they are destroyed and track both client and server side.
    /// </summary>
    [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.HandleDestroyedZDO))]
    private static class Patch_ZDOMan_HandleDestroyedZDO
    {
        private static void Prefix(ZDOID uid)
        {
            ZDO zdo = ZDOMan.instance.GetZDO(uid);

            if (zdo != null && AssetManager.IsTravelTotem(zdo.GetPrefab()))
            {
                RemoveTotem(zdo);
            }
        }
    }

    [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.AddPeer))]
    private static class Patch_ZDOMan_AddPeer2
    {
        private static void Postfix(ZNetPeer netPeer)
        {
            if (ZNet.instance.IsServer())
            {
                netPeer.m_rpc.Register(RPCNAME_RequestTotems, RPC_RequestTotems);
            }

            if (!ZNet.instance.IsDedicated())
            {
                netPeer.m_rpc.Register<ZPackage>(RPCNAME_SendTotems, RPC_SendTotems);
                netPeer.m_rpc.Invoke(RPCNAME_RequestTotems);
            }
        }
    }

    /// <summary>
    /// Only allow creators of the totems to destroy them if they can be destroyed by another mod.
    /// </summary>
    [HarmonyPatch(typeof(Piece), nameof(Piece.CanBeRemoved))]
    private static class Patch_Piece_CanBeRemoved
    {
        private static void Postfix(Piece __instance, ref bool __result)
        {
            if (__result == false)
            {
                return;
            }

            ZDO zdo = __instance.m_nview.GetZDO();

            if (zdo != null && AssetManager.IsTravelTotem(zdo.GetPrefab()))
            {
                bool isCreator = __instance.IsCreator();
                if (!TravelTotemsPlugin.GetOverrideTotemAccess(isCreator))
                {
                    __result = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.Start))]
    private static class Patch_Game_Start
    {
        private static void Postfix()
        {
            try
            {
                ZRoutedRpc.instance.Register(RPCNAME_ConsumeTotemID,
                    new Action<long>(RPC_ConsumeTotemID));
                ZRoutedRpc.instance.Register(RPCNAME_ConsumeTotemIDResponse,
                    new Action<long, int, int>(RPC_ConsumeTotemIDResponse));
            }
            catch
            {
                TravelTotemsPlugin.TravelTotemsLogger.LogDebug("Totem RPCs have already been registered. Skipping.");
            }
        }
    }
}