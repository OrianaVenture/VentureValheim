using System;
using HarmonyLib;
using UnityEngine;

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

        public void Update(FootStep foot, Transform transform)
        {
            if (OnPath(foot, transform))
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

        public bool OnPath(FootStep foot, Transform transform)
        {
            var ground = foot.m_character.GetLastGroundCollider();

            if (ground == null)
            {
                return false;
            }

            var heightmap = ground.GetComponent<Heightmap>();
            if (heightmap != null && heightmap.IsCleared(transform.position))
            {
                return true;
            }

            return false;
        }

        #region Patches

        [HarmonyPatch(typeof(FootStep), nameof(FootStep.OnFoot), new Type[] { typeof(Transform) })]
        public static class Patch_FootStep_OnFoot
        {
            private static void Postfix(FootStep __instance, Transform foot)
            {
                if (__instance.m_character == Player.m_localPlayer)
                {
                    Instance.Update(__instance, foot);
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