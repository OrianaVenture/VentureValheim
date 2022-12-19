using Moq;
using VentureValheim.Progression;
using Xunit;

namespace VentureValheim.ProgressionTests
{
    public class SkillsTests
    {
        public class TestSkillsManager : SkillsManager
        {
            public TestSkillsManager() : base()
            {
            }

            public float GetBossSkillCeilingTest(int a) => GetBossSkillCeiling(a);
            public float GetBossSkillFloorTest(int a) => GetBossSkillFloor(a);
            public float GetSkillDrainTest(float a, float b, float c) => GetSkillDrain(a, b, c);
            public float GetSkillAccumulationGainTest(float a, float b, float c) => GetSkillAccumulationGain(a, b, c);
            public float NormalizeSkillLevelTest(float a) => NormalizeSkillLevel(a);
        }

        private TestSkillsManager skillsManager = new TestSkillsManager();

        private const float belowMinSkill = -0.1f;
        private const float minSkill = 0f;
        private const float midSkill = 50.6f;
        private const float maxSkill = 100f;
        private const float aboveMaxSkill = 100.1f;

        private const float skillDrain = 0.25f;
        private const int skillDrainAbsolute = 2;

        private void Setup()
        {
            var mockManager = new Mock<IProgressionConfiguration>();
            mockManager.Setup(x => x.GetBossKeysSkillPerKey()).Returns(0);
            mockManager.Setup(x => x.GetOverrideMaximumSkillLevel()).Returns(false);
            mockManager.Setup(x => x.GetMaximumSkillLevel()).Returns(100);
            mockManager.Setup(x => x.GetOverrideMinimumSkillLevel()).Returns(false);
            mockManager.Setup(x => x.GetMinimumSkillLevel()).Returns(0);
            mockManager.Setup(x => x.GetBossKeysSkillPerKey()).Returns(10);

            new ProgressionConfiguration(mockManager.Object);
        }

        private void Setup(bool useSkillDrain, int skillDrain, bool compare, bool useMinimum)
        {
            var mockManager = new Mock<IProgressionConfiguration>();
            mockManager.Setup(x => x.GetUseAbsoluteSkillDrain()).Returns(useSkillDrain);
            mockManager.Setup(x => x.GetAbsoluteSkillDrain()).Returns(skillDrain);
            mockManager.Setup(x => x.GetCompareAndSelectDrain()).Returns(compare);
            mockManager.Setup(x => x.GetCompareUseMinimumDrain()).Returns(useMinimum);

            new ProgressionConfiguration(mockManager.Object);
        }

        [Theory]
        [InlineData(false, skillDrain * midSkill)]
        [InlineData(true, skillDrainAbsolute)]
        public void GetSkillDrain_HappyPaths(bool a, float expected)
        {
            Setup(a, skillDrainAbsolute, false, true);
            Assert.Equal(expected, skillsManager.GetSkillDrainTest(midSkill, minSkill, skillDrain));
        }


        [Theory]
        [InlineData(belowMinSkill, minSkill, 0f)]
        [InlineData(minSkill, minSkill, 0f)]
        [InlineData(midSkill, minSkill, midSkill * skillDrain)]
        [InlineData(midSkill, maxSkill, 0f)]
        [InlineData(aboveMaxSkill, maxSkill, aboveMaxSkill * skillDrain)]
        public void GetSkillDrain_Vanilla(float level, float floor, float expected)
        {
            Setup(false, skillDrainAbsolute, false, true);
            Assert.Equal(expected, skillsManager.GetSkillDrainTest(level, floor, skillDrain));
        }


        [Theory]
        [InlineData(belowMinSkill, minSkill, 0f)]
        [InlineData(minSkill, minSkill, 0f)]
        [InlineData(midSkill, minSkill, skillDrainAbsolute)]
        [InlineData(midSkill, maxSkill, 0f)]
        [InlineData(aboveMaxSkill, maxSkill, skillDrainAbsolute)]
        public void GetSkillDrain_Flavor(float level, float floor, float expected)
        {
            Setup(true, skillDrainAbsolute, false, true);
            Assert.Equal(expected, skillsManager.GetSkillDrainTest(level, floor, skillDrain));
        }

        [Fact]
        public void GetSkillDrain_CompareMin()
        {
            Setup(true, 0, true, true);
            Assert.Equal(0f, skillsManager.GetSkillDrainTest(midSkill, minSkill, skillDrain));
        }

        [Fact]
        public void GetSkillDrain_CompareMax()
        {
            Setup(true, 0, true, false);
            Assert.Equal(midSkill * skillDrain, skillsManager.GetSkillDrainTest(midSkill, minSkill, skillDrain));
        }


        [Theory]
        [InlineData(belowMinSkill, minSkill)]
        [InlineData(minSkill, minSkill)]
        [InlineData(midSkill, midSkill)]
        [InlineData(maxSkill, maxSkill)]
        [InlineData(aboveMaxSkill, maxSkill)]
        public void NormalizeSkillLevel_All(float level, float expected)
        {
            Setup();
            Assert.Equal(expected, skillsManager.NormalizeSkillLevelTest(level));
        }

        [Theory]
        [InlineData(maxSkill, maxSkill, 1f, 0f)]
        [InlineData(midSkill, maxSkill, 1f, 1f)]
        [InlineData(midSkill, minSkill, 1f, 0f)]
        public void GetSkillGainAccumulation_All(float a, float b, float c, float expected)
        {
            Setup();
            Assert.Equal(expected, skillsManager.GetSkillAccumulationGainTest(a, b, c));
        }

        [Theory]
        [InlineData(0, 40f)]
        [InlineData(1, 50f)]
        [InlineData(2, 60f)]
        [InlineData(3, 70f)]
        [InlineData(4, 80f)]
        [InlineData(5, 90f)]
        [InlineData(6, 100f)]
        public void GetBossSkillCeiling_All(int bosses, float expected)
        {
            Setup();
            Assert.Equal(expected, skillsManager.GetBossSkillCeilingTest(bosses));
        }

        [Theory]
        [InlineData(0, 0f)]
        [InlineData(1, 10f)]
        [InlineData(2, 20f)]
        [InlineData(3, 30f)]
        [InlineData(4, 40f)]
        [InlineData(5, 50f)]
        [InlineData(6, 60f)]
        public void GetBossSkillFloor_All(int bosses, float expected)
        {
            Setup();
            Assert.Equal(expected, skillsManager.GetBossSkillFloorTest(bosses));
        }
    }
}
