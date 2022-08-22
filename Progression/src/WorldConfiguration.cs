using System;
using System.Collections.Generic;

namespace VentureValheim.Progression
{
    public class WorldConfiguration
    {
        // TODO
        // Ability to set biome order for scaling calculations
        // Biomes can share difficulty
        // This should be able to change with the configs (live?)
        // This can make all biomes the same scale
        // Ability to set minimum difficulty? => how does that change the line above?

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
            Harmless = 0,
            Novice = 1,
            Average = 2,
            Intermediate = 3,
            Expert = 4,
            Boss = 5
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

        private int _worldScale = (int)Scaling.Vanilla;
        private float _scaleFactor = 0.75f;

        public int GetWorldScale()
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
            // TODO consider different datatype to store this info
            AddBiome((int)Biome.Meadow);
            AddBiome((int)Biome.BlackForest);
            AddBiome((int)Biome.Swamp);
            AddBiome((int)Biome.Mountain);
            AddBiome((int)Biome.Plain);
            AddBiome((int)Biome.AshLand);
            AddBiome((int)Biome.DeepNorth);
            AddBiome((int)Biome.Mistland);
            AddBiome((int)Biome.Ocean);
        }

        public void Initialize(int worldScale, float factor)
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
                // TODO log message
                return null;
            }
        }

        /// <summary>
        /// Adds a configuration for the given biome with default scaling order.
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="overrideBiome"></param>
        public void AddBiome(int biome, bool overrideBiome = false)
        {
            AddBiome(biome, -1, overrideBiome);
        }

        /// <summary>
        /// Adds a configuration for the given biome with the specified scaling order.
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="order">The difficulty order, 0 for lowest difficulty, -1 to use default order</param>
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
        public void AddCustomBiome(int biome, float customScale, bool overrideBiome = false)
        {
            BiomeData data = new BiomeData(biome, customScale);
            AddBiomeData(data, overrideBiome);
        }

        private void AddBiomeData(BiomeData data, bool overrideBiome = false)
        {
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
                else
                {
                    // TODO log message warning
                }
            }
        }

        /// <summary>
        /// Create BiomeData with the specified order of difficulty.
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="order">default -1 to use built-in scale order</param>
        /// <returns></returns>
        private BiomeData CreateBiomeData(int biome, int order = -1)
        {
            float scale = -1;
            if (order < 0)
            {
                switch (biome)
                {
                    case (int)Biome.Meadow:
                        scale = GetScaling(0, _scaleFactor);
                        break;
                    case (int)Biome.BlackForest:
                        scale = GetScaling(1, _scaleFactor);
                        break;
                    case (int)Biome.Swamp:
                        scale = GetScaling(2, _scaleFactor);
                        break;
                    case (int)Biome.Mountain:
                        scale = GetScaling(3, _scaleFactor);
                        break;
                    case (int)Biome.Plain:
                        scale = GetScaling(4, _scaleFactor);
                        break;
                    case (int)Biome.AshLand:
                        scale = GetScaling(5, _scaleFactor);
                        break;
                    case (int)Biome.DeepNorth:
                        scale = GetScaling(6, _scaleFactor);
                        break;
                    case (int)Biome.Mistland:
                        scale = GetScaling(7, _scaleFactor);
                        break;
                    case (int)Biome.Ocean:
                        scale = GetScaling(0, _scaleFactor);
                        break;
                }
            }
            else
            {
                scale = GetScaling(order, _scaleFactor);
            }

            if (scale == -1)
            {
                // Error, biome is not in the defaults/vanilla list and was not set by argument
                // Log warning, set scale to 1
                // or is this an exception?
            }

            return new BiomeData(biome, scale);
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
                ProgressionPlugin.VentureProgressionLogger.LogDebug("Biome data not found, returning 1.");
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

            if (_worldScale == (int)Scaling.Vanilla)
            {
                return 1f;
            }
            else if (_worldScale == (int)Scaling.Exponential)
            {
                return (float)Math.Round(Math.Pow((double)(1 + factor), order), 2);
            }
            else if (_worldScale == (int)Scaling.Linear)
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
    }
}