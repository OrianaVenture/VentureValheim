using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace VentureValheim.TravelTotems;

public static class TravelTotemPlayer
{
    private const string PLAYER_SAVE_KEY_PREFIX = "VV_TD_";
    private static List<int> KnownTravelTotemIDs = new List<int>();
    private static string CurrentSaveString = string.Empty;

    private static bool IsValid(int bin)
    {
        if (bin < 0 || bin >= KnownTravelTotemIDs.Count)
        {
            return false;
        }

        return true;
    }

    public static bool ContainsID(int bin, int id)
    {
        if (!IsValid(bin))
        {
            return false;
        }

        return ContainsIDInternal(bin, id);
    }

    private static bool ContainsIDInternal(int bin, int id)
    {
        return (KnownTravelTotemIDs[bin] & id) != 0;
    }

    public static bool AddKnownTotem(int bin, int id)
    {
        if (bin < 0)
        {
            return false;
        }

        if (bin >= KnownTravelTotemIDs.Count)
        {
            for (int lcv = KnownTravelTotemIDs.Count; lcv <= bin; lcv++)
            {
                KnownTravelTotemIDs.Add(0);
            }
        }

        KnownTravelTotemIDs[bin] = KnownTravelTotemIDs[bin] | id;

        TravelTotemMap.RefreshMapPins();

        return true;
    }

    public static void TryRemoveTotem(string idString)
    {
        if (TryGetTotemID(idString, out int bin, out int id))
        {
            RemoveTotem(bin, id);
        }
    }

    public static void TryAddTotem(string idString)
    {
        string[] entry = idString.Split(new string[1] { ":" }, StringSplitOptions.RemoveEmptyEntries);
        if (TryGetTotemID(idString, out int bin, out int id))
        {
            AddKnownTotem(bin, id);
        }
    }

    private static bool TryGetTotemID(string idString, out int bin, out int id)
    {
        string[] entry = idString.Split(new string[1] { ":" }, StringSplitOptions.RemoveEmptyEntries);
        if (entry.Length == 2 &&
            int.TryParse(entry[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out bin) &&
            int.TryParse(entry[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
        {
            return true;
        }

        bin = -1;
        id = 0;
        return false;
    }

    public static bool RemoveTotem(int bin, int id)
    {
        if (!IsValid(bin))
        {
            return false;
        }

        if (ContainsIDInternal(bin, id))
        {
            KnownTravelTotemIDs[bin] = KnownTravelTotemIDs[bin] - id;
        }

        TravelTotemMap.RefreshMapPins();

        return true;
    }

    public static void ClearKnownTotems()
    {
        KnownTravelTotemIDs.Clear();
        ClearData(ref Player.m_localPlayer);
        TravelTotemMap.RefreshMapPins();
    }

    public static bool TotemUnlocked(bool isCreator, TravelTotemType type, int bin, int id)
    {
        if (isCreator)
        {
            return true;
        }

        if (type == TravelTotemType.AlwaysUnlock)
        {
            return true;
        }

        return ContainsID(bin, id);
    }

    /// <summary>
    /// Returns all the totems that the player can access on the map.
    /// </summary>
    public static List<ZDO> GetValidTotems()
    {
        long playerID = Game.instance.GetPlayerProfile().GetPlayerID();
        List<ZDO> validTotems = new List<ZDO>();

        foreach (ZDO totem in TravelTotems.GetTravelTotems())
        {
            if (totem == null)
            {
                continue;
            }

            bool isCreator = totem.GetLong(ZDOVars.s_creator, 0L) == playerID;
            if (!TravelTotemsPlugin.GetOverrideTotemAccess(isCreator))
            {
                TravelTotemType type = TravelTotemPortal.GetTotemType(totem);

                if (!TotemUnlocked(isCreator, type,
                    totem.GetInt(TravelTotemPortal.TOTEM_ID_BIN),
                    totem.GetInt(TravelTotemPortal.TOTEM_ID)))
                {
                    continue;
                }
            }

            validTotems.Add(totem);
        }

        return validTotems;
    }

    #region Player Save & Load

    private readonly struct FileData
    {
        public List<int> TravelTotemIDs { get; }

        public FileData(List<int> totems)
        {
            TravelTotemIDs = totems;
        }

        public FileData(string saveString)
        {
            TravelTotemIDs = new List<int>();
            if (saveString != null)
            {
                string[] data = saveString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                if (data != null)
                {
                    for (int lcv = 0; lcv < data.Length; lcv++)
                    {
                        if (Int32.TryParse(data[lcv], out int totems))
                        {
                            TravelTotemIDs.Add(totems);
                        }
                        else
                        {
                            TravelTotemsPlugin.TravelTotemsLogger.LogWarning($"Unable to parse travel totem save data for bin: {lcv}");
                            TravelTotemIDs.Add(0);
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            string saveString = "";
            foreach (int totems in TravelTotemIDs)
            {
                saveString += totems.ToString() + ";";
            }

            return saveString;
        }
    }

    /// <summary>
    /// Saves the totem data to the player custom data list.
    /// </summary>
    private static void SaveData(ref Player player, FileData data)
    {
        if (player == null || CurrentSaveString.IsNullOrWhiteSpace() || data.TravelTotemIDs.Count < 1)
        {
            return;
        }

        if (player.m_customData.ContainsKey(CurrentSaveString))
        {
            player.m_customData[CurrentSaveString] = data.ToString();
        }
        else
        {
            player.m_customData.Add(CurrentSaveString, data.ToString());
        }
    }

    /// <summary>
    /// Clears the totem data from the player custom data list.
    /// </summary>
    private static void ClearData(ref Player player)
    {
        if (player != null && player.m_customData.ContainsKey(CurrentSaveString))
        {
            player.m_customData[CurrentSaveString] = "";
        }
    }

    /// <summary>
    /// Attempts to load the player totem data from the player custom data list.
    /// </summary>
    private static FileData? LoadData(ref Player player)
    {
        if (player != null && player.m_customData.ContainsKey(CurrentSaveString))
        {
            return new FileData(player.m_customData[CurrentSaveString]);
        }

        return null;
    }

    #endregion

    #region Patches

    [HarmonyPatch(typeof(Player), nameof(Player.Save))]
    public static class Patch_Player_Save
    {
        private static void Prefix(Player __instance)
        {
            if (!TravelTotemsPlugin.IsInTheMainScene())
            {
                return;
            }

            TravelTotemsPlugin.TravelTotemsLogger.LogDebug($"Attempting to save data with key: {CurrentSaveString}");

            FileData fileData = new FileData(KnownTravelTotemIDs);
            SaveData(ref __instance, fileData);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Load))]
    public static class Patch_Player_Load
    {
        private static void Postfix(Player __instance)
        {
            if (!TravelTotemsPlugin.IsInTheMainScene())
            {
                CurrentSaveString = string.Empty;
                return;
            }

            KnownTravelTotemIDs.Clear();
            long saveKey = WorldGenerator.instance.m_world.m_uid;
            
            if (saveKey == 0L)
            {
                TravelTotemsPlugin.TravelTotemsLogger.LogError("World key not set! Will not be able to retrieve saved data.");
                return;
            }

            CurrentSaveString = PLAYER_SAVE_KEY_PREFIX + saveKey;
            TravelTotemsPlugin.TravelTotemsLogger.LogDebug($"Attempting to load data with key: {CurrentSaveString}");
            FileData? data = LoadData(ref __instance);

            if (data == null)
            {
                return;
            }

            FileData totemData = data.Value;

            if (totemData.TravelTotemIDs != null)
            {
                for (int lcv = 0; lcv < totemData.TravelTotemIDs.Count; lcv++)
                {
                    KnownTravelTotemIDs.Add(totemData.TravelTotemIDs[lcv]);
                }
            }

            TravelTotemMap.RefreshMapPins();
        }
    }

    #endregion
}