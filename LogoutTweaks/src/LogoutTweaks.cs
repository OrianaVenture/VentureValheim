using System;
using System.IO;
using BepInEx;
using HarmonyLib;

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

        public void Initialize()
        {
            _filepath = "";
            _fileDirectory = "";
        }

        [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.SavePlayerData))]
        public static class Patch_PlayerProfile_SavePlayerData
        {
            private static void Prefix(Player player, PlayerProfile __instance, out StatusEffect __state)
            {
                LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug("Patch_PlayerProfile_SavePlayerData prefix called.");
                __state = player.m_seman.GetStatusEffect(Rested);
            }

            private static void Postfix(PlayerProfile __instance, StatusEffect __state)
            {
                LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug("Patch_PlayerProfile_SavePlayerData postfix called.");
                if (_instance.SetFilePaths(__instance.m_fileSource, __instance.GetPath()))
                {
                    if (__state)
                    {
                        LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug($"Saving to file: rested bonus is {true}, {__state.m_ttl} total, {__state.m_time} time passed.");
                        _instance.SaveFile(__instance.m_fileSource, true, __state.m_ttl, __state.m_time);
                    }
                    else
                    {
                        LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug($"Saving to file: rested bonus is {false}, {0} total, {0} time passed.");
                        _instance.SaveFile(__instance.m_fileSource, false, 0f, 0f);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.SetLocalPlayer))]
        public static class Patch_Player_SetLocalPlayer
        {
            private static void Postfix(Player __instance)
            {
                LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug("Patch_Player_SetLocalPlayer called.");
                var profile = Game.instance.GetPlayerProfile();
                if (!_instance.SetFilePaths(profile.m_fileSource, profile.GetPath())) return;

                var restedBonus = _instance.LoadFile(profile.m_fileSource);
                LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug(
                    $"Found from file: rested bonus is {restedBonus.rested}, {restedBonus.time} total, {restedBonus.passed} time passed.");
                if (restedBonus.rested)
                {
                    __instance.m_seman.AddStatusEffect(Rested, true);
                    StatusEffect statusEffect = __instance.m_seman.GetStatusEffect(Rested);

                    statusEffect.m_ttl = restedBonus.time;
                    statusEffect.m_time = restedBonus.passed;

                    Hud.instance.UpdateStatusEffects(__instance.m_seman.m_statusEffects);

                    _instance.SaveFile(profile.m_fileSource, false, 0f, 0f);
                    LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug("Added rested bonus from file.");
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        public static class Patch_Player_Awake
        {
            private static void Postfix()
            {
                _instance.Initialize();
            }
        }

        private void SaveFile(FileHelpers.FileSource filesource, bool rested, float time, float passed)
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
            zPackage.Write(passed);

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

        private (bool rested, float time, float passed) LoadFile(FileHelpers.FileSource filesource)
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
                fileReader.Dispose();

                return (rested, time, passed);
            }
            catch (Exception e)
            {
                LogoutTweaksPlugin.LogoutTweaksLogger.LogWarning($"Failed to load Source: {filesource}, Path: {GetFilePath(_filepath)}");
                LogoutTweaksPlugin.LogoutTweaksLogger.LogWarning(e);
                fileReader?.Dispose();
                return (false, 0f, 0f);
            }
        }

        private bool SetFilePaths(FileHelpers.FileSource filesource, string path)
        {
            if (_instance._filepath.IsNullOrWhiteSpace())
            {
                _instance._filepath = path;
                _instance._fileDirectory = PlayerProfile.GetCharacterFolderPath(filesource);
            }

            if (!_instance._filepath.IsNullOrWhiteSpace()) return true;

            LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug($"File paths could not be set.");
            return false;
        }
    }
}