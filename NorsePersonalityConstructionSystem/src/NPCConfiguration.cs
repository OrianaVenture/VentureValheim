using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VentureValheim.NPCS;

public class NPCConfiguration
{
    private static readonly string FileName = "VV.NPCS.yaml";
    protected static Dictionary<string, NPCConfig> Configurations;
    
    [Serializable]
    public class NPCS
    {
        public List<NPCConfig> npcs;
    }

    [Serializable]
    public class NPCConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public NPC.NPCType Type { get; set; }

        public string DefaultText { get; set; }
        public string RequiredKeys { get; set; }
        public string NotRequiredKeys { get; set; }
        public string InteractKey { get; set; }
        public NPC.NPCKeyType InteractKeyType { get; set; }
        public string DefeatKey { get; set; }
        public bool TrueDeath { get; set; }
        public bool StandStill { get; set; }
        // Interact
        public string InteractText { get; set; }
        // Use Item
        public string GiveItem { get; set; }
        public int? GiveItemQuality { get; set; }
        public int? GiveItemAmount { get; set; }
        // Reward
        public string RewardText { get; set; }
        public string RewardItem { get; set; }
        public int? RewardItemQuality { get; set; }
        public int? RewardItemAmount { get; set; }
        public string RewardKey { get; set; }
        public NPC.NPCKeyType RewardKeyType { get; set; }
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
        public int? ShoulderVariant { get; set; }
        public string Utility { get; set; }
        public string RightHand { get; set; }
        public string LeftHand { get; set; }
        public int? LeftHandVariant { get; set; }
        //public string RightBack { get; set; }
        //public string LeftBack { get; set; }
        //public int? LeftBackVariant { get; set; }
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
            cleaned.Name ??= "Ragnar";
            cleaned.DefaultText ??= "";
            cleaned.RequiredKeys ??= "";
            cleaned.NotRequiredKeys ??= "";
            cleaned.InteractKey ??= "";
            cleaned.DefeatKey ??= "";
            cleaned.InteractText ??= "";
            cleaned.GiveItem ??= "";
            cleaned.GiveItemQuality ??= -1;
            cleaned.GiveItemAmount ??= 1;
            cleaned.RewardText ??= "";
            cleaned.RewardItem ??= "";
            cleaned.RewardItemQuality ??= 1;
            cleaned.RewardItemAmount ??= 1;
            cleaned.RewardKey ??= "";
            cleaned.RewardLimit ??= -1; // Unlimited
            cleaned.Model ??= "Player";
            cleaned.Hair ??= "";
            cleaned.Beard ??= "";
            cleaned.Helmet ??= "";
            cleaned.Chest ??= "";
            cleaned.Legs ??= "";
            cleaned.Shoulder ??= "";
            cleaned.ShoulderVariant ??= 0;
            cleaned.Utility ??= "";
            cleaned.RightHand ??= "";
            cleaned.LeftHand ??= "";
            cleaned.LeftHandVariant ??= 0;
            //cleaned.RightBack ??= "";
            //cleaned.LeftBack ??= "";
            //cleaned.LeftBackVariant ??= 0;

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

        for (int lcv = 0; lcv < list.npcs.Count; lcv++)
        {
            var config = list.npcs[lcv];

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
            NPCSPlugin.NPCSLogger.LogWarning(e.Message);
        }

        return null;
    }
}