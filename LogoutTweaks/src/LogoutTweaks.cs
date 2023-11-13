using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace VentureValheim.LogoutTweaks
{
    public class LogoutTweaks
    {
        private LogoutTweaks()
        {
        }
        private static readonly LogoutTweaks _instance = new LogoutTweaks();

        public static LogoutTweaks Instance
        {
            get => _instance;
        }

        private const string PLAYER_SAVE_KEY = "VV_LogoutData";

        private readonly struct FileData
        {
            public List<StatusEffectData> StatusEffects { get; }

            public FileData(List<StatusEffectData> effects)
            {
                StatusEffects = effects;
            }

            public FileData(string saveString)
            {
                StatusEffects = new List<StatusEffectData>();
                if (saveString != null)
                {
                    var data = saveString.Split(';');

                    if (data != null)
                    {
                        // Status Effects
                        for (int lcv = 0; lcv < data.Length; lcv++)
                        {
                            var effectData = data[lcv].Split(':');
                            if (effectData.Length == 3)
                            {
                                if (int.TryParse(effectData[0], out int name) &&
                                    float.TryParse(effectData[1], out float ttl) &&
                                    float.TryParse(effectData[2], out float time))
                                {
                                    StatusEffects.Add(new StatusEffectData(name, ttl, time));
                                }
                            }
                        }
                    }
                }
            }

            public override string ToString()
            {
                string saveString = "";
                foreach (var effect in StatusEffects)
                {
                    saveString += effect.ToString() + ";";
                }

                return saveString;
            }
        }

        private readonly struct StatusEffectData
        {
            public int Name { get; }
            public float Ttl { get; }
            public float Time { get; }

            public StatusEffectData(int name, float ttl, float time)
            {
                Name = name;
                Ttl = ttl;
                Time = time;
            }

            public override string ToString()
            {
                return $"{Name}:{Ttl}:{Time}";
            }
        }

        /// <summary>
        /// Saves the effects data to the player custom data list.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        private void SaveData(ref Player player, FileData data)
        {
            if (player == null)
            {
                return;
            }

            if (player.m_customData.ContainsKey(PLAYER_SAVE_KEY))
            {
                player.m_customData[PLAYER_SAVE_KEY] = data.ToString();
            }
            else
            {
                player.m_customData.Add(PLAYER_SAVE_KEY, data.ToString());
            }
        }

        /// <summary>
        /// Clears the effects data from the player custom data list.
        /// </summary>
        /// <param name="player"></param>
        private void ClearData(ref Player player)
        {
            if (player != null && player.m_customData.ContainsKey(PLAYER_SAVE_KEY))
            {
                player.m_customData[PLAYER_SAVE_KEY] = "";
            }
        }

        /// <summary>
        /// Attempts to load the player effects data from the player custom data list.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private FileData? LoadData(ref Player player)
        {
            if (player != null && player.m_customData.ContainsKey(PLAYER_SAVE_KEY))
            {
                return new FileData(player.m_customData[PLAYER_SAVE_KEY]);
            }

            return null;
        }

        public static bool IsInTheMainScene()
        {
            return SceneManager.GetActiveScene().name.Equals("main");
        }

        #region Patches

        [HarmonyPatch(typeof(Player), nameof(Player.Save))]
        public static class Patch_Player_Save
        {
            private static void Prefix(Player __instance)
            {
                if (!IsInTheMainScene())
                {
                    return;
                }

                try
                {
                    List<StatusEffectData> data = new List<StatusEffectData>();
                    var effects = __instance.m_seman.GetStatusEffects();

                    if (effects == null)
                    {
                        LogoutTweaksPlugin.LogoutTweaksLogger.LogInfo("Unable to determine status effects, this can indicate a mod conflict.");
                    }
                    else
                    {
                        for (int lcv = 0; lcv < effects.Count; lcv++)
                        {
                            int name = effects[lcv].NameHash();
                            float ttl = effects[lcv].m_ttl;
                            float time = effects[lcv].m_time;

                            StatusEffectData effect = new StatusEffectData(name, ttl, time);
                            data.Add(effect);
                        }
                    }

                    FileData fileData = new FileData(data);
                    Instance.SaveData(ref __instance, fileData);
                }
                catch (Exception e)
                {
                    LogoutTweaksPlugin.LogoutTweaksLogger.LogError("Error trying to parse and save extra data.");
                    LogoutTweaksPlugin.LogoutTweaksLogger.LogError(e);
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Load))]
        public static class Patch_Player_Load
        {
            private static void Postfix(Player __instance)
            {
                if (!IsInTheMainScene())
                {
                    return;
                }

                var data = Instance.LoadData(ref __instance);

                if (data == null)
                {
                    return;
                }

                FileData logoutData = data.Value;

                if (logoutData.StatusEffects != null)
                {
                    var effects = logoutData.StatusEffects;
                    for (int lcv = 0; lcv < effects.Count; lcv++)
                    {
                        int name = effects[lcv].Name;
                        float ttl = effects[lcv].Ttl;
                        float time = effects[lcv].Time;

                        try
                        {
                            __instance.m_seman.AddStatusEffect(name);
                            StatusEffect statusEffect = __instance.m_seman.GetStatusEffect(name);
                            if (statusEffect != null)
                            {
                                statusEffect.m_ttl = ttl;
                                statusEffect.m_time = time;
                            }
                            else
                            {
                                LogoutTweaksPlugin.LogoutTweaksLogger.LogWarning($"Status Effect {name} could not be restored.");
                            }
                        }
                        catch (Exception e)
                        {
                            LogoutTweaksPlugin.LogoutTweaksLogger.LogWarning($"Status Effect {name} could not be restored.");
                            LogoutTweaksPlugin.LogoutTweaksLogger.LogDebug(e);
                        }
                    }

                    Hud.instance.UpdateStatusEffects(__instance.m_seman.m_statusEffects);
                }

                // Wipe data after loading
                Instance.ClearData(ref __instance);
            }
        }

        #endregion
    }
}