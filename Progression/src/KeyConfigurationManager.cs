using System.Collections.Generic;
using BepInEx;

namespace VentureValheim.Progression
{
    public partial class KeyManager
    {
        public string BlockedGlobalKeys { get; protected set; }
        public string AllowedGlobalKeys { get; protected set; }
        public string EnforcedGlobalKeys { get; protected set; }
        public string BlockedPrivateKeys { get; protected set; }
        public string AllowedPrivateKeys { get; protected set; }
        public string EnforcedPrivateKeys { get; protected set; }
        public string TamingKeys { get; protected set; }
        public string SummoningKeys { get; protected set; }
        public HashSet<string> BlockedGlobalKeysList { get; protected set; }
        public HashSet<string> AllowedGlobalKeysList { get; protected set; }
        public HashSet<string> EnforcedGlobalKeysList { get; protected set; }
        public HashSet<string> BlockedPrivateKeysList { get; protected set; }
        public HashSet<string> AllowedPrivateKeysList { get; protected set; }
        public HashSet<string> EnforcedPrivateKeysList { get; protected set; }
        public Dictionary<string, string> TamingKeysList { get; protected set; }
        public Dictionary<string, string> SummoningKeysList { get; protected set; }

        // Cache for Hildir's items
        public Dictionary<string, string> HildirOriginalItemsList { get; protected set; }

        public void UpdateConfigurations()
        {
            UpdateConfigs();

            // Only update skill configurations if enabled
            if (ProgressionConfiguration.Instance.GetEnableSkillManager())
            {
                _cachedPublicBossKeys = CountPublicBossKeys();
                _cachedPrivateBossKeys = CountPrivateBossKeys();

                SkillsManager.Instance.UpdateCache();
            }
        }

        protected void ResetConfigurations()
        {
            BlockedGlobalKeys = "";
            AllowedGlobalKeys = "";
            EnforcedGlobalKeys = "";
            BlockedGlobalKeysList = new HashSet<string>();
            AllowedGlobalKeysList = new HashSet<string>();
            EnforcedGlobalKeysList = new HashSet<string>();

            BlockedPrivateKeys = "";
            AllowedPrivateKeys = "";
            EnforcedPrivateKeys = "";
            BlockedPrivateKeysList = new HashSet<string>();
            AllowedPrivateKeysList = new HashSet<string>();
            EnforcedPrivateKeysList = new HashSet<string>();

            // Null if defaults need to be set
            TamingKeys = null;
            SummoningKeys = null;
        }

        protected void ResetServer()
        {
            ServerPrivateKeysList = new Dictionary<long, HashSet<string>>();
        }

        protected void ResetPlayer()
        {
            PrivateKeysList = new HashSet<string>();

            _cachedPublicBossKeys = -1;
            _cachedPrivateBossKeys = -1;
        }

        /// <summary>
        /// Counts all the boss keys in the Player's private list.
        /// </summary>
        /// <returns></returns>
        protected int CountPrivateBossKeys()
        {
            int count = 0;

            foreach (var key in BossKeyOrderList.Keys)
            {
                if (!key.IsNullOrWhiteSpace() && PrivateKeysList.Contains(key))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Counts all the boss keys in the public list.
        /// </summary>
        /// <returns></returns>
        private int CountPublicBossKeys()
        {
            int count = 0;

            foreach (var key in BossKeyOrderList.Keys)
            {
                if (!key.IsNullOrWhiteSpace() && HasGlobalKey(key))
                {
                    count++;
                }
            }

            return count;
        }

        private void UpdateConfigs()
        {
            UpdateGlobalKeyConfiguration(ProgressionConfiguration.Instance.GetBlockedGlobalKeys(), ProgressionConfiguration.Instance.GetAllowedGlobalKeys());
            UpdatePrivateKeyConfiguration(ProgressionConfiguration.Instance.GetBlockedPrivateKeys(), ProgressionConfiguration.Instance.GetAllowedPrivateKeys());
            UpdateEnforcedKeyConfiguration(ProgressionConfiguration.Instance.GetEnforcedGlobalKeys(), ProgressionConfiguration.Instance.GetEnforcedPrivateKeys());
            UpdateTamingConfiguration(ProgressionConfiguration.Instance.GetOverrideLockTamingDefaults());
            UpdateSummoningConfiguration(ProgressionConfiguration.Instance.GetOverrideLockBossSummonsDefaults());
        }

        /// <summary>
        /// Set the values for BlockedGlobalKeysList and AllowedGlobalKeysList if changed.
        /// </summary>
        /// <param name="blockedGlobalKeys"></param>
        /// <param name="allowedGlobalKeys"></param>
        protected void UpdateGlobalKeyConfiguration(string blockedGlobalKeys, string allowedGlobalKeys)
        {
            if (BlockedGlobalKeys == null || !BlockedGlobalKeys.Equals(blockedGlobalKeys))
            {
                BlockedGlobalKeys = blockedGlobalKeys;
                BlockedGlobalKeysList = ProgressionAPI.StringToSet(blockedGlobalKeys);
            }

            if (AllowedGlobalKeys == null || !AllowedGlobalKeys.Equals(allowedGlobalKeys))
            {
                AllowedGlobalKeys = allowedGlobalKeys;
                AllowedGlobalKeysList = ProgressionAPI.StringToSet(allowedGlobalKeys);
            }
        }

        /// <summary>
        /// Set the values for BlockedPrivateKeysList and AllowedPrivateKeysList if changed.
        /// </summary>
        /// <param name="blockedPrivateKeys"></param>
        /// <param name="allowedPrivateKeys"></param>
        protected void UpdatePrivateKeyConfiguration(string blockedPrivateKeys, string allowedPrivateKeys)
        {
            if (BlockedPrivateKeys == null || !BlockedPrivateKeys.Equals(blockedPrivateKeys))
            {
                BlockedPrivateKeys = blockedPrivateKeys;
                BlockedPrivateKeysList = ProgressionAPI.StringToSet(blockedPrivateKeys);
            }

            if (AllowedPrivateKeys == null || !AllowedPrivateKeys.Equals(allowedPrivateKeys))
            {
                AllowedPrivateKeys = allowedPrivateKeys;
                AllowedPrivateKeysList = ProgressionAPI.StringToSet(allowedPrivateKeys);
            }
        }

        /// <summary>
        /// Set the values for EnforcedGlobalKeysList and EnforcedPrivateKeysList if changed.
        /// </summary>
        /// <param name="blockedPrivateKeys"></param>
        /// <param name="allowedPrivateKeys"></param>
        protected void UpdateEnforcedKeyConfiguration(string enforcedGlobalKeys, string enforcedPrivateKeys)
        {
            if (EnforcedGlobalKeys == null || !EnforcedGlobalKeys.Equals(enforcedGlobalKeys))
            {
                EnforcedGlobalKeys = enforcedGlobalKeys;
                EnforcedGlobalKeysList = ProgressionAPI.StringToSet(enforcedGlobalKeys);
            }

            if (EnforcedPrivateKeys == null || !EnforcedPrivateKeys.Equals(enforcedPrivateKeys))
            {
                EnforcedPrivateKeys = enforcedPrivateKeys;
                EnforcedPrivateKeysList = ProgressionAPI.StringToSet(enforcedPrivateKeys);
            }
        }

        /// <summary>
        /// Set the values for TamingKeysList if changed.
        /// </summary>
        /// <param name="tamingString"></param>
        protected void UpdateTamingConfiguration(string tamingString)
        {
            if (TamingKeys == null || !TamingKeys.Equals(tamingString))
            {
                TamingKeys = tamingString;

                if (TamingKeys.IsNullOrWhiteSpace())
                {
                    TamingKeysList = new Dictionary<string, string>
                    {
                        { "Wolf", BOSS_KEY_SWAMP },
                        { "Lox", BOSS_KEY_MOUNTAIN }
                    };
                }
                else
                {
                    TamingKeysList = ProgressionAPI.StringToDictionary(tamingString);
                }
            }
        }

        /// <summary>
        /// Set the values for SummoningKeysList if changed.
        /// </summary>
        /// <param name="summoningString"></param>
        protected void UpdateSummoningConfiguration(string summoningString)
        {
            if (SummoningKeys == null || !SummoningKeys.Equals(summoningString))
            {
                SummoningKeys = summoningString;

                if (SummoningKeys.IsNullOrWhiteSpace())
                {
                    SummoningKeysList = new Dictionary<string, string>
                    {
                        { "Eikthyr", "" },
                        { "gd_king", BOSS_KEY_MEADOW },
                        { "Bonemass", BOSS_KEY_BLACKFOREST },
                        { "Dragon", BOSS_KEY_SWAMP },
                        { "GoblinKing", BOSS_KEY_MOUNTAIN },
                        { "SeekerQueen", BOSS_KEY_PLAIN },
                    };
                }
                else
                {
                    SummoningKeysList = ProgressionAPI.StringToDictionary(summoningString);
                }
            }
        }

        /// <summary>
        /// Reads the configuration values and creates a dictionary of all Haldor items
        /// and their new key requirements.
        /// </summary>
        /// <returns></returns>
        protected Dictionary<string, string> GetTraderConfiguration()
        {
            var trades = new Dictionary<string, string>();
            if (!ProgressionConfiguration.Instance.GetHelmetYuleKey().IsNullOrWhiteSpace())
            {
                trades.Add("HelmetYule", ProgressionConfiguration.Instance.GetHelmetYuleKey());
            }
            if (!ProgressionConfiguration.Instance.GetHelmetDvergerKey().IsNullOrWhiteSpace())
            {
                trades.Add("HelmetDverger", ProgressionConfiguration.Instance.GetHelmetDvergerKey());
            }
            if (!ProgressionConfiguration.Instance.GetBeltStrengthKey().IsNullOrWhiteSpace())
            {
                trades.Add("BeltStrength", ProgressionConfiguration.Instance.GetBeltStrengthKey());
            }
            if (!ProgressionConfiguration.Instance.GetYmirRemainsKey().IsNullOrWhiteSpace())
            {
                trades.Add("YmirRemains", ProgressionConfiguration.Instance.GetYmirRemainsKey());
            }
            if (!ProgressionConfiguration.Instance.GetFishingRodKey().IsNullOrWhiteSpace())
            {
                trades.Add("FishingRod", ProgressionConfiguration.Instance.GetFishingRodKey());
            }
            if (!ProgressionConfiguration.Instance.GetFishingBaitKey().IsNullOrWhiteSpace())
            {
                trades.Add("FishingBait", ProgressionConfiguration.Instance.GetFishingBaitKey());
            }
            if (!ProgressionConfiguration.Instance.GetThunderstoneKey().IsNullOrWhiteSpace())
            {
                trades.Add("Thunderstone", ProgressionConfiguration.Instance.GetThunderstoneKey());
            }
            if (!ProgressionConfiguration.Instance.GetChickenEggKey().IsNullOrWhiteSpace())
            {
                trades.Add("ChickenEgg", ProgressionConfiguration.Instance.GetChickenEggKey());
            }

            return trades;
        }

        /// <summary>
        /// Record Hildir's original item-key requirements.
        /// </summary>
        /// <param name="items"></param>
        protected void SetHildirOriginalItemsList(List<Trader.TradeItem> items)
        {
            if (HildirOriginalItemsList == null)
            {
                HildirOriginalItemsList = new Dictionary<string, string>();
                foreach (var item in items)
                {
                    if (item.m_prefab != null)
                    {
                        var name = Utils.GetPrefabName(item.m_prefab.gameObject);
                        if (!HildirOriginalItemsList.ContainsKey(name))
                        {
                            HildirOriginalItemsList.Add(name, item.m_requiredGlobalKey);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reads the configuration values and creates a dictionary of all Hildir items
        /// and their new key requirements.
        /// </summary>
        /// <returns></returns>
        protected Dictionary<string, string> GetHildirConfiguration(List<Trader.TradeItem> items)
        {
            SetHildirOriginalItemsList(items);

            var keyReplacements = new Dictionary<string, string>();
            if (!ProgressionConfiguration.Instance.GetCryptItemsKey().IsNullOrWhiteSpace())
            {
                keyReplacements.Add(HILDIR_KEY_CRYPT, ProgressionConfiguration.Instance.GetCryptItemsKey());
            }
            if (!ProgressionConfiguration.Instance.GetCaveItemsKey().IsNullOrWhiteSpace())
            {
                keyReplacements.Add(HILDIR_KEY_CAVE, ProgressionConfiguration.Instance.GetCaveItemsKey());
            }
            if (!ProgressionConfiguration.Instance.GetTowerItemsKey().IsNullOrWhiteSpace())
            {
                keyReplacements.Add(HILDIR_KEY_TOWER, ProgressionConfiguration.Instance.GetTowerItemsKey());
            }

            var trades = new Dictionary<string, string>();
            foreach (var item in HildirOriginalItemsList)
            {
                if (keyReplacements.ContainsKey(item.Value))
                {
                    trades.Add(item.Key, keyReplacements[item.Value]);
                }
            }

            return trades;
        }
    }
}