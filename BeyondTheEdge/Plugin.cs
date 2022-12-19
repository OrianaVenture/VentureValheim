using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace VentureValheim.BeyondTheEdge
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class BeyondTheEdgePlugin : BaseUnityPlugin
    {
        private const string ModName = "BeyondTheEdge";
        private const string ModVersion = "0.1.0";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource BeyondTheEdgeLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        public void Awake()
        {
            BeyondTheEdgeLogger.LogInfo("Disabling Edge Of World Kill.");
            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);
        }

        [HarmonyPatch(typeof(Player), nameof(Player.EdgeOfWorldKill))]
        public static class Patch_Player_EdgeOfWorldKill
        {
            private static bool Prefix()
            {
                return false; // Skip this method
            }
        }
    }
}