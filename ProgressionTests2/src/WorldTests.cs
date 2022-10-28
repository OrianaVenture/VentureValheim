using Moq;
using Xunit;
using VentureValheim.Progression;

namespace VentureValheim.ProgressionTests
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
        private Mock<IWorldConfiguration> mockWorld;
        private TestWorldConfiguration worldConfiguration;

        private void Setup(WorldConfiguration.Scaling a, float b)
        {
            mockWorld = new Mock<IWorldConfiguration>();
            mockWorld.SetupGet(x => x.WorldScale).Returns(a);
            mockWorld.SetupGet(x => x.ScaleFactor).Returns(b);

            worldConfiguration = new TestWorldConfiguration(mockWorld.Object);
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
            Setup(WorldConfiguration.Scaling.Linear, factor);
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
            Setup(WorldConfiguration.Scaling.Exponential, factor);
            Assert.Equal(expected, worldConfiguration.GetScaling(order, factor));
        }


        [Theory]
        [InlineData(0, _factor, 1f)]
        [InlineData(1, _factor, 1f)]
        [InlineData(10, _factor, 1f)]
        [InlineData(100, _factor, 1f)]
        public void GetScalingVanilla_HappyPaths(int order, float factor, float expected)
        {
            Setup(WorldConfiguration.Scaling.Vanilla, factor);
            Assert.Equal(expected, worldConfiguration.GetScaling(order, factor));
        }
    }
}
