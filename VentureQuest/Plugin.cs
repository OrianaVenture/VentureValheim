using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace VentureValheim.VentureQuest
{
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class VentureQuestPlugin : BaseUnityPlugin
    {
        private const string ModName = "VentureQuest";
        private const string ModVersion = "0.1.0";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource VentureQuestLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        public static GameObject Root;
        public static AssetBundle Assets;

        #region ConfigurationEntries

        public static ConfigEntry<bool> CE_ExampleConfig = null!;

        private readonly ConfigurationManagerAttributes AdminConfig = new ConfigurationManagerAttributes { IsAdminOnly = true };
        private readonly ConfigurationManagerAttributes ClientConfig = new ConfigurationManagerAttributes { IsAdminOnly = false };

        private void AddConfig<T>(string key, string section, string description, bool synced, T value, ref ConfigEntry<T> configEntry)
        {
            string extendedDescription = GetExtendedDescription(description, synced);
            configEntry = Config.Bind(section, key, value,
                new ConfigDescription(extendedDescription, null, synced ? AdminConfig : ClientConfig));
        }

        public string GetExtendedDescription(string description, bool synchronizedSetting)
        {
            return description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]");
        }

        #endregion

        public void Awake()
        {
            #region Configuration

            const string general = "General";

            AddConfig("Example Config", general, "Example (boolean).",
                true, true, ref CE_ExampleConfig);

            #endregion

            VentureQuestLogger.LogInfo("Initializing VentureQuest configurations...");

            // Create a dummy root object to reference
            Root = new GameObject("VentureQuestRoot");
            Root.SetActive(false);
            DontDestroyOnLoad(Root);

            /*Assets = AssetUtils.LoadAssetBundleFromResources("vv_quest", Assembly.GetExecutingAssembly());
            var go = Assets.LoadAsset<GameObject>(NPC.NPC_NAME);*/

            PrefabManager.OnPrefabsRegistered += NPCFactory.AddNPCS;

            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);
            SetupWatcher();
        }

        private void OnDestroy()
        {
            Config.Save();
        }
        
        private void SetupWatcher()
        {
            _lastReloadTime = DateTime.Now;
            FileSystemWatcher watcher = new(BepInEx.Paths.ConfigPath, ConfigFileName);
            // Due to limitations of technology this can trigger twice in a row
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private DateTime _lastReloadTime;
        private const long RELOAD_DELAY = 10000000; // One second

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            var now = DateTime.Now;
            var time = now.Ticks - _lastReloadTime.Ticks;
            if (!File.Exists(ConfigFileFullPath) || time < RELOAD_DELAY) return;

            try
            {
                VentureQuestLogger.LogInfo("Attempting to reload configuration...");
                Config.Reload();
            }
            catch
            {
                VentureQuestLogger.LogError($"There was an issue loading {ConfigFileName}");
                return;
            }

            _lastReloadTime = now;

            if (ZNet.instance != null && !ZNet.instance.IsDedicated())
            {
                // Update configurations
            }
        }
    }
}