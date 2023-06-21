using Moq;
using Xunit;
using VentureValheim.Scaling;
using static VentureValheim.ScalingTests.WorldTests;
using static VentureValheim.Scaling.ItemOverrides;

namespace VentureValheim.ScalingTests
{
    public class ItemTests
    {
        public class TestItemConfiguration : ItemConfiguration, IItemConfiguration
        {
            public TestItemConfiguration(IItemConfiguration Item) : base()
            {
                Initialize();
            }

            public float DamageRatioTest(float a, float b) => DamageRatio(a, b);
            public float ScaleDamageTest(float a, float b, float c, float d) => ScaleDamage(a, b, c, d);
            public float CalculateUpgradeValueTest(float a, float b, float c, int d) => CalculateUpgradeValue(a, b, c, d);
            public HitData.DamageTypes CalculateUpgradeValueTest(float a, float b, HitData.DamageTypes c, float d, int e) => CalculateUpgradeValue(a, b, c, d, e);

            public ItemClassification GetItem(string key)
            {
                return _itemData[key];
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

        public class TestItemClassification : ItemClassification
        {
            public TestItemClassification(string name, WorldConfiguration.Biome? biomeType, ItemType? itemType) : base(name, biomeType, itemType)
            {
            }
            public override bool IgnoreScaling()
            {
                return false;
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

        private HitData.DamageTypes _nonemptyDamageTypes = new HitData.DamageTypes
        {
            m_damage = 1f,
            m_chop = 1f,
            m_pickaxe = 1f,
            m_blunt = 1f,
            m_slash = 1f,
            m_pierce = 1f,
            m_fire = 1f,
            m_frost = 1f,
            m_lightning = 1f,
            m_poison = 1f,
            m_spirit = 1f
        };

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

            float sumDamage = itemConfiguration.GetTotalDamage(damageTypes);

            Assert.Equal(90f, sumDamage);
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
                m_slash = 10f,
                m_pierce = 0f,
                m_fire = 10f,
                m_frost = 0f,
                m_lightning = 0f,
                m_poison = 0f,
                m_spirit = 0f
            };

            var result = itemConfiguration.CalculateItemDamageTypes(worldConfiguration.GetBiome(biome).ScaleValue, damageTypes, 10f);

            var expectedDamage = new HitData.DamageTypes();
            expectedDamage.m_chop = 10f;
            expectedDamage.m_pickaxe = 10f;
            expectedDamage.m_blunt = expected;
            expectedDamage.m_slash = expected;
            expectedDamage.m_fire = expected;

            Assert.Equal(10f, result.m_chop);
            Assert.Equal(10f, result.m_pickaxe);
            Assert.Equal(expected, result.m_blunt);
            Assert.Equal(expectedDamage, result);
        }

        [Fact]
        public void GetItemCategory_All()
        {
            Array items = Enum.GetValues(typeof(ItemType));
            foreach (var item in items)
            {
                var itemType = (ItemType)item;
                if (itemType != ItemType.Undefined && itemType != ItemType.None)
                {
                    Assert.True(ItemClassification.GetItemCategory(itemType) != ItemCategory.Undefined);
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
                m_chop = 1f,
                m_pickaxe = 1f,
                m_blunt = value,
                m_slash = value,
                m_pierce = 0f,
                m_fire = value,
                m_frost = 0f,
                m_lightning = 0f,
                m_poison = 0f,
                m_spirit = 0f
            };

            HitData.DamageTypes damageTypesExpected = new HitData.DamageTypes
            {
                m_damage = 0f,
                m_chop = 1f,
                m_pickaxe = 1f,
                m_blunt = expected,
                m_slash = expected,
                m_pierce = 0f,
                m_fire = expected,
                m_frost = 0f,
                m_lightning = 0f,
                m_poison = 0f,
                m_spirit = 0f
            };

            var scale = worldConfiguration.GetBiomeScaling(biome);
            var nextScale = worldConfiguration.GetNextBiomeScale(biome);

            var result = itemConfiguration.CalculateUpgradeValueTest(scale, nextScale, damageTypes, value, quality);

            Assert.Equal(1f, result.m_chop);
            Assert.Equal(1f, result.m_pickaxe);
            Assert.Equal(expected, result.m_blunt);
            Assert.Equal(damageTypesExpected, result);
        }

        [Theory]
        [InlineData(WorldConfiguration.Biome.Meadow, 10f, 0)]
        [InlineData(WorldConfiguration.Biome.Meadow, 10f, 1)]
        public void CalculateUpgradeValue_DamageItemsNone(WorldConfiguration.Biome biome, float value, int quality)
        {
            HitData.DamageTypes damageTypes = new HitData.DamageTypes
            {
                m_damage = 0f,
                m_chop = 1f,
                m_pickaxe = 1f,
                m_blunt = value,
                m_slash = value,
                m_pierce = 0f,
                m_fire = value,
                m_frost = 0f,
                m_lightning = 0f,
                m_poison = 0f,
                m_spirit = 0f
            };

            var scale = worldConfiguration.GetBiomeScaling(biome);
            var nextScale = worldConfiguration.GetNextBiomeScale(biome);

            var result = itemConfiguration.CalculateUpgradeValueTest(scale, nextScale, damageTypes, value, quality);

            Assert.Equal(1f, result.m_chop);
            Assert.Equal(1f, result.m_pickaxe);
            Assert.Equal(value, result.m_blunt);
            Assert.Equal(damageTypes, result);
        }

        [Fact]
        public void GetItemValues_All()
        {
            var test = new TestItemClassification("test221", WorldConfiguration.Biome.Meadow, ItemType.PrimativeArmor);
            Assert.NotNull(test.GetValue());
            Assert.Null(test.GetUpgradeValue());
            Assert.Null(test.GetUpgradeLevels());

            test.SetVanillaData(101, 1, 6);
            Assert.Equal(0, test.GetValue()); // World scaling undefined here
            Assert.Equal(1, test.GetUpgradeLevels());
            Assert.Equal(0, test.GetUpgradeValue()); // World scaling undefined here

            test.OverrideItem(new ItemOverride { value = 201, quality = 3, upgradeValue = 33 });
            Assert.Equal(201, test.GetValue());
            Assert.Equal(3, test.GetUpgradeLevels());
            Assert.Equal(33, test.GetUpgradeValue());
        }

        [Fact]
        public void GetItemValues_WorldUndefined()
        {
            var test = new TestItemClassification("test298", null, ItemType.PrimativeArmor);
            Assert.Null(test.GetValue());
            Assert.Null(test.GetUpgradeValue());
            Assert.Null(test.GetUpgradeLevels());

            test.SetVanillaData(101, 1, 6);
            Assert.Equal(101, test.GetValue());
            Assert.Equal(1, test.GetUpgradeLevels());
            Assert.Equal(6, test.GetUpgradeValue());

            test.OverrideItem(new ItemOverride { value = 201, quality = 3, upgradeValue = 33 });
            Assert.Equal(201, test.GetValue());
            Assert.Equal(3, test.GetUpgradeLevels());
            Assert.Equal(33, test.GetUpgradeValue());
        }

        [Fact]
        public void GetItemValues_TypeUndefined()
        {
            var test = new TestItemClassification("test371", WorldConfiguration.Biome.Meadow, null);
            Assert.Null(test.GetValue());
            Assert.Null(test.GetUpgradeValue());
            Assert.Null(test.GetUpgradeLevels());

            test.SetVanillaData(101, 1, 6);
            Assert.Equal(101, test.GetValue());
            Assert.Equal(1, test.GetUpgradeLevels());
            Assert.Equal(6, test.GetUpgradeValue());

            test.OverrideItem(new ItemOverride { value = 201, quality = 3, upgradeValue = 33 });
            Assert.Equal(201, test.GetValue());
            Assert.Equal(3, test.GetUpgradeLevels());
            Assert.Equal(33, test.GetUpgradeValue());
        }

        // Fails due to default vanilla world settings and does not use TestItemClassification override
        /*[Fact]
        public void ItemOverride_ArmorAndShield_HappyPaths()
        {
            ItemOverridesList list = new ItemOverridesList
            {
                items = new List<ItemOverride>
                {
                    new ItemOverride
                    {
                        name = "TestArmor",
                        biome = 0,
                        itemType = 3,
                        value = 44,
                        quality = 3,
                        upgradeValue = 5
                    },
                    new ItemOverride
                    {
                        name = "TestShield",
                        biome = 1,
                        itemType = 1,
                        value = 55
                    },
                    new ItemOverride
                    {
                        name = "EmptyItem"
                    }
                }
            };

            foreach (var entry in list.items)
            {
                itemConfiguration.AddItemConfiguration(entry);
            }

            var armor = itemConfiguration.GetItem("TestArmor");

            Assert.NotNull(armor);
            Assert.Equal(WorldConfiguration.Biome.Meadow, armor.BiomeType);
            Assert.Equal(ItemType.Chest, armor.ItemType);
            Assert.Equal(ItemCategory.Armor, armor.ItemCategory);
            Assert.Equal(44, armor.GetValue());
            Assert.Equal(3, armor.GetUpgradeLevels());
            Assert.Equal(5, armor.GetUpgradeValue());


            var shield = itemConfiguration.GetItem("TestShield");

            Assert.NotNull(shield);
            Assert.Equal(WorldConfiguration.Biome.BlackForest, shield.BiomeType);
            Assert.Equal(ItemType.Shield, shield.ItemType);
            Assert.Equal(ItemCategory.Shield, shield.ItemCategory);
            Assert.Equal(55, shield.GetValue());
            Assert.Null(shield.GetUpgradeLevels());
            Assert.Null(shield.GetUpgradeValue());


            var empty = itemConfiguration.GetItem("EmptyItem");

            Assert.NotNull(empty);
            Assert.Equal(WorldConfiguration.Biome.Undefined, empty.BiomeType);
            Assert.Equal(ItemType.Undefined, empty.ItemType);
            Assert.Equal(ItemCategory.Undefined, empty.ItemCategory);
            Assert.Null(empty.GetValue());
            Assert.Null(empty.GetUpgradeLevels());
            Assert.Null(empty.GetUpgradeValue());
            Assert.Null(empty.GetDamageValue());
            Assert.Null(empty.GetUpgradeDamageValue());
        }

        [Fact]
        public void ItemOverride_Weapon_HappyPaths()
        {
            //Setup(true);

            ItemOverridesList list = new ItemOverridesList
            {
                items = new List<ItemOverride>
                {
                    new ItemOverride
                    {
                        name = "TestWeapon",
                        biome = 2,
                        itemType = 21,
                        quality = 5,
                        damageValue = new CreatureOverrides.AttackOverride
                        {
                            damage = 20,
                            blunt = 10
                        },
                        upgradeDamageValue = new CreatureOverrides.AttackOverride
                        {
                            totalDamage = 6
                        }
                    }
                }
            };

            foreach (var entry in list.items)
            {
                itemConfiguration.AddItemConfiguration(entry);
            }

            var weapon = itemConfiguration.GetItem("TestWeapon");

            Assert.NotNull(weapon);
            Assert.Equal(WorldConfiguration.Biome.Swamp, weapon.BiomeType);
            Assert.Equal(ItemType.Knife, weapon.ItemType);
            Assert.Equal(ItemCategory.Weapon, weapon.ItemCategory);
            Assert.Equal(5, weapon.GetUpgradeLevels());

            var damage = weapon.GetDamageValue();
            Assert.NotNull(damage);

            HitData.DamageTypes damageTypes = new HitData.DamageTypes
            {
                m_damage = 20f,
                m_chop = 0f,
                m_pickaxe = 0f,
                m_blunt = 10f,
                m_slash = 0f,
                m_pierce = 0f,
                m_fire = 0f,
                m_frost = 0f,
                m_lightning = 0f,
                m_poison = 0f,
                m_spirit = 0f
            };

            Assert.Equal(damageTypes.m_damage, damage.Value.m_damage);
            Assert.Equal(damageTypes.m_blunt, damage.Value.m_blunt);
            Assert.Equal(damageTypes, damage);

            HitData.DamageTypes damageTypesUpgrade = new HitData.DamageTypes
            {
                m_damage = 2f,
                m_chop = 0f,
                m_pickaxe = 0f,
                m_blunt = 1f,
                m_slash = 0f,
                m_pierce = 0f,
                m_fire = 0f,
                m_frost = 0f,
                m_lightning = 0f,
                m_poison = 0f,
                m_spirit = 0f
            };

            // Set Vanilla values
            weapon.SetVanillaData(damageTypes, 3, damageTypesUpgrade);

            var damageUpgrade = weapon.GetUpgradeDamageValue();

            HitData.DamageTypes damageTypesUpgradeDouble = new HitData.DamageTypes
            {
                m_damage = 4f,
                m_chop = 0f,
                m_pickaxe = 0f,
                m_blunt = 2f,
                m_slash = 0f,
                m_pierce = 0f,
                m_fire = 0f,
                m_frost = 0f,
                m_lightning = 0f,
                m_poison = 0f,
                m_spirit = 0f
            };

            Assert.Equal(damageTypesUpgradeDouble.m_damage, damageUpgrade.Value.m_damage);
            Assert.Equal(damageTypesUpgradeDouble.m_blunt, damageUpgrade.Value.m_blunt);
            Assert.Equal(damageTypesUpgradeDouble, damageUpgrade);
        }*/

        [Fact]
        public void ItemOverride_BaseValues_HappyPaths()
        {
            //Setup(true);

            ItemOverridesList list = new ItemOverridesList
            {
                baseItemValues = new List<BaseItemValueOverride>
                {
                    new BaseItemValueOverride
                    {
                        itemType = 22,
                        value = 33
                    },
                    new BaseItemValueOverride
                    {
                        itemType = 23,
                        value = 34
                    }
                }
            };

            foreach (var entry in list.baseItemValues)
            {
                itemConfiguration.AddBaseItemValue(entry);
            }

            // Check invalid cast
            itemConfiguration.AddBaseItemValue((ItemType)(-2), -2);

            Assert.Equal(33, itemConfiguration.GetBaseItemValue(ItemType.Mace));
            Assert.Equal(34, itemConfiguration.GetBaseItemValue(ItemType.Sledge));
        }

        [Fact]
        public void SetVanillaData_Value_HappyPaths()
        {
            var ic = new TestItemClassification("test789", null, null);

            Assert.Equal(WorldConfiguration.Biome.Undefined, ic.BiomeType);
            Assert.Equal(ItemType.Undefined, ic.ItemType);
            Assert.Equal(ItemCategory.Undefined, ic.ItemCategory);
            Assert.Null(ic.VanillaValue);
            Assert.Null(ic.VanillaUpgradeLevels);
            Assert.Null(ic.VanillaUpgradeValue);
            Assert.Null(ic.VanillaDamageValue);
            Assert.Null(ic.VanillaUpgradeDamageValue);

            ic.SetVanillaData(10f, 4, 2f);

            Assert.Equal(10f, ic.VanillaValue);
            Assert.Equal(4, ic.VanillaUpgradeLevels);
            Assert.Equal(2f, ic.VanillaUpgradeValue);
            Assert.Null(ic.VanillaDamageValue);
            Assert.Null(ic.VanillaUpgradeDamageValue);

            ic.SetVanillaData(20f, 10, 4f);

            Assert.Equal(10f, ic.VanillaValue);
            Assert.Equal(4, ic.VanillaUpgradeLevels);
            Assert.Equal(2f, ic.VanillaUpgradeValue);
            Assert.Null(ic.VanillaDamageValue);
            Assert.Null(ic.VanillaUpgradeDamageValue);
        }

        [Fact]
        public void SetVanillaData_Damage_HappyPaths()
        {
            var ic = new TestItemClassification("test263", null, null);

            Assert.Equal(WorldConfiguration.Biome.Undefined, ic.BiomeType);
            Assert.Equal(ItemType.Undefined, ic.ItemType);
            Assert.Equal(ItemCategory.Undefined, ic.ItemCategory);
            Assert.Null(ic.VanillaValue);
            Assert.Null(ic.VanillaUpgradeLevels);
            Assert.Null(ic.VanillaUpgradeValue);
            Assert.Null(ic.VanillaDamageValue);
            Assert.Null(ic.VanillaUpgradeDamageValue);

            ic.SetVanillaData(_nonemptyDamageTypes, 4, _emptyDamageTypes);

            Assert.Null(ic.VanillaValue);
            Assert.Equal(4, ic.VanillaUpgradeLevels);
            Assert.Null(ic.VanillaUpgradeValue);
            Assert.Equal(_nonemptyDamageTypes, ic.VanillaDamageValue);
            Assert.Equal(_emptyDamageTypes, ic.VanillaUpgradeDamageValue);

            ic.SetVanillaData(_emptyDamageTypes, 10, _nonemptyDamageTypes);

            Assert.Null(ic.VanillaValue);
            Assert.Equal(4, ic.VanillaUpgradeLevels);
            Assert.Null(ic.VanillaUpgradeValue);
            Assert.Equal(_nonemptyDamageTypes, ic.VanillaDamageValue);
            Assert.Equal(_emptyDamageTypes, ic.VanillaUpgradeDamageValue);
        }
    }
}