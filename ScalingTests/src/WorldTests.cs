using Moq;
using Xunit;
using VentureValheim.Scaling;

namespace VentureValheim.ScalingTests
{
    public class WorldTests
    {
        public class TestWorldConfiguration : WorldConfiguration, IWorldConfiguration
        {
            public TestWorldConfiguration(IWorldConfiguration world) : base()
            {
                Initialize(world.WorldScale, world.ScaleFactor);
            }
        }

        private const float _factor = 0.1f;
        private const float _factor2 = 1.6f;

        private TestWorldConfiguration Setup(WorldConfiguration.Scaling a, float b)
        {
            var mockWorld = new Mock<IWorldConfiguration>();
            mockWorld.SetupGet(x => x.WorldScale).Returns(a);
            mockWorld.SetupGet(x => x.ScaleFactor).Returns(b);

            return new TestWorldConfiguration(mockWorld.Object);
        }

        [Theory]
        [InlineData(0, _factor, 1f)]
        [InlineData(1, _factor, 1.1f)]
        [InlineData(2, _factor, 1.2f)]
        [InlineData(3, _factor, 1.3f)]
        [InlineData(10, _factor, 2f)]
        [InlineData(100, _factor, 11f)]
        public void GetScalingLinear_HappyPaths(int order, float factor, float expected)
        {
            var worldConfiguration = Setup(WorldConfiguration.Scaling.Linear, factor);
            Assert.Equal(expected, worldConfiguration.GetScaling(order, factor));
        }

        [Theory]
        [InlineData(0, _factor, 1f)]
        [InlineData(1, _factor, 1.1f)]
        [InlineData(2, _factor, 1.21f)]
        [InlineData(3, _factor, 1.33f)]
        [InlineData(10, _factor, 2.59f)]
        [InlineData(100, _factor, 13780.64f)]
        public void GetScalingExponential_HappyPaths(int order, float factor, float expected)
        {
            var worldConfiguration = Setup(WorldConfiguration.Scaling.Exponential, factor);
            Assert.Equal(expected, worldConfiguration.GetScaling(order, factor), 0.1f);
        }

        [Theory]
        [InlineData(0, _factor2, 1f)]
        [InlineData(1, _factor2, 2.47f)]
        [InlineData(2, _factor2, 3.34f)]
        [InlineData(3, _factor2, 3.95f)]
        [InlineData(10, _factor2, 6.10f)]
        [InlineData(100, _factor2, 10.82f)]
        public void GetScalingLogarithmic_HappyPaths(int order, float factor, float expected)
        {
            var worldConfiguration = Setup(WorldConfiguration.Scaling.Logarithmic, factor);
            Assert.Equal(expected, worldConfiguration.GetScaling(order, factor), 0.1f);
        }

        [Theory]
        [InlineData(0, _factor, 1f)]
        [InlineData(1, _factor, 1f)]
        [InlineData(10, _factor, 1f)]
        [InlineData(100, _factor, 1f)]
        public void GetScalingVanilla_HappyPaths(int order, float factor, float expected)
        {
            var worldConfiguration = Setup(WorldConfiguration.Scaling.Vanilla, factor);
            Assert.Equal(expected, worldConfiguration.GetScaling(order, factor));
        }

        [Theory]
        [InlineData(WorldConfiguration.Biome.Meadow, (int)WorldConfiguration.Biome.BlackForest)]
        [InlineData(WorldConfiguration.Biome.BlackForest, (int)WorldConfiguration.Biome.Swamp)]
        [InlineData(WorldConfiguration.Biome.Swamp, (int)WorldConfiguration.Biome.Mountain)]
        [InlineData(WorldConfiguration.Biome.Mountain, (int)WorldConfiguration.Biome.Plain)]
        [InlineData(WorldConfiguration.Biome.Plain, (int)WorldConfiguration.Biome.Mistland)]
        [InlineData(WorldConfiguration.Biome.Mistland, (int)WorldConfiguration.Biome.AshLand)]
        [InlineData(WorldConfiguration.Biome.AshLand, (int)WorldConfiguration.Biome.DeepNorth)]
        public void GetNextBiome_HappyPaths(WorldConfiguration.Biome biome, int expected)
        {
            var worldConfiguration = Setup(WorldConfiguration.Scaling.Exponential, 0.75f);
            Assert.Equal(expected, worldConfiguration.GetNextBiome(biome).BiomeType);
        }

        [Fact]
        public void GetNextBiome_NotFound()
        {
            var worldConfiguration = Setup(WorldConfiguration.Scaling.Exponential, 0.75f);
            Assert.Null(worldConfiguration.GetNextBiome(WorldConfiguration.Biome.DeepNorth));
        }

        [Theory]
        [InlineData(WorldConfiguration.Biome.Meadow, 1.75f)]
        [InlineData(WorldConfiguration.Biome.BlackForest, 3.06f)]
        [InlineData(WorldConfiguration.Biome.Swamp, 5.36f)]
        [InlineData(WorldConfiguration.Biome.Mountain, 9.38f)]
        [InlineData(WorldConfiguration.Biome.Plain, 16.41f)]
        [InlineData(WorldConfiguration.Biome.Mistland, 28.72f)]
        [InlineData(WorldConfiguration.Biome.AshLand, 50.27f)]
        [InlineData(WorldConfiguration.Biome.DeepNorth, 87.96f)]
        public void GetNextBiomeScaling_HappyPaths(WorldConfiguration.Biome biome, float expected)
        {
            var worldConfiguration = Setup(WorldConfiguration.Scaling.Exponential, 0.75f);
            float result = worldConfiguration.GetNextBiomeScale(biome);
            Assert.Equal(expected, result, 0.1f);
        }
    }
}
