using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace VentureValheim.LogoutTweaks
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class LogoutTweaksPlugin : BaseUnityPlugin
    {
        private const string ModName = "LogoutTweaks";
        private const string ModVersion = "0.4.1";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource LogoutTweaksLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        public void Awake()
        {
            LogoutTweaksLogger.LogInfo("Initializing LogoutTweaks.");

            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);
        }
    }
}