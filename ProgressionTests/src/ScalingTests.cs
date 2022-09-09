using NUnit.Framework;
using VentureValheim.Progression;

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
    }
}
