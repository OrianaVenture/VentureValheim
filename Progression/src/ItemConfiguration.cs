using System;
using System.Collections.Generic;

namespace VentureValheim.Progression
{
    public class ItemConfiguration
    {
        private ItemConfiguration() { }
        private static readonly ItemConfiguration _instance = new ItemConfiguration();

        public static ItemConfiguration Instance
        {
            get => _instance;
        }

        public enum ItemType
		{
            Undefined = -1,
			None = 0,
			Shield = 1,
			Helmet = 2,
			Chest = 3,
			Legs = 4,
			Shoulder = 5,
			Utility = 6,
			Tool = 7,
            PickAxe = 8,
            Axe = 9,
			Bow = 10,
			Ammo = 11,
            Sword = 20,
            Knife = 21,
            Mace = 22,
            Sledge = 23,
            Atgeir = 25,
            Battleaxe = 26,

		}

        public class ItemClassification
        {
            public string Name;
            public int BiomeType;
            public ItemType ItemType;

            public ItemClassification(string name, int biomeType, ItemType itemType)
            {
                Name = name;
                BiomeType = biomeType;
                ItemType = itemType;
            }

            /// <summary>
            /// Get the total damage for a Player or Creature.
            /// </summary>
            /// <param name="OriginalDamage"></param>
            /// <param name="playerItem">True if for the Player</param>
            /// <returns></returns>
            public static float GetTotalDamage(HitData.DamageTypes OriginalDamage, bool playerItem)
            {
                var damage = OriginalDamage.m_damage +
                    OriginalDamage.m_blunt +
                    OriginalDamage.m_slash +
                    OriginalDamage.m_pierce +
                    OriginalDamage.m_fire +
                    OriginalDamage.m_frost +
                    OriginalDamage.m_lightning +
                    OriginalDamage.m_poison +
                    OriginalDamage.m_spirit;

                if (playerItem)
                {
                    return damage +
                    OriginalDamage.m_chop +
                    OriginalDamage.m_pickaxe;
                }

                return damage;
            }

            /// <summary>
            /// Scales the damage given the original damage and new total base damage.
            /// </summary>
            /// <param name="biome"></param>
            /// <param name="OriginalDamage"></param>
            /// <param name="baseDamage"></param>
            /// <param name="ratio"></param>
            /// <param name="playerItem"></param>
            /// <returns></returns>
            public static HitData.DamageTypes CalculateDamageTypes(WorldConfiguration.Biome biome, HitData.DamageTypes OriginalDamage, float baseDamage, float ratio, bool playerItem = true)
            {
                var scale = WorldConfiguration.GetBiomeScaling((int)biome);
                var multiplier = ratio * scale;
                return CalculateDamageTypesFinal(OriginalDamage, baseDamage, multiplier, playerItem);
            }

            /// <summary>
            /// Scales the damage given the original damage and new total base damage.
            /// </summary>
            /// <param name="OriginalDamage"></param>
            /// <param name="newDamage"></param>
            /// <param name="multiplier"></param>
            /// <param name="playerItem"></param>
            /// <returns></returns>
            private static HitData.DamageTypes CalculateDamageTypesFinal(HitData.DamageTypes OriginalDamage, float newDamage, float multiplier, bool playerItem = true)
            {
                HitData.DamageTypes damageTypes = new HitData.DamageTypes();

                if (playerItem)
                {
                    // If a mob item leave the chop and pickaxe damage alone because it's a little weird for auto-scaling
                    damageTypes.m_chop = ScaleDamage(OriginalDamage.m_chop, newDamage, multiplier);
                    damageTypes.m_pickaxe = ScaleDamage(OriginalDamage.m_pickaxe, newDamage, multiplier);
                }

                damageTypes.m_damage = ScaleDamage(OriginalDamage.m_damage, newDamage, multiplier);
                damageTypes.m_blunt = ScaleDamage(OriginalDamage.m_blunt, newDamage, multiplier);
                damageTypes.m_slash = ScaleDamage(OriginalDamage.m_slash, newDamage, multiplier);
                damageTypes.m_pierce = ScaleDamage(OriginalDamage.m_pierce, newDamage, multiplier);
                damageTypes.m_fire = ScaleDamage(OriginalDamage.m_fire, newDamage, multiplier);
                damageTypes.m_frost = ScaleDamage(OriginalDamage.m_frost, newDamage, multiplier);
                damageTypes.m_lightning = ScaleDamage(OriginalDamage.m_lightning, newDamage, multiplier);
                damageTypes.m_poison = ScaleDamage(OriginalDamage.m_poison, newDamage, multiplier);
                damageTypes.m_spirit = ScaleDamage(OriginalDamage.m_spirit, newDamage, multiplier);

                return damageTypes;
            }

            /// <summary>
            /// Scale the given newDamage by the multiplier if the original value is greater than 0.
            /// </summary>
            /// <param name="original"></param>
            /// <param name="newDamage"></param>
            /// <param name="multiplier"></param>
            /// <returns></returns>
            private static float ScaleDamage(float original, float newDamage, float multiplier)
            {
                if (original <= 0f || newDamage <= 0f)
                {
                    return 0f;
                }
                var value = newDamage * multiplier;
                return (int)value;
            }

            private static void Normalize(ref HitData.DamageTypes OriginalDamage)
            {
                OriginalDamage.m_damage = Normalize(OriginalDamage.m_damage);
                OriginalDamage.m_blunt = Normalize(OriginalDamage.m_blunt);
                OriginalDamage.m_slash = Normalize(OriginalDamage.m_slash);
                OriginalDamage.m_pierce = Normalize(OriginalDamage.m_pierce);
                OriginalDamage.m_chop = Normalize(OriginalDamage.m_chop);
                OriginalDamage.m_pickaxe = Normalize(OriginalDamage.m_pickaxe);
                OriginalDamage.m_fire = Normalize(OriginalDamage.m_fire);
                OriginalDamage.m_frost = Normalize(OriginalDamage.m_frost);
                OriginalDamage.m_lightning = Normalize(OriginalDamage.m_lightning);
                OriginalDamage.m_poison = Normalize(OriginalDamage.m_poison);
                OriginalDamage.m_spirit = Normalize(OriginalDamage.m_spirit);
            }

            private static float Normalize(float num)
            {
                if (num < 0f)
                {
                    return 0f;
                }

                return num;
            }
        }

        private Dictionary<string, ItemClassification> _ItemData;

        /// <summary>
        /// Set the default values for Vanilla Player Items.
        /// </summary>
        public void Initialize()
        {
            _ItemData = new Dictionary<string, ItemClassification>();

            // TODO
        }

        /// <summary>
        /// Adds a new ItemClassification for scaling or replaces the existing if a configuration already exists.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="biome"></param>
        /// <param name="difficulty"></param>
        /*public void AddItemConfiguration(string name, WorldConfiguration.Biome biome, ItemType itemType, bool playerItem = true, string characterName = "")
        {
            try
            {
                _ItemData.Add(name, new ItemClassification(name, (int)biome, itemType));
            }
            catch (Exception e)
            {
                ProgressionPlugin.GetProgressionLogger().LogWarning($"Item Configuration already exists, replacing value for {name}.");
                _ItemData[name] = new ItemClassification(name, (int)biome, itemType);
            }
        }

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
        public static class Patch_ObjectDB_Awake
        {
            private static void Prefix()
            {
                ProgressionPlugin.GetProgressionLogger().LogDebug("CreatureConfiguration.Patch_ObjectDB_Awake called.");

                if (SceneManager.GetActiveScene().name.Equals("main"))
                {
                    if (WorldConfiguration.Instance.GetWorldScale() != (int)WorldConfiguration.Scaling.Vanilla)
                    {

                    }
                }
                else
                {
                    ProgressionPlugin.GetProgressionLogger().LogDebug("Skipping generating data because not in the main scene.");
                }
            }
        }*/
    }
}