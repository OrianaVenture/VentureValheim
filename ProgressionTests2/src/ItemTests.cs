using Moq;
using Xunit;
using VentureValheim.Progression;
using static VentureValheim.ProgressionTests.WorldTests;

namespace VentureValheim.ProgressionTests
{
    public class ItemTests
    {
        public class TestItemConfiguration : ItemConfiguration, IItemConfiguration
        {
            public TestItemConfiguration(IItemConfiguration Item) : base()
            {
                _vanillaBackupCreated = true;
                Initialize();
            }

            public float DamageRatioTest(float a, float b) => DamageRatio(a, b);
            public float ScaleDamageTest(float a, float b, float c, float d) => ScaleDamage(a, b, c, d);
            public float CalculateUpgradeValueTest(float a, float b, float c, int d) => CalculateUpgradeValue(a, b, c, d);
            public HitData.DamageTypes CalculateUpgradeValueTest(float a, float b, HitData.DamageTypes c, float d, int e) => CalculateUpgradeValue(a, b, c, d, e);
        }

        private Mock<IWorldConfiguration> mockWorld;
        private TestWorldConfiguration worldConfiguration;
        private TestItemConfiguration itemConfiguration;

        public ItemTests()
        {
            mockWorld = new Mock<IWorldConfiguration>();
            mockWorld.SetupGet(x => x.WorldScale).Returns(WorldConfiguration.Scaling.Exponential);
            mockWorld.SetupGet(x => x.ScaleFactor).Returns(0.75f);
            worldConfiguration = new TestWorldConfiguration(mockWorld.Object);

            var mockItem = new Mock<IItemConfiguration>();
            itemConfiguration = new TestItemConfiguration(mockItem.Object);
        }

        [Fact]
        public void GetItemTotalDamage()
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

            float sumDamage = itemConfiguration.GetTotalDamage(damageTypes, true);

            Assert.Equal(110f, sumDamage);
        }

        [Theory]
        [InlineData(WorldConfiguration.Biome.Meadow, 3f)]
        [InlineData(WorldConfiguration.Biome.BlackForest, 5f)]
        [InlineData(WorldConfiguration.Biome.Swamp, 10f)]
        [InlineData(WorldConfiguration.Biome.Mountain, 17f)]
        [InlineData(WorldConfiguration.Biome.Plain, 31f)]
        [InlineData(WorldConfiguration.Biome.Mistland, 54f)]
        [InlineData(WorldConfiguration.Biome.AshLand, 95f)]
        [InlineData(WorldConfiguration.Biome.DeepNorth, 167f)]
        public void GetItemDamage_All(WorldConfiguration.Biome biome, float expected)
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

            var result = itemConfiguration.CalculateItemDamageTypes(worldConfiguration.GetBiome(biome), damageTypes, 10f);

            var expectedDamage = new HitData.DamageTypes();
            expectedDamage.m_chop = expected;
            expectedDamage.m_pickaxe = expected;
            expectedDamage.m_blunt = expected;

            Assert.Equal(expected, result.m_chop);
            Assert.Equal(expected, result.m_pickaxe);
            Assert.Equal(expected, result.m_blunt);
            Assert.Equal(expectedDamage, result);
        }

        [Fact]
        public void GetItemCategory_All()
        {
            Array items = Enum.GetValues(typeof(ItemConfiguration.ItemType));
            foreach (var item in items)
            {
                var itemType = (ItemConfiguration.ItemType)item;
                if (itemType != ItemConfiguration.ItemType.Undefined && itemType != ItemConfiguration.ItemType.None)
                {
                    Assert.True(itemConfiguration.GetItemCategory(itemType) != ItemConfiguration.ItemCategory.Undefined);
                }
            }
        }

        [Theory]
        [InlineData(100f, 0f, 0f)]
        [InlineData(0f, 100f, 0f)]
        [InlineData(10f, 100f, 0.1f)]
        [InlineData(100f, 10f, 10f)]
        [InlineData(100f, 100f, 1f)]
        public void DamageRatio_All(float damage, float maximum, float expected)
        {
            Assert.Equal(expected, itemConfiguration.DamageRatioTest(damage, maximum));
        }

        [Theory]
        [InlineData(0f, 0f, 0f, 0f, 0f)]
        [InlineData(0f, 0f, 100f, 100f, 0f)]
        [InlineData(1f, 1f, 100f, 0.5f, 50f)]
        [InlineData(1f, 1f, 100f, 1f, 100f)]
        [InlineData(1f, 1f, 100f, 2f, 200f)]
        [InlineData(10f, 5f, 100f, 2f, 100f)]
        public void ScaleDamage_All(float originalSum, float original, float total, float multiplier, float expected)
        {
            Assert.Equal(expected, itemConfiguration.ScaleDamageTest(originalSum, original, total, multiplier));
        }

        // Base values per biome: 10, 18, 31, 54, 94, 164, 287, 503
        [Theory]
        [InlineData(WorldConfiguration.Biome.Meadow, 10f, 0, 0f)]
        [InlineData(WorldConfiguration.Biome.Meadow, 10f, 1, 0f)]
        [InlineData(WorldConfiguration.Biome.Meadow, 10f, 2, 4f)]
        [InlineData(WorldConfiguration.Biome.BlackForest, 10f, 2, 6f)]
        [InlineData(WorldConfiguration.Biome.Swamp, 10f, 2, 12f)]
        [InlineData(WorldConfiguration.Biome.Mountain, 10f, 2, 20f)]
        [InlineData(WorldConfiguration.Biome.Plain, 10f, 2, 36f)]
        [InlineData(WorldConfiguration.Biome.Mistland, 10f, 2, 62f)]
        [InlineData(WorldConfiguration.Biome.AshLand, 10f, 2, 108f)]
        [InlineData(WorldConfiguration.Biome.DeepNorth, 10f, 2, 188f)]
        public void CalculateUpgradeValue_ValueItems(WorldConfiguration.Biome biome, float value, int quality, float expected)
        {
            var scale = worldConfiguration.GetBiomeScaling(biome);
            var nextScale = worldConfiguration.GetNextBiomeScale(biome);

            Assert.Equal(expected, itemConfiguration.CalculateUpgradeValueTest(scale, nextScale, value, quality));
        }

        [Theory]
        [InlineData(WorldConfiguration.Biome.Meadow, 10f, 0, 0f)]
        [InlineData(WorldConfiguration.Biome.Meadow, 10f, 1, 0f)]
        [InlineData(WorldConfiguration.Biome.Meadow, 10f, 2, 1f)]
        [InlineData(WorldConfiguration.Biome.BlackForest, 10f, 2, 2f)]
        [InlineData(WorldConfiguration.Biome.Swamp, 10f, 2, 4f)]
        [InlineData(WorldConfiguration.Biome.Mountain, 10f, 2, 6f)]
        [InlineData(WorldConfiguration.Biome.Plain, 10f, 2, 11f)]
        [InlineData(WorldConfiguration.Biome.Mistland, 10f, 2, 20f)]
        [InlineData(WorldConfiguration.Biome.AshLand, 10f, 2, 36f)]
        [InlineData(WorldConfiguration.Biome.DeepNorth, 10f, 2, 62f)]
        public void CalculateUpgradeValue_DamageItems(WorldConfiguration.Biome biome, float value, int quality, float expected)
        {
            HitData.DamageTypes damageTypes = new HitData.DamageTypes
            {
                m_damage = 0f,
                m_chop = value,
                m_pickaxe = value,
                m_blunt = value,
                m_slash = 0f,
                m_pierce = 0f,
                m_fire = 0f,
                m_frost = 0f,
                m_lightning = 0f,
                m_poison = 0f,
                m_spirit = 0f
            };

            HitData.DamageTypes damageTypesExpected = new HitData.DamageTypes
            {
                m_damage = 0f,
                m_chop = expected,
                m_pickaxe = expected,
                m_blunt = expected,
                m_slash = 0f,
                m_pierce = 0f,
                m_fire = 0f,
                m_frost = 0f,
                m_lightning = 0f,
                m_poison = 0f,
                m_spirit = 0f
            };

            var scale = worldConfiguration.GetBiomeScaling(biome);
            var nextScale = worldConfiguration.GetNextBiomeScale(biome);

            var result = itemConfiguration.CalculateUpgradeValueTest(scale, nextScale, damageTypes, value, quality);

            Assert.Equal(expected, result.m_chop);
            Assert.Equal(expected, result.m_pickaxe);
            Assert.Equal(expected, result.m_blunt);
            Assert.Equal(damageTypesExpected, result);
        }
    }
}