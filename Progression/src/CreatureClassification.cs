using System;
using System.Collections.Generic;
using UnityEngine;

namespace VentureValheim.Progression
{
    public class CreatureClassification
    {
        public string Name { get; private set; }
        public WorldConfiguration.Biome BiomeType { get; private set; }
        public WorldConfiguration.Difficulty CreatureDifficulty { get; private set; }
        public Dictionary<string, HitData.DamageTypes> VanillaAttacks { get; private set; }
        public float VanillaHealth { get; private set; }
        public Dictionary<string, HitData.DamageTypes> OverrideAttacks { get; private set; }
        public float OverrideHealth { get; private set; }

        public CreatureClassification(string name, WorldConfiguration.Biome biomeType, WorldConfiguration.Difficulty creatureDifficulty)
        {
            Name = name;
            BiomeType = biomeType;
            CreatureDifficulty = creatureDifficulty;
            VanillaHealth = -1;
            VanillaAttacks = null;
            OverrideHealth = -1;
            OverrideAttacks = null;
        }

        public void UpdateCreature(WorldConfiguration.Biome biomeType, WorldConfiguration.Difficulty creatureDifficulty)
        {
            BiomeType = biomeType;
            CreatureDifficulty = creatureDifficulty;
            // TODO confirm override here
            OverrideHealth = -1;
            OverrideAttacks = null;
        }

        public void SetVanillaHealth(float health)
        {
            VanillaHealth = health;
        }

        public void SetVanillaAttacks(GameObject[] attacks)
        {
            VanillaAttacks = new Dictionary<string, HitData.DamageTypes>();

            for (int lcv = 0; lcv < attacks.Length; lcv++)
            {
                var item = attacks[lcv].GetComponent<ItemDrop>();
                if (item != null)
                {
                    var name = item.name;
                    var damage = item.m_itemData.m_shared.m_damages;
                    AddVanillaAttack(name, damage);
                }
                else
                {
                    ProgressionPlugin.VentureProgressionLogger.LogWarning($"Vanilla attack for {attacks[lcv].name} was not added to creature {Name}. ItemDrop not found.");
                }
            }
        }

        /// <summary>
        /// Adds the given vanilla DamageTypes for an attack.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="damage"></param>
        /// <returns></returns>
        private void AddVanillaAttack(string name, HitData.DamageTypes damage)
        {
            try
            {
                if (ItemConfiguration.Instance.GetTotalDamage(damage) > 0)
                {
                    VanillaAttacks.Add(name, damage);
                }
                else
                {
                    ProgressionPlugin.VentureProgressionLogger.LogDebug($"Vanilla attack {name} was not added to creature {Name}. No damage component.");
                }
            }
            catch (ArgumentException)
            {
                ProgressionPlugin.VentureProgressionLogger.LogWarning($"Vanilla attack {name} was not added to creature {Name}. Already exists.");
            }
        }

        // TODO
        public void SetOverrideAttack(string name, HitData.DamageTypes damage)
        {
            try
            {
                OverrideAttacks.Add(name, damage);
            }
            catch (ArgumentException)
            {
                OverrideAttacks[name] = damage;
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"Attack {name} already exists for {Name}, overriding.");
            }
        }

        public void SetOverrideHealth(float health)
        {
            OverrideHealth = health;
        }

        public bool HealthOverriden()
        {
            return OverrideHealth >= 0;
        }

        public bool AttackOverriden(string name)
        {
            return OverrideAttacks.ContainsKey(name);
        }
    }
}
