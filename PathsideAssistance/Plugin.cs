using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.PathsideAssistance
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class PathsideAssistancePlugin : BaseUnityPlugin
    {
        private const string ModName = "PathsideAssistance";
        private const string ModVersion = "0.2.1";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource PathsideAssistanceLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        public static GameObject Root;

        public void Awake()
        {
            PathsideAssistanceLogger.LogInfo("Thank you for your call, help is on the way.");

            // Create a dummy root object to reference
            Root = new GameObject("PathsideRoot");
            Root.SetActive(false);
            DontDestroyOnLoad(Root);

            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);
        }
    }
}