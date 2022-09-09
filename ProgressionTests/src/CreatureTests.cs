using NUnit.Framework;
using VentureValheim.Progression;

namespace VentureValheim.ProgressionTests
{
    [TestFixture]
    public class CreatureTests
    {
        private float scale = 0.75f;

        [OneTimeSetUp]
        public void Setup()
        {
            WorldConfiguration.Instance.Initialize((int)WorldConfiguration.Scaling.Exponential, scale);
            CreatureConfiguration.Instance.Initialize();
            ItemConfiguration.Instance.Initialize();
        }

        [TestCase(WorldConfiguration.Difficulty.Harmless, 5)]
        [TestCase(WorldConfiguration.Difficulty.Novice, 10)]
        [TestCase(WorldConfiguration.Difficulty.Average, 30)]
        [TestCase(WorldConfiguration.Difficulty.Intermediate, 50)]
        [TestCase(WorldConfiguration.Difficulty.Expert, 200)]
        [TestCase(WorldConfiguration.Difficulty.Boss, 500)]
        public void GetCreatureHealth_EnsureDefaultUnchanged(WorldConfiguration.Difficulty d, int expected)
        {
            var health = CreatureConfiguration.Instance.GetBaseHealth(d);

            Assert.AreEqual(expected, health);
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
            var test = new CreatureConfiguration.CreatureClassification("test", biome, d, null);
            Assert.AreEqual(expected, CreatureConfiguration.Instance.CalculateHealth(test));
        }

        [TestCase(WorldConfiguration.Difficulty.Harmless, 0)]
        [TestCase(WorldConfiguration.Difficulty.Novice, 5)]
        [TestCase(WorldConfiguration.Difficulty.Average, 10)]
        [TestCase(WorldConfiguration.Difficulty.Intermediate, 12)]
        [TestCase(WorldConfiguration.Difficulty.Expert, 15)]
        [TestCase(WorldConfiguration.Difficulty.Boss, 20)]
        public void GetCreatureDamage_EnsureDefaultUnchanged(WorldConfiguration.Difficulty d, int expected)
        {
            var damage = CreatureConfiguration.Instance.GetBaseDamage(d);
            Assert.AreEqual(expected, damage);
        }

        [Test]
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

            float sumDamage = ItemConfiguration.Instance.GetTotalDamage(damageTypes, false);

            Assert.AreEqual(90f, sumDamage);
        }

        [TestCase(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Harmless, 0)]
        [TestCase(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Novice, 5)]
        [TestCase(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Average, 10)]
        [TestCase(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Intermediate, 12)]
        [TestCase(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Expert, 15)]
        [TestCase(WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Boss, 20)]
        [TestCase(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Harmless, 0)]
        [TestCase(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Novice, 30)]
        [TestCase(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Average, 91)]
        [TestCase(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Intermediate, 153)]
        [TestCase(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Expert, 612)]
        [TestCase(WorldConfiguration.Biome.Swamp, WorldConfiguration.Difficulty.Boss, 1530)]
        [TestCase(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Harmless, 0)]
        [TestCase(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Novice, 93)]
        [TestCase(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Average, 281)]
        [TestCase(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Intermediate, 469)]
        [TestCase(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Expert, 1876)]
        [TestCase(WorldConfiguration.Biome.Plain, WorldConfiguration.Difficulty.Boss, 4690)]
        public void GetCreatureDamage_All(WorldConfiguration.Biome biome, WorldConfiguration.Difficulty d, int expected)
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

            float sumDamage = ItemConfiguration.Instance.GetTotalDamage(damageTypes, false);
            var newDamage = CreatureConfiguration.Instance.GetBaseDamage(d);
            var ratio = newDamage / sumDamage;

            var result = ItemConfiguration.Instance.CalculateDamageTypes(biome, damageTypes, newDamage, ratio, false);

            var expectedDamage = new HitData.DamageTypes();
            expectedDamage.m_blunt = expected;

            Assert.AreEqual(expectedDamage, result);
        }
    }
}
