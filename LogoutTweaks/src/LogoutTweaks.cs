using System;
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

        private const string Rested = "Rested";
        private string _filepath = "";

        private readonly struct FileData
        {
            public bool IsRested { get; }
            public float RestedTime { get; }
            public float RestedTimePassed { get; }
            public float Stamina { get; }

            public FileData(bool rested, float time, float passed, float stamina)
            {
                IsRested = rested;
                RestedTime = time;
                RestedTimePassed = passed;
                Stamina = stamina;
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

        [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.SavePlayerData))]
        public static class Patch_PlayerProfile_SavePlayerData
        {
            private static void Prefix(Player player, out StatusEffect __state)
            {
                LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug("Patch_PlayerProfile_SavePlayerData prefix called.");
                __state = player.m_seman.GetStatusEffect(Rested);
            }

            private static void Postfix(Player player, PlayerProfile __instance, StatusEffect __state)
            {
                try
                {
                    LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug("Patch_PlayerProfile_SavePlayerData postfix called.");
                    if (Instance.SetFilePaths(__instance.m_fileSource, __instance.GetPath()))
                    {
                        if (__state)
                        {
                            LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug($"Saving to file: rested bonus is {true}, {__state.m_ttl} total, {__state.m_time} time passed. Stamina: {player.m_stamina}");
                            FileData fileData = new FileData(true, __state.m_ttl, __state.m_time, player.m_stamina);
                            Instance.SaveFile(__instance.m_fileSource, fileData);
                        }
                        else
                        {
                            LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug($"Saving to file: rested bonus is {false}, {0} total, {0} time passed. Stamina: {0}");
                            FileData fileData = new FileData(false, 0f, 0f, 0f);
                            Instance.SaveFile(__instance.m_fileSource, fileData);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogoutTweaksPlugin.LogoutTweaksLogger.LogError("Error saving extra data from file.");
                    LogoutTweaksPlugin.LogoutTweaksLogger.LogError(e);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.LoadPlayerData))]
        public static class Patch_PlayerProfile_LoadPlayerData
        { 
            private static void Postfix(Player player)
            {
                if (!SceneManager.GetActiveScene().name.Equals("main") || !(LogoutTweaksPlugin.GetUseRestedBonus() || LogoutTweaksPlugin.GetUseStamina()))
                {
                    return;
                }

                try
                {
                    var profile = Game.instance.GetPlayerProfile();
                    if (!Instance.SetFilePaths(profile.m_fileSource, profile.GetPath())) return;

                    var fileData = Instance.LoadFile(profile.m_fileSource);
                    LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug(
                        $"Loaded file. Rested bonus is {fileData.IsRested}, {fileData.RestedTime} time total, {fileData.RestedTimePassed} time passed. " +
                        $"Stamina: {fileData.Stamina}");

                    if (LogoutTweaksPlugin.GetUseRestedBonus() && fileData.IsRested)
                    {
                        player.m_seman.AddStatusEffect(Rested, true);
                        StatusEffect statusEffect = player.m_seman.GetStatusEffect(Rested);

                        statusEffect.m_ttl = fileData.RestedTime;
                        statusEffect.m_time = fileData.RestedTimePassed;

                        Hud.instance.UpdateStatusEffects(player.m_seman.m_statusEffects);

                        LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug("Added rested bonus from file.");
                    }

                    if (LogoutTweaksPlugin.GetUseStamina())
                    {
                        player.m_stamina = Mathf.Clamp(fileData.Stamina, 0f, player.m_maxStamina);
                        player.m_staminaRegenTimer = 5f;
                    }

                    FileData wipe = new FileData(false, 0f, 0f, 0f);
                    Instance.SaveFile(profile.m_fileSource, wipe);
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

        private void SaveFile(FileHelpers.FileSource filesource, FileData fileData)
        {
            if (Directory.Exists(_filepath))
            {
                return;
            }

            // Create ZPackage
            ZPackage zPackage = new ZPackage();
            zPackage.Write(fileData.IsRested);
            zPackage.Write(fileData.RestedTime);
            zPackage.Write(fileData.RestedTimePassed);
            zPackage.Write(fileData.Stamina);

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

                bool rested = package.ReadBool();
                float time = package.ReadSingle();
                float passed = package.ReadSingle();
                float stamina = package.ReadSingle();
                fileReader.Dispose();

                return new FileData(rested, time, passed, stamina);
            }
            catch
            {
                LogoutTweaksPlugin.LogoutTweaksLogger.LogWarning($"Failed to load Source: {filesource}, Path: {GetFilePath(_filepath)}");
                fileReader?.Dispose();
                return new FileData(false, 0f, 0f, 0f);
            }
        }

        /// <summary>
        /// Set the file path if defined.
        /// </summary>
        /// <param name="filesource"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool SetFilePaths(FileHelpers.FileSource filesource, string path)
        {
            if (Instance._filepath.IsNullOrWhiteSpace())
            {
                Instance._filepath = path;
            }

            if (!Instance._filepath.IsNullOrWhiteSpace()) return true;

            LogoutTweaksPlugin.LogoutTweaksLogger.LogWarning($"File paths could not be set for FileSource: {filesource}, data may not load correctly.");
            return false;
        }
    }
}