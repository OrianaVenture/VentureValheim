using HarmonyLib;
using System.Collections.Generic;

namespace VentureValheim.Progression
{
    public partial class KeyManager
    {
        private static HashSet<string> GetPossiblePlayerEvents(string player)
        {
            HashSet<string> playerKeys;
            if (Instance.ServerPrivateKeysList.ContainsKey(player))
            {
                playerKeys = Instance.ServerPrivateKeysList[player];
            }
            else
            {
                playerKeys = new HashSet<string>();
            }

            return GetPossiblePlayerEvents(playerKeys);
        }

        private static HashSet<string> GetPossiblePlayerEvents(HashSet<string> playerKeys)
        {
            var events = new HashSet<string>();

            foreach (RandomEvent randEvent in RandEventSystem.instance.m_events)
            {
                if (IsValidEvent(randEvent, playerKeys))
                {
                    events.Add(randEvent.m_name);
                }
            }

            return events;
        }

        private static bool IsValidEvent(RandomEvent randEvent, HashSet<string> playerKeys)
        {
            foreach (string requiredGlobalKey in randEvent.m_requiredGlobalKeys)
            {
                if (!playerKeys.Contains(requiredGlobalKey))
                {
                    return false;
                }
            }
            foreach (string notRequiredGlobalKey in randEvent.m_notRequiredGlobalKeys)
            {
                if (playerKeys.Contains(notRequiredGlobalKey))
                {
                    return false;
                }
            }

            return true;
        }

        private static RandEventSystem.PlayerEventData GetPlayerEventData(ZNetPeer peer)
        {
            var eventData = default(RandEventSystem.PlayerEventData);
            eventData.position = peer.m_refPos;
            eventData.possibleEvents = GetPossiblePlayerEvents(peer.m_playerName);
            eventData.baseValue = 0;
            if (peer.m_serverSyncedPlayerData.TryGetValue("baseValue", out var basevalue))
            {
                int.TryParse(basevalue, out eventData.baseValue);
            }
            return eventData;
        }

        private static RandEventSystem.PlayerEventData GetHostPlayerEventData()
        {
            var eventData = default(RandEventSystem.PlayerEventData);
            eventData.position = ZNet.instance.GetReferencePosition();
            eventData.possibleEvents = GetPossiblePlayerEvents(Instance.PrivateKeysList);
            eventData.baseValue = Player.m_localPlayer.m_nview.GetZDO().GetInt(ZDOVars.s_baseValue);
            return eventData;
        }

        /// <summary>
        /// Fills the RandEventSystem.playerEventDatas list with all possible random events
        /// when using private keys.
        /// </summary>
        [HarmonyPatch(typeof(RandEventSystem), nameof(RandEventSystem.RefreshPlayerEventData))]
        public static class Patch_RandEventSystem_RefreshPlayerEventData
        {
            private static bool Prefix(ref List<RandEventSystem.PlayerEventData> __result)
            {
                if (!ProgressionConfiguration.Instance.GetUsePrivateRaids() ||
                    !ProgressionConfiguration.Instance.GetUsePrivateKeys())
                {
                    return true;
                }

                RandEventSystem.playerEventDatas.Clear();

                if (!ZNet.instance.IsDedicated())
                {
                    RandEventSystem.playerEventDatas.Add(GetHostPlayerEventData());
                }

                foreach (ZNetPeer peer in ZNet.instance.GetPeers())
                {
                    if (peer.IsReady())
                    {
                        RandEventSystem.playerEventDatas.Add(GetPlayerEventData(peer));
                    }
                }

                __result = RandEventSystem.playerEventDatas;
                return false; // Skip 
            }
        }

        /// <summary>
        /// Always return true if using this mod's private raids.
        /// Allows entry into GetValidEventPoints which will check the playerEventDatas list
        /// which is only populated with valid events.
        /// </summary>
        [HarmonyPatch(typeof(RandEventSystem), nameof(RandEventSystem.HaveGlobalKeys))]
        public static class Patch_RandEventSystem_HaveGlobalKeys
        {
            private static bool Prefix(ref bool __result)
            {
                if (!ProgressionConfiguration.Instance.GetUsePrivateRaids() || 
                    !ProgressionConfiguration.Instance.GetUsePrivateKeys())
                {
                    return true;
                }

                __result = true;
                return false; // Skip
            }
        }
    }
}
