using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;

namespace VentureValheim.LogoutTweaks
{
    public class LogoutTweaks
    {
        private LogoutTweaks() {}
        private static readonly LogoutTweaks _instance = new LogoutTweaks();

        public static LogoutTweaks Instance
        {
            get => _instance;
        }

        private bool _rested = false;
        private float _restedTime;
        private const string rested = "Rested";
        private string _filepath = "";
        private string _fileDirectory = "";

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
                
                for (int lcv = 0; lcv < player.m_seman.m_statusEffects.Count; ++lcv)
                {
                    StatusEffect statusEffect = player.m_seman.m_statusEffects[lcv];
                    if (statusEffect.name == rested)
                    {
                        _instance._restedTime = statusEffect.m_time;
                        _instance._rested = true; // Set to true on save
                        LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug("Saved rested bonus for next login.");
                    }
                }
            
                _instance.SaveFile(__instance.m_fileSource);
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

                if (_instance.LoadFile(__instance.m_fileSource) && _instance._rested)
                {
                    var se = player.m_seman.AddStatusEffect(rested, resetTime: false);
                    se.m_time = _instance._restedTime;
                    _instance._restedTime = 0f;
                    _instance._rested = false; // Reset after load
                    
                    Hud.instance.UpdateStatusEffects(new List<StatusEffect> {se});
                    LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug("Adding rested bonus from file.");
                }
            }
        }
        
        private void SaveFile(FileHelpers.FileSource filename)
        {
            FileHelpers.CheckMove(ref filename, GetFilePath(_filepath));
            if (!Directory.Exists(_fileDirectory) && filename != FileHelpers.FileSource.SteamCloud)
            {
                Directory.CreateDirectory(_fileDirectory);
            }
            
            // Create ZPackage
            ZPackage zPackage = new ZPackage();
            zPackage.Write(_rested);
            zPackage.Write(_restedTime);
            
            // Save ZPackage
            FileWriter fileWriter = new FileWriter(GetNewFilePath(_filepath) , FileHelpers.FileHelperType.Binary, filename);
            byte[] zPackageHash = zPackage.GenerateHash();
            byte[] zPackageArray = zPackage.GetArray();
            fileWriter.m_binary.Write(zPackageArray.Length);
            fileWriter.m_binary.Write(zPackageArray);
            fileWriter.m_binary.Write(zPackageHash.Length);
            fileWriter.m_binary.Write(zPackageHash);
            fileWriter.Finish();
            FileHelpers.ReplaceOldFile(GetFilePath(_filepath), GetNewFilePath(_filepath), GetOldFilePath(_filepath), filename);
        }
        
        private bool LoadFile(FileHelpers.FileSource filename)
        {
            FileReader fileReader;
            try
            {
                fileReader = new FileReader(GetFilePath(_filepath), filename);
            }
            catch (Exception ex)
            {
                LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug("  failed to load: " + GetFilePath(_filepath) + " (" + ex.Message + ")");
                return false;
            }
            
            try
            {
                BinaryReader binary = fileReader.m_binary;
                _rested = binary.ReadBoolean();
                _restedTime = binary.ReadSingle();
            }
            catch (Exception)
            {
                LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug($"Source: {filename}, Path: {GetFilePath(_filepath)}");
                fileReader.Dispose();
                return false;
            }
            
            fileReader.Dispose();
            return true;
        
        }
    }
}