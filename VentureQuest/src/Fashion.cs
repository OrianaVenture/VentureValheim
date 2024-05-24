using BepInEx;
using System;
using System.IO;
using UnityEngine;

namespace VentureValheim.VentureQuest;

public class Fashion
{
    // TODO:
    // Load json files for fashion
    // Add command for setting NPC style from json
    // Add command for setting player style from json, or ability to give all equipment for it
    // can do an override command to bypass updating visequipment
    // this can end up being it's own style mod like we've talked about

    [Serializable]
    public class NPCFashion
    {
        public string name { get; set; }
        public Vector3? hairColor { get; set; }
        public Vector3? skinColor { get; set; }
        public int? modelIndex { get; set; }
        public string? hair { get; set; }
        public string? beard { get; set; }
        public string? helmet { get; set; }
        public string? chest { get; set; }
        public string? legs { get; set; }
        public string? shoulder { get; set; }
        public string? utility { get; set; }
        public string? rightHand { get; set; }
        public string? leftHand { get; set; }
        public string? rightBack { get; set; }
        public string? leftBack { get; set; }
    }

    public static NPCFashion ReadJsonFile(string filename)
    {
        /*var filePath = Paths.ConfigPath + Path.DirectorySeparatorChar + filename;
        try
        {
            using var fileReader = new StreamReader(filePath);
            string fileData = File.ReadAllText(filePath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return deserializer.Deserialize<NPCFashion>(fileData);
        }
        catch (Exception e)
        {
            VentureQuestPlugin.VentureQuestLogger.LogError($"Could not read file {filename}");
            VentureQuestPlugin.VentureQuestLogger.LogWarning(e);
        }*/

        return null;
    }
}