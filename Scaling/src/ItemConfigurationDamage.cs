using System;

namespace VentureValheim.Scaling
{
    public partial class ItemConfiguration : IItemConfiguration
    {
        private void InitializeBaseDamageValues()
        {
            AddBaseItemValue(ItemType.Primative, 8f);
            AddBaseItemValue(ItemType.Knife, 8f);
            AddBaseItemValue(ItemType.GemKnife, 9f);
            AddBaseItemValue(ItemType.Fist, 8f);
            AddBaseItemValue(ItemType.Ammo, 12f);
            AddBaseItemValue(ItemType.Bolt, 12f);
            AddBaseItemValue(ItemType.TurretBolt, 25f);
            AddBaseItemValue(ItemType.PickAxe, 10f);
            AddBaseItemValue(ItemType.Sword, 12f);
            AddBaseItemValue(ItemType.GemSword, 13f);
            AddBaseItemValue(ItemType.Mace, 12f);
            AddBaseItemValue(ItemType.GemMace, 13f);
            AddBaseItemValue(ItemType.Spear, 12f);
            AddBaseItemValue(ItemType.GemSpear, 13f);
            AddBaseItemValue(ItemType.Axe, 12f);
            AddBaseItemValue(ItemType.GemAxe, 13f);
            AddBaseItemValue(ItemType.Sledge, 15f);
            AddBaseItemValue(ItemType.GemSledge, 16f);
            AddBaseItemValue(ItemType.Atgeir, 15f);
            AddBaseItemValue(ItemType.GemAtgeir, 16f);
            AddBaseItemValue(ItemType.Battleaxe, 15f);
            AddBaseItemValue(ItemType.GemBattleaxe, 16f);
            AddBaseItemValue(ItemType.Bow, 18f);
            AddBaseItemValue(ItemType.GemBow, 19f);
            AddBaseItemValue(ItemType.Crossbow, 40f);
            AddBaseItemValue(ItemType.GemCrossbow, 42f);
            AddBaseItemValue(ItemType.StaffRapid, 5f);
            AddBaseItemValue(ItemType.StaffSlow, 24f);

            AddBaseItemValue(ItemType.Tool, 0f);
            AddBaseItemValue(ItemType.Utility, 0f);
            AddBaseItemValue(ItemType.None, 0f);
            AddBaseItemValue(ItemType.Undefined, 0f);
        }

        private void InitializeWeapons()
        {
            AddItemConfiguration("PlayerUnarmed", WorldConfiguration.Biome.Meadow, ItemType.Fist);
            AddItemConfiguration("Club", WorldConfiguration.Biome.Meadow, ItemType.Primative);
            AddItemConfiguration("AxeStone", WorldConfiguration.Biome.Meadow, ItemType.Primative);
            AddItemConfiguration("PickaxeStone", WorldConfiguration.Biome.Meadow, ItemType.Primative);
            AddItemConfiguration("Torch", WorldConfiguration.Biome.Meadow, ItemType.Primative);

            AddItemConfiguration("AxeFlint", WorldConfiguration.Biome.Meadow, ItemType.Axe);
            AddItemConfiguration("AxeBronze", WorldConfiguration.Biome.BlackForest, ItemType.Axe);
            AddItemConfiguration("AxeIron", WorldConfiguration.Biome.Swamp, ItemType.Axe);
            AddItemConfiguration("AxeBlackMetal", WorldConfiguration.Biome.Plain, ItemType.Axe);
            AddItemConfiguration("AxeJotunBane", WorldConfiguration.Biome.Mistland, ItemType.Axe);
            AddItemConfiguration("AxeBerzerkr", WorldConfiguration.Biome.AshLand, ItemType.Axe);
            AddItemConfiguration("AxeBerzerkrBlood", WorldConfiguration.Biome.AshLand, ItemType.Axe);
            AddItemConfiguration("AxeBerzerkrLightning", WorldConfiguration.Biome.AshLand, ItemType.GemAxe);
            AddItemConfiguration("AxeBerzerkrNature", WorldConfiguration.Biome.AshLand, ItemType.GemAxe);

            AddItemConfiguration("PickaxeAntler", WorldConfiguration.Biome.Meadow, ItemType.PickAxe);
            AddItemConfiguration("PickaxeBronze", WorldConfiguration.Biome.BlackForest, ItemType.PickAxe);
            AddItemConfiguration("PickaxeIron", WorldConfiguration.Biome.Swamp, ItemType.PickAxe);
            AddItemConfiguration("PickaxeBlackMetal", WorldConfiguration.Biome.Plain, ItemType.PickAxe);

            AddItemConfiguration("KnifeChitin", WorldConfiguration.Biome.Ocean, ItemType.Knife);
            AddItemConfiguration("KnifeFlint", WorldConfiguration.Biome.Meadow, ItemType.Knife);
            AddItemConfiguration("KnifeCopper", WorldConfiguration.Biome.BlackForest, ItemType.Knife);
            AddItemConfiguration("KnifeSilver", WorldConfiguration.Biome.Mountain, ItemType.Knife);
            AddItemConfiguration("KnifeBlackMetal", WorldConfiguration.Biome.Plain, ItemType.Knife);
            AddItemConfiguration("KnifeSkollAndHati", WorldConfiguration.Biome.Mistland, ItemType.Knife);

            AddItemConfiguration("MaceBronze", WorldConfiguration.Biome.BlackForest, ItemType.Mace);
            AddItemConfiguration("MaceIron", WorldConfiguration.Biome.Swamp, ItemType.Mace);
            AddItemConfiguration("MaceSilver", WorldConfiguration.Biome.Mountain, ItemType.Mace);
            AddItemConfiguration("MaceNeedle", WorldConfiguration.Biome.Plain, ItemType.Mace);
            AddItemConfiguration("MaceEldner", WorldConfiguration.Biome.AshLand, ItemType.Mace);
            AddItemConfiguration("MaceEldnerBlood", WorldConfiguration.Biome.AshLand, ItemType.Mace);
            AddItemConfiguration("MaceEldnerLightning", WorldConfiguration.Biome.AshLand, ItemType.GemMace);
            AddItemConfiguration("MaceEldnerNature", WorldConfiguration.Biome.AshLand, ItemType.GemMace);

            AddItemConfiguration("SwordBronze", WorldConfiguration.Biome.BlackForest, ItemType.Sword);
            AddItemConfiguration("SwordIron", WorldConfiguration.Biome.Swamp, ItemType.Sword);
            AddItemConfiguration("SwordIronFire", WorldConfiguration.Biome.Swamp, ItemType.Sword);
            AddItemConfiguration("SwordSilver", WorldConfiguration.Biome.Mountain, ItemType.Sword);
            AddItemConfiguration("SwordBlackmetal", WorldConfiguration.Biome.Plain, ItemType.Sword);
            AddItemConfiguration("SwordMistwalker", WorldConfiguration.Biome.Mistland, ItemType.Sword);
            AddItemConfiguration("SwordDyrnwyn", WorldConfiguration.Biome.AshLand, ItemType.GemSword);
            AddItemConfiguration("SwordNiedhogg", WorldConfiguration.Biome.AshLand, ItemType.Sword);
            AddItemConfiguration("SwordNiedhoggBlood", WorldConfiguration.Biome.AshLand, ItemType.Sword);
            AddItemConfiguration("SwordNiedhoggLightning", WorldConfiguration.Biome.AshLand, ItemType.GemSword);
            AddItemConfiguration("SwordNiedhoggNature", WorldConfiguration.Biome.AshLand, ItemType.GemSword);

            AddItemConfiguration("AtgeirBronze", WorldConfiguration.Biome.BlackForest, ItemType.Atgeir);
            AddItemConfiguration("AtgeirIron", WorldConfiguration.Biome.Swamp, ItemType.Atgeir);
            AddItemConfiguration("AtgeirBlackmetal", WorldConfiguration.Biome.Plain, ItemType.Atgeir);
            AddItemConfiguration("AtgeirHimminAfl", WorldConfiguration.Biome.Mistland, ItemType.Atgeir);

            AddItemConfiguration("Battleaxe", WorldConfiguration.Biome.Swamp, ItemType.Battleaxe);
            AddItemConfiguration("BattleaxeCrystal", WorldConfiguration.Biome.Mountain, ItemType.Battleaxe);
            AddItemConfiguration("THSwordKrom", WorldConfiguration.Biome.Mistland, ItemType.Battleaxe);
            AddItemConfiguration("THSwordSlayer", WorldConfiguration.Biome.AshLand, ItemType.Battleaxe);
            AddItemConfiguration("THSwordSlayerBlood", WorldConfiguration.Biome.AshLand, ItemType.Battleaxe);
            AddItemConfiguration("THSwordSlayerLightning", WorldConfiguration.Biome.AshLand, ItemType.GemBattleaxe);
            AddItemConfiguration("THSwordSlayerNature", WorldConfiguration.Biome.AshLand, ItemType.GemBattleaxe);

            AddItemConfiguration("SledgeStagbreaker", WorldConfiguration.Biome.Meadow, ItemType.Sledge);
            AddItemConfiguration("SledgeIron", WorldConfiguration.Biome.Swamp, ItemType.Sledge);
            AddItemConfiguration("SledgeDemolisher", WorldConfiguration.Biome.Mistland, ItemType.Sledge);

            AddItemConfiguration("SpearChitin", WorldConfiguration.Biome.Ocean, ItemType.Spear);
            AddItemConfiguration("SpearFlint", WorldConfiguration.Biome.Meadow, ItemType.Spear);
            AddItemConfiguration("SpearBronze", WorldConfiguration.Biome.BlackForest, ItemType.Spear);
            AddItemConfiguration("SpearElderbark", WorldConfiguration.Biome.Swamp, ItemType.Spear);
            AddItemConfiguration("SpearWolfFang", WorldConfiguration.Biome.Mountain, ItemType.Spear);
            AddItemConfiguration("SpearCarapace", WorldConfiguration.Biome.Mistland, ItemType.Spear);
            AddItemConfiguration("SpearSplitner", WorldConfiguration.Biome.AshLand, ItemType.Spear);
            AddItemConfiguration("SpearSplitner_Blood", WorldConfiguration.Biome.AshLand, ItemType.Spear);
            AddItemConfiguration("SpearSplitner_Lightning", WorldConfiguration.Biome.AshLand, ItemType.GemSpear);
            AddItemConfiguration("SpearSplitner_Nature", WorldConfiguration.Biome.AshLand, ItemType.GemSpear);

            AddItemConfiguration("FistFenrirClaw", WorldConfiguration.Biome.Mountain, ItemType.Fist);

            AddItemConfiguration("Bow", WorldConfiguration.Biome.Meadow, ItemType.Bow);
            AddItemConfiguration("BowFineWood", WorldConfiguration.Biome.BlackForest, ItemType.Bow);
            AddItemConfiguration("BowHuntsman", WorldConfiguration.Biome.Swamp, ItemType.Bow);
            AddItemConfiguration("BowDraugrFang", WorldConfiguration.Biome.Mountain, ItemType.Bow);
            AddItemConfiguration("BowSpineSnap", WorldConfiguration.Biome.Mistland, ItemType.Bow);
            AddItemConfiguration("BowAshlands", WorldConfiguration.Biome.AshLand, ItemType.Bow);
            AddItemConfiguration("BowAshlandsBlood", WorldConfiguration.Biome.AshLand, ItemType.Bow);
            AddItemConfiguration("BowAshlandsRoot", WorldConfiguration.Biome.AshLand, ItemType.GemBow);
            AddItemConfiguration("BowAshlandsStorm", WorldConfiguration.Biome.AshLand, ItemType.GemBow);

            AddItemConfiguration("ArrowWood", WorldConfiguration.Biome.Meadow, ItemType.Ammo);
            AddItemConfiguration("ArrowFlint", WorldConfiguration.Biome.Meadow, ItemType.Ammo);
            AddItemConfiguration("ArrowFire", WorldConfiguration.Biome.Meadow, ItemType.Ammo);
            AddItemConfiguration("ArrowBronze", WorldConfiguration.Biome.BlackForest, ItemType.Ammo);
            AddItemConfiguration("ArrowIron", WorldConfiguration.Biome.Plain, ItemType.Ammo);
            AddItemConfiguration("ArrowSilver", WorldConfiguration.Biome.Mountain, ItemType.Ammo);
            AddItemConfiguration("ArrowPoison", WorldConfiguration.Biome.Mountain, ItemType.Ammo);
            AddItemConfiguration("ArrowObsidian", WorldConfiguration.Biome.Mountain, ItemType.Ammo);
            AddItemConfiguration("ArrowFrost", WorldConfiguration.Biome.Mountain, ItemType.Ammo);
            AddItemConfiguration("ArrowNeedle", WorldConfiguration.Biome.Plain, ItemType.Ammo);
            AddItemConfiguration("ArrowCarapace", WorldConfiguration.Biome.Mistland, ItemType.Ammo);
            AddItemConfiguration("ArrowCharred", WorldConfiguration.Biome.AshLand, ItemType.Ammo);

            AddItemConfiguration("CrossbowArbalest", WorldConfiguration.Biome.Mistland, ItemType.Crossbow);
            AddItemConfiguration("CrossbowRipper", WorldConfiguration.Biome.AshLand, ItemType.Crossbow);
            AddItemConfiguration("CrossbowRipperBlood", WorldConfiguration.Biome.AshLand, ItemType.Crossbow);
            AddItemConfiguration("CrossbowRipperLightning", WorldConfiguration.Biome.AshLand, ItemType.GemCrossbow);
            AddItemConfiguration("CrossbowRipperNature", WorldConfiguration.Biome.AshLand, ItemType.GemCrossbow);

            AddItemConfiguration("BoltBone", WorldConfiguration.Biome.BlackForest, ItemType.Bolt);
            AddItemConfiguration("BoltIron", WorldConfiguration.Biome.Swamp, ItemType.Bolt);
            AddItemConfiguration("BoltBlackmetal", WorldConfiguration.Biome.Plain, ItemType.Bolt);
            AddItemConfiguration("BoltCarapace", WorldConfiguration.Biome.Mistland, ItemType.Bolt);
            AddItemConfiguration("BoltCharred", WorldConfiguration.Biome.AshLand, ItemType.Bolt);

            AddItemConfiguration("TurretBoltWood", WorldConfiguration.Biome.BlackForest, ItemType.TurretBolt);
            AddItemConfiguration("TurretBolt", WorldConfiguration.Biome.Plain, ItemType.TurretBolt);
            AddItemConfiguration("TurretBoltFlametal", WorldConfiguration.Biome.AshLand, ItemType.TurretBolt);

            AddItemConfiguration("StaffIceShards", WorldConfiguration.Biome.Mistland, ItemType.StaffRapid);
            AddItemConfiguration("StaffFireball", WorldConfiguration.Biome.Mistland, ItemType.StaffSlow);
            AddItemConfiguration("StaffClusterbomb", WorldConfiguration.Biome.AshLand, ItemType.StaffRapid);
            AddItemConfiguration("StaffLightning", WorldConfiguration.Biome.AshLand, ItemType.StaffRapid);

            // TODO: Implement projectile aoe damage changes
            //AddItemConfiguration("BombOoze", WorldConfiguration.Biome.Swamp, ItemType.Bomb);
            //AddItemConfiguration("BombBile", WorldConfiguration.Biome.Mistland, ItemType.Bomb);
            //AddItemConfiguration("BombLava", WorldConfiguration.Biome.AshLand, ItemType.Bomb);
            //AddItemConfiguration("BombSmoke", WorldConfiguration.Biome.AshLand, ItemType.Bomb);
        }

        /// <summary>
        /// Get the total damage, excluding chop and pickaxe damage.
        /// </summary>
        /// <param name="originalDamage"></param>
        /// <returns></returns>
        public float GetTotalDamage(HitData.DamageTypes originalDamage)
        {
            var damage = originalDamage.m_damage +
                originalDamage.m_blunt +
                originalDamage.m_slash +
                originalDamage.m_pierce +
                originalDamage.m_fire +
                originalDamage.m_frost +
                originalDamage.m_lightning +
                originalDamage.m_poison +
                originalDamage.m_spirit;

            return damage;
        }

        /// <summary>
        /// Scales the damage given the original damage, new total base damage,
        /// and the original maximum total damage of any of this Creature's attack.
        /// </summary>
        /// <param name="biomeScale"></param>
        /// <param name="originalDamage"></param>
        /// <param name="baseTotalDamage">New base damage to be scaled.</param>
        /// <param name="maxTotalDamage">Maximum damage by any attack this creature has.</param>
        /// <returns></returns>
        public HitData.DamageTypes CalculateCreatureDamageTypes(float biomeScale,
            HitData.DamageTypes originalDamage, float baseTotalDamage, float maxTotalDamage)
        {
            Normalize(ref originalDamage);

            // Consider the maximum total damage this creature can do when auto-scaling for multiple attacks.
            float ratio = DamageRatio(GetTotalDamage(originalDamage), maxTotalDamage);

            var multiplier = ratio * biomeScale;

            return CalculateDamageTypesFinal(originalDamage, baseTotalDamage, multiplier);
        }

        /// <summary>
        /// Scales the damage given the original damage, new total base damage,
        /// and the original maximum total damage of any of this Creature's attack.
        /// </summary>
        /// <param name="biomeScale"></param>
        /// <param name="originalDamage"></param>
        /// <param name="baseTotalDamage">New base damage to be scaled.</param>
        /// <returns></returns>
        public HitData.DamageTypes CalculateItemDamageTypes(float biomeScale,
            HitData.DamageTypes originalDamage, float baseTotalDamage)
        {
            Normalize(ref originalDamage);

            return CalculateDamageTypesFinal(originalDamage, baseTotalDamage, biomeScale);
        }

        /// <summary>
        /// Scales the damage given the original damage and new total base damage.
        /// </summary>
        /// <param name="OriginalDamage"></param>
        /// <param name="baseTotalDamage"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public HitData.DamageTypes CalculateDamageTypesFinal(HitData.DamageTypes OriginalDamage,
            float baseTotalDamage, float multiplier)
        {
            HitData.DamageTypes damageTypes = new HitData.DamageTypes();
            var sum = GetTotalDamage(OriginalDamage);

            // Do not scale chop or pickaxe damage, makes mining stupid
            damageTypes.m_chop = OriginalDamage.m_chop;
            damageTypes.m_pickaxe = OriginalDamage.m_pickaxe;

            damageTypes.m_damage = ScaleDamage(sum, OriginalDamage.m_damage, baseTotalDamage, multiplier);
            damageTypes.m_blunt = ScaleDamage(sum, OriginalDamage.m_blunt, baseTotalDamage, multiplier);
            damageTypes.m_slash = ScaleDamage(sum, OriginalDamage.m_slash, baseTotalDamage, multiplier);
            damageTypes.m_pierce = ScaleDamage(sum, OriginalDamage.m_pierce, baseTotalDamage, multiplier);
            damageTypes.m_fire = ScaleDamage(sum, OriginalDamage.m_fire, baseTotalDamage, multiplier);
            damageTypes.m_frost = ScaleDamage(sum, OriginalDamage.m_frost, baseTotalDamage, multiplier);
            damageTypes.m_lightning = ScaleDamage(sum, OriginalDamage.m_lightning, baseTotalDamage, multiplier);
            damageTypes.m_poison = ScaleDamage(sum, OriginalDamage.m_poison, baseTotalDamage, multiplier);
            damageTypes.m_spirit = ScaleDamage(sum, OriginalDamage.m_spirit, baseTotalDamage, multiplier);

            return damageTypes;
        }

        /// <summary>
        /// Returns the percentage of the maximum damage done by any one damage type given the total damage
        /// </summary>
        /// <param name="totalDamage"></param>
        /// <param name="maxDamageOfAnyAttackType"></param>
        /// <returns></returns>
        protected float DamageRatio(float totalDamage, float maxDamageOfAnyAttackType)
        {
            if (totalDamage == 0f || maxDamageOfAnyAttackType == 0f)
            {
                return 0f;
            }

            return totalDamage / maxDamageOfAnyAttackType;
        }

        /// <summary>
        /// Scale the given newDamage by the multiplier if the original value is greater than 0.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="baseTotalDamage"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        protected float ScaleDamage(float originalSum, float original, float baseTotalDamage, float multiplier)
        {
            if (original <= 0f || baseTotalDamage <= 0f)
            {
                return 0f;
            }

            var value = baseTotalDamage * multiplier * (original / originalSum);
            return (float)Math.Round(value, 1);
        }

        /// <summary>
        /// Sets all negative damage values to 0, use to normalize data for calculations.
        /// </summary>
        /// <param name="OriginalDamage"></param>
        private void Normalize(ref HitData.DamageTypes OriginalDamage)
        {
            OriginalDamage.m_damage = Normalize(OriginalDamage.m_damage);
            OriginalDamage.m_blunt = Normalize(OriginalDamage.m_blunt);
            OriginalDamage.m_slash = Normalize(OriginalDamage.m_slash);
            OriginalDamage.m_pierce = Normalize(OriginalDamage.m_pierce);
            OriginalDamage.m_chop = Normalize(OriginalDamage.m_chop);
            OriginalDamage.m_pickaxe = Normalize(OriginalDamage.m_pickaxe);
            OriginalDamage.m_fire = Normalize(OriginalDamage.m_fire);
            OriginalDamage.m_frost = Normalize(OriginalDamage.m_frost);
            OriginalDamage.m_lightning = Normalize(OriginalDamage.m_lightning);
            OriginalDamage.m_poison = Normalize(OriginalDamage.m_poison);
            OriginalDamage.m_spirit = Normalize(OriginalDamage.m_spirit);
        }

        private float Normalize(float num)
        {
            if (num < 0f)
            {
                return 0f;
            }

            return num;
        }

        /// <summary>
        /// Calculates the item upgrade values such that an item that is fully upgraded will be
        /// as strong as the next biome's natural item.
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="original"></param>
        /// <param name="baseTotalDamage">The item's new base total damage</param>
        /// <param name="quality">The item's max quality, 1 indicated no upgrades for the item</param>
        /// <returns></returns>
        public HitData.DamageTypes CalculateUpgradeValue(WorldConfiguration.Biome biome,
            HitData.DamageTypes original, float baseTotalDamage, int quality)
        {
            var scale = WorldConfiguration.Instance.GetBiomeScaling(biome);
            var nextScale = WorldConfiguration.Instance.GetNextBiomeScale(biome);

            return CalculateUpgradeValue(scale, nextScale, original, baseTotalDamage, quality);
        }

        /// <summary>
        /// Calculates the item upgrade values such that an item that is fully upgraded will be
        /// as strong as the next biome's natural item.
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="nextScale"></param>
        /// <param name="original"></param>
        /// <param name="baseTotalDamage">The item's new base total damage</param>
        /// <param name="quality">The item's max quality, 1 indicates no upgrades for the item</param>
        /// <returns></returns>
        protected HitData.DamageTypes CalculateUpgradeValue(float scale, float nextScale,
            HitData.DamageTypes original, float baseTotalDamage, int quality)
        {
            if (quality <= 1)
            {
                // This item has no upgrades
                return original;
            }

            var startValue = baseTotalDamage * scale;
            var endValue = baseTotalDamage * nextScale;
            var range = endValue - startValue;

            if (range > 0f)
            {
                float damagePerLevel = (float)range / quality;
                // No rounding here
                return CalculateDamageTypesFinal(original, damagePerLevel, 1f);
            }

            return original;
        }

        /// <summary>
        /// Apply Auto-Scaling to the Weapon by Prefab name.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="value">The new total damage.</param>
        public void UpdateWeapon(ref ItemDrop item, HitData.DamageTypes? value,
            int? upgrades, HitData.DamageTypes? upgradeValue, bool playerItem = true)
        {
            // Damage
            if (value == null)
            {
                ScalingPlugin.VentureScalingLogger.LogWarning(
                    $"{item.name} NOT updated with new scaled damage values. DamageTypes undefined.");
                return;
            }

            var original = item.m_itemData.m_shared.m_damages;
            float sumDamage = GetTotalDamage(original);
            float newSumDamage = GetTotalDamage(value.Value);
            item.m_itemData.m_shared.m_damages = value.Value;

            ScalingPlugin.VentureScalingLogger.LogDebug(
                $"{item.name}: Total damage changed from {sumDamage} to {newSumDamage}.");

            // Upgrades
            if (!playerItem)
            {
                return;
            }

            if (upgrades == null)
            {
                ScalingPlugin.VentureScalingLogger.LogWarning(
                    $"{item.name} NOT updated with new scaled upgrade damage values. " +
                    $"Total upgrades undefined.");
                return;
            }

            if (upgradeValue == null)
            {
                ScalingPlugin.VentureScalingLogger.LogWarning(
                    $"{item.name} NOT updated with new scaled upgrade damage values. " +
                    $"DamageTypes for item upgrades undefined.");
                return;
            }

            var quality = item.m_itemData.m_shared.m_maxQuality;
            var upgradeAmount = item.m_itemData.m_shared.m_damagesPerLevel;
            float sumDamageUpgrade = GetTotalDamage(upgradeAmount);
            float newSumDamageUpgrade = GetTotalDamage(upgradeValue.Value);

            item.m_itemData.m_shared.m_maxQuality = upgrades.Value;
            item.m_itemData.m_shared.m_damagesPerLevel = upgradeValue.Value;

            // TODO: Add support for scaling the entire weapon values such as block armor

            ScalingPlugin.VentureScalingLogger.LogDebug(
                $"{item.name}: Total item upgrades changed from {quality} to {upgrades}." +
                $"Total upgrade damage changed from {sumDamageUpgrade} to {newSumDamageUpgrade}");
        }
    }
}
