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

    public class KeyManager : IKeyManager
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
            private static float _lastUpdateTime = 0f;
            private static readonly float _updateInterval = 10f;

            public void Start()
            {
                Instance.UpdateConfigs();
                _cachedPublicBossKeys = Instance.CountPublicBossKeys();
                _cachedPrivateBossKeys = Instance.CountPrivateBossKeys();
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"Starting Key Manager Updater.");
            }

            public void Update()
            {
                var time = Time.time;
                var delta = time - _lastUpdateTime;

                if (delta >= _updateInterval)
                {
                    Instance.UpdateConfigs();
                    _cachedPublicBossKeys = Instance.CountPublicBossKeys();
                    _cachedPrivateBossKeys = Instance.CountPrivateBossKeys();

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
        public string QualifyingKeys { get; protected set; }
        public HashSet<string> BlockedGlobalKeysList { get; protected set; }
        public HashSet<string> AllowedGlobalKeysList { get; protected set; }
        public HashSet<string> EnforcedGlobalKeysList { get; protected set; }
        public HashSet<string> BlockedPrivateKeysList { get; protected set; }
        public HashSet<string> AllowedPrivateKeysList { get; protected set; }
        public HashSet<string> EnforcedPrivateKeysList { get; protected set; }
        public Dictionary<string, string> TamingKeysList { get; protected set; }
        public Dictionary<string, string> SummoningKeysList { get; protected set; }
        public HashSet<string> QualifyingKeysList { get; protected set; }

        public HashSet<string> PrivateKeysList { get; protected set; }
        public Dictionary<string, HashSet<string>> ServerPrivateKeysList { get; protected set; }

        public const string BOSS_KEY_MEADOW = "defeated_eikthyr";
        public const string BOSS_KEY_BLACKFOREST = "defeated_gdking";
        public const string BOSS_KEY_SWAMP = "defeated_bonemass";
        public const string BOSS_KEY_MOUNTAIN = "defeated_dragon";
        public const string BOSS_KEY_PLAIN = "defeated_goblinking";
        public const string BOSS_KEY_MISTLAND = "defeated_queen";

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
            // Mistlands
            { "BlackMarble", BOSS_KEY_PLAIN },
            { "BlackCore", BOSS_KEY_PLAIN },
            { "Carapace", BOSS_KEY_PLAIN },
            { "Eitr", BOSS_KEY_PLAIN },
            { "ScaleHide", BOSS_KEY_PLAIN },
            { "Wisp", BOSS_KEY_PLAIN },
            { "YggdrasilWood", BOSS_KEY_PLAIN }
        };

        public readonly Dictionary<string, string> FoodKeysList = new Dictionary<string, string>
        {
            // Black Forest
            { "Blueberries", BOSS_KEY_MEADOW },
            { "MushroomYellow", BOSS_KEY_MEADOW },
            { "Carrot", BOSS_KEY_MEADOW },
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

            QualifyingKeys = "";
            QualifyingKeysList = new HashSet<string>()
            {
                BOSS_KEY_MEADOW,
                BOSS_KEY_BLACKFOREST,
                BOSS_KEY_SWAMP,
                BOSS_KEY_MOUNTAIN,
                BOSS_KEY_PLAIN,
                BOSS_KEY_MISTLAND,
                "KilledTroll",
                "killed_surtling",
                "KilledBat",
                "Hildir1",
                "Hildir2",
                "Hildir3"
            };

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
            UpdateQualifyingKeysConfiguration(ProgressionConfiguration.Instance.GetQualifyingKeys());
            UpdateGlobalKeyConfiguration(ProgressionConfiguration.Instance.GetBlockedGlobalKeys(), ProgressionConfiguration.Instance.GetAllowedGlobalKeys());
            UpdatePrivateKeyConfiguration(ProgressionConfiguration.Instance.GetBlockedPrivateKeys(), ProgressionConfiguration.Instance.GetAllowedPrivateKeys());
            UpdateEnforcedKeyConfiguration(ProgressionConfiguration.Instance.GetEnforcedGlobalKeys(), ProgressionConfiguration.Instance.GetEnforcedPrivateKeys());
            UpdateTamingConfiguration(ProgressionConfiguration.Instance.GetOverrideLockTamingDefaults());
            UpdateSummoningConfiguration(ProgressionConfiguration.Instance.GetOverrideLockBossSummonsDefaults());
        }

        /// <summary>
        /// Set the values for QualifyingKeysList if changed.
        /// </summary>
        /// <param name="qualifyingKeys"></param>
        protected void UpdateQualifyingKeysConfiguration(string qualifyingKeys)
        {
            if (!QualifyingKeys.Equals(qualifyingKeys))
            {
                QualifyingKeys = qualifyingKeys;
                var additionalStrings = ProgressionAPI.StringToSet(qualifyingKeys);
                foreach(var key in additionalStrings)
                {
                    if (!QualifyingKeysList.Contains(key))
                    {
                        QualifyingKeysList.Add(key);
                    }
                }
            }
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
        /// Reads the configuration values and creates a dictionary of all vanilla trader items
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

            if (!QualifyingKeysList.Contains(globalKey))
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
            if (key.IsNullOrWhiteSpace() || !QualifyingKeysList.Contains(key))
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
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey(string key)
        {
            if (ProgressionConfiguration.Instance.GetUsePrivateKeys() && Instance.QualifyingKeysList.Contains(key))
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

            if (checkBossItems && BossItemKeysList.ContainsKey(item))
            {
                return HasKey(BossItemKeysList[item]);
            }
            else if (checkMaterials && MaterialKeysList.ContainsKey(item))
            {
                return HasKey(MaterialKeysList[item]);
            }
            else if (checkFood && FoodKeysList.ContainsKey(item))
            {
                return HasKey(FoodKeysList[item]);
            }

            return true;
        }

        /// <summary>
        /// Checks if an action is blocked based on prefab categories and keys.
        /// Checks the passed item and the item recipe.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="checkBossItems"></param>
        /// <param name="checkMaterials"></param>
        /// <param name="checkFood"></param>
        /// <returns></returns>
        private bool IsActionBlocked(ItemDrop.ItemData item, bool checkBossItems, bool checkMaterials, bool checkFood)
        {
            if (item?.m_dropPrefab == null || !Instance.HasItemKey(Utils.GetPrefabName(item.m_dropPrefab), checkBossItems, checkMaterials, checkFood))
            {
                return true;
            }
            else
            {
                var recipe = ObjectDB.instance.GetRecipe(item);
                return IsActionBlocked(recipe, checkBossItems, checkMaterials, checkFood);
            }
        }

        /// <summary>
        /// Checks if an action is blocked based on prefab categories and keys.
        /// Checks the passed recipe.
        /// </summary>
        /// <param name="recipe"></param>
        /// <param name="checkBossItems"></param>
        /// <param name="checkMaterials"></param>
        /// <param name="checkFood"></param>
        /// <returns></returns>
        private bool IsActionBlocked(Recipe recipe, bool checkBossItems, bool checkMaterials, bool checkFood)
        {
            if (recipe != null)
            {
                for (int lcv = 0; lcv < recipe.m_resources.Length; lcv++)
                {
                    if (recipe.m_resources[lcv].m_resItem == null)
                    {
                        return false;
                    }

                    if (!Instance.HasItemKey(Utils.GetPrefabName(recipe.m_resources[lcv].m_resItem.gameObject), checkBossItems, checkMaterials, checkFood))
                    {
                        return true;
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
                if (PrivateKeysList.Contains(key))
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
                if (HasGlobalKey(key))
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
            if (player != null && ProgressionConfiguration.Instance.GetUseBlockedActionMessage())
            {
                player.GetSEMan()?.AddStatusEffect(Character.s_statusEffectBurning, resetTime: false);
                player.Message(MessageHud.MessageType.Center, ProgressionConfiguration.Instance.GetBlockedActionMessage());
            }
        }

        /// <summary>
        /// Returns a lower case string of a GlobalKeys enum.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetGlobalKeysEnumString(GlobalKeys key)
        {
            return key.ToString().ToLower();
        }

        #region Patches

        /// <summary>
        /// Skips the original ZoneSystem.SetGlobalKey method if a key is blocked.
        /// </summary>
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SetGlobalKey), new Type[] { typeof(string) })]
        public static class Patch_ZoneSystem_SetGlobalKey
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(string name)
            {
                return Instance.SkipAddKeyMethod(name);
            }

            private static void Postfix(string name)
            {
                if (Player.m_localPlayer != null && !Instance.BlockPrivateKey(name))
                {
                    List<Player> nearbyPlayers = new List<Player>();
                    Player.GetPlayersInRange(Player.m_localPlayer.transform.position, 100, nearbyPlayers);

                    if (nearbyPlayers != null && nearbyPlayers.Count == 0)
                    {
                        ProgressionPlugin.VentureProgressionLogger.LogDebug($"No players in range to send key!");
                    }
                    else
                    {
                        for (int lcv = 0; lcv < nearbyPlayers.Count; lcv++)
                        {
                            var player = nearbyPlayers[lcv].GetPlayerName();
                            ProgressionPlugin.VentureProgressionLogger.LogDebug(
                                    $"Attempting to send private key: {name} to \"{player}\".");
                            Instance.SendPrivateKey(player, name);
                        }
                    }
                }
                else
                {
                    ProgressionPlugin.VentureProgressionLogger.LogDebug($"Skipping adding private key: {name}.");
                }
            }
        }

        // Server side global key cleanup logic, used for servers with vanilla players
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.RPC_SetGlobalKey))]
        public static class Patch_ZoneSystem_RPC_SetGlobalKey
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(string name)
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"RPC_SetGlobalKey called for: {name}.");
                bool runOriginal = Instance.SkipAddKeyMethod(name);
                if (!runOriginal)
                {
                    ZoneSystem.instance.SendGlobalKeys(ZRoutedRpc.Everybody);
                }

                return runOriginal;
            }
        }

        /// <summary>
        /// Returns false if the global key is blocked. Used to determine if the add global key game methods
        /// should be skipped or not.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private bool SkipAddKeyMethod(string key)
        {
            if (Instance.BlockGlobalKey(ProgressionConfiguration.Instance.GetBlockAllGlobalKeys(), key))
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"Skipping adding global key: {key}.");
                return false; // Skip adding the global key
            }

            ProgressionPlugin.VentureProgressionLogger.LogDebug($"Adding global key: {key}.");
            return true; // Continue adding the global key
        }

        /// <summary>
        /// If using private keys, returns true if the key is in the global list when
        /// the instance is a dedicated server, or true if the local player has the private key.
        /// If not using private keys uses default behavior.
        /// </summary>
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GetGlobalKey), new Type[] { typeof(string) })]
        public static class Patch_ZoneSystem_GetGlobalKey
        {
            private static void Postfix(string name, ref bool __result)
            {
                if (ProgressionConfiguration.Instance.GetUsePrivateKeys() &&
                    !ZNet.instance.IsDedicated() &&
                    Instance.QualifyingKeysList.Contains(name))
                {
                    __result = Instance.HasPrivateKey(name);
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Save))]
        public static class Patch_Player_Save
        {
            private static void Prefix(Player __instance)
            {
                if (!ProgressionAPI.IsInTheMainScene())
                {
                    // Prevent keys from last game session saving to the wrong player file when using logout
                    Instance.ResetPlayer();
                }
                else
                {
                    if (__instance.m_customData.ContainsKey(PLAYER_SAVE_KEY))
                    {
                        __instance.m_customData[PLAYER_SAVE_KEY] = Instance.GetPrivateKeysString();
                    }
                    else
                    {
                        __instance.m_customData.Add(PLAYER_SAVE_KEY, Instance.GetPrivateKeysString());
                    }
                }
            }
        }

        /// <summary>
        /// Load private keys from the player file if the data exists,
        /// fallback try to load a legacy save file. Cleans up private keys
        /// based off configurations then syncs the data with the server.
        ///
        /// Patches before EquipInventoryItems since that is the first method
        /// that needs access to the player private keys, and only happens
        /// during the Player.Load method.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.EquipInventoryItems))]
        public static class Patch_Player_Load
        {
            private static void Prefix(Player __instance)
            {
                if (!ProgressionAPI.IsInTheMainScene())
                {
                    return;
                }

                ProgressionPlugin.VentureProgressionLogger.LogInfo("Starting Player Key Management. Cleaning up private keys!");

                Instance.ResetPlayer();
                Instance.UpdateConfigs();

                HashSet<string> loadedKeys = new HashSet<string>();

                if (__instance.m_customData.ContainsKey(PLAYER_SAVE_KEY))
                {
                    loadedKeys = ProgressionAPI.StringToSet(__instance.m_customData[PLAYER_SAVE_KEY]);
                }

                // Add loaded private keys if not blocked
                foreach (var key in loadedKeys)
                {
                    if (!Instance.BlockPrivateKey(key))
                    {
                        Instance.PrivateKeysList.Add(key);
                    }
                }

                // Add enforced private keys regardless of settings
                foreach (var key in Instance.EnforcedPrivateKeysList)
                {
                    Instance.PrivateKeysList.Add(key);
                }

                try
                {
                    ZRoutedRpc.instance.Register(RPCNAME_SetPrivateKey, new Action<long, string>(Instance.RPC_SetPrivateKey));
                    ZRoutedRpc.instance.Register(RPCNAME_RemovePrivateKey, new Action<long, string>(Instance.RPC_RemovePrivateKey));
                    ZRoutedRpc.instance.Register(RPCNAME_ResetPrivateKeys, new Action<long>(Instance.RPC_ResetPrivateKeys));
                }
                catch
                {
                    ProgressionPlugin.VentureProgressionLogger.LogDebug("Player RPCs have already been registered. Skipping.");
                }

                if (Instance._keyManagerUpdater == null)
                {
                    var obj = GameObject.Instantiate(new GameObject());
                    Instance._keyManagerUpdater = obj.AddComponent<KeyManagerUpdater>();
                }

                // Sync data on connect
                Instance.SendPrivateKeysToServer(Instance.PrivateKeysList);
            }
        }

        /// <summary>
        /// Register RPCs and perform a server key cleanup when starting up.
        /// </summary>
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Load))]
        public static class Patch_ZoneSystem_Load
        {
            private static void Postfix()
            {
                if (ZNet.instance.IsServer())
                {
                    ProgressionPlugin.VentureProgressionLogger.LogInfo("Starting Server Key Management. Cleaning up public keys!");

                    Instance.Reset();
                    Instance.UpdateConfigs();

                    var keys = ZoneSystem.instance.GetGlobalKeys();
                    var blockAll = ProgressionConfiguration.Instance.GetBlockAllGlobalKeys();

                    // Remove any blocked global keys from the list
                    for (int lcv = 0; lcv < keys.Count; lcv++)
                    {
                        if (Instance.BlockGlobalKey(blockAll, keys[lcv]))
                        {
                            ZoneSystem.instance.m_globalKeys.Remove(keys[lcv]);
                        }
                    }

                    // Add enforced global keys regardless of settings
                    foreach (var key in Instance.EnforcedGlobalKeysList)
                    {
                        ZoneSystem.instance.m_globalKeys.Add(key);
                    }

                    if (ProgressionConfiguration.Instance.GetUsePrivateKeys())
                    {
                        // Add player based raids setting
                        var eventKey = GetGlobalKeysEnumString(GlobalKeys.PlayerEvents);
                        if (!ZoneSystem.instance.m_globalKeys.Contains(eventKey))
                        {
                            ZoneSystem.instance.GlobalKeyAdd(eventKey, false);
                        }
                    }

                    // Register Server RPCs
                    try
                    {
                        ZRoutedRpc.instance.Register(RPCNAME_ServerListKeys, new Action<long>(Instance.RPC_ServerListKeys));
                        ZRoutedRpc.instance.Register(RPCNAME_ServerSetPrivateKeys, new Action<long, string, string>(Instance.RPC_ServerSetPrivateKeys));
                        ZRoutedRpc.instance.Register(RPCNAME_ServerSetPrivateKey, new Action<long, string, string>(Instance.RPC_ServerSetPrivateKey));
                        ZRoutedRpc.instance.Register(RPCNAME_ServerRemovePrivateKey, new Action<long, string, string>(Instance.RPC_ServerRemovePrivateKey));

                        ZRoutedRpc.instance.Register(RPCNAME_SetPrivateKey, new Action<long, string>(Instance.RPC_SetPrivateKey));
                        ZRoutedRpc.instance.Register(RPCNAME_RemovePrivateKey, new Action<long, string>(Instance.RPC_RemovePrivateKey));
                        ZRoutedRpc.instance.Register(RPCNAME_ResetPrivateKeys, new Action<long>(Instance.RPC_ResetPrivateKeys));
                    }
                    catch
                    {
                        ProgressionPlugin.VentureProgressionLogger.LogDebug("Server RPCs have already been registered. Skipping.");
                    }


                    if (Instance._keyManagerUpdater == null)
                    {
                        var obj = GameObject.Instantiate(new GameObject());
                        Instance._keyManagerUpdater = obj.AddComponent<KeyManagerUpdater>();
                    }
                }
            }
        }

        /// <summary>
        /// Fix my mistake of adding GlobalKeys.PlayerEvents to the list multiple times
        /// </summary>
        [HarmonyPatch(typeof(ZPlayFabMatchmaking), nameof(ZPlayFabMatchmaking.CreateLobby))]
        public static class Patch_ZPlayFabMatchmaking_CreateLobby
        {
            private static void Prefix()
            {
                RemoveDuplicates(ref ZPlayFabMatchmaking.m_instance.m_serverData.modifiers);
            }
        }

        /// <summary>
        /// Fix my mistake of adding GlobalKeys.PlayerEvents to the list multiple times (server patch)
        /// </summary>
        [HarmonyPatch(typeof(ZSteamMatchmaking), nameof(ZSteamMatchmaking.RegisterServer))]
        public static class Patch_ZSteamMatchmaking_RegisterServer
        {
            private static void Prefix(ref List<string> modifiers)
            {
                RemoveDuplicates(ref modifiers);
            }
        }

        private static void RemoveDuplicates(ref List<string> keys)
        {
            if (keys != null)
            {
                var fixedKeys = new HashSet<string>();
                foreach (string key in keys)
                {
                    if (!fixedKeys.Contains(key))
                    {
                        fixedKeys.Add(key);
                    }
                    else
                    {
                        ProgressionPlugin.VentureProgressionLogger.LogWarning($"Found duplicate world modifier key {key}, fixing.");
                    }
                }

                keys = fixedKeys.ToList();
            }
        }

        /// <summary>
        /// Enables all of Haldor's items by bypassing key checking.
        /// </summary>
        [HarmonyPatch(typeof(Trader), nameof(Trader.GetAvailableItems))]
        public static class Patch_Trader_GetAvailableItems
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix()
            {
                if (ProgressionConfiguration.Instance.GetUnlockAllHaldorItems())
                {
                    return false; // Skip main method to save some calculations
                }

                return true;
            }

            [HarmonyPriority(Priority.First)]
            private static void Postfix(Trader __instance, ref List<Trader.TradeItem> __result)
            {
                if (ProgressionConfiguration.Instance.GetUnlockAllHaldorItems())
                {
                    __result = new List<Trader.TradeItem>(__instance.m_items);
                }
            }
        }

        /// <summary>
        /// Set up custom keys for Haldor's items.
        /// </summary>
        [HarmonyPatch(typeof(Trader), nameof(Trader.Start))]
        public static class Patch_Trader_Start
        {
            [HarmonyPriority(Priority.First)]
            private static void Postfix(Trader __instance)
            {
                var keys = Instance.GetTraderConfiguration();
                foreach (var item in __instance.m_items)
                {
                    if (item.m_prefab != null)
                    {
                        var name = Utils.GetPrefabName(item.m_prefab.gameObject);
                        if (keys.ContainsKey(name))
                        {
                            item.m_requiredGlobalKey = keys[name];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds commands for managing player keys.
        /// </summary>
        [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
        private static class Patch_Terminal_InitTerminal
        {
            [HarmonyPriority(Priority.First)]
            private static void Prefix(out bool __state)
            {
                __state = Terminal.m_terminalInitialized;
            }

            private static void Postfix(bool __state)
            {
                if (__state)
                {
                    return;
                }

                ProgressionPlugin.VentureProgressionLogger.LogInfo("Adding Terminal Commands for key management.");

                new Terminal.ConsoleCommand("setglobalkey", "[name]", delegate (Terminal.ConsoleEventArgs args)
                {
                    if (args.Length >= 2)
                    {
                        ProgressionAPI.AddGlobalKey(args[1]);
                        args.Context.AddString($"Setting global key {args[1]}.");
                    }
                    else
                    {
                        args.Context.AddString("Syntax: setglobalkey [key]");
                    }
                }, isCheat: true, isNetwork: false, onlyServer: true);
                new Terminal.ConsoleCommand("setprivatekey", "[name] [optional: player name]", delegate (Terminal.ConsoleEventArgs args)
                {
                    if (args.Length >= 3)
                    {
                        var name = args[2];
                        for (int lcv = 3; lcv < args.Length; lcv++)
                        {
                            name += " " + args[lcv];
                        }
                        Instance.AddPrivateKey(args[1], name);
                        args.Context.AddString($"Setting private key {args[1]} for player {name}.");
                    }
                    else if (args.Length == 2)
                    {
                        Instance.AddPrivateKey(args[1]);
                        args.Context.AddString($"Setting private key {args[1]}.");
                    }
                    else
                    {
                        args.Context.AddString("Syntax: setprivatekey [key]");
                    }
                }, isCheat: true, isNetwork: false, onlyServer: true);
                new Terminal.ConsoleCommand("removeprivatekey", "[name] [optional: player name]", delegate (Terminal.ConsoleEventArgs args)
                {
                    if (args.Length >= 3)
                    {
                        var name = args[2];
                        for (int lcv = 3; lcv < args.Length; lcv++)
                        {
                            name += " " + args[lcv];
                        }
                        Instance.RemovePrivateKey(args[1], name);
                        args.Context.AddString($"Removing private key {args[1]} for player {name}.");
                    }
                    else if (args.Length == 2)
                    {
                        Instance.RemovePrivateKey(args[1]);
                        args.Context.AddString($"Removing private key {args[1]}.");
                    }
                    else
                    {
                        args.Context.AddString("Syntax: removeprivatekey [key] [optional: player name]");
                    }
                }, isCheat: true, isNetwork: false, onlyServer: true);
                new Terminal.ConsoleCommand("resetprivatekeys", "[optional: player name]", delegate (Terminal.ConsoleEventArgs args)
                {
                    if (args.Length >= 2)
                    {

                        var name = args[1];
                        for (int lcv = 2; lcv < args.Length; lcv++)
                        {
                            name += " " + args[lcv];
                        }
                        Instance.ResetPrivateKeys(args[1]);
                        args.Context.AddString($"Private keys cleared for player {name}.");
                    }
                    else if (args.Length == 1)
                    {
                        Instance.ResetPrivateKeys();
                        args.Context.AddString("Private keys cleared");
                    }
                    else
                    {
                        args.Context.AddString("Syntax: resetprivatekeys [optional: player name]");
                    }
                }, isCheat: true, isNetwork: false, onlyServer: true);
                new Terminal.ConsoleCommand("listprivatekeys", "", delegate (Terminal.ConsoleEventArgs args)
                {
                    args.Context.AddString($"Total Keys {Instance.PrivateKeysList.Count}");
                    foreach (string key in Instance.PrivateKeysList)
                    {
                        args.Context.AddString(key);
                    }
                }, isCheat: false, isNetwork: false, onlyServer: false);
                new Terminal.ConsoleCommand("listserverkeys", "", delegate (Terminal.ConsoleEventArgs args)
                {
                    if (ZNet.instance.IsServer())
                    {
                        args.Context.AddString($"Total Players Recorded This Session: {Instance.ServerPrivateKeysList.Count}");

                        foreach (var set in Instance.ServerPrivateKeysList)
                        {
                            var numKeys = set.Value?.Count ?? 0;

                            args.Context.AddString($"Player {set.Key} has {numKeys} recorded keys:");

                            if (set.Value != null)
                            {
                                foreach (string key in set.Value)
                                {
                                    args.Context.AddString(key);
                                }
                            }
                        }
                    }
                    else
                    {
                        args.Context.AddString($"You are not the server, no data available client side. Printing key information to server logoutput.log file.");
                        Instance.SendServerListKeys();
                    }
                }, isCheat: true, isNetwork: false, onlyServer: true);
            }
        }

        /// <summary>
        /// Only increase taming if the player has the private key.
        /// </summary>
        [HarmonyPatch(typeof(Tameable), nameof(Tameable.DecreaseRemainingTime))]
        public static class Patch_Tameable_DecreaseRemainingTime
        {
            [HarmonyPriority(Priority.Low)]
            private static void Prefix(Tameable __instance, ref float time)
            {
                if (ProgressionConfiguration.Instance.GetLockTaming())
                {
                    if (__instance.m_character == null ||
                        !Instance.HasTamingKey(Utils.GetPrefabName(__instance.m_character.gameObject)))
                    {
                        time = 0f;
                    }
                }
            }
        }

        /// <summary>
        /// Block getting guardian powers without the key.
        /// </summary>
        [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.DelayedPowerActivation))]
        public static class Patch_ItemStand_DelayedPowerActivation
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(ItemStand __instance)
            {
                if (ProgressionConfiguration.Instance.GetLockGuardianPower())
                {
                    if (!Instance.HasGuardianKey(__instance.m_guardianPower?.name))
                    {
                        Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                        return false; // Skip giving power
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Block activating guardian powers without the key if a Player already has one.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.ActivateGuardianPower))]
        public static class Patch_Player_ActivateGuardianPower
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(Player __instance, ref bool __result)
            {
                if (!__instance.m_guardianPower.IsNullOrWhiteSpace() && ProgressionConfiguration.Instance.GetLockGuardianPower())
                {
                    if (!Instance.HasGuardianKey(__instance.m_guardianPower))
                    {
                        Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                        __result = false; // Not sure why they have a return type on this, watch for game changes
                        return false; // Skip giving power
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Block the boss spawn when the player has not defeated the previous boss
        /// </summary>
        [HarmonyPatch(typeof(OfferingBowl), nameof(OfferingBowl.SpawnBoss))]
        public static class Patch_OfferingBowl_SpawnBoss
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(OfferingBowl __instance, ref bool __result)
            {
                if (ProgressionConfiguration.Instance.GetLockBossSummons() && __instance.m_bossPrefab != null)
                {
                    if (!Instance.HasSummoningKey(Utils.GetPrefabName(__instance.m_bossPrefab.gameObject)))
                    {
                        Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                        __result = false;
                        return false; // Skip summoning
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Block equipping items without the proper keys.
        /// </summary>
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
        public static class Patch_Humanoid_EquipItem
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(Humanoid __instance, ref bool __result, ItemDrop.ItemData item)
            {
                if (__instance != Player.m_localPlayer)
                {
                    return true;
                }

                if (ProgressionConfiguration.Instance.GetLockEquipment())
                {
                    if (Instance.IsActionBlocked(item, true, true, false))
                    {
                        Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                        __result = false;
                        return false; // Skip equipping item
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Block opening doors without the proper keys.
        /// </summary>
        [HarmonyPatch(typeof(Door), nameof(Door.HaveKey))]
        public static class Patch_Door_HaveKey
        {
            [HarmonyPriority(Priority.Low)]
            private static void Postfix(Door __instance, ref bool __result)
            {
                if (__result && ProgressionConfiguration.Instance.GetLockEquipment() && __instance.m_keyItem != null &&
                    !Instance.HasItemKey(Utils.GetPrefabName(__instance.m_keyItem.gameObject), true, false, false))
                {
                    Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                    __result = false;
                }
            }
        }

        /// <summary>
        /// Block crafting items without the proper keys.
        /// </summary>
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.DoCrafting))]
        public static class Patch_InventoryGui_DoCrafting
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(InventoryGui __instance)
            {
                if (ProgressionConfiguration.Instance.GetLockCrafting())
                {
                    if (Instance.IsActionBlocked(__instance.m_craftRecipe, true, true, false))
                    {
                        Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                        return false; // Skip crafting
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Block placing items without the proper keys.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
        public static class Patch_Player_PlacePiece
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(ref bool __result, Piece piece)
            {
                if (ProgressionConfiguration.Instance.GetLockBuilding() && piece?.m_resources != null)
                {
                    for (int lcv = 0; lcv < piece.m_resources.Length; lcv++)
                    {
                        if (piece.m_resources[lcv]?.m_resItem != null &&
                            !Instance.HasItemKey(Utils.GetPrefabName(piece.m_resources[lcv].m_resItem.gameObject), true, true, false))
                        {
                            Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                            __result = false;
                            return false; // Skip placing
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Block cooking items without the proper keys.
        /// </summary>
        [HarmonyPatch(typeof(CookingStation), nameof(CookingStation.OnUseItem))]
        public static class Patch_CookingStation_OnUseItem
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(ItemDrop.ItemData item, ref bool __result)
            {
                if (ProgressionConfiguration.Instance.GetLockCooking() && Instance.IsActionBlocked(item, false, false, true))
                {
                    Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                    __result = false;
                    return false; // Skip cooking
                }

                return true;
            }
        }

        #endregion
    }
}