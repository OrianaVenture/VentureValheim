using BepInEx;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace VentureValheim.Progression
{
    public interface IProgressionAPI
    {
    }

    [PublicAPI]
    public class ProgressionAPI : IProgressionAPI
    {
        static ProgressionAPI() { }
        protected ProgressionAPI() { }
        private static readonly ProgressionAPI _instance = new ProgressionAPI();

        public static ProgressionAPI Instance
        {
            get => _instance;
        }

        public static bool IsInTheMainScene()
        {
            return SceneManager.GetActiveScene().name.Equals("main");
        }

        /// <summary>
        /// Converts a comma separated string to a HashSet of lowercase strings.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static HashSet<string> StringToSet(string str)
        {
            var set = new HashSet<string>();

            if (!str.IsNullOrWhiteSpace())
            {
                List<string> keys = str.Split(',').ToList();
                for (var lcv = 0; lcv < keys.Count; lcv++)
                {
                    set.Add(keys[lcv].Trim().ToLower());
                }
            }

            return set;
        }

        /// <summary>
        /// Converts a comma separated string to a Dictionary of strings.
        /// If odd number of items ignores the last item.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Dictionary<string, string> StringToDictionary(string str)
        {
            var dict = new Dictionary<string, string>();

            if (!str.IsNullOrWhiteSpace())
            {
                List<string> keys = str.Split(',').ToList();
                for (var lcv = 0; lcv < keys.Count - 1; lcv += 2)
                {
                    var key = keys[lcv].Trim();
                    if (!dict.ContainsKey(key))
                    {
                        dict.Add(key, keys[lcv + 1].Trim());
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// Merges two lists together on unique entries.
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static List<string> MergeLists(List<string> list1, List<string> list2)
        {
            return list1.Union(list2).ToList();
        }

        /// <summary>
        /// Method to determine if a Global Key exists to bypass patches.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool GetGlobalKey(string key)
        {
            if (ZoneSystem.instance != null && ZoneSystem.instance.m_globalKeys != null)
            {
                return ZoneSystem.instance.m_globalKeys.Contains(key);
            }

            return false;
        }

        /// <summary>
        /// Method to get the global keys list to bypass patches.
        /// </summary>
        /// <returns></returns>
        public static HashSet<string> GetGlobalKeys()
        {
            return ZoneSystem.instance.m_globalKeys;
        }

        /// <summary>
        /// Method to add a global key to bypass patches.
        /// </summary>
        /// <param name="key"></param>
        public static void AddGlobalKey(string key)
        {
            if (key.IsNullOrWhiteSpace())
            {
                return;
            }

            key = key.ToLower();
            if (!ZoneSystem.instance.m_globalKeys.Contains(key))
            {
                ZoneSystem.instance.m_globalKeys.Add(key);
                ZoneSystem.instance.SendGlobalKeys(ZRoutedRpc.Everybody);
            }
        }

        /// <summary>
        /// Method to remove a global key to bypass patches.
        /// </summary>
        /// <param name="key"></param>
        public static void RemoveGlobalKey(string key)
        {
            if (key.IsNullOrWhiteSpace())
            {
                return;
            }

            key = key.ToLower();
            ZoneSystem.instance.m_globalKeys.Remove(key);
        }

        /// <summary>
        /// Attempts to find the player id with the given name.
        /// Case-insensitive, ignores whitespace.
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public static long GetPlayerID(string playerName)
        {
            var nameSimple = playerName.Trim().ToLower();
            var players = ZNet.instance.GetPlayerList();

            for (int lcv = 0; lcv < players.Count; lcv++)
            {
                var player = players[lcv].m_name.Trim().ToLower();
                if (player.Equals(nameSimple))
                {
                    return players[lcv].m_characterID.UserID;
                }
            }

            return 0L;
        }

        /// <summary>
        /// Attempts to find the player name with the given id.
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public static string GetPlayerName(long playerID)
        {
            if (GetLocalPlayerID() == playerID)
            {
                // In singleplayer the player list may not be populated
                return GetLocalPlayerName();
            }

            var players = ZNet.instance.GetPlayerList();

            for (int lcv = 0; lcv < players.Count; lcv++)
            {
                var player = players[lcv].m_characterID.UserID;
                if (player == playerID)
                {
                    return players[lcv].m_name;
                }
            }

            return playerID.ToString();
        }

        /// <summary>
        /// Returns the player name of the client player if exists.
        /// </summary>
        /// <returns></returns>
        public static string GetLocalPlayerName()
        {
            var profile = Game.instance.GetPlayerProfile();
            if (profile != null)
            {
                return profile.m_playerName;
            }

            return "";
        }

        /// <summary>
        /// Returns the player ID of the client player if exists.
        /// </summary>
        /// <returns></returns>
        public static long GetLocalPlayerID()
        {
            var profile = Game.instance.GetPlayerProfile();
            if (profile != null)
            {
                return profile.m_playerID;
            }

            return 0L;
        }

        /// <summary>
        /// Returns the current in-game day.
        /// </summary>
        /// <returns></returns>
        public static int GetGameDay()
        {
            return EnvMan.instance.GetCurrentDay();
        }

        /// <summary>
        /// Returns the quality of an item based off the given upgrade
        /// for that item.
        /// </summary>
        /// <param name="item">The crafting upgrade item</param>
        /// <returns></returns>
        public static int GetQualityLevel(ItemDrop.ItemData item)
        {
            return (item == null) ? 1 : (item.m_quality + 1);
        }
    }
}