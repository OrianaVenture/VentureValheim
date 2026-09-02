using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace VentureValheim.TravelTotems;

public static class TravelTotemMap
{
    public static bool TravelMapActive { get; private set; } = false;
    public static bool TotemTeleportActive { get; private set; } = false;
    private static bool TravelMapPinsAdded = false;

    private static List<Minimap.PinData> TotemMapPins = new List<Minimap.PinData>();
    private static List<Vector3> LocationMapPins = new List<Vector3>();

    private const string START_LOC = "StartTemple";
    private const string HALDOR_LOC = "Vendor_BlackForest";
    private const string HILDIR_LOC = "Hildir_camp";
    private const string BOGWITCH_LOC = "BogWitch_Camp";

    public static void SetTravelMapActive(bool active)
    {
        TravelMapActive = active;
    }

    public static void SetTotemTeleportActive(bool active)
    {
        TotemTeleportActive = active;
    }

    public static void AddMapPin(ZDO totem)
    {
        if (totem == null || Minimap.instance == null)
        {
            return;
        }

        Minimap.PinData pin = Minimap.instance.AddPin(totem.GetPosition(), TravelTotemMapPin.GetTotemPinType(),
            totem.GetString(TravelTotemPortal.TOTEM_NAME), false, false);
        TotemMapPins.Add(pin);
    }

    public static void RefreshMapPins()
    {
        if (!Minimap.instance)
        {
            return;
        }

        ClearPins();
        UpdateMapPins();
    }

    private static void UpdateMapPins()
    {
        if ((TravelMapActive || TravelTotemsPlugin.GetShowMapPins()) && !TravelMapPinsAdded)
        {
            // Add pins
            List<ZDO> totems = TravelTotemPlayer.GetValidTotems();

            foreach (ZDO totem in totems)
            {
                AddMapPin(totem);
            }

            TravelMapPinsAdded = true;
        }
        else if (!TravelMapActive && TravelMapPinsAdded && !TravelTotemsPlugin.GetShowMapPins())
        {
            ClearPins();
        }
    }

    private static void ClearPins()
    {
        Minimap.PinType totemPinType = TravelTotemMapPin.GetTotemPinType();
        foreach (Minimap.PinData pin in TotemMapPins)
        {
            if (pin.m_type == totemPinType)
            {
                Minimap.instance.RemovePin(pin);
            }
        }

        TotemMapPins.Clear();
        TravelMapPinsAdded = false;
    }

    public static void TryTriggerTeleportGamepad()
    {
        // TODO: see if the check/uncheck pin code can be easily skipped here
        if (!TravelMapActive)
        {
            return;
        }

        Vector3 position = Minimap.instance.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2));
        Vector3 pinLocation = GetClosestTravelLocation(position,
            Minimap.instance.m_removeRadius * (Minimap.instance.m_largeZoom * 2f), out bool isTotem);

        TryTriggerTeleport(pinLocation, isTotem);
    }

    private static void TryTriggerTeleport(Vector3 position, bool isTotem)
    {
        if (position == Vector3.zero)
        {
            return;
        }

        StopTotemMap();
        SetTotemTeleportActive(isTotem);
        Player.m_localPlayer.TeleportTo(position, Quaternion.identity, true);
    }

    private static bool AllowedPinType(int pinType)
    {
        if (pinType == (int)TravelTotemMapPin.GetTotemPinType())
        {
            return true;
        }
        else if (pinType == (int)Minimap.PinType.Bed)
        {
            return TravelTotemsPlugin.GetAllowSelectBed();
        }
        else if (TravelTotemsPlugin.GetAllowSelectTraders() && TravelTotemsPlugin.MultiplayerTweaksInstalled)
        {
            return IsMultiplayerTweaksTraderPin(pinType);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool IsMultiplayerTweaksTraderPin(int pinType)
    {
        try
        {
            int haldor = MultiplayerTweaks.API.GetHaldorPinIndex();
            int hildir = MultiplayerTweaks.API.GetHildirPinIndex();
            int bogWitch = MultiplayerTweaks.API.GetBogWitchPinIndex();
            if (pinType == haldor || pinType == hildir || pinType == bogWitch)
            {
                return true;
            }
        }
        catch
        {
            TravelTotemsPlugin.TravelTotemsLogger.LogWarning("Multiplayer Tweaks version mismatch! Update or contact the author for more help.");
        }

        return false;
    }

    private static Vector3 GetClosestTravelLocation(Vector3 pos, float radius, out bool isTotem)
    {
        Vector3 closestTeleport = Vector3.zero;
        float minDistance = 999999f;
        foreach (Minimap.PinData pin in Minimap.instance.m_pins)
        {
            if (AllowedPinType((int)pin.m_type) &&
                !((!pin.m_uiElement || !pin.m_uiElement.gameObject.activeInHierarchy)))
            {
                float distance = Utils.DistanceXZ(pos, pin.m_pos);
                if (distance < radius && (distance < minDistance || closestTeleport == Vector3.zero))
                {
                    closestTeleport = pin.m_pos;
                    minDistance = distance;
                    isTotem = pin.m_type == TravelTotemMapPin.GetTotemPinType();
                }
            }
        }

        isTotem = false;

        if (LocationMapPins.Count > 0)
        {
            foreach (Vector3 location in LocationMapPins)
            {
                float distance = Utils.DistanceXZ(pos, location);
                if (distance < radius && (distance < minDistance || closestTeleport == null))
                {
                    closestTeleport = location;
                    minDistance = distance;
                }
            }
        }

        return closestTeleport;
    }

    public static void HandleTotemOnTriggerEnter(TravelTotemPortal totem = null)
    {
        if (TotemTeleportActive)
        {
            // Player has just finished teleporting and triggered another totem enter.
            SetTotemTeleportActive(false);
            StopTotemMap();
        }
        else
        {
            if (totem && !totem.TryTrackTotem(false))
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_blocked");
                return;
            }

            if (Player.m_localPlayer.IsTeleportable())
            {
                StartTotemMap();
            }
            else
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_noteleport");
            }
        }
    }

    public static void HandleTotemOnTriggerExit(TravelTotemPortal totem = null)
    {
        StopTotemMap();
    }

    public static void StartTotemMap()
    {
        RefreshLocationIcons();
        SetTravelMapActive(true);
        SetMapMode(Minimap.MapMode.Large);
    }

    public static void StopTotemMap()
    {
        SetTravelMapActive(false);
        Minimap.MapMode mode = Game.m_noMap ? Minimap.MapMode.None : Minimap.MapMode.Small;
        SetMapMode(mode);
    }

    /// <summary>
    /// Bypass no map mode and set map.
    /// </summary>
    /// <param name="mode"></param>
    private static void SetMapMode(Minimap.MapMode mode)
    {
        bool nomap = Game.m_noMap;
        Game.m_noMap = false;
        Minimap.instance.SetMapMode(mode);
        Game.m_noMap = nomap;
    }

    public static void RefreshLocationIcons()
    {
        Dictionary<Vector3, string> icons = new Dictionary<Vector3, string>();
        ZoneSystem.instance.GetLocationIcons(icons);
        LocationMapPins.Clear();
        foreach (KeyValuePair<Vector3, string> item in icons)
        {
            switch (item.Value)
            {
                case START_LOC:
                    if (TravelTotemsPlugin.GetAllowSelectDefaultSpawn())
                    {
                        LocationMapPins.Add(item.Key);
                    }
                    break;
                case HALDOR_LOC:
                case HILDIR_LOC:
                case BOGWITCH_LOC:
                    if (TravelTotemsPlugin.GetAllowSelectTraders())
                    {
                        LocationMapPins.Add(item.Key);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdateDynamicPins))]
    public static class Patch_Minimap_UpdateDynamicPins
    {
        private static void Postfix()
        {
            UpdateMapPins();
        }
    }

    /// <summary>
    /// UpdateMap patch for gamepad
    /// </summary>
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdateMap))]
    private static class Patch_Minimap_UpdateMap
    {
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> MinimapUpdateMapTranspiler(
            IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions, generator)
                .Start()
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldstr, "JoyTabLeft"),
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ZInput), nameof(ZInput.GetButtonDown))))
                .ThrowIfInvalid($"Could not patch Minimap.UpdateMap!")
                .Advance(offset: 3)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TravelTotemMap), nameof(TryTriggerTeleportGamepad))))
                .InstructionEnumeration();
        }
    }

    /// <summary>
    /// UpdateMap patch for keyboard
    /// </summary>
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.OnMapLeftClick))]
    private static class Patch_Minimap_OnMapLeftClick
    {
        private static bool Prefix()
        {
            if (!TravelMapActive)
            {
                return true;
            }

            Vector3 position = Minimap.instance.ScreenToWorldPoint(ZInput.mousePosition);
            Vector3 pinLocation = GetClosestTravelLocation(position,
                Minimap.instance.m_removeRadius * (Minimap.instance.m_largeZoom * 2f), out bool isTotem);

            TryTriggerTeleport(pinLocation, isTotem);
            return false; // Skip vanilla check/uncheck pin
        }
    }
}