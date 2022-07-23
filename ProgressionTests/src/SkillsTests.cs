using NUnit.Framework;
using VentureValheim.Progression;

namespace VentureValheim.ProgressionTests
{
    [TestFixture]
    public class SkillsTests
    {
        private const float belowMinSkill = -0.1f;
        private const float minSkill = 0f;
        private const float midSkill = 50.6f;
        private const float maxSkill = 100f;
        private const float aboveMaxSkill = 100.1f;

        private const float skillDrain = 0.25f;
        private const float skillDrainAbsolute = 2f;

        [OneTimeSetUp]
        public void Setup()
        {
            ProgressionManager.Instance.Initialize(false, "", "");
        }

        [TestCase(true, false, skillDrain * midSkill)]
        [TestCase(true, true, skillDrainAbsolute)]
        [TestCase(false, false, skillDrain * midSkill)]
        [TestCase(false, true, skillDrainAbsolute)]
        public void GetSkillDrain_HappyPaths(bool a, bool b, float expected)
        {
            SkillsManager.Instance.Initialize(a, b, skillDrainAbsolute);
            Assert.AreEqual(expected, SkillsManager.Instance.GetSkillDrain(midSkill, minSkill, skillDrain));
        }
        
        [TestCase(belowMinSkill, minSkill, 0f)]
        [TestCase(minSkill, minSkill, 0f)]
        [TestCase(midSkill, minSkill, midSkill * skillDrain)]
        [TestCase(midSkill, maxSkill, 0f)]
        [TestCase(aboveMaxSkill, maxSkill, aboveMaxSkill * skillDrain)]
        public void GetSkillDrain_Vanilla(float level, float floor, float expected)
        {
            SkillsManager.Instance.Initialize(true, false, skillDrainAbsolute);
            Assert.AreEqual(expected, SkillsManager.Instance.GetSkillDrain(level, floor, skillDrain));
        }
        
        [TestCase(belowMinSkill, minSkill, 0f)]
        [TestCase(minSkill, minSkill, 0f)]
        [TestCase(midSkill, minSkill, skillDrainAbsolute)]
        [TestCase(midSkill, maxSkill, 0f)]
        [TestCase(aboveMaxSkill, maxSkill, skillDrainAbsolute)]
        public void GetSkillDrain_Flavor(float level, float floor, float expected)
        {
            SkillsManager.Instance.Initialize(true, true, skillDrainAbsolute);
            Assert.AreEqual(expected, SkillsManager.Instance.GetSkillDrain(level, floor, skillDrain));
        }

        [TestCase(belowMinSkill, minSkill)]
        [TestCase(minSkill, minSkill)]
        [TestCase(midSkill, midSkill)]
        [TestCase(maxSkill, maxSkill)]
        [TestCase(aboveMaxSkill, maxSkill)]
        public void NormalizeSkillLevel_All(float level, float expected)
        {
            Assert.AreEqual(expected, SkillsManager.Instance.NormalizeSkillLevel(level));
        }
        
        
        [TestCase(maxSkill, maxSkill, 1f, 0f)]
        [TestCase(midSkill, maxSkill, 1f, 1f)]
        [TestCase(midSkill, minSkill, 1f, 0f)]
        public void GetSkillGainAccumulation_All(float a, float b, float c, float expected)
        {
            Assert.AreEqual(expected, SkillsManager.Instance.GetSkillAccumulationGain(a, b, c));
        }
    }
}
