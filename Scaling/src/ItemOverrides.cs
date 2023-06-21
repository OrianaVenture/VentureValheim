using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VentureValheim.Scaling
{
    public class ItemOverrides
    {
        private static string filePath = Paths.ConfigPath + Path.DirectorySeparatorChar + "VWS.ItemOverrides.yaml";

        public static ItemOverridesList ReadYaml()
        {
            if (File.Exists(filePath))
            {
                using var fileReader = new StreamReader(filePath);
                string fileData = File.ReadAllText(filePath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                ItemOverridesList items = deserializer.Deserialize<ItemOverridesList>(fileData);

                return items;
            }
            else
            {
                ScalingPlugin.VentureScalingLogger.LogWarning("No yaml file found, to use overrides the VWS.ItemOverrides.yaml file must be in your bepinex config folder.");
                return null;
            }
        }

        [Serializable]
        public class ItemOverridesList
        {
            public IEnumerable<ItemOverride> items { get; set; }
            public IEnumerable<BaseItemValueOverride> baseItemValues { get; set; }

            public override string ToString()
            {
                if (items == null)
                {
                    return "No Data.";
                }

                string str = $"Items in list {items.Count()}:\n";
                foreach (var entry in items)
                {
                    str += entry.ToString();
                }
                return str;
            }
        }

        [Serializable]
        public class ItemOverride
        {
            public string name { get; set; }
            public int? biome { get; set; }
            public int? itemType { get; set; }
            public float? value { get; set; }
            public int? quality { get; set; }
            public float? upgradeValue { get; set; }
            public CreatureOverrides.AttackOverride damageValue { get; set; }
            public CreatureOverrides.AttackOverride upgradeDamageValue { get; set; }

            public override string ToString()
            {
                string str = $"{name}: Biome {biome}, Item Type {itemType}, Value {value}, Quality {quality}, Upgrade Value {upgradeValue}\n";
                if (damageValue != null)
                {
                    str += "+Damage Value: " + damageValue.ToString();
                }

                if (upgradeDamageValue != null)
                {
                    str += "+Upgrade Damage Value: " + upgradeDamageValue.ToString();
                }

                return str;
            }
        }

        public class BaseItemValueOverride
        {
            public int? itemType { get; set; }
            public float? value { get; set; }
        }
    }
}
