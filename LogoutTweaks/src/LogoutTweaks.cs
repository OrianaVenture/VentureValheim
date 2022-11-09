using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.LogoutTweaks
{
    public class LogoutTweaks
    {
        private LogoutTweaks()
        {
        }
        private static readonly LogoutTweaks _instance = new LogoutTweaks();

        public static LogoutTweaks Instance
        {
            get => _instance;
        }

        private string _filepath = "";

        private readonly struct FileData
        {
            public List<StatusEffectData> StatusEffects { get; }
            public float Stamina { get; }

            public FileData(float stamina, List<StatusEffectData> effects)
            {
                Stamina = stamina;
                StatusEffects = effects;
            }
        }

        private readonly struct StatusEffectData
        {
            public string Name { get; }
            public float Ttl { get; }
            public float Time { get; }

            public StatusEffectData(string name, float ttl, float time)
            {
                Name = name;
                Ttl = ttl;
                Time = time;
            }
        }

        private string GetNewFilePath(string original)
        {
            return original + ".newextras";
        }

        private string GetOldFilePath(string original)
        {
            return original + ".oldextras";
        }

        private string GetFilePath(string original)
        {
            return original + ".extras";
        }

        public void Initialize()
        {
            _filepath = "";
        }

        private void SaveFile(FileHelpers.FileSource filesource, FileData fileData)
        {
            // Create ZPackage
            ZPackage zPackage = new ZPackage();
            zPackage.Write(fileData.Stamina);
            if (fileData.StatusEffects == null)
            {
                zPackage.Write(0);
            }
            else
            {
                zPackage.Write(fileData.StatusEffects.Count);
                for (int lcv = 0; lcv < fileData.StatusEffects.Count; lcv++)
                {
                    var effect = fileData.StatusEffects[lcv];
                    zPackage.Write(effect.Name);
                    zPackage.Write(effect.Ttl);
                    zPackage.Write(effect.Time);
                }
            }

            // Save ZPackage
            FileWriter fileWriter = new FileWriter(GetNewFilePath(_filepath), FileHelpers.FileHelperType.Binary, filesource);
            byte[] zPackageHash = zPackage.GenerateHash();
            byte[] zPackageArray = zPackage.GetArray();
            fileWriter.m_binary.Write(zPackageArray.Length);
            fileWriter.m_binary.Write(zPackageArray);
            fileWriter.m_binary.Write(zPackageHash.Length);
            fileWriter.m_binary.Write(zPackageHash);
            fileWriter.Finish();
            FileHelpers.ReplaceOldFile(GetFilePath(_filepath), GetNewFilePath(_filepath), GetOldFilePath(_filepath), filesource);
        }

        private FileData LoadFile(FileHelpers.FileSource filesource)
        {
            FileReader? fileReader = null;
            try
            {
                fileReader = new FileReader(GetFilePath(_filepath), filesource);

                byte[] data;

                BinaryReader binary = fileReader.m_binary;
                int count = binary.ReadInt32();
                data = binary.ReadBytes(count);
                int count2 = binary.ReadInt32();
                binary.ReadBytes(count2);

                var package = new ZPackage(data);

                float stamina = package.ReadSingle();
                int totalEffects = package.ReadInt();

                List<StatusEffectData> effects = new List<StatusEffectData>();
                for (int lcv = 0; lcv < totalEffects; lcv++)
                {
                    var name = package.ReadString();
                    var ttl = package.ReadSingle();
                    var time = package.ReadSingle();
                    effects.Add(new StatusEffectData(name, ttl, time));
                }

                fileReader.Dispose();

                return new FileData(stamina, effects);
            }
            catch
            {
                LogoutTweaksPlugin.LogoutTweaksLogger.LogWarning($"Failed to load Source: {filesource}, Path: {GetFilePath(_filepath)}");
                fileReader?.Dispose();
                return new FileData(0f, null);
            }
        }

        /// <summary>
        /// Set the file path if not already defined.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Returns true if the file path is defined.</returns>
        protected bool SetFilePaths(string path)
        {
            if (_filepath.IsNullOrWhiteSpace())
            {
                if (!path.IsNullOrWhiteSpace())
                {
                    _filepath = path;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public static bool IsInTheMainScene()
        {
            return SceneManager.GetActiveScene().name.Equals("main");
        }

        #region Patches

        [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.SavePlayerData))]
        public static class Patch_PlayerProfile_SavePlayerData
        {
            private static void Prefix(Player player, out List<StatusEffect> __state)
            {
                LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug("Grabbing all status effects.");
                __state = player.m_seman.GetStatusEffects(); // Should never be null
            }

            private static void Postfix(Player player, PlayerProfile __instance, List<StatusEffect> __state)
            {
                try
                {
                    if (!Instance.SetFilePaths(__instance.GetPath()))
                    {
                        LogoutTweaksPlugin.LogoutTweaksLogger.LogWarning("No file path was set, cannot save extra data.");
                        return;
                    }

                    float stamina;

                    if (LogoutTweaksPlugin.GetUseStamina())
                    {
                        stamina = player.m_stamina;
                    }
                    else
                    {
                        stamina = player.GetMaxStamina();
                    }

                    LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug($"Stamina found: {stamina}.");

                    List<StatusEffectData> data = new List<StatusEffectData>();

                    if (LogoutTweaksPlugin.GetUseStatusEffects())
                    {

                        if (__state == null)
                        {
                            LogoutTweaksPlugin.LogoutTweaksLogger.LogInfo("Unable to determine status effects, this can indicate a mod conflict.");
                        }
                        else
                        {
                            for (int lcv = 0; lcv < __state.Count; lcv++)
                            {
                                string name = __state[lcv].name;
                                float ttl = __state[lcv].m_ttl;
                                float time = __state[lcv].m_time;

                                LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug($"Status Effect found \"{name}\": {ttl} total, {time} time passed.");
                                StatusEffectData effect = new StatusEffectData(name, ttl, time);
                                data.Add(effect);
                            }
                        }
                    }

                    FileData fileData = new FileData(stamina, data);
                    Instance.SaveFile(__instance.m_fileSource, fileData);
                }
                catch (Exception e)
                {
                    LogoutTweaksPlugin.LogoutTweaksLogger.LogError("Error trying to parse and save extra data to file.");
                    LogoutTweaksPlugin.LogoutTweaksLogger.LogError(e);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.LoadPlayerData))]
        public static class Patch_PlayerProfile_LoadPlayerData
        {
            private static void Postfix(Player player, PlayerProfile __instance)
            {
                try
                {
                    if (!IsInTheMainScene())
                    {
                        return;
                    }

                    if (!Instance.SetFilePaths(__instance.GetPath()))
                    {
                        LogoutTweaksPlugin.LogoutTweaksLogger.LogWarning("No file path was set, cannot load extra data.");
                        return;
                    }


                    var fileData = Instance.LoadFile(__instance.m_fileSource);

                    if (LogoutTweaksPlugin.GetUseStamina())
                    {
                        player.m_staminaRegenTimer = 5f;
                        player.m_stamina = Mathf.Clamp(fileData.Stamina, 0f, player.m_maxStamina);
                        LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug($"Stamina found in file: {fileData.Stamina}.");
                    }

                    if (LogoutTweaksPlugin.GetUseStatusEffects())
                    {
                        var effects = fileData.StatusEffects;
                        for (int lcv = 0; lcv < effects.Count; lcv++)
                        {
                            string name = effects[lcv].Name;
                            float ttl = effects[lcv].Ttl;
                            float time = effects[lcv].Time;

                            LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug($"Status Effect found in file \"{name}\": {ttl} total, {time} time passed.");
                            
                            player.m_seman.AddStatusEffect(name);
                            StatusEffect statusEffect = player.m_seman.GetStatusEffect(name);
                            if (statusEffect != null)
                            {
                                statusEffect.m_ttl = ttl;
                                statusEffect.m_time = time;
                            }
                            else
                            {
                                LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug($"Status Effect {name} could not be initialized.");
                            }
                        }

                        Hud.instance.UpdateStatusEffects(player.m_seman.m_statusEffects);
                    }

                    // Wipe data after loading
                    Instance.SaveFile(__instance.m_fileSource, new FileData(0f, null));
                }
                catch (Exception e)
                {
                    LogoutTweaksPlugin.LogoutTweaksLogger.LogError("Error loading extra data from file.");
                    LogoutTweaksPlugin.LogoutTweaksLogger.LogError(e);
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        public static class Patch_Player_Awake
        {
            private static void Postfix()
            {
                Instance.Initialize();
            }
        }

        #endregion
    }
}