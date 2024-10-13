using System.Collections;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.NeedForSpeed
{
    public class NeedForSpeed
    {
        private NeedForSpeed()
        {
        }
        private static readonly NeedForSpeed _instance = new NeedForSpeed();

        public static NeedForSpeed Instance
        {
            get => _instance;
        }

        private float _runSpeedMultiplier = 1f;
        private float _jogSpeedMultiplier = 1f;

        private IEnumerator _resetCoroutine;

        public IEnumerator WatchGround()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.25f);
                if (Player.m_localPlayer == null)
                {
                    break;
                }
                UpdateMultiplier();
            }
        }

        public void UpdateMultiplier()
        {
            if (OnPath())
            {
                _runSpeedMultiplier = NeedForSpeedPlugin.Instance.GetRunSpeedMultiplier();
                _jogSpeedMultiplier = NeedForSpeedPlugin.Instance.GetJogSpeedMultiplier();
            }
            else
            {
                _runSpeedMultiplier = 1f;
                _jogSpeedMultiplier = 1f;
            }
        }

        public bool OnPath()
        {
            var ground = Player.m_localPlayer.m_lastGroundCollider;

            if (ground == null)
            {
                return false;
            }

            var heightmap = ground.GetComponent<Heightmap>();
            if (heightmap != null && heightmap.IsCleared(Player.m_localPlayer.transform.position))
            {
                return true;
            }

            return false;
        }

        #region Patches

        [HarmonyPatch(typeof(Player), nameof(Player.SetLocalPlayer))]
        public static class Patch_Player_SetLocalPlayer
        {
            private static void Postfix(Player __instance)
            {
                if (SceneManager.GetActiveScene().name.Equals("main"))
                {
                    Instance._resetCoroutine = Instance.WatchGround();
                    __instance.StartCoroutine(Instance._resetCoroutine);
                    
                }
                else if(Instance._resetCoroutine != null)
                {
                    __instance.StopCoroutine(Instance._resetCoroutine);
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.GetJogSpeedFactor))]
        public static class Patch_Character_GetJogSpeedFactor
        {
            private static void Postfix(ref float __result)
            {
                __result *= Instance._jogSpeedMultiplier;
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.GetRunSpeedFactor))]
        public static class Patch_Character_GetRunSpeedFactor
        {
            private static void Postfix(ref float __result)
            {
                __result *= Instance._runSpeedMultiplier;
            }
        }

        #endregion
    }
}