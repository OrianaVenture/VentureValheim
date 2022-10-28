using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.UI.GridLayoutGroup;
using static VentureValheim.Progression.WorldConfiguration;

namespace VentureValheim.Progression
{
    public interface IWorldConfiguration
    {
        public Scaling WorldScale { get; }
        public float ScaleFactor { get; }
        public BiomeData GetBiome(int biome);
        public void AddBiome(Biome biome, int order, bool overrideBiome = false);
        public void AddBiome(int biome, int order, bool overrideBiome = false);
        public void AddCustomBiome(Biome biome, float customScale, int order = -1, bool overrideBiome = false);
        public void AddCustomBiome(int biome, float customScale, int order = -1, bool overrideBiome = false);
        public float GetBiomeScaling(Biome biome);
        public float GetBiomeScaling(int biome);
        public float GetScaling(int order, float factor);
        public BiomeData GetNextBiome(Biome originalBiome);
    }

    public class WorldConfiguration : IWorldConfiguration
    {
        static WorldConfiguration() { }
        protected WorldConfiguration()
        {
            WorldScale = Scaling.Vanilla;
            ScaleFactor = 0.75f;
        }
        private static readonly IWorldConfiguration _instance = new WorldConfiguration();

        public static WorldConfiguration Instance
        {
            get => _instance as WorldConfiguration;
        }

        public Scaling WorldScale { get; protected set; }
        public float ScaleFactor { get; protected set; }

        public const int MAX_BIOME_ORDER = 100;

        private Dictionary<int, BiomeData> _biomeData = new Dictionary<int, BiomeData>();

        #region Biome, Scaling & Difficulty

        public enum Biome
        {
            Undefined = -1,
            Meadow = 0,
            BlackForest = 1,
            Swamp = 2,
            Mountain = 3,
            Plain = 4,
            AshLand = 5,
            DeepNorth = 6,
            Ocean = 7,
            Mistland = 8
        }

        public enum Scaling
        {
            Vanilla = 0,
            Linear = 1,
            Exponential = 2
        }

        public enum Difficulty
        {
            Undefined = -1,
            Vanilla = 0,
            Harmless = 1,
            Novice = 2,
            Average = 3,
            Intermediate = 4,
            Expert = 5,
            Boss = 6
        }

        public class BiomeData
        {
            public int BiomeType;
            public int BiomeOrder;
            public float ScaleValue;

            public BiomeData(int biomeType, int biomeOrder, float scaleValue)
            {
                BiomeType = biomeType;
                BiomeOrder = biomeOrder;
                ScaleValue = scaleValue;
            }
        }

        private void Initialize()
        {
            AddBiome(Biome.Meadow, 0);
            AddBiome(Biome.BlackForest, 1);
            AddBiome(Biome.Swamp, 2);
            AddBiome(Biome.Mountain, 3);
            AddBiome(Biome.Plain, 4);
            AddBiome(Biome.Mistland, 5);
            AddBiome(Biome.AshLand, 6);
            AddBiome(Biome.DeepNorth, 7);
            AddBiome(Biome.Ocean, 1);
        }

        protected void Initialize(Scaling worldScale, float factor)
        {
            WorldScale = worldScale;
            ScaleFactor = factor;
            // TODO warnings for custom set values that can break things with large numbers
            _biomeData = new Dictionary<int, BiomeData>();
            Initialize();
        }

        public BiomeData GetBiome(Biome biome)
        {
            return GetBiome((int)(biome));
        }

        /// <summary>
        /// Returns the BiomeData for a particular biome if it exists.
        /// </summary>
        /// <param name="biome"></param>
        /// <returns></returns>
        public BiomeData GetBiome(int biome)
        {
            try
            {
                return _biomeData[biome];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Adds a configuration for the given biome with scaling order.
        /// </summary>
        public void AddBiome(Biome biome, int order, bool overrideBiome = false)
        {
            AddBiome((int)biome, order, overrideBiome);
        }

        /// <summary>
        /// Adds a configuration for the given biome with the specified scaling order.
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="order">The difficulty order, 0 for lowest difficulty</param>
        /// <param name="overrideBiome"></param>
        public void AddBiome(int biome, int order, bool overrideBiome = false)
        {
            BiomeData data = CreateBiomeData(biome, order);
            AddBiomeData(data, overrideBiome);
        }

        /// <summary>
        /// Adds a configuration for the given biome with the specified custom scaling value.
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="customScale"></param>
        /// <param name="overrideBiome"></param>
        public void AddCustomBiome(Biome biome, float customScale, int order = -1, bool overrideBiome = false)
        {
            BiomeData data = new BiomeData((int)biome, order, customScale);
            AddBiomeData(data, overrideBiome);
        }

        /// <summary>
        /// Adds a configuration for the given biome with the specified custom scaling value.
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="customScale"></param>
        /// <param name="overrideBiome"></param>
        public void AddCustomBiome(int biome, float customScale, int order = -1, bool overrideBiome = false)
        {
            BiomeData data = new BiomeData(biome, order, customScale);
            AddBiomeData(data, overrideBiome);
        }

        /// <summary>
        /// Adds or updates the BiomeData if not null.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="overrideBiome">True to update an existing entry.</param>
        private void AddBiomeData(BiomeData data, bool overrideBiome = false)
        {
            if (data == null)
            {
                return;
            }

            try
            {
                _biomeData.Add(data.BiomeType, data);
            }
            catch (ArgumentException)
            {
                if (overrideBiome)
                {
                    _biomeData[data.BiomeType] = data;
                }
            }
        }

        /// <summary>
        /// Create BiomeData with the specified order of difficulty.
        /// If the order is greater than the maximum it will set to the maximum value.
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="order">default -1 to use built-in scale order</param>
        /// <returns></returns>
        private BiomeData CreateBiomeData(int biome, int order)
        {
            if (order < 0)
            {
                return null;
            }

            if (order > MAX_BIOME_ORDER)
            {
                order = MAX_BIOME_ORDER;
            }

            var scale = GetScaling(order, ScaleFactor);

            if (scale < 0)
            {
                return null;
            }

            return new BiomeData(biome, order, scale);
        }

        /// <summary>
        /// Returns the scaling value for the specified biome
        /// </summary>
        /// <param name="biome"></param>
        /// <returns>Value or 1 if not found</returns>
        public float GetBiomeScaling(Biome biome)
        {
            return GetBiomeScaling((int)biome);
        }

        /// <summary>
        /// Returns the scaling value for the specified biome
        /// </summary>
        /// <param name="biome"></param>
        /// <returns>Value or 1 if not found</returns>
        public float GetBiomeScaling(int biome)
        {
            var data = GetBiome(biome);

            if (data == null)
            {
                return 1f;
            }

            return data.ScaleValue;
        }

        /// <summary>
        /// Returns the scaling factor given the biome order and scaling factor.
        /// </summary>
        /// <param name="order">The difficulty ordering starting with 0 for the first biome</param>
        /// <param name="factor">The scaling factor percent represented as decimal</param>
        /// <returns>Scaling percent as a decimal value or -1 if error</returns>
        public float GetScaling(int order, float factor)
        {
            if (order == 0)
            {
                return 1f;
            }

            if (WorldScale == Scaling.Vanilla)
            {
                return 1f;
            }
            else if (WorldScale == Scaling.Exponential)
            {
                return (float)Math.Round(Math.Pow((double)(1 + factor), order), 2);
            }
            else if (WorldScale == Scaling.Linear)
            {
                return 1 + (float)Math.Round((double)(factor * order), 2);
            }

            return -1f;
        }

        /// <summary>
        /// Searches biome data for the next occuring biome.
        /// </summary>
        /// <param name="originalBiome"></param>
        /// <returns></returns>
        public BiomeData GetNextBiome(Biome originalBiome)
        {
            return GetNextBiome(GetBiome((int)originalBiome).BiomeOrder);
        }

        /// <summary>
        /// Recursivly searches biome data for the next occuring biome.
        /// </summary>
        /// <param name="originalOrder"></param>
        /// <returns></returns>
        protected BiomeData GetNextBiome(int originalOrder)
        {
            if (originalOrder >= MAX_BIOME_ORDER)
            {
                return null;
            }

            var next = GetBiomeByOrder(originalOrder + 1);

            if (next == null)
            {
                return GetNextBiome(originalOrder + 1);
            }

            return next;
        }

        /// <summary>
        /// Searches the biome data for a biome with the given order.
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        protected BiomeData GetBiomeByOrder(int order)
        {
            foreach (var biome in _biomeData.Values)
            {
                if (biome.BiomeOrder == order)
                {
                    return biome;
                }
            }

            return null;
        }

        #endregion

        #region Setup

        /// <summary>
        /// Read Configuration values and update the Scaling systems and game data with scaling settings.
        /// </summary>
        public void SetupScaling()
        {
            try
            {
                if (ProgressionPlugin.Instance.GetUseAutoScaling())
                {
                    SetupWorld(ProgressionPlugin.Instance.GetAutoScaleType(), ProgressionPlugin.Instance.GetAutoScaleFactor());

                    if (WorldScale == Scaling.Vanilla)
                    {
                        ProgressionPlugin.GetProgressionLogger().LogInfo("Restoring Vanilla Values...");
                        CreatureConfiguration.Instance.VanillaReset();
                        ItemConfiguration.Instance.VanillaReset();
                        return;
                    }

                    ProgressionPlugin.GetProgressionLogger().LogInfo(
                        $"WorldConfiguration Initializing with scale: {WorldScale}, factor: {ScaleFactor}.");

                    if (ProgressionPlugin.Instance.GetAutoScaleCreatures())
                    {
                        SetupCreatures();
                    }

                    if (ProgressionPlugin.Instance.GetAutoScaleItems())
                    {
                        SetupItems();
                    }
                }
            }
            catch (Exception e)
            {
                ProgressionPlugin.GetProgressionLogger().LogError("Error configuring Auto-Scaling features, your game may behave unexpectedly.");
                ProgressionPlugin.GetProgressionLogger().LogError(e);
            }
        }

        private void SetupWorld(string type, float factor)
        {
            var scale = Scaling.Vanilla;
            type = type.Trim().ToLower();

            if (type.Equals("exponential"))
            {
                scale = Scaling.Exponential;
            }
            else if (type.Equals("linear"))
            {
                scale = Scaling.Linear;
            }

            Initialize(scale, factor);
        }

        private void SetupCreatures()
        {
            try
            {
                var healthString = ProgressionPlugin.Instance.GetAutoScaleCreatureHealth();
                var arr = ProgressionAPI.Instance.StringToIntArray(healthString);
                CreatureConfiguration.Instance.SetBaseHealth(arr);
            }
            catch
            {
                ProgressionPlugin.GetProgressionLogger().LogWarning("Issue parsing Creature Health configuration, using defaults.");
            }

            try
            {
                var damageString = ProgressionPlugin.Instance.GetAutoScaleCreatureDamage();
                var arr = ProgressionAPI.Instance.StringToIntArray(damageString);
                CreatureConfiguration.Instance.SetBaseDamage(arr);
            }
            catch
            {
                ProgressionPlugin.GetProgressionLogger().LogWarning("Issue parsing Creature Damage configuration, using defaults.");
            }

            ProgressionPlugin.GetProgressionLogger().LogInfo("Updating Creature Configurations with auto-scaling...");
            CreatureConfiguration.Instance.Initialize();

            // Debugging logs
            // TODO remove or improve
            var prefabs = ZNetScene.m_instance.m_prefabs;
            for (int lcv = 0; lcv < prefabs.Count; lcv++)
            {
                var name = prefabs[lcv].name;
                if (!CreatureConfiguration.Instance.ContainsCreature(name))
                {
                    ProgressionPlugin.GetProgressionLogger().LogDebug($"No configuration found for GameObject, skipping: {name}.");
                }
            }

            CreatureConfiguration.Instance.UpdateCreatures();
        }

        private void SetupItems()
        {
            ProgressionPlugin.GetProgressionLogger().LogInfo("Updating Item Configurations with auto-scaling...");
            ItemConfiguration.Instance.Initialize();
            ItemConfiguration.Instance.UpdateItems();
        }

        /// <summary>
        /// Configure World settings on Player's first spawn.
        /// </summary>
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        public static class Patch_Player_Awake
        {
            private static void Postfix()
            {
                if (ProgressionAPI.Instance.IsInTheMainScene())
                {
                    ProgressionPlugin.GetProgressionLogger().LogInfo("Setting up world configurations...");
                    Instance.SetupScaling();
                    ProgressionPlugin.GetProgressionLogger().LogInfo("Done setting up world configurations.");
                }
            }
        }

        #endregion
    }
}