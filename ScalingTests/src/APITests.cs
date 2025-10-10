using VentureValheim.Scaling;
using Xunit;

namespace VentureValheim.ScalingTests;

public class APITests
{
    public class TestScalingAPI : ScalingAPI, IScalingAPI
    {
        public TestScalingAPI(IScalingAPI api) : base()
        {

        }
    }

    [Theory]
    [InlineData(105, 5, true, 105)]
    [InlineData(104, 5, true, 105)]
    [InlineData(103, 5, true, 105)]
    [InlineData(102, 5, true, 105)]
    [InlineData(101, 5, true, 105)]
    [InlineData(100, 5, true, 100)]
    [InlineData(105, 5, false, 105)]
    [InlineData(104, 5, false, 100)]
    [InlineData(103, 5, false, 100)]
    [InlineData(102, 5, false, 100)]
    [InlineData(101, 5, false, 100)]
    [InlineData(100, 5, false, 100)]
    public void PrettifyNumber_All(int num, int roundTo, bool roundUp, int expected)
    {
        Assert.Equal(expected, ScalingAPI.PrettifyNumber(num, roundTo, roundUp));
    }

    [Fact]
    public void Enum_All()
    {
        ItemCategory ic2 = (ItemCategory)(-2);

        ItemCategory ic = (ItemCategory)(-1);
        Assert.Equal(ItemCategory.Undefined, ic);
    }
}