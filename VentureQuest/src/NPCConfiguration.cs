using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VentureValheim.VentureQuest;

public class NPCConfiguration
{
    private static readonly string FileName = "VQ.NPCS.yaml";
    protected static Dictionary<string, NPCConfig> Configurations;
    
    [Serializable]
    public class NPCS
    {
        public List<NPCConfig> npcs;
    }

    [Serializable]
    public class NPCConfig
    {
        // TODO check data types
        public string Id { get; set; }
        public string Name { get; set; }
        public NPC.NPCType Type { get; set; }

        public string DefaultText { get; set; }
        public string RequiredKeys { get; set; }
        public string NotRequiredKeys { get; set; }
        public string InteractKey { get; set; }
        public string DefeatKey { get; set; }
        public bool TrueDeath { get; set; }
        public bool StandStill { get; set; }
        // Interact
        public string InteractText { get; set; }
        // Use Item
        public string GiveItem { get; set; }
        public int? GiveItemAmount { get; set; }
        // Reward
        public string RewardText { get; set; }
        public string RewardItem { get; set; }
        public int? RewardItemAmount { get; set; }
        public string RewardKey { get; set; }
        public int? RewardLimit { get; set; }
        // Style
        public string Model { get; set; }
        public float? HairColorR { get; set; }
        public float? HairColorG { get; set; }
        public float? HairColorB { get; set; }
        public float? SkinColorR { get; set; }
        public float? SkinColorG { get; set; }
        public float? SkinColorB { get; set; }
        public int? ModelIndex { get; set; }
        public string Hair { get; set; }
        public string Beard { get; set; }
        public string Helmet { get; set; }
        public string Chest { get; set; }
        public string Legs { get; set; }
        public string Shoulder { get; set; }
        public string Utility { get; set; }
        public string RightHand { get; set; }
        public string LeftHand { get; set; }
        public string RightBack { get; set; }
        public string LeftBack { get; set; }
    }

    public static NPCConfig GetConfig(string id)
    {
        if (Configurations == null)
        {
            ReloadFile();
        }

        if (Configurations.ContainsKey(id))
        {
            var cleaned = Configurations[id];
            cleaned.Name ??= "";
            cleaned.DefaultText ??= "";
            cleaned.RequiredKeys ??= "";
            cleaned.NotRequiredKeys ??= "";
            cleaned.InteractKey ??= "";
            cleaned.DefeatKey ??= "";
            cleaned.InteractText ??= "";
            cleaned.GiveItem ??= "";
            cleaned.GiveItemAmount ??= 1;
            cleaned.RewardText ??= "";
            cleaned.RewardItem ??= "";
            cleaned.RewardItemAmount ??= 1;
            cleaned.RewardKey ??= "";
            cleaned.RewardLimit ??= -1; // Unlimted
            cleaned.Model ??= "Player";
            cleaned.Hair ??= "";
            cleaned.Beard ??= "";
            cleaned.Helmet ??= "";
            cleaned.Chest ??= "";
            cleaned.Shoulder ??= "";
            cleaned.Utility ??= "";
            cleaned.RightHand ??= "";
            cleaned.LeftHand ??= "";
            cleaned.RightBack ??= "";
            cleaned.LeftBack ??= "";

            return cleaned;
        }

        return null;
    }

    public static void ReloadFile()
    {
        Configurations = new Dictionary<string, NPCConfig>();
        var list = ReadFile();

        for (int lcv = 0; lcv < list.npcs.Count; lcv++)
        {
            var config = list.npcs[lcv];

            Configurations.Add(config.Id, config);
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
            VentureQuestPlugin.VentureQuestLogger.LogError($"Could not read file {FileName}");
            VentureQuestPlugin.VentureQuestLogger.LogWarning(e);
        }

        return null;
    }
}