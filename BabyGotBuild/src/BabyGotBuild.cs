using BepInEx;
using HarmonyLib;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;

namespace VentureValheim.BabyGotBuild;

public static class BabyGotBuild
{
    private static PlayerProfile _profile;

    private static string _filepath;

    private static string GetFilePath(string original)
    {
        return original + ".extras";
    }

    private static string GetNewFilePath(string original)
    {
        return original + ".newextras";
    }

    private static string GetOldFilePath(string original)
    {
        return original + ".oldextras";
    }

    private static void SaveFile(FileHelpers.FileSource filesource)
    {
        // Create ZPackage
        FavoritePieceList pieces = Hud.instance?.m_buildUi.m_favoritePieceList;
        if (pieces == null)
        {
            BabyGotBuildPlugin.BabyGotBuildLogger.LogWarning("Favorite Pieces could not be found, cannot save.");
            return;
        }

        SetFilePath(filesource);

        ZPackage zPackage = new ZPackage();
        UTF8Encoding textEncoding = new UTF8Encoding();
        Hud.instance.m_buildUi.m_recentPieceList.Serialize(zPackage, textEncoding);
        Hud.instance.m_buildUi.m_favoritePieceList.Serialize(zPackage, textEncoding);

        // Save ZPackage
        FileWriter fileWriter = new FileWriter(GetNewFilePath(_filepath),
            Splatform.CloudStorageFileGrouping.SameFolder, FileHelpers.FileHelperType.Binary, filesource);
        byte[] zPackageHash = zPackage.GenerateHash();
        byte[] zPackageArray = zPackage.GetArray();
        fileWriter.m_binary.Write(zPackageArray.Length);
        fileWriter.m_binary.Write(zPackageArray);
        fileWriter.m_binary.Write(zPackageHash.Length);
        fileWriter.m_binary.Write(zPackageHash);


        fileWriter.Finish();
        FileHelpers.ReplaceOldFile(GetFilePath(_filepath), GetNewFilePath(_filepath), GetOldFilePath(_filepath),
            Splatform.CloudStorageFileGrouping.SameFolder, filesource);
    }

    private static void LoadFile(FileHelpers.FileSource filesource)
    {
        FavoritePieceList pieces = Hud.instance?.m_buildUi.m_favoritePieceList;

        if (pieces == null)
        {
            BabyGotBuildPlugin.BabyGotBuildLogger.LogWarning("Favorite Pieces could not be found, cannot load.");
            return;
        }
        else
        {
            // TODO: potentially grab exisitng favorites and merge by a config?
        }

        SetFilePath(filesource);

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

            Hud.instance.m_buildUi.LoadFromBinary(data);

            fileReader.Dispose();
        }
        catch
        {
            BabyGotBuildPlugin.BabyGotBuildLogger.LogWarning($"Failed to load Source: {filesource}, Path: {GetFilePath(_filepath)}");
            fileReader?.Dispose();
        }
    }

    public static bool IsInTheMainScene()
    {
        return SceneManager.GetActiveScene().name.Equals("main");
    }

    private static void SetFilePath(FileHelpers.FileSource filesource)
    {
        if (_filepath.IsNullOrWhiteSpace())
        {
            _filepath = SaveSystem.GetCharacterFolderPath(filesource) + "BabyGotBuildData";
        }
    }

    [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.Load))]
    public static class Patch_PlayerProfile_Load
    {
        private static void Prefix(PlayerProfile __instance)
        {
            _profile = __instance;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Load))]
    public static class Patch_Player_Load
    {
        private static void Postfix()
        {
            if (!IsInTheMainScene() || _profile == null)
            {
                return;
            }

            LoadFile(_profile.m_fileSource);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Save))]
    public static class Patch_Player_Save
    {
        private static void Prefix()
        {
            if (!IsInTheMainScene() || _profile == null)
            {
                return;
            }

            SaveFile(_profile.m_fileSource);
        }
    }
}