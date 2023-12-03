using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace VentureValheim.NoSeasonalRestrictions
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class NoSeasonalRestrictionsPlugin : BaseUnityPlugin
    {
        private const string ModName = "NoSeasonalRestrictions";
        private const string ModVersion = "0.1.7";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource NoSeasonalRestrictionsLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        public void Awake()
        {
            NoSeasonalRestrictionsLogger.LogInfo("The weather is hot and snowy!");

            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);
        }
    }
}