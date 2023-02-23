using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace VentureValheim.DeluxeParticles
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class DeluxeParticlesPlugin : BaseUnityPlugin
    {
        private const string ModName = "DeluxeParticles";
        private const string ModVersion = "0.1.0";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource DeluxeParticlesLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        public void Awake()
        {
            DeluxeParticlesLogger.LogInfo("Initializing DeluxeParticles!");

            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);
        }
    }
}