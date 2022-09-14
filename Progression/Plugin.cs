﻿using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;

namespace VentureValheim.Progression
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class ProgressionPlugin : BaseUnityPlugin
    {
        private const string ModName = "WorldAdvancementProgression";
        private const string ModVersion = "0.0.9";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        private readonly ManualLogSource VentureProgressionLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        public static ManualLogSource GetProgressionLogger() => Instance.VentureProgressionLogger;

        private ProgressionPlugin() { }
        private static readonly ProgressionPlugin _instance = new ProgressionPlugin();

        public static ProgressionPlugin Instance
        {
            get => _instance;
        }

        #region ConfigurationEntries

        private static readonly ConfigSync ConfigurationSync = new(ModGUID)
        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        private ConfigEntry<bool> CE_ServerConfigLocked = null!;

        private static ConfigEntry<bool> CE_ModEnabled = null!;

        // Progression Manager
        private static ConfigEntry<bool> CE_BlockAllGlobalKeys = null!;
        private static ConfigEntry<string> CE_BlockedGlobalKeys = null!;
        private static ConfigEntry<string> CE_AllowedGlobalKeys = null!;

        public virtual bool GetBlockAllGlobalKeys() => CE_BlockAllGlobalKeys.Value;
        public virtual string GetBlockedGlobalKeys() => CE_BlockedGlobalKeys.Value;
        public virtual string GetAllowedGlobalKeys() => CE_AllowedGlobalKeys.Value;

        // Skills Manager
        private static ConfigEntry<bool> CE_EnableSkillManager = null!;
        private static ConfigEntry<bool> CE_AllowSkillDrain = null!;
        private static ConfigEntry<bool> CE_UseAbsoluteSkillDrain = null!;
        private static ConfigEntry<int> CE_AbsoluteSkillDrain = null!;
        private static ConfigEntry<bool> CE_CompareAndSelectDrain = null!;
        private static ConfigEntry<bool> CE_CompareUseMinimumDrain = null!;
        private static ConfigEntry<bool> CE_OverrideMaximumSkillLevel = null!;
        private static ConfigEntry<int> CE_MaximumSkillLevel = null!;
        private static ConfigEntry<bool> CE_OverrideMinimumSkillLevel = null!;
        private static ConfigEntry<int> CE_MinimumSkillLevel = null!;

        public virtual bool GetEnableSkillManager() => CE_EnableSkillManager.Value;
        public virtual bool GetAllowSkillDrain() => CE_AllowSkillDrain.Value;
        public virtual bool GetUseAbsoluteSkillDrain() => CE_UseAbsoluteSkillDrain.Value;
        public virtual int GetAbsoluteSkillDrain() => CE_AbsoluteSkillDrain.Value;
        public virtual bool GetCompareAndSelectDrain() => CE_CompareAndSelectDrain.Value;
        public virtual bool GetCompareUseMinimumDrain() => CE_CompareUseMinimumDrain.Value;
        public virtual bool GetOverrideMaximumSkillLevel() => CE_OverrideMaximumSkillLevel.Value;
        public virtual int GetMaximumSkillLevel() => CE_MaximumSkillLevel.Value;
        public virtual bool GetOverrideMinimumSkillLevel() => CE_OverrideMinimumSkillLevel.Value;
        public virtual int GetMinimumSkillLevel() => CE_MinimumSkillLevel.Value;

        // Auto-Scaling Configuration
        private static ConfigEntry<bool> CE_AutoScaling = null!;
        private static ConfigEntry<string> CE_AutoScaleType = null!;
        private static ConfigEntry<float> CE_AutoScaleFactor = null!;
        private static ConfigEntry<bool> CE_AutoScaleCreatures = null!;
        private static ConfigEntry<string> CE_AutoScaleCreatureHealth = null!;
        private static ConfigEntry<string> CE_AutoScaleCreatureDamage = null!;
        private static ConfigEntry<bool> CE_AutoScaleItems = null!;

        public virtual bool GetUseAutoScaling() => CE_AutoScaling.Value;
        public virtual string GetAutoScaleType() => CE_AutoScaleType.Value;
        public virtual float GetAutoScaleFactor() => CE_AutoScaleFactor.Value;
        public virtual bool GetAutoScaleCreatures() => CE_AutoScaleCreatures.Value;
        public virtual string GetAutoScaleCreatureHealth() => CE_AutoScaleCreatureHealth.Value;
        public virtual string GetAutoScaleCreatureDamage() => CE_AutoScaleCreatureDamage.Value;
        public virtual bool GetAutoScaleItems() => CE_AutoScaleItems.Value;

        private void AddConfig<T>(string key, string section, string description, bool synced, T value, ref ConfigEntry<T> configEntry)
        {
            string extendedDescription = GetExtendedDescription(description, synced);
            configEntry = Config.Bind(section, key, value, extendedDescription);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigurationSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synced;
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
            const string skills = "Skills";
            const string autoScaling = "Auto-Scaling";

            AddConfig("Force Server Config", general, "Force Server Config (boolean)",
                true, true, ref CE_ServerConfigLocked);
            AddConfig("Enabled", general, "Enable module (boolean).",
                true, true, ref CE_ModEnabled);

            AddConfig("BlockAllGlobalKeys", general,
                "True to stop all global keys from being added to the global list (boolean).",
                true, true, ref CE_BlockAllGlobalKeys);
            AddConfig("BlockedGlobalKeys", general,
                "Stop only these keys being added to the global list when BlockAllGlobalKeys is false (comma-separated).",
                true, "", ref CE_BlockedGlobalKeys);
            AddConfig("AllowedGlobalKeys", general,
                "Allow only these keys being added to the global list when BlockAllGlobalKeys is true (comma-separated).",
                true, "", ref CE_AllowedGlobalKeys);

            AddConfig("EnableSkillManager", skills,
                "Enable the Skill Manager feature (boolean).",
                true, true, ref CE_EnableSkillManager);
            AddConfig("AllowSkillDrain", skills,
                "Enable skill drain on death (boolean).",
                true, true, ref CE_AllowSkillDrain);
            AddConfig("UseAbsoluteSkillDrain", skills,
                "Reduce skills by a set number of levels (boolean).",
                true, false, ref CE_UseAbsoluteSkillDrain);
            AddConfig("AbsoluteSkillDrain", skills,
                "The number of levels (When UseAbsoluteSkillDrain is true) (int).",
                true, 1, ref CE_AbsoluteSkillDrain);
            AddConfig("CompareAndSelectDrain", skills,
                "Enable comparing skill drain original vs absolute value (When UseAbsoluteSkillDrain is true) (boolean).",
                true, false, ref CE_CompareAndSelectDrain);
            AddConfig("CompareUseMinimumDrain", skills,
                "True to use the smaller value (When CompareAndSelectDrain is true) (boolean).",
                true, true, ref CE_CompareUseMinimumDrain);
            AddConfig("OverrideMaximumSkillLevel", skills,
                "Override the maximum (ceiling) skill level for all skill gain (boolean).",
                true, false, ref CE_OverrideMaximumSkillLevel);
            AddConfig("MaximumSkillLevel", skills,
                "If overridden, the maximum (ceiling) skill level for all skill gain (int).",
                true, (int)SkillsManager.SKILL_MAXIMUM, ref CE_MaximumSkillLevel);
            AddConfig("OverrideMinimumSkillLevel", skills,
                "Override the minimum (floor) skill level for all skill loss (boolean).",
                true, false, ref CE_OverrideMinimumSkillLevel);
            AddConfig("MinimumSkillLevel", skills,
                "If overridden, the minimum (floor) skill level for all skill loss (int).",
                true, (int)SkillsManager.SKILL_MINIMUM, ref CE_MinimumSkillLevel);

            AddConfig("AutoScale", autoScaling,
                "Use Auto-scaling (boolean).",
                true, false, ref CE_AutoScaling);
            AddConfig("AutoScaleType", autoScaling,
                "Auto-scaling type: Vanilla, Linear, or Exponential (string).",
                true, "Vanilla", ref CE_AutoScaleType);
            AddConfig("AutoScaleFactor", autoScaling,
                "Auto-scaling factor, 0.75 = 75% growth per biome \"difficulty order\" (float).",
                true, 0.75f, ref CE_AutoScaleFactor);
            AddConfig("AutoScaleCreatures", autoScaling,
                "Auto-scale Creatures (boolean).",
                true, true, ref CE_AutoScaleCreatures);
            AddConfig("AutoScaleCreaturesHealth", autoScaling,
                "Override the Base Health distribution for Creatures (comma-separated list of 6 integers) (string).",
                true, "", ref CE_AutoScaleCreatureHealth);
            AddConfig("AutoScaleCreaturesDamage", autoScaling,
                "Override the Base Damage distribution for Creatures (comma-separated list of 6 integers) (string).",
                true, "", ref CE_AutoScaleCreatureDamage);
            AddConfig("AutoScaleItems", autoScaling,
                "Auto-scale Items (boolean).",
                true, true, ref CE_AutoScaleItems);

            #endregion

            if (!CE_ModEnabled.Value)
                return;

            if (!ConfigureAllModules())
            {
                return;
            }

            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);
            SetupWatcher();
        }

        public bool ConfigureAllModules()
        {
            GetProgressionLogger().LogInfo("Initializing Progression configurations...");

            #region Progression Manager
            try
            {
                ProgressionManager.Instance.Initialize(GetBlockedGlobalKeys(), GetAllowedGlobalKeys());
                GetProgressionLogger().LogInfo(GetBlockAllGlobalKeys()
                    ? $"Blocking all keys except {ProgressionManager.AllowedGlobalKeysList?.Count ?? 0} globally allowed keys.."
                    : $"Allowing all keys except {ProgressionManager.BlockedGlobalKeysList?.Count ?? 0} globally blocked keys..");
            }
            catch (Exception e)
            {
                GetProgressionLogger().LogError("Error configuring ProgressionManager, aborting...");
                GetProgressionLogger().LogError(e);
                return false;
            }
            #endregion

            #region Skills Manager
            try
            {
                GetProgressionLogger().LogInfo($"Skill Manager is " + (GetEnableSkillManager() == true ? "Enabled" : "Disabled") + ".");
                if (GetEnableSkillManager())
                {
                    SkillsManager.Instance.Initialize();
                    GetProgressionLogger().LogDebug($"Skill loss is {GetAllowSkillDrain()}.");
                    GetProgressionLogger().LogDebug($"Maximum level for gain is " + (GetOverrideMaximumSkillLevel() ? GetMaximumSkillLevel() : SkillsManager.SKILL_MAXIMUM) + ".");
                    GetProgressionLogger().LogDebug($"Minimum level for loss is " + (GetOverrideMinimumSkillLevel() ? GetMinimumSkillLevel() : SkillsManager.SKILL_MINIMUM) + ".");
                }
            }
            catch (Exception e)
            {
                GetProgressionLogger().LogError("Error configuring SkillsManager, aborting...");
                GetProgressionLogger().LogError(e);
                return false;
            }
            #endregion

            #region Auto-Scaling
            try
            {
                if (GetUseAutoScaling())
                {
                    float factor = GetAutoScaleFactor();
                    var scale = WorldConfiguration.Scaling.Vanilla;
                    if (GetAutoScaleType().ToLower().Equals("exponential"))
                    {
                        scale = WorldConfiguration.Scaling.Exponential;
                    }
                    else if (GetAutoScaleType().ToLower().Equals("linear"))
                    {
                        scale = WorldConfiguration.Scaling.Linear;
                    }

                    GetProgressionLogger().LogDebug($"WorldConfiguration Initializing with scale: {scale}, factor: {factor}.");
                    WorldConfiguration.Instance.Initialize(scale, factor);

                    if (GetAutoScaleCreatures())
                    {
                        var healthString = GetAutoScaleCreatureHealth();
                        if (!healthString.IsNullOrWhiteSpace())
                        {
                            try
                            {
                                var list = healthString.Split(',');
                                var copy = new int[list.Length];
                                for (var lcv = 0; lcv < list.Length; lcv++)
                                {
                                    copy[lcv] = int.Parse(list[lcv].Trim());
                                }

                                CreatureConfiguration.Instance.SetBaseHealth(copy);
                            }
                            catch
                            {
                                GetProgressionLogger().LogWarning("Issue parsing Creature Health configuration, using defaults.");
                            }
                        }

                        var damageString = GetAutoScaleCreatureDamage();
                        if (!damageString.IsNullOrWhiteSpace())
                        {
                            try
                            {
                                var list = damageString.Split(',');
                                var copy = new int[list.Length];
                                for (var lcv = 0; lcv < list.Length; lcv++)
                                {
                                    copy[lcv] = int.Parse(list[lcv].Trim());
                                }

                                CreatureConfiguration.Instance.SetBaseDamage(copy);
                            }
                            catch
                            {
                                GetProgressionLogger().LogWarning("Issue parsing Creature Damage configuration, using defaults.");
                            }
                        }

                        CreatureConfiguration.Instance.Initialize();
                    }

                    if (GetAutoScaleItems())
                    {
                        ItemConfiguration.Instance.Initialize();
                    }
                }
            }
            catch (Exception e)
            {
                GetProgressionLogger().LogError("Error configuring Auto-Scaling features, aborting...");
                GetProgressionLogger().LogError(e);
                return false;
            }
            #endregion

            return true;
        }

        private void OnDestroy()
        {
            Config.Save();
            HarmonyInstance.UnpatchSelf();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                GetProgressionLogger().LogDebug("Attempting to reload configuration...");
                Config.Reload();
            }
            catch
            {
                GetProgressionLogger().LogError($"There was an issue loading {ConfigFileName}");
            }
        }
    }
}