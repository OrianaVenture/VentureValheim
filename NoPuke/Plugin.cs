using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace VentureValheim.NoPuke
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class NoPukePlugin : BaseUnityPlugin
    {
        private const string ModName = "NoPuke";
        private const string ModVersion = "0.1.1";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource NoPukeLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);
        }
    }
}