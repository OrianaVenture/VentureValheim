using BepInEx;
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
        public float? VanillaHealth { get; private set; }
        public bool Overridden { get; private set; }
        public Dictionary<string, CreatureOverrides.AttackOverride> OverrideAttacks { get; private set; }
        public float? OverrideHealth { get; private set; }

        public CreatureClassification(string name, WorldConfiguration.Biome? biomeType, WorldConfiguration.Difficulty? creatureDifficulty)
        {
            Reset();

            Name = name;

            if (biomeType != null)
            {
                BiomeType = biomeType.Value;
            }

            if (creatureDifficulty != null)
            {
                CreatureDifficulty = creatureDifficulty.Value;
            }

            VanillaHealth = null;
            VanillaAttacks = null;
        }

        /// <summary>
        /// Reset the custom values.
        /// </summary>
        public void Reset()
        {
            BiomeType = WorldConfiguration.Biome.Undefined;
            CreatureDifficulty = WorldConfiguration.Difficulty.Undefined;
            Overridden = false;
            OverrideHealth = null;
            OverrideAttacks = new Dictionary<string, CreatureOverrides.AttackOverride>();
        }

        /// <summary>
        /// Returns true if the scaling factor is Vanilla or if set to Custom and this entry is not overriden.
        /// </summary>
        /// <returns></returns>
        public virtual bool IgnoreScaling()
        {
            if (WorldConfiguration.Instance.WorldScale == WorldConfiguration.Scaling.Vanilla ||
                (WorldConfiguration.Instance.WorldScale == WorldConfiguration.Scaling.Custom && Overridden == false) ||
                CreatureDifficulty == WorldConfiguration.Difficulty.Vanilla)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reset overrides and update creature with new values.
        /// </summary>
        /// <param name="biomeType"></param>
        /// <param name="creatureDifficulty"></param>
        public void UpdateCreature(WorldConfiguration.Biome? biomeType, WorldConfiguration.Difficulty? creatureDifficulty)
        {
            if (biomeType != null)
            {
                BiomeType = biomeType.Value;
            }

            if (creatureDifficulty != null)
            {
                CreatureDifficulty = creatureDifficulty.Value;
            }
        }

        /// <summary>
        /// Refresh creature overide values with the new override values.
        /// </summary>
        /// <param name="creatureOverride"></param>
        public void OverrideCreature(CreatureOverrides.CreatureOverride creatureOverride)
        {
            Overridden = true;

            WorldConfiguration.Biome? biome = null;

            if (creatureOverride.biome != null)
            {
                biome = (WorldConfiguration.Biome)creatureOverride.biome;
            }

            WorldConfiguration.Difficulty? difficulty = null;

            if (creatureOverride.difficulty != null)
            {
                difficulty = (WorldConfiguration.Difficulty)creatureOverride.difficulty;
            }

            UpdateCreature(biome, difficulty);

            OverrideHealth = creatureOverride.health;
            OverrideAttacks = new Dictionary<string, CreatureOverrides.AttackOverride>();

            if (creatureOverride.attacks != null)
            {
                foreach (var entry in creatureOverride.attacks)
                {
                    if (!entry.name.IsNullOrWhiteSpace())
                    {
                        SetOverrideAttack(entry);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the vanilla data ensuring it is not overridden.
        /// </summary>
        /// <param name="health"></param>
        /// <param name="attacks"></param>
        public void SetVanillaData(float health, List<GameObject> attacks)
        {
            SetVanillaHealth(health);
            SetVanillaAttacks(attacks);
        }

        /// <summary>
        /// Sets the vanilla attacks ensuring they are not overridden.
        /// </summary>
        /// <param name="attacks"></param>
        private void SetVanillaAttacks(List<GameObject> attacks)
        {
            if (VanillaAttacks != null)
            {
                return;
            }

            VanillaAttacks = new Dictionary<string, HitData.DamageTypes>();

            if (attacks == null)
            {
                return;
            }

            for (int lcv = 0; lcv < attacks.Count; lcv++)
            {
                var item = attacks[lcv].GetComponent<ItemDrop>();
                if (item == null)
                {
                    ProgressionPlugin.VentureProgressionLogger.LogWarning($"Attack {attacks[lcv].name} not added to creature {Name}. ItemDrop not found. Will not override.");
                }
                else if (!VanillaAttacks.ContainsKey(item.name))
                {
                    var damage = item.m_itemData.m_shared.m_damages;
                    if (ItemConfiguration.Instance.GetTotalDamage(damage) > 0)
                    {
                        VanillaAttacks.Add(item.name, damage);
                    }
                    else
                    {
                        ProgressionPlugin.VentureProgressionLogger.LogDebug($"Attack for {attacks[lcv].name} not added to creature {Name}. Does 0 damage. Will not override.");
                    }
                }
            }
        }

        #region Health

        /// <summary>
        /// Sets the vanilla health ensuring it is not overridden.
        /// </summary>
        /// <param name="health"></param>
        private void SetVanillaHealth(float health)
        {
            VanillaHealth ??= health;
        }

        public bool HealthOverridden()
        {
            return OverrideHealth != null;
        }

        public float? GetHealth()
        {
            if (IgnoreScaling())
            {
                return VanillaHealth;
            }
            else if (HealthOverridden())
            {
                return OverrideHealth;
            }
            else
            {
                var scale = WorldConfiguration.Instance.GetBiomeScaling(BiomeType);
                int baseHealth = CreatureConfiguration.Instance.GetBaseHealth(CreatureDifficulty);
                var health = CreatureConfiguration.Instance.CalculateHealth(scale, baseHealth);
                return ProgressionAPI.PrettifyNumber(health);
            }
        }

        #endregion

        #region Damage

        public HitData.DamageTypes? GetAttack(string name, float maxTotalDamage)
        {
            if (name.IsNullOrWhiteSpace() || !VanillaAttacks.ContainsKey(name))
            {
                return null;
            }

            var vanillaAttack = VanillaAttacks[name];

            if (IgnoreScaling())
            {
                return vanillaAttack;
            }
            else if (AttackOverridden(name))
            {
                var attack = OverrideAttacks[name];
                if (attack.totalDamage != null)
                {
                    return ItemConfiguration.Instance.CalculateDamageTypesFinal(vanillaAttack, attack.totalDamage.Value, 1);
                }
                else
                {
                    var damage = CreatureOverrides.GetDamageTypes(attack);
                    // Prevent an overwrite of chop and pickaxe damage to reduce confusion hopefully
                    if (attack.pickaxe == null)
                    {
                        damage.m_pickaxe = vanillaAttack.m_pickaxe;
                    }
                    if (attack.chop == null)
                    {
                        damage.m_chop = vanillaAttack.m_chop;
                    }
                    return damage;
                }
            }
            else
            {
                var baseTotalDamage = CreatureConfiguration.Instance.GetBaseTotalDamage(CreatureDifficulty);
                var biomeData = WorldConfiguration.Instance.GetBiome(BiomeType);

                if (biomeData != null)
                {
                    return ItemConfiguration.Instance.CalculateCreatureDamageTypes(biomeData.ScaleValue, vanillaAttack, baseTotalDamage, maxTotalDamage);
                }
            }

            return vanillaAttack;
        }

        public bool AttackOverridden(string name)
        {
            if (OverrideAttacks == null)
            {
                return false;
            }

            return OverrideAttacks.ContainsKey(name);
        }

        /// <summary>
        /// Returns the overridden attack if exists, if not calulates the new DamageTypes.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public HitData.DamageTypes? GetAttackOverride(string name)
        {
            if (OverrideAttacks.ContainsKey(name))
            {
                return CreatureOverrides.GetDamageTypes(OverrideAttacks[name]);
            }
            else
            {
                return null;
            }
        }

        public float? GetAttackOverrideTotal(string name)
        {
            if (OverrideAttacks.ContainsKey(name))
            {
                return OverrideAttacks[name].totalDamage;
            }
            else
            {
                return null;
            }
        }

        private void SetOverrideAttack(CreatureOverrides.AttackOverride attack)
        {
            if (OverrideAttacks.ContainsKey(attack.name))
            {
                OverrideAttacks[attack.name] = attack;
            }
            else
            {
                OverrideAttacks.Add(attack.name, attack);
            }
        }

        #endregion
    }
}
