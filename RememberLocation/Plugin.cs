using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.RememberLocation;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class RememberLocationPlugin : BaseUnityPlugin
{
    private const string ModName = "RememberLocation";
    private const string ModVersion = "0.1.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource RememberLocationLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    public void Awake()
    {
        RememberLocationLogger.LogInfo("With a bit of a mind flip you're into the time slip and nothing can ever be the same.");

        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
    }

    [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.SetLogoutPoint))]
    public static class Patch_PlayerProfile_GetLogoutPoint
    {
        private static void Postfix(PlayerProfile __instance, Vector3 point)
        {
            RememberLocationLogger.LogDebug($"Overwriting all world logout points with {point}.");
            point.y = 0; // Set height to 0 to prevent falling to death on spawn
            var currentWorldID = ZNet.instance.GetWorldUID();
            foreach (var worldData in __instance.m_worldData)
            {
                if (worldData.Key != currentWorldID) // Do not override point for current world
                {
                    worldData.Value.m_haveLogoutPoint = true;
                    worldData.Value.m_logoutPoint = point;
                }
            }
        }
    }
}