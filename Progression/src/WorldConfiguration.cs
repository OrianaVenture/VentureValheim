using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace VentureValheim.Progression
{
    public class WorldConfiguration
    {
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
            public float ScaleValue;

            public BiomeData()
            {
                BiomeType = (int)Biome.Undefined;
                ScaleValue = 1f;
            }

            public BiomeData(int biomeType, float scaleValue)
            {
                BiomeType = biomeType;
                ScaleValue = scaleValue;
            }
        }

        private Scaling _worldScale = Scaling.Vanilla;
        private float _scaleFactor = 0.75f;

        public Scaling GetWorldScale()
        {
            return _worldScale;
        }

        public float GetWorldScaleFactor()
        {
            return _scaleFactor;
        }

        private Dictionary<int, BiomeData> _biomeData = new Dictionary<int, BiomeData>();

        private WorldConfiguration() {}
        private static readonly WorldConfiguration _instance = new WorldConfiguration();

        public static WorldConfiguration Instance
        {
            get => _instance;
        }

        public void Initialize()
        {
            AddBiome(Biome.Meadow, 0);
            AddBiome(Biome.BlackForest, 1);
            AddBiome(Biome.Swamp, 2);
            AddBiome(Biome.Mountain, 3);
            AddBiome(Biome.Plain, 4);
            AddBiome(Biome.AshLand, 5);
            AddBiome(Biome.DeepNorth, 6);
            AddBiome(Biome.Mistland, 7);
            AddBiome(Biome.Ocean, 1);
        }

        public void Initialize(Scaling worldScale, float factor)
        {
            _worldScale = worldScale;
            _scaleFactor = factor;
            // TODO warnings for custom set values that can break things with large numbers
            Initialize();
        }

        /// <summary>
        /// Returns the BiomeData for a particular biome if it exists.
        /// </summary>
        /// <param name="biome"></param>
        /// <returns></returns>
        public static BiomeData? GetBiome(int biome)
        {
            try
            {
                BiomeData data;
                _instance._biomeData.TryGetValue(biome, out data);
                return data;
            }
            catch (Exception e)
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
        public void AddCustomBiome(Biome biome, float customScale, bool overrideBiome = false)
        {
            BiomeData data = new BiomeData((int)biome, customScale);
            AddBiomeData(data, overrideBiome);
        }

        /// <summary>
        /// Adds a configuration for the given biome with the specified custom scaling value.
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="customScale"></param>
        /// <param name="overrideBiome"></param>
        public void AddCustomBiome(int biome, float customScale, bool overrideBiome = false)
        {
            BiomeData data = new BiomeData(biome, customScale);
            AddBiomeData(data, overrideBiome);
        }

        private void AddBiomeData(BiomeData? data, bool overrideBiome = false)
        {
            if (data == null)
            {
                return;
            }

            try
            {
                _biomeData.Add(data.BiomeType, data);
            }
            catch (Exception e)
            {
                if (overrideBiome)
                {
                    _biomeData[data.BiomeType] = data;
                }
            }
        }

        /// <summary>
        /// Create BiomeData with the specified order of difficulty.
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

            var scale = GetScaling(order, _scaleFactor);

            if (scale < 0)
            {
                return null;
            }

            return new BiomeData(biome, scale);
        }

        /// <summary>
        /// Returns the scaling value for the specified biome
        /// </summary>
        /// <param name="biome"></param>
        /// <returns>Value or 1 if not found</returns>
        public static float GetBiomeScaling(Biome biome)
        {
            return GetBiomeScaling((int)biome);
        }

        /// <summary>
        /// Returns the scaling value for the specified biome
        /// </summary>
        /// <param name="biome"></param>
        /// <returns>Value or 1 if not found</returns>
        public static float GetBiomeScaling(int biome)
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

            if (_worldScale == Scaling.Vanilla)
            {
                return 1f;
            }
            else if (_worldScale == Scaling.Exponential)
            {
                return (float)Math.Round(Math.Pow((double)(1 + factor), order), 2);
            }
            else if (_worldScale == Scaling.Linear)
            {
                return 1 + (float)Math.Round((double)(factor * order), 2);
            }

            return -1f;
        }

        /// <summary>
        /// Rounds a number up or down to the nearest roundTo value. Rounds up to the nearest 5 by default.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="roundTo">5 by default</param>
        /// <param name="roundUp">true by default</param>
        /// <returns></returns>
        public static int PrettifyNumber(int number, int roundTo = 5, bool roundUp = true)
        {
            var remainder = number % roundTo;
            if (remainder == 0)
            {
                return number;
            }

            if (roundUp)
            {
                return number + (roundTo - remainder);
            }
            else
            {
                return number - remainder;
            }
        }

        /// <summary>
        /// Read Configuration values and update the Scaling systems and game data with scaling settings.
        /// </summary>
        public void SetupScaling()
        {
            try
            {
                if (ProgressionPlugin.Instance.GetUseAutoScaling())
                {
                    float factor = ProgressionPlugin.Instance.GetAutoScaleFactor();
                    var scale = Scaling.Vanilla;
                    if (ProgressionPlugin.Instance.GetAutoScaleType().ToLower().Equals("exponential"))
                    {
                        scale = Scaling.Exponential;
                    }
                    else if (ProgressionPlugin.Instance.GetAutoScaleType().ToLower().Equals("linear"))
                    {
                        scale = Scaling.Linear;
                    }

                    ProgressionPlugin.GetProgressionLogger().LogInfo($"WorldConfiguration Initializing with scale: {scale}, factor: {factor}.");
                    Instance.Initialize(scale, factor);

                    // Skip generating data if scaling is set to Vanilla
                    if (scale == Scaling.Vanilla) return;

                    if (ProgressionPlugin.Instance.GetAutoScaleCreatures())
                    {
                        var healthString = ProgressionPlugin.Instance.GetAutoScaleCreatureHealth();
                        if (!healthString.IsNullOrWhiteSpace())
                        {
                            try
                            {
                                var list = healthString.Split(',');
                                var copy = new int[list.Length];
                                for (var lcv = 0; lcv < list.Length; lcv++)
                                {
                                    copy[lcv] = int.Parse(list[lcv].Trim());
                                }

                                CreatureConfiguration.Instance.SetBaseHealth(copy);
                            }
                            catch
                            {
                                ProgressionPlugin.GetProgressionLogger().LogWarning("Issue parsing Creature Health configuration, using defaults.");
                            }
                        }

                        var damageString = ProgressionPlugin.Instance.GetAutoScaleCreatureDamage();
                        if (!damageString.IsNullOrWhiteSpace())
                        {
                            try
                            {
                                var list = damageString.Split(',');
                                var copy = new int[list.Length];
                                for (var lcv = 0; lcv < list.Length; lcv++)
                                {
                                    copy[lcv] = int.Parse(list[lcv].Trim());
                                }

                                CreatureConfiguration.Instance.SetBaseDamage(copy);
                            }
                            catch
                            {
                                ProgressionPlugin.GetProgressionLogger().LogWarning("Issue parsing Creature Damage configuration, using defaults.");
                            }
                        }

                        CreatureConfiguration.Instance.Initialize();
                        ProgressionPlugin.GetProgressionLogger().LogInfo("Updating Creature Configurations with auto-scaling...");
                        CreatureConfiguration.Instance.UpdateCreatures();
                    }

                    if (ProgressionPlugin.Instance.GetAutoScaleItems())
                    {
                        ItemConfiguration.Instance.Initialize();
                        ProgressionPlugin.GetProgressionLogger().LogInfo("Updating Item Configurations with auto-scaling...");
                        ItemConfiguration.Instance.UpdateItems();
                    }
                }
            }
            catch (Exception e)
            {
                ProgressionPlugin.GetProgressionLogger().LogError("Error configuring Auto-Scaling features, your game may behave unexpectedly.");
                ProgressionPlugin.GetProgressionLogger().LogError(e);
            }
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
                if (ProgressionAPI.IsInTheMainScene())
                {
                    ProgressionPlugin.GetProgressionLogger().LogInfo("Setting up world configurations...");
                    Instance.SetupScaling();
                    ProgressionPlugin.GetProgressionLogger().LogInfo("Done setting up world configurations.");
                }
            }
        }
    }
}