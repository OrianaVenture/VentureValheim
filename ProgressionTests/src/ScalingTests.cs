using NUnit.Framework;
using VentureValheim.Progression;
using static Character;

namespace VentureValheim.ProgressionTests
{
    [TestFixture]
    public class ScalingTests
    {
        private const float _factor = 0.1f;

        [TestCase(0, _factor, 1f)]
        [TestCase(1, _factor, 1.1f)]
        [TestCase(2, _factor, 1.2f)]
        [TestCase(3, _factor, 1.3f)]
        [TestCase(10, _factor, 2f)]
        [TestCase(100, _factor, 11f)]
        public void GetScalingLinear_HappyPaths(int order, float factor, float expected)
        {
            WorldConfiguration.Instance.Initialize((int)WorldConfiguration.Scaling.Linear, factor);
            Assert.AreEqual(expected, WorldConfiguration.Instance.GetScaling(order, factor));
        }

        [TestCase(0, _factor, 1f)]
        [TestCase(1, _factor, 1.1f)]
        [TestCase(2, _factor, 1.21f)]
        [TestCase(3, _factor, 1.33f)]
        [TestCase(10, _factor, 2.59f)]
        [TestCase(100, _factor, 13780.64f)]
        public void GetScalingExponential_HappyPaths(int order, float factor, float expected)
        {
            WorldConfiguration.Instance.Initialize((int)WorldConfiguration.Scaling.Exponential, factor);
            Assert.AreEqual(expected, WorldConfiguration.Instance.GetScaling(order, factor));
        }

        [TestCase(0, _factor, 1f)]
        [TestCase(1, _factor, 1f)]
        [TestCase(10, _factor, 1f)]
        [TestCase(100, _factor, 1f)]
        public void GetScalingVanilla_HappyPaths(int order, float factor, float expected)
        {
            WorldConfiguration.Instance.Initialize((int)WorldConfiguration.Scaling.Vanilla, factor);
            Assert.AreEqual(expected, WorldConfiguration.Instance.GetScaling(order, factor));
        }

        [TestCase(105, 5, true, 105)]
        [TestCase(104, 5, true, 105)]
        [TestCase(103, 5, true, 105)]
        [TestCase(102, 5, true, 105)]
        [TestCase(101, 5, true, 105)]
        [TestCase(100, 5, true, 100)]
        [TestCase(105, 5, false, 105)]
        [TestCase(104, 5, false, 100)]
        [TestCase(103, 5, false, 100)]
        [TestCase(102, 5, false, 100)]
        [TestCase(101, 5, false, 100)]
        [TestCase(100, 5, false, 100)]
        public void PrettifyNumber_All(int num, int roundTo, bool roundUp, int expected)
        {
            Assert.AreEqual(expected, WorldConfiguration.PrettifyNumber(num, roundTo, roundUp));
        }

        [Test]
        public void GetCreatureHealth_EnsureDefaultUnchanged()
        {
            CreatureConfiguration.Instance.Initialize();

            var baseHealth = CreatureConfiguration.Instance.GetBaseHealth(0);
            Assert.AreEqual(5, baseHealth);
            baseHealth = CreatureConfiguration.Instance.GetBaseHealth(1);
            Assert.AreEqual(10, baseHealth);
            baseHealth = CreatureConfiguration.Instance.GetBaseHealth(2);
            Assert.AreEqual(30, baseHealth);
            baseHealth = CreatureConfiguration.Instance.GetBaseHealth(3);
            Assert.AreEqual(50, baseHealth);
            baseHealth = CreatureConfiguration.Instance.GetBaseHealth(4);
            Assert.AreEqual(200, baseHealth);
            baseHealth = CreatureConfiguration.Instance.GetBaseHealth(5);
            Assert.AreEqual(500, baseHealth);
        }

        [TestCase(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Harmless, 5)]
        [TestCase(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Novice, 10)]
        [TestCase(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Average, 30)]
        [TestCase(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Intermediate, 50)]
        [TestCase(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Expert, 200)]
        [TestCase(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Boss, 500)]
        [TestCase(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Harmless, 15)]
        [TestCase(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Novice, 30)]
        [TestCase(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Average, 91)]
        [TestCase(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Intermediate, 153)]
        [TestCase(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Expert, 612)]
        [TestCase(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Boss, 1530)]
        [TestCase(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Harmless, 46)]
        [TestCase(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Novice, 93)]
        [TestCase(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Average, 281)]
        [TestCase(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Intermediate, 469)]
        [TestCase(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Expert, 1876)]
        [TestCase(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Boss, 4690)]
        public void GetCreatureHealth_All(WorldConfiguration.Biome biome, WorldConfiguration.Difficulty d, int expected)
        {
            var scale = 0.75f;
            WorldConfiguration.Instance.Initialize((int)WorldConfiguration.Scaling.Exponential, scale);
            CreatureConfiguration.Instance.Initialize();

            var config = new CreatureConfiguration.CreatureClassification("test", (int)biome, d);
            Assert.AreEqual(expected, config.CalculateHealth());
        }
    }
}
