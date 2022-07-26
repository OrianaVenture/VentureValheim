using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;

namespace VentureValheim.LogoutTweaks
{
    public class LogoutTweaks
    {
        private LogoutTweaks()
        {
            _filepath = "";
            _fileDirectory = "";
        }
        private static readonly LogoutTweaks _instance = new LogoutTweaks();

        public static LogoutTweaks Instance
        {
            get => _instance;
        }

        private const string Rested = "Rested";
        private string _filepath;
        private string _fileDirectory;

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
        }

        [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.SavePlayerData))]
        public static class Patch_PlayerProfile_SavePlayerData
        {
            private static void Postfix(Player player, PlayerProfile __instance)
            {
                LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug("Patch_PlayerProfile_SavePlayerData called.");

                if (_instance._filepath.IsNullOrWhiteSpace())
                {
                    _instance._filepath = __instance.GetPath();
                    _instance._fileDirectory = PlayerProfile.GetCharacterFolderPath(__instance.m_fileSource);
                }

                StatusEffect statusEffect = player.m_seman.GetStatusEffect(Rested);
                if (statusEffect)
                {
                    LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug($"Saving to file: rested bonus is {true}, {statusEffect.m_time} duration");
                    _instance.SaveFile(__instance.m_fileSource, true, statusEffect.m_time);
                }
                else
                {
                    LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug($"Saving to file: rested bonus is {false}, {0f} duration");
                    _instance.SaveFile(__instance.m_fileSource, false, 0f);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.LoadPlayerData))]
        public static class Patch_PlayerProfile_LoadPlayerData
        {
            private static void Postfix(Player player, PlayerProfile __instance)
            {
                LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug("Patch_PlayerProfile_LoadPlayerData called.");
                if (_instance._filepath.IsNullOrWhiteSpace())
                {
                    _instance._filepath = __instance.GetPath();
                    _instance._fileDirectory = PlayerProfile.GetCharacterFolderPath(__instance.m_fileSource);
                }

                var restedBonus = _instance.LoadFile(__instance.m_fileSource);
                LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug($"Found from file: rested bonus is {restedBonus.Item1}, {restedBonus.Item2} duration");
                if (restedBonus.Item1)
                {
                    StatusEffect statusEffect = player.m_seman.GetStatusEffect(Rested);
                    
                    
                    if (statusEffect != null)
                    {
                        statusEffect.m_time = restedBonus.Item2;
                    }
                    else
                    {
                        statusEffect = ObjectDB.instance.GetStatusEffect(Rested);
                        statusEffect.m_time = restedBonus.Item2;
                    }
                    
                    player.m_seman.AddStatusEffect(statusEffect, false);
                    
                    Hud.instance.UpdateStatusEffects(player.m_seman.m_statusEffects);
                    LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug("Adding rested bonus from file.");
                }
            }
        }

        private void SaveFile(FileHelpers.FileSource filesource, bool rested, float time)
        {
            FileHelpers.CheckMove(ref filesource, GetFilePath(_filepath));
            if (!Directory.Exists(_fileDirectory) && filesource != FileHelpers.FileSource.SteamCloud)
            {
                Directory.CreateDirectory(_fileDirectory);
            }

            // Create ZPackage
            ZPackage zPackage = new ZPackage();
            zPackage.Write(rested);
            zPackage.Write(time);

            // Save ZPackage
            FileWriter fileWriter = new FileWriter(GetNewFilePath(_filepath) , FileHelpers.FileHelperType.Binary, filesource);
            byte[] zPackageHash = zPackage.GenerateHash();
            byte[] zPackageArray = zPackage.GetArray();
            fileWriter.m_binary.Write(zPackageArray.Length);
            fileWriter.m_binary.Write(zPackageArray);
            fileWriter.m_binary.Write(zPackageHash.Length);
            fileWriter.m_binary.Write(zPackageHash);
            fileWriter.Finish();
            FileHelpers.ReplaceOldFile(GetFilePath(_filepath), GetNewFilePath(_filepath), GetOldFilePath(_filepath), filesource);
        }

        private (bool rested, float time) LoadFile(FileHelpers.FileSource filesource)
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
                fileReader.Dispose();
                
                return (rested, time);
            }
            catch (Exception e)
            {
                LogoutTweaksPlugin.LogoutTweaksLogger.LogWarning($"Failed to load Source: {filesource}, Path: {GetFilePath(_filepath)}");
                LogoutTweaksPlugin.LogoutTweaksLogger.LogWarning(e);
                fileReader?.Dispose();
                return (false, 0f);
            }
        }
    }
}