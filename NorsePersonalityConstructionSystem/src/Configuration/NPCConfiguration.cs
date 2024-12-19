using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VentureValheim.NPCS;

public static class NPCConfiguration
{
    // TODO: Data migration
    // Replace all {reward} and {giveitem} with the actual names
    // 

    private static readonly string FileName = "VV.NPCS.yaml";
    private static Dictionary<string, NPCConfig> Configurations;

    public static List<string> ReplaceReservedCharacters(List<string> texts)
    {
        for (int lcv = 0; lcv < texts.Count; lcv++)
        {
            texts[lcv] = ReplaceReservedCharacters(texts[lcv]);
        }

        return texts;
    }

    public static string ReplaceReservedCharacters(string text)
    {
        text.Replace('`', '\'');
        text.Replace('|', '/');
        return text;
    }

    [Serializable]
    public class NPCS
    {
        public List<NPCConfig> Npcs;
    }

    public static NPCConfig GetConfig(string id)
    {
        if (Configurations == null)
        {
            ReloadFile();
        }

        id = id.ToLower();

        if (Configurations.ContainsKey(id))
        {
            var cleaned = Configurations[id];
            cleaned.CleanData();

            return cleaned;
        }

        return null;
    }

    public static void ReloadFile()
    {
        Configurations = new Dictionary<string, NPCConfig>();
        var list = ReadFile();
        if (list == null)
        {
            return;
        }

        for (int lcv = 0; lcv < list.Npcs.Count; lcv++)
        {
            var config = list.Npcs[lcv];

            Configurations.Add(config.Id.ToLower(), config);
        }
    }

    public static NPCS ReadFile()
    {
        var filePath = Paths.ConfigPath + Path.DirectorySeparatorChar + FileName;
        try
        {
            using var fileReader = new StreamReader(filePath);
            string fileData = File.ReadAllText(filePath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return deserializer.Deserialize<NPCS>(fileData);
        }
        catch (Exception e)
        {
            NPCSPlugin.NPCSLogger.LogError($"Could not read file {FileName}");
            NPCSPlugin.NPCSLogger.LogWarning(e);
        }

        return null;
    }
}