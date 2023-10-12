using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace VentureValheim.VentureDebugger
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class VentureDebuggerPlugin : BaseUnityPlugin
    {
        private const string ModName = "VentureDebugger";
        private const string ModVersion = "0.0.2";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource VentureDebuggerLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);
        }
    }
}