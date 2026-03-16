using HarmonyLib;

namespace VentureValheim.MultiplayerTweaks;

public class MapTweaks
{
    [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
    public static class Patch_Player_OnSpawned
    {
        /// <summary>
        /// Set the Player map position as public or private if overridden.
        /// </summary>
        private static void Postfix()
        {
            if (MultiplayerTweaksPlugin.GetOverridePlayerMapPins())
            {
                ZNet.instance.SetPublicReferencePosition(MultiplayerTweaksPlugin.GetForcePlayerMapPinsOn());
            }
        }
    }

    /// <summary>
    /// Force the player's public position on or off based of configs.
    /// </summary>
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.SetPublicReferencePosition))]
    public static class Patch_ZNet_SetPublicReferencePosition
    {
        private static void Prefix(ref bool pub)
        {
            if (MultiplayerTweaksPlugin.GetOverridePlayerMapPins())
            {
                pub = MultiplayerTweaksPlugin.GetForcePlayerMapPinsOn();
            }
        }
    }

    /// <summary>
    /// Prevent the shout world texts list from populating to hide display
    /// when shout pings are disabled.
    /// </summary>
    [HarmonyPatch(typeof(Chat), nameof(Chat.GetShoutWorldTexts))]
    public static class Patch_Chat_GetShoutWorldTexts
    {
        private static bool Prefix()
        {
            if (!MultiplayerTweaksPlugin.GetAllowShoutPings())
            {
                return false; // Skip populating list
            }

            return true;
        }
    }

    /// <summary>
    /// Prevent ability to send map pings when config is enabled.
    /// </summary>
    [HarmonyPatch(typeof(Chat), nameof(Chat.SendPing))]
    public static class Patch_Chat_SendPing
    {
        [HarmonyPriority(Priority.LowerThanNormal)]
        private static bool Prefix(ref bool __runOriginal)
        {
            // Compatibility for other mod patches (like Groups)
            // Might not be needed but putting it in anyway
            if (!MultiplayerTweaksPlugin.GetAllowMapPings() || !__runOriginal)
            {
                return false; // Skip sending ping
            }

            return true;
        }
    }
}
