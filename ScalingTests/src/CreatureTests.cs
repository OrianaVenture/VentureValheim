using Moq;
using Xunit;
using VentureValheim.Scaling;
using static VentureValheim.ScalingTests.WorldTests;
using static VentureValheim.ScalingTests.ItemTests;
using static VentureValheim.Scaling.CreatureOverrides;
using UnityEngine;

namespace VentureValheim.ScalingTests
{
    public class CreatureTests
    {
        public class TestCreatureConfiguration : CreatureConfiguration, ICreatureConfiguration
        {
            public TestCreatureConfiguration(ICreatureConfiguration creature) : base()
            {
                Initialize();
            }

            public CreatureClassification GetCreature(string key)
            {
                return _creatureData[key];
            }

            protected override void CreateVanillaBackup()
            {
                // Do nothing
            }

            protected override void ReadCustomValues()
            {
                // Do nothing
            }
        }

        public class TestCreatureClassification : CreatureClassification
        {
            public TestCreatureClassification(string name, WorldConfiguration.Biome? biomeType, WorldConfiguration.Difficulty? creatureDifficulty) : base(name, biomeType, creatureDifficulty)
            {
            }
            public override bool IgnoreScaling()
            {
                return CreatureDifficulty == WorldConfiguration.Difficulty.Vanilla;
            }
        }

        private HitData.DamageTypes _emptyDamageTypes = new HitData.DamageTypes
        {
            m_damage = 0f,
            m_chop = 0f,
            m_pickaxe = 0f,
            m_blunt = 0f,
            m_slash = 0f,
            m_pierce = 0f,
            m_fire = 0f,
            m_frost = 0f,
            m_lightning = 0f,
            m_poison = 0f,
            m_spirit = 0f
        };

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
        public void GetCreatureHealth_Calculation(WorldConfiguration.Biome biome, WorldConfiguration.Difficulty d, int expected)
        {
            Assert.Equal(expected, creatureConfiguration.CalculateHealth(worldConfiguration.GetBiomeScaling(biome), creatureConfiguration.GetBaseHealth(d)));
        }

        [Fact]
        public void GetCreatureHealth_VanillaDifficulty()
        {
            var test = new TestCreatureClassification("test145", WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Vanilla);
            Assert.Null(test.GetHealth());

            test.SetVanillaData(101, null);
            Assert.Equal(101, test.GetHealth());

            test.OverrideCreature(new CreatureOverride { health = 201 });
            Assert.Equal(101, test.GetHealth());
        }

        [Fact]
        public void GetCreatureHealth_Override()
        {
            var test = new TestCreatureClassification("test275", WorldConfiguration.Biome.Meadow, WorldConfiguration.Difficulty.Average);
            Assert.NotNull(test.GetHealth());

            test.SetVanillaData(101, null);
            Assert.Equal(30, test.GetHealth());

            test.OverrideCreature(new CreatureOverride { health = 201 });
            Assert.Equal(201, test.GetHealth());
        }

        [Theory]
        [InlineData(WorldConfiguration.Difficulty.Harmless, 5)]
        [InlineData(WorldConfiguration.Difficulty.Novice, 15)]
        [InlineData(WorldConfiguration.Difficulty.Average, 20)]
        [InlineData(WorldConfiguration.Difficulty.Intermediate, 25)]
        [InlineData(WorldConfiguration.Difficulty.Expert, 30)]
        [InlineData(WorldConfiguration.Difficulty.Boss, 30)]
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

            float sumDamage = itemConfiguration.GetTotalDamage(damageTypes);

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
            creatureConfiguration.SetBaseDamage(new int[] { 0, 5, 10, 12, 15, 20 });
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

            var max = itemConfiguration.GetTotalDamage(damageTypes);

            var newDamage = creatureConfiguration.GetBaseTotalDamage(d);

            var result = itemConfiguration.CalculateCreatureDamageTypes(worldConfiguration.GetBiome(biome).ScaleValue, damageTypes, newDamage, max);

            Assert.Equal(10f, result.m_chop);
            Assert.Equal(10f, result.m_pickaxe);
            Assert.Equal(expected, result.m_blunt, 1f);
        }

        [Fact]
        public void CreatureOverride_HappyPaths()
        {
            string attack1 = "Attack1";
            string attack2 = "Attack2";
            string attack3 = "Attack3";
            CreatureOverridesList list = new CreatureOverridesList
            {
                creatures = new List<CreatureOverride>
                {
                    new CreatureOverride
                    {
                        name = "TestCreature",
                        health = 100f,
                        biome = 0,
                        difficulty = 1,
                        attacks = new List<AttackOverride>
                        {
                            new AttackOverride
                            {
                                name = attack1,
                                totalDamage = 200f
                            },
                            new AttackOverride
                            {
                                name = attack2,
                                blunt = 200f
                            }
                        }
                    },
                    new CreatureOverride
                    {
                        name = "EmptyCreature"
                    }
                }
            };

            foreach (var entry in list.creatures)
            {
                creatureConfiguration.AddCreatureConfiguration(entry);
            }

            var creature = creatureConfiguration.GetCreature("TestCreature");

            Assert.NotNull(creature);

            Assert.True(creature.HealthOverridden());
            Assert.Equal(100f, creature.OverrideHealth);
            Assert.Equal(WorldConfiguration.Biome.Meadow, creature.BiomeType);
            Assert.Equal(WorldConfiguration.Difficulty.Harmless, creature.CreatureDifficulty);

            Assert.True(creature.AttackOverridden(attack1));
            Assert.Equal(200f, creature.GetAttackOverrideTotal(attack1));
            var attackOverride1 = creature.GetAttackOverride(attack1);
            Assert.NotNull(attackOverride1);
            Assert.Equal(_emptyDamageTypes, attackOverride1);

            Assert.True(creature.AttackOverridden(attack2));
            Assert.Null(creature.GetAttackOverrideTotal(attack2));
            var attackOverride2 = creature.GetAttackOverride(attack2);
            Assert.NotNull(attackOverride2);
            Assert.Equal(200f, attackOverride2.Value.m_blunt);

            Assert.False(creature.AttackOverridden(attack3));
            Assert.Null(creature.GetAttackOverrideTotal(attack3));
            var attackOverride3 = creature.GetAttackOverride(attack3);
            Assert.Null(attackOverride3);

            var creature2 = creatureConfiguration.GetCreature("EmptyCreature");

            Assert.NotNull(creature2);
            Assert.Equal(WorldConfiguration.Biome.Undefined, creature2.BiomeType);
            Assert.Equal(WorldConfiguration.Difficulty.Undefined, creature2.CreatureDifficulty);
            Assert.False(creature2.HealthOverridden());
            Assert.False(creature2.AttackOverridden(attack1));
        }

        [Fact]
        public void SetVanillaData_HappyPaths()
        {
            TestCreatureClassification cc = new TestCreatureClassification("test285", null, null);

            Assert.Equal(WorldConfiguration.Biome.Undefined, cc.BiomeType);
            Assert.Equal(WorldConfiguration.Difficulty.Undefined, cc.CreatureDifficulty);
            Assert.Null(cc.VanillaHealth);
            Assert.Null(cc.VanillaAttacks);

            cc.SetVanillaData(50f, new List<GameObject>());

            Assert.Equal(50f, cc.VanillaHealth);
            Assert.NotNull(cc.VanillaAttacks);

            cc.SetVanillaData(100f, null);

            Assert.Equal(50f, cc.VanillaHealth);
            Assert.NotNull(cc.VanillaAttacks);
        }
    }
}
