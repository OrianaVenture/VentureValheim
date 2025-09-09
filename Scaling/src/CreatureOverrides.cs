using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VentureValheim.Scaling;

public class CreatureOverrides
{
    private static string filePath = Paths.ConfigPath + Path.DirectorySeparatorChar + "VWS.CreatureOverrides.yaml";

    public static CreatureOverridesList ReadYaml()
    {
        if (File.Exists(filePath))
        {
            using var fileReader = new StreamReader(filePath);
            string fileData = File.ReadAllText(filePath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            CreatureOverridesList creatures = deserializer.Deserialize<CreatureOverridesList>(fileData);

            return creatures;
        }
        else
        {
            ScalingPlugin.VentureScalingLogger.LogWarning("No yaml file found, to use overrides the VWS.CreatureOverrides.yaml file must be in your bepinex config folder.");
            return null;
        }
    }

    [Serializable]
    public class CreatureOverridesList
    {
        public IEnumerable<CreatureOverride> creatures { get; set; }

        public override string ToString()
        {
            if (creatures == null)
            {
                return "No Data.";
            }

            string str = $"Creatures in list {creatures.Count()}:\n";
            foreach (var entry in creatures)
            {
                str += entry.ToString();
            }
            return str;
        }
    }

    [Serializable]
    public class CreatureOverride
    {
        public string name { get; set; }
        public int? biome { get; set; }
        public int? difficulty { get; set; }
        public float? health { get; set; }
        public IEnumerable<AttackOverride> attacks { get; set; }

        public override string ToString()
        {
            string str = $"{name}: Biome {biome}, Difficulty {difficulty}, Health {health}, Attacks:\n";
            if (attacks != null)
            {
                foreach (var entry in attacks)
                {
                    str += entry.ToString();
                }
            }

            return str;
        }
    }

    [Serializable]
    public class AttackOverride
    {
        public string name { get; set; }
        public float? totalDamage { get; set; }
        public float? damage { get; set; }
        public float? blunt { get; set; }
        public float? slash { get; set; }
        public float? pierce { get; set; }
        public float? fire { get; set; }
        public float? frost { get; set; }
        public float? lightning { get; set; }
        public float? poison { get; set; }
        public float? spirit { get; set; }
        public float? chop { get; set; }
        public float? pickaxe { get; set; }

        public override string ToString()
        {
            string str = "";
            if (!name.IsNullOrWhiteSpace())
            {
                str += $"+{name}: ";
            }

            // Override uses total damage OR all other types
            if (totalDamage != null)
            {
                str += $"total damage {totalDamage}\n";
            }
            else
            {
                str += $"damage {damage}, blunt {blunt}, slash {slash}, pierce {pierce}, fire {fire}, frost {frost}, " +
                    $"lightning {lightning}, poison {poison}, spirit {spirit}, chop {chop}, pickaxe {pickaxe}\n";
            }

            return str;
        }
    }

    public static HitData.DamageTypes GetDamageTypes(AttackOverride attack)
    {
        return new HitData.DamageTypes
        {
            m_damage = attack.damage == null ? 0f : attack.damage.Value,
            m_blunt = attack.blunt == null ? 0f : attack.blunt.Value,
            m_slash = attack.slash == null ? 0f : attack.slash.Value,
            m_pierce = attack.pierce == null ? 0f : attack.pierce.Value,
            m_fire = attack.fire == null ? 0f : attack.fire.Value,
            m_frost = attack.frost == null ? 0f : attack.frost.Value,
            m_lightning = attack.lightning == null ? 0f : attack.lightning.Value,
            m_poison = attack.poison == null ? 0f : attack.poison.Value,
            m_spirit = attack.spirit == null ? 0f : attack.spirit.Value,
            m_chop = attack.chop == null ? 0f : attack.chop.Value,
            m_pickaxe = attack.pickaxe == null ? 0f : attack.pickaxe.Value
        };
    }
}
