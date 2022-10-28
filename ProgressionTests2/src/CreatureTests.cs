using Moq;
using Xunit;
using VentureValheim.Progression;
using static VentureValheim.ProgressionTests.WorldTests;
using static VentureValheim.ProgressionTests.ItemTests;

namespace VentureValheim.ProgressionTests
{
    public class CreatureTests
    {
        public class TestCreatureConfiguration : CreatureConfiguration, ICreatureConfiguration
        {
            public TestCreatureConfiguration(ICreatureConfiguration creature) : base()
            {
                _vanillaBackupCreated = true;
                Initialize();
            }
        }

        private Mock<IWorldConfiguration> mockWorld;
        private TestWorldConfiguration worldConfiguration;
        private TestCreatureConfiguration creatureConfiguration;
        private TestItemConfiguration itemConfiguration;

        public CreatureTests()
        {
            mockWorld = new Mock<IWorldConfiguration>();
            mockWorld.SetupGet(x => x.WorldScale).Returns(WorldConfiguration.Scaling.Exponential);
            mockWorld.SetupGet(x => x.ScaleFactor).Returns(0.75f);

            worldConfiguration = new TestWorldConfiguration(mockWorld.Object);

            var mockCreature = new Mock<ICreatureConfiguration>();
            creatureConfiguration = new TestCreatureConfiguration(mockCreature.Object);

            var mockItem = new Mock<IItemConfiguration>();
            itemConfiguration = new TestItemConfiguration(mockItem.Object);
        }

        [Theory]
        [InlineData(WorldConfiguration.Difficulty.Harmless, 5)]
        [InlineData(WorldConfiguration.Difficulty.Novice, 10)]
        [InlineData(WorldConfiguration.Difficulty.Average, 30)]
        [InlineData(WorldConfiguration.Difficulty.Intermediate, 50)]
        [InlineData(WorldConfiguration.Difficulty.Expert, 200)]
        [InlineData(WorldConfiguration.Difficulty.Boss, 500)]
        public void GetCreatureHealth_EnsureDefaultUnchanged(WorldConfiguration.Difficulty d, int expected)
        {
            var health = creatureConfiguration.GetBaseHealth(d);

            Assert.Equal(expected, health);
        }

        [Theory]
        [InlineData(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Harmless, 5)]
        [InlineData(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Novice, 10)]
        [InlineData(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Average, 30)]
        [InlineData(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Intermediate, 50)]
        [InlineData(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Expert, 200)]
        [InlineData(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Boss, 500)]
        [InlineData(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Harmless, 15)]
        [InlineData(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Novice, 30)]
        [InlineData(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Average, 91)]
        [InlineData(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Intermediate, 153)]
        [InlineData(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Expert, 612)]
        [InlineData(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Boss, 1530)]
        [InlineData(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Harmless, 46)]
        [InlineData(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Novice, 93)]
        [InlineData(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Average, 281)]
        [InlineData(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Intermediate, 469)]
        [InlineData(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Expert, 1876)]
        [InlineData(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Boss, 4690)]
        public void GetCreatureHealth_All(WorldConfiguration.Biome biome, WorldConfiguration.Difficulty d, int expected)
        {
            var test = new CreatureConfiguration.CreatureClassification("test", biome, d);
            Assert.Equal(expected, creatureConfiguration.CalculateHealth(test, worldConfiguration.GetBiomeScaling(biome)));
        }

        [Theory]
        [InlineData(WorldConfiguration.Difficulty.Harmless, 0)]
        [InlineData(WorldConfiguration.Difficulty.Novice, 5)]
        [InlineData(WorldConfiguration.Difficulty.Average, 10)]
        [InlineData(WorldConfiguration.Difficulty.Intermediate, 12)]
        [InlineData(WorldConfiguration.Difficulty.Expert, 15)]
        [InlineData(WorldConfiguration.Difficulty.Boss, 20)]
        public void GetCreatureDamage_EnsureDefaultUnchanged(WorldConfiguration.Difficulty d, int expected)
        {
            var damage = creatureConfiguration.GetBaseTotalDamage(d);
            Assert.Equal(expected, damage);
        }

        [Fact]
        public void GetCreatureTotalDamage()
        {
            HitData.DamageTypes damageTypes = new HitData.DamageTypes
            {
                m_damage = 10f,
                m_chop = 10f,
                m_pickaxe = 10f,
                m_blunt = 10f,
                m_slash = 10f,
                m_pierce = 10f,
                m_fire = 10f,
                m_frost = 10f,
                m_lightning = 10f,
                m_poison = 10f,
                m_spirit = 10f
            };

            float sumDamage = itemConfiguration.GetTotalDamage(damageTypes, false);

            Assert.Equal(90f, sumDamage);
        }

        [Theory]
        [InlineData(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Harmless, 0f)]
        [InlineData(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Novice, 5f)]
        [InlineData(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Average, 10f)]
        [InlineData(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Intermediate, 12f)]
        [InlineData(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Expert, 15f)]
        [InlineData(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Boss, 20f)]
        [InlineData(WorldConfiguration.Biome.BlackForest, WorldConfiguration.Difficulty.Harmless, 0f)]
        [InlineData(WorldConfiguration.Biome.BlackForest, WorldConfiguration.Difficulty.Novice, 8f)]
        [InlineData(WorldConfiguration.Biome.BlackForest, WorldConfiguration.Difficulty.Average, 17f)]
        [InlineData(WorldConfiguration.Biome.BlackForest, WorldConfiguration.Difficulty.Intermediate, 21f)]
        [InlineData(WorldConfiguration.Biome.BlackForest, WorldConfiguration.Difficulty.Expert, 26f)]
        [InlineData(WorldConfiguration.Biome.BlackForest, WorldConfiguration.Difficulty.Boss, 35f)]
        [InlineData(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Harmless, 0f)]
        [InlineData(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Novice, 15f)]
        [InlineData(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Average, 30f)]
        [InlineData(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Intermediate, 36f)]
        [InlineData(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Expert, 45f)]
        [InlineData(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Boss, 61f)]
        [InlineData(WorldConfiguration.Biome.Mountain, WorldConfiguration.Difficulty.Harmless, 0f)]
        [InlineData(WorldConfiguration.Biome.Mountain, WorldConfiguration.Difficulty.Novice, 26f)]
        [InlineData(WorldConfiguration.Biome.Mountain, WorldConfiguration.Difficulty.Average, 53f)]
        [InlineData(WorldConfiguration.Biome.Mountain, WorldConfiguration.Difficulty.Intermediate, 64f)]
        [InlineData(WorldConfiguration.Biome.Mountain, WorldConfiguration.Difficulty.Expert, 80f)]
        [InlineData(WorldConfiguration.Biome.Mountain, WorldConfiguration.Difficulty.Boss, 107f)]
        [InlineData(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Harmless, 0f)]
        [InlineData(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Novice, 46f)]
        [InlineData(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Average, 93f)]
        [InlineData(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Intermediate, 112f)]
        [InlineData(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Expert, 140f)]
        [InlineData(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Boss, 187f)]
        public void GetCreatureDamage_All(WorldConfiguration.Biome biome, WorldConfiguration.Difficulty d, float expected)
        {
            HitData.DamageTypes damageTypes = new HitData.DamageTypes
            {
                m_damage = 0f,
                m_chop = 10f,
                m_pickaxe = 10f,
                m_blunt = 10f,
                m_slash = 0f,
                m_pierce = 0f,
                m_fire = 0f,
                m_frost = 0f,
                m_lightning = 0f,
                m_poison = 0f,
                m_spirit = 0f
            };

            var max = itemConfiguration.GetTotalDamage(damageTypes, false);

            var newDamage = creatureConfiguration.GetBaseTotalDamage(d);

            var result = itemConfiguration.CalculateCreatureDamageTypes(worldConfiguration.GetBiome(biome), damageTypes, newDamage, max);

            var expectedDamage = new HitData.DamageTypes();
            expectedDamage.m_chop = 10f;
            expectedDamage.m_pickaxe = 10f;
            expectedDamage.m_blunt = expected;

            Assert.Equal(10f, result.m_chop);
            Assert.Equal(10f, result.m_pickaxe);
            Assert.Equal(expected, result.m_blunt);
            Assert.Equal(expectedDamage, result);
        }
    }
}
