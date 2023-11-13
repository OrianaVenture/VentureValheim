using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.Progression
{
    public interface IKeyManager
    {
        string BlockedGlobalKeys { get; }
        string AllowedGlobalKeys { get; }
        string EnforcedGlobalKeys { get; }
        string BlockedPrivateKeys { get; }
        string AllowedPrivateKeys { get; }
        string EnforcedPrivateKeys { get; }
        HashSet<string> BlockedGlobalKeysList { get; }
        HashSet<string> AllowedGlobalKeysList { get; }
        HashSet<string> EnforcedGlobalKeysList { get; }
        HashSet<string> BlockedPrivateKeysList { get; }
        HashSet<string> AllowedPrivateKeysList { get; }
        HashSet<string> EnforcedPrivateKeysList { get; }
        HashSet<string> PrivateKeysList { get; }
        public int GetPublicBossKeysCount();
        public int GetPrivateBossKeysCount();
        public bool BlockGlobalKey(bool blockAll, string globalKey);
        public bool HasPrivateKey(string key);
    }

    public partial class KeyManager : IKeyManager
    {
        static KeyManager() { }
        protected KeyManager()
        {
            ServerPrivateKeysList = new Dictionary<string, HashSet<string>>();

            ResetPlayer();
        }

        protected static readonly IKeyManager _instance = new KeyManager();

        public static KeyManager Instance
        {
            get => _instance as KeyManager;
        }

        private KeyManagerUpdater _keyManagerUpdater;

        /// <summary>
        /// Updates class data if cached values have expired.
        /// </summary>
        public class KeyManagerUpdater : MonoBehaviour
        {
            private static float _lastUpdateTime = Time.time - 10f;
            private static readonly float _updateInterval = 10f;

            public void Start()
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"Starting Key Manager Updater.");
            }

            public void Update()
            {
                var time = Time.time;
                var delta = time - _lastUpdateTime;

                if (delta >= _updateInterval)
                {
                    Instance.UpdateConfigs();

                    // Only update skill configurations if enabled
                    if (ProgressionConfiguration.Instance.GetEnableSkillManager())
                    {
                        _cachedPublicBossKeys = Instance.CountPublicBossKeys();
                        _cachedPrivateBossKeys = Instance.CountPrivateBossKeys();

                        SkillsManager.Instance.UpdateCache();
                    }

                    _lastUpdateTime = time;
                }
            }
        }

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

        public HashSet<string> PrivateKeysList { get; protected set; }
        public Dictionary<string, HashSet<string>> ServerPrivateKeysList { get; protected set; }

        public const string BOSS_KEY_MEADOW = "defeated_eikthyr";
        public const string BOSS_KEY_BLACKFOREST = "defeated_gdking";
        public const string BOSS_KEY_SWAMP = "defeated_bonemass";
        public const string BOSS_KEY_MOUNTAIN = "defeated_dragon";
        public const string BOSS_KEY_PLAIN = "defeated_goblinking";
        public const string BOSS_KEY_MISTLAND = "defeated_queen";

        public const string HILDIR_KEY_CRYPT = "Hildir1";
        public const string HILDIR_KEY_CAVE = "Hildir2";
        public const string HILDIR_KEY_TOWER = "Hildir3";

        public const int TOTAL_BOSSES = 6;
        public readonly Dictionary<string, int> BossKeyOrderList = new Dictionary<string, int>
        {
            { "", 0 },
            { BOSS_KEY_MEADOW, 1 },
            { BOSS_KEY_BLACKFOREST, 2 },
            { BOSS_KEY_SWAMP, 3 },
            { BOSS_KEY_MOUNTAIN, 4 },
            { BOSS_KEY_PLAIN, 5 },
            { BOSS_KEY_MISTLAND, 6 }
        };

        public readonly Dictionary<string, string> GuardianKeysList = new Dictionary<string, string>
        {
            { "GP_Eikthyr", BOSS_KEY_MEADOW },
            { "GP_TheElder", BOSS_KEY_BLACKFOREST },
            { "GP_Bonemass", BOSS_KEY_SWAMP },
            { "GP_Moder", BOSS_KEY_MOUNTAIN },
            { "GP_Yagluth", BOSS_KEY_PLAIN },
            { "GP_Queen", BOSS_KEY_MISTLAND }
        };

        public readonly Dictionary<string, string> BossItemKeysList = new Dictionary<string, string>
        {
            { "HardAntler", BOSS_KEY_MEADOW },
            { "CryptKey", BOSS_KEY_BLACKFOREST },
            { "Wishbone", BOSS_KEY_SWAMP },
            { "DragonTear", BOSS_KEY_MOUNTAIN },
            { "YagluthDrop", BOSS_KEY_PLAIN },
            { "DvergrKey", BOSS_KEY_PLAIN },
            { "QueenDrop", BOSS_KEY_MISTLAND }
        };

        public readonly Dictionary<string, string> MaterialKeysList = new Dictionary<string, string>
        {
            // Black Forest
            { "FineWood", BOSS_KEY_MEADOW },
            { "Tin", BOSS_KEY_MEADOW },
            { "Copper", BOSS_KEY_MEADOW },
            { "Bronze", BOSS_KEY_MEADOW },
            { "BronzeNails", BOSS_KEY_MEADOW },
            { "TrollHide", BOSS_KEY_MEADOW },
            // Swamp
            { "Iron", BOSS_KEY_BLACKFOREST },
            { "IronNails", BOSS_KEY_BLACKFOREST },
            { "Chain", BOSS_KEY_BLACKFOREST },
            { "ElderBark", BOSS_KEY_BLACKFOREST },
            { "Root", BOSS_KEY_BLACKFOREST },
            { "SharpeningStone", BOSS_KEY_BLACKFOREST },
            // Mountain
            { "Silver", BOSS_KEY_SWAMP },
            { "WolfHairBundle", BOSS_KEY_SWAMP },
            { "WolfPelt", BOSS_KEY_SWAMP },
            { "WolfClaw", BOSS_KEY_SWAMP },
            { "WolfFang", BOSS_KEY_SWAMP },
            { "JuteRed", BOSS_KEY_SWAMP },
            { "Obsidian", BOSS_KEY_SWAMP },
            // Plains
            { "BlackMetal", BOSS_KEY_MOUNTAIN },
            { "Tar", BOSS_KEY_MOUNTAIN },
            { "Needle", BOSS_KEY_MOUNTAIN },
            { "LinenThread", BOSS_KEY_MOUNTAIN },
            { "LoxPelt", BOSS_KEY_MOUNTAIN },
            // Mistlands
            { "BlackMarble", BOSS_KEY_PLAIN },
            { "BlackCore", BOSS_KEY_PLAIN },
            { "Carapace", BOSS_KEY_PLAIN },
            { "Eitr", BOSS_KEY_PLAIN },
            { "ScaleHide", BOSS_KEY_PLAIN },
            { "Wisp", BOSS_KEY_PLAIN },
            { "YggdrasilWood", BOSS_KEY_PLAIN },
            { "JuteBlue", BOSS_KEY_PLAIN }
        };

        public readonly Dictionary<string, string> FoodKeysList = new Dictionary<string, string>
        {
            // Black Forest
            { "Blueberries", BOSS_KEY_MEADOW },
            { "MushroomYellow", BOSS_KEY_MEADOW },
            { "Carrot", BOSS_KEY_MEADOW },
            { "Thistle", BOSS_KEY_MEADOW },
            { "Entrails", BOSS_KEY_MEADOW }, // soft lock due to draugr villages
            // Swamp
            { "Turnip", BOSS_KEY_BLACKFOREST },
            { "Bloodbag", BOSS_KEY_BLACKFOREST },
            { "Ooze", BOSS_KEY_BLACKFOREST },
            { "SerpentMeat", BOSS_KEY_BLACKFOREST },
            { "SerpentMeatCooked", BOSS_KEY_BLACKFOREST },
            // Mountain
            { "FreezeGland", BOSS_KEY_SWAMP },
            { "WolfMeat", BOSS_KEY_SWAMP },
            { "Onion", BOSS_KEY_SWAMP },
            // Plains
            { "Barley", BOSS_KEY_MOUNTAIN },
            { "LoxMeat", BOSS_KEY_MOUNTAIN },
            { "BarleyFlour", BOSS_KEY_MOUNTAIN },
            { "Cloudberry", BOSS_KEY_MOUNTAIN },
            { "ChickenMeat", BOSS_KEY_MOUNTAIN },
            { "ChickenEgg", BOSS_KEY_MOUNTAIN },
            { "BreadDough", BOSS_KEY_MOUNTAIN },
            // Mistlands
            { "GiantBloodSack", BOSS_KEY_PLAIN },
            { "BugMeat", BOSS_KEY_PLAIN },
            { "RoyalJelly", BOSS_KEY_PLAIN },
            { "HareMeat", BOSS_KEY_PLAIN },
            { "Sap", BOSS_KEY_PLAIN },
            { "MushroomJotunPuffs", BOSS_KEY_PLAIN }
        };

        public Dictionary<string, string> HildirOriginalItemsList { get; protected set; }

        public const string RPCNAME_ServerListKeys = "VV_ServerListKeys";
        public const string RPCNAME_ServerSetPrivateKeys = "VV_ServerSetPrivateKeys";
        public const string RPCNAME_ServerSetPrivateKey = "VV_ServerSetPrivateKey";
        public const string RPCNAME_ServerRemovePrivateKey = "VV_ServerRemovePrivateKey";
        public const string RPCNAME_SetPrivateKey = "VV_SetPrivateKey";
        public const string RPCNAME_RemovePrivateKey = "VV_RemovePrivateKey";
        public const string RPCNAME_ResetPrivateKeys = "VV_ResetPrivateKeys";

        public const string PLAYER_SAVE_KEY = "VV_PrivateKeys";

        private static int _cachedPublicBossKeys = -1;
        private static int _cachedPrivateBossKeys = -1;

        protected void Reset()
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

        protected void ResetPlayer()
        {
            Reset();

            PrivateKeysList = new HashSet<string>();

            _cachedPublicBossKeys = -1;
            _cachedPrivateBossKeys = -1;
        }

        public int GetPublicBossKeysCount()
        {
            return _cachedPublicBossKeys;
        }

        public int GetPrivateBossKeysCount()
        {
            return _cachedPrivateBossKeys;
        }

        public void UpdateConfigs()
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
            if (!BlockedGlobalKeys.Equals(blockedGlobalKeys))
            {
                BlockedGlobalKeys = blockedGlobalKeys;
                BlockedGlobalKeysList = ProgressionAPI.StringToSet(blockedGlobalKeys);
            }

            if (!AllowedGlobalKeys.Equals(allowedGlobalKeys))
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
            if (!BlockedPrivateKeys.Equals(blockedPrivateKeys))
            {
                BlockedPrivateKeys = blockedPrivateKeys;
                BlockedPrivateKeysList = ProgressionAPI.StringToSet(blockedPrivateKeys);
            }

            if (!AllowedPrivateKeys.Equals(allowedPrivateKeys))
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
            if (!EnforcedGlobalKeys.Equals(enforcedGlobalKeys))
            {
                EnforcedGlobalKeys = enforcedGlobalKeys;
                EnforcedGlobalKeysList = ProgressionAPI.StringToSet(enforcedGlobalKeys);
            }

            if (!EnforcedPrivateKeys.Equals(enforcedPrivateKeys))
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

        private bool IsWorldModifier(string key)
        {
            ZoneSystem.GetKeyValue(key.ToLower(), out string value, out GlobalKeys gk);
            if (gk < GlobalKeys.NonServerOption || gk == GlobalKeys.activeBosses)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Whether to block a Global Key based on configuration settings.
        /// </summary>
        /// <param name="blockAll"></param>
        /// <param name="globalKey"></param>
        /// <returns>True when default blocked and does not exist in the allowed list,
        /// or when default unblocked and key is in the blocked list.</returns>
        public bool BlockGlobalKey(bool blockAll, string globalKey)
        {
            if (globalKey.IsNullOrWhiteSpace())
            {
                return true;
            }

            if (IsWorldModifier(globalKey))
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"{globalKey} is non-qualifying, can not block.");
                return false;
            }

            if (blockAll)
            {
                return (AllowedGlobalKeysList.Count > 0) ? !AllowedGlobalKeysList.Contains(globalKey) : true;
            }

            return (BlockedGlobalKeysList.Count > 0) ? BlockedGlobalKeysList.Contains(globalKey) : false;
        }

        /// <summary>
        /// Whether to block a private Key based on configuration settings.
        /// If instance is a dedicated server returns true.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool BlockPrivateKey(string key)
        {
            if (ProgressionConfiguration.Instance.GetUsePrivateKeys() && !ZNet.instance.IsDedicated())
            {
                return PrivateKeyIsBlocked(key);
            }

            return true;
        }

        /// <summary>
        /// Whether to block a private Key based on list configurations.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>True if blocked and in the blocked list,
        /// false when allowed and in the allowed list or both lists are empty.</returns>
        protected bool PrivateKeyIsBlocked(string key)
        {
            if (key.IsNullOrWhiteSpace() || IsWorldModifier(key))
            {
                return true;
            }

            if (BlockedPrivateKeysList.Count > 0)
            {
                return BlockedPrivateKeysList.Contains(key);
            }
            else if (AllowedPrivateKeysList.Count > 0)
            {
                return !AllowedPrivateKeysList.Contains(key);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns whether the Player has unlocked the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasPrivateKey(string key)
        {
            if (key.IsNullOrWhiteSpace())
            {
                return true;
            }

            return PrivateKeysList.Contains(key);
        }

        /// <summary>
        /// Returns whether the Player has unlocked the given key, or false if no keys are recorded.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name">Player name</param>
        /// <returns></returns>
        public bool ServerHasPrivateKey(string key, string name)
        {
            if (ServerPrivateKeysList.ContainsKey(name))
            {
                var set = ServerPrivateKeysList[name];
                if (set != null)
                {
                    return set.Contains(key);
                }
            }

            return false;
        }

        /// <summary>
        /// If using private keys returns whether the Player has unlocked the given key.
        /// Otherwise returns whether the global key is unlocked.
        ///
        /// World Modifier keys should not be passed into this method.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey(string key)
        {
            if (ProgressionConfiguration.Instance.GetUsePrivateKeys())
            {
                return Instance.HasPrivateKey(key);
            }
            else
            {
                return Instance.HasGlobalKey(key);
            }
        }

        /// <summary>
        /// Returns whether the Player contains the necessary key for taming the specified creature,
        /// or true if the configuration does not exist for the creature.
        /// </summary>
        /// <param name="creature"></param>
        /// <returns></returns>
        private bool HasTamingKey(string creature)
        {
            if (creature.IsNullOrWhiteSpace())
            {
                return false;
            }

            if (TamingKeysList.ContainsKey(creature))
            {
                return HasKey(TamingKeysList[creature]);
            }

            return true;
        }

        /// <summary>
        /// Returns whether the Player contains the necessary key for summoning the specified creature,
        /// or true if the configuration does not exist for the creature.
        /// </summary>
        /// <param name="creature"></param>
        /// <returns></returns>
        private bool HasSummoningKey(string creature)
        {
            if (creature.IsNullOrWhiteSpace())
            {
                return false;
            }

            if (SummoningKeysList.ContainsKey(creature))
            {
                if (HasKey(SummoningKeysList[creature]))
                {
                    if (ProgressionConfiguration.Instance.GetUnlockBossSummonsOverTime())
                    {
                        return SummoningTimeReached(SummoningKeysList[creature], ProgressionAPI.GetGameDay());
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// True if the in-game days passed allows for the creature to be summoned.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected bool SummoningTimeReached(string key, int gameDay)
        {
            if (BossKeyOrderList.ContainsKey(key))
            {
                int requiredDay = BossKeyOrderList[key] * ProgressionConfiguration.Instance.GetUnlockBossSummonsTime();
                return gameDay >= requiredDay;
            }

            return true;
        }

        /// <summary>
        /// Returns whether the Player contains the necessary key for accepting a boss power.
        /// </summary>
        /// <param name="guardianPower"></param>
        /// <returns></returns>
        private bool HasGuardianKey(string guardianPower)
        {
            if (guardianPower.IsNullOrWhiteSpace())
            {
                return false;
            }

            if (GuardianKeysList.ContainsKey(guardianPower))
            {
                return HasKey(GuardianKeysList[guardianPower]);
            }

            return false; // If there are other mods that add powers will need to revisit this
        }

        /// <summary>
        /// Returns whether the Player contains the necessary key for handling the item.
        /// </summary>
        /// <param name="item">Prefab name of the item<</param>
        /// <param name="checkBossItems"></param>
        /// <param name="checkMaterials"></param>
        /// <param name="checkFood"></param>
        /// <returns></returns>
        private bool HasItemKey(string item, bool checkBossItems, bool checkMaterials, bool checkFood)
        {
            if (item.IsNullOrWhiteSpace())
            {
                return false;
            }

            if ((checkBossItems && BossItemKeysList.ContainsKey(item) && !HasKey(BossItemKeysList[item])) ||
               (checkMaterials && MaterialKeysList.ContainsKey(item) && !HasKey(MaterialKeysList[item])) ||
               (checkFood && FoodKeysList.ContainsKey(item) && !HasKey(FoodKeysList[item])))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if an action is blocked based on prefab categories and keys.
        /// Checks the passed item and the item recipe.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="quality"></param>
        /// <param name="checkBossItems"></param>
        /// <param name="checkMaterials"></param>
        /// <param name="checkFood"></param>
        /// <returns></returns>
        private bool IsActionBlocked(ItemDrop.ItemData item, int quality, bool checkBossItems, bool checkMaterials, bool checkFood)
        {
            if (item?.m_dropPrefab == null || !Instance.HasItemKey(Utils.GetPrefabName(item.m_dropPrefab), checkBossItems, checkMaterials, checkFood))
            {
                return true;
            }

            var recipe = ObjectDB.instance.GetRecipe(item);
            return IsActionBlocked(recipe, quality, checkBossItems, checkMaterials, checkFood);
        }

        /// <summary>
        /// Checks if an action is blocked based on prefab categories and keys.
        /// Checks the passed recipe.
        /// </summary>
        /// <param name="recipe"></param>
        /// <param name="quality"></param>
        /// <param name="checkBossItems"></param>
        /// <param name="checkMaterials"></param>
        /// <param name="checkFood"></param>
        /// <returns></returns>
        private bool IsActionBlocked(Recipe recipe, int quality, bool checkBossItems, bool checkMaterials, bool checkFood)
        {
            if (recipe != null)
            {
                for (int lcv1 = 0; lcv1 < recipe.m_resources.Length; lcv1++)
                {
                    if (recipe.m_resources[lcv1].m_resItem == null)
                    {
                        return false;
                    }

                    // Loop through current quality level and all previous.
                    // Blocked should account for both base requirements and
                    // all valid upgrades on an item.
                    for (int lcv2 = 1; lcv2 <= quality; lcv2++)
                    {
                        if (recipe.m_resources[lcv1].GetAmount(lcv2) > 0)
                        {
                            if (!Instance.HasItemKey(Utils.GetPrefabName(recipe.m_resources[lcv1].m_resItem.gameObject), checkBossItems, checkMaterials, checkFood))
                            {
                                return true;
                            }
                            break;
                        }
                    }
                }
            }

            return false;
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

        /// <summary>
        /// Shorthand for checking the global key list.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual bool HasGlobalKey(string key)
        {
            if (key.IsNullOrWhiteSpace())
            {
                return true;
            }

            return ProgressionAPI.GetGlobalKey(key);
        }

        #region Add Private Key

        /// <summary>
        /// Adds the given key to the Player's private list.
        /// If added sends a response back to the server for tracking.
        /// </summary>
        /// <param name="key"></param>
        private void AddPrivateKey(string key)
        {
            if (key.IsNullOrWhiteSpace())
            {
                return;
            }

            bool added = PrivateKeysList.Add(key);
            if (added)
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"Adding Private Key {key}.");
                SendPrivateKeyToServer(key);
            }
        }

        /// <summary>
        /// If the playerName is empty adds the key to the current player,
        /// else sends the key to the target player.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="playerName"></param>
        private void AddPrivateKey(string key, string playerName)
        {
            if (playerName.IsNullOrWhiteSpace())
            {
                AddPrivateKey(key);
            }
            else
            {
                SendPrivateKey(playerName, key);
            }
        }

        /// <summary>
        /// Invokes the RPC to add a key to a player.
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="key"></param>
        private void SendPrivateKey(string playerName, string key)
        {
            var id = ProgressionAPI.GetPlayerID(playerName);
            if (id != 0)
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(id, RPCNAME_SetPrivateKey, key);
            }
        }

        /// <summary>
        /// RPC for adding a private key.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="key"></param>
        private void RPC_SetPrivateKey(long sender, string key)
        {
            ProgressionPlugin.VentureProgressionLogger.LogDebug($"Got private key {key} from {sender}: adding to list.");
            AddPrivateKey(key);
        }

        #endregion

        #region Remove Private Key

        /// <summary>
        /// Removes the given key from the Player's private list.
        /// If removed sends a response back to the server for tracking.
        /// </summary>
        /// <param name="key"></param>
        private void RemovePrivateKey(string key)
        {
            bool removed = PrivateKeysList.Remove(key);
            if (removed)
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"Removing Private Key {key}.");
                SendRemovePrivateKeyFromServer(key);
            }
        }

        /// <summary>
        /// Remove private key method for commands.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="playerName"></param>
        private void RemovePrivateKey(string key, string playerName)
        {
            if (playerName.IsNullOrWhiteSpace())
            {
                RemovePrivateKey(key);
            }
            else
            {
                SendRemovePrivateKey(playerName, key);
            }
        }

        /// <summary>
        /// Invokes the RPC to remove the given player's private key.
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="key"></param>
        private void SendRemovePrivateKey(string playerName, string key)
        {
            var id = ProgressionAPI.GetPlayerID(playerName);
            if (id != 0)
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(id, RPCNAME_RemovePrivateKey, key);
            }
        }

        /// <summary>
        /// RPC to remove a private key.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="key"></param>
        private void RPC_RemovePrivateKey(long sender, string key)
        {
            ProgressionPlugin.VentureProgressionLogger.LogDebug($"Got private key {key} from {sender}: removing from list.");
            RemovePrivateKey(key);
        }

        #endregion

        #region Reset Private Keys

        /// <summary>
        /// Resets the keys for the Player's private list.
        /// Sends a response back to the server for tracking.
        /// </summary>
        private void ResetPrivateKeys()
        {
            ProgressionPlugin.VentureProgressionLogger.LogDebug($"Resetting Private Keys.");

            PrivateKeysList = new HashSet<string>();
            SendPrivateKeysToServer(PrivateKeysList);
        }

        /// <summary>
        /// Reset private keys method for commands.
        /// </summary>
        /// <param name="playerName"></param>
        private void ResetPrivateKeys(string playerName)
        {
            if (playerName.IsNullOrWhiteSpace())
            {
                ResetPrivateKeys();
            }
            else
            {
                SendResetPrivateKeys(playerName);
            }
        }

        /// <summary>
        /// Invokes the RPC to reset the given player's private keys.
        /// </summary>
        /// <param name="playerName"></param>
        private void SendResetPrivateKeys(string playerName)
        {
            var id = ProgressionAPI.GetPlayerID(playerName);
            if (id != 0)
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(id, RPCNAME_ResetPrivateKeys);
            }
        }

        /// <summary>
        /// RPC for reseting private keys.
        /// </summary>
        /// <param name="sender"></param>
        private void RPC_ResetPrivateKeys(long sender)
        {
            ProgressionPlugin.VentureProgressionLogger.LogDebug($"Got reset keys command from {sender}.");
            ResetPrivateKeys();
        }

        #endregion

        #region Server Keys

        /// <summary>
        /// Sends the private key data to the server for tracking.
        /// </summary>
        /// <param name="keys"></param>
        private void SendPrivateKeysToServer(HashSet<string> keys)
        {
            string setString = string.Join<string>(",", keys);
            ZRoutedRpc.instance.InvokeRoutedRPC(RPCNAME_ServerSetPrivateKeys, setString, ProgressionAPI.GetLocalPlayerName());
        }

        /// <summary>
        /// Sets the Server keys for a player for tracking.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="keys">Comma-seperated string of keys</param>
        /// <param name="name">Player name</param>
        private void RPC_ServerSetPrivateKeys(long sender, string keys, string name)
        {
            if (!name.IsNullOrWhiteSpace())
            {
                var set = ProgressionAPI.StringToSet(keys);
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"Updating Server Player: {set.Count} keys found for peer {sender}: \"{name}\".");
                SetServerKeys(name, set);
            }
        }

        /// <summary>
        /// Sends the private key to the server for tracking.
        /// </summary>
        /// <param name="key"></param>
        private void SendPrivateKeyToServer(string key)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(RPCNAME_ServerSetPrivateKey, key, ProgressionAPI.GetLocalPlayerName());
        }

        /// <summary>
        /// Adds a Server key for a player for tracking.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="key"></param>
        private void RPC_ServerSetPrivateKey(long sender, string key, string name)
        {
            if (!name.IsNullOrWhiteSpace())
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"Updating Server Player: Adding key {key} for peer {sender}: \"{name}\".");
                SetServerKey(name, key);
            }
        }

        /// <summary>
        /// Sends the private key to remove data on the server for tracking.
        /// </summary>
        /// <param name="key"></param>
        private void SendRemovePrivateKeyFromServer(string key)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(RPCNAME_ServerRemovePrivateKey, key, ProgressionAPI.GetLocalPlayerName());
        }

        /// <summary>
        /// Removes a Server key for a player for tracking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="key"></param>
        private void RPC_ServerRemovePrivateKey(long sender, string key, string name)
        {
            if (!name.IsNullOrWhiteSpace())
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"Updating Server Player: Removing key {key} for peer {sender}: \"{name}\".");
                RemoveServerKey(name, key);
            }
        }

        /// <summary>
        /// Sends the command to list the current server keys.
        /// </summary>
        private void SendServerListKeys()
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(RPCNAME_ServerListKeys);
        }

        /// <summary>
        /// Prints the current server keys to the log file.
        /// </summary>
        /// <param name="sender"></param>
        private void RPC_ServerListKeys(long sender)
        {
            foreach (var player in ServerPrivateKeysList)
            {
                string keys = "";
                foreach (var key in player.Value)
                {
                    keys += $"{key}, ";
                }

                ProgressionPlugin.VentureProgressionLogger.LogInfo($"Player {player.Key} has {player.Value.Count} recorded keys: {keys}");
            }
        }

        /// <summary>
        /// Records the private key set for a player in the server dataset.
        /// </summary>
        /// <param name="name">Player name</param>
        /// <param name="keys"></param>
        private void SetServerKeys(string name, HashSet<string> keys)
        {
            if (ServerPrivateKeysList.ContainsKey(name))
            {
                ServerPrivateKeysList[name] = keys;
            }
            else
            {
                ServerPrivateKeysList.Add(name, keys);
            }
        }

        /// <summary>
        /// Adds the private key for a player to the server dataset.
        /// </summary>
        /// <param name="name">Player name</param>
        /// <param name="key"></param>
        private void SetServerKey(string name, string key)
        {
            if (ServerPrivateKeysList.ContainsKey(name))
            {
                ServerPrivateKeysList[name].Add(key);
            }
            else
            {
                var set = new HashSet<string>
                {
                    key
                };
                ServerPrivateKeysList.Add(name, set);
            }
        }

        /// <summary>
        /// Removes the private key for a player from the server dataset
        /// </summary>
        /// <param name="name">Player name</param>
        /// <param name="key"></param>
        private void RemoveServerKey(string name, string key)
        {
            if (ServerPrivateKeysList.ContainsKey(name))
            {
                ServerPrivateKeysList[name].Remove(key);
            }
        }

        #endregion

        /// <summary>
        /// Get private keys as a comma-separated string.
        /// </summary>
        /// <returns></returns>
        private string GetPrivateKeysString()
        {
            return string.Join<string>(",", PrivateKeysList);
        }

        /// <summary>
        /// Applies the burning effect and displays the blocked action message.
        /// </summary>
        /// <param name="player"></param>
        private void ApplyBlockedActionEffects(Player player)
        {
            if (player != null)
            {
                if (ProgressionConfiguration.Instance.GetUseBlockedActionEffect())
                {
                    player.GetSEMan()?.AddStatusEffect(Character.s_statusEffectBurning, resetTime: false);
                }

                if (ProgressionConfiguration.Instance.GetUseBlockedActionMessage())
                {
                    player.Message(MessageHud.MessageType.Center, ProgressionConfiguration.Instance.GetBlockedActionMessage());
                }
            }
        }
    }
}