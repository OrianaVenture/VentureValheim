using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.Progression
{
    public interface ISkillsManager
    {
        public int BossKeysSkillPerKey { get; }
        public bool OverrideMaximumSkillLevel { get; }
        public float MaximumSkillLevel { get; }
        public bool OverrideMinimumSkillLevel { get; }
        public float MinimumSkillLevel { get; }
        public bool UseAbsoluteSkillDrain { get; }
        public int AbsoluteSkillDrain { get; }
        public bool CompareAndSelectDrain { get; }
        public bool CompareUseMinimumDrain { get; }
    }

    public class SkillsManager : ISkillsManager
    {
        static SkillsManager() { }
        protected SkillsManager() { }
        private static readonly ISkillsManager _instance = new SkillsManager();

        public static SkillsManager Instance
        {
            get => _instance as SkillsManager;
        }

        public bool UseBossKeysForSkillLevel { get; protected set; }
        public bool UsePrivateBossKeysForSkillLevel { get; protected set; }
        public int BossKeysSkillPerKey { get; protected set; }
        public int PublicBossKeysCount { get; protected set; }
        public bool OverrideMaximumSkillLevel { get; protected set; }
        public float MaximumSkillLevel { get; protected set; }
        public bool OverrideMinimumSkillLevel { get; protected set; }
        public float MinimumSkillLevel { get; protected set; }
        public bool UseAbsoluteSkillDrain { get; protected set; }
        public int AbsoluteSkillDrain { get; protected set; }
        public bool CompareAndSelectDrain { get; protected set; }
        public bool CompareUseMinimumDrain { get; protected set; }

        public const float SKILL_MINIMUM = 0f;
        public const float SKILL_MAXIMUM = 100f;

        private float _cachedSkillCeiling, _cachedSkillFloor;
        private static float _timer = 0f;
        private readonly float _update = 5f;

        /// <summary>
        /// Updates class data if cached values have expired.
        /// </summary>
        public void Update()
        {
            var time = Time.time;
            var delta = time - _timer;

            if (delta > _update)
            {
                UpdateConfigs(delta);

                int bossesDefeated;
                if (UsePrivateBossKeysForSkillLevel)
                {
                    bossesDefeated = KeyManager.Instance.GetPrivateBossKeysCount();
                }
                else
                {
                    bossesDefeated = KeyManager.Instance.GetPublicBossKeysCount();
                }


                _cachedSkillCeiling = GetBossSkillCeiling(bossesDefeated);
                _cachedSkillFloor = GetBossSkillFloor(bossesDefeated);

                _timer = time;
            }
        }

        /// <summary>
        /// Update cached configuration values.
        /// </summary>
        /// <param name="delta"></param>
        protected virtual void UpdateConfigs(float delta)
        {
            ProgressionPlugin.GetProgressionLogger().LogDebug($"Updating cached Skill Information: {delta} time passed.");

            UseBossKeysForSkillLevel = ProgressionPlugin.Instance.GetUseBossKeysForSkillLevel();
            UsePrivateBossKeysForSkillLevel = ProgressionPlugin.Instance.GetUsePrivateBossKeysForSkillLevel();
            BossKeysSkillPerKey = ProgressionPlugin.Instance.GetBossKeysSkillPerKey();
            OverrideMaximumSkillLevel = ProgressionPlugin.Instance.GetOverrideMaximumSkillLevel();
            MaximumSkillLevel = ProgressionPlugin.Instance.GetMaximumSkillLevel();
            OverrideMinimumSkillLevel = ProgressionPlugin.Instance.GetOverrideMinimumSkillLevel();
            MinimumSkillLevel = ProgressionPlugin.Instance.GetMinimumSkillLevel();
            UseAbsoluteSkillDrain = ProgressionPlugin.Instance.GetUseAbsoluteSkillDrain();
            AbsoluteSkillDrain = ProgressionPlugin.Instance.GetAbsoluteSkillDrain();
            CompareAndSelectDrain = ProgressionPlugin.Instance.GetCompareAndSelectDrain();
            CompareUseMinimumDrain = ProgressionPlugin.Instance.GetCompareUseMinimumDrain();
        }

        /// <summary>
        /// Returns the skill ceiling based on number of bosses defeated and configuration for skill per level.
        /// </summary>
        /// <param name="bossesDefeated"></param>
        /// <returns></returns>
        protected float GetBossSkillCeiling(int bossesDefeated)
        {
            return NormalizeSkillLevel(SKILL_MAXIMUM - (BossKeysSkillPerKey * (KeyManager.TOTAL_BOSSES - bossesDefeated)));
        }

        /// <summary>
        /// Returns the skill floor based on number of bosses defeated and configuration for skill per level.
        /// </summary>
        /// <param name="bossesDefeated"></param>
        /// <returns></returns>
        protected float GetBossSkillFloor(int bossesDefeated)
        {
            return NormalizeSkillLevel(BossKeysSkillPerKey * bossesDefeated);
        }

        /// <summary>
        /// Returns the total skill drain based off configurations.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="floor"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        protected float GetSkillDrain(float level, float floor, float factor)
        {
            if (floor < level)
            {
                if (UseAbsoluteSkillDrain)
                {
                    var drain = AbsoluteSkillDrain;

                    if (CompareAndSelectDrain)
                    {
                        if (CompareUseMinimumDrain)
                        {
                            return Mathf.Min(level * factor, drain);
                        }
                        else
                        {
                            return Mathf.Max(level * factor, drain);
                        }
                    }

                    return drain;
                }
                else
                {
                    return level * factor;
                }
            }

            return 0f;
        }

        /// <summary>
        /// Get the accumulation gain for a skill.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="ceiling"></param>
        /// <param name="increase"></param>
        /// <returns>Skill increase or 0 if no gain</returns>
        protected float GetSkillAccumulationGain(float level, float ceiling, float increase)
        {
            return ceiling > level ? increase : 0f;
        }

        /// <summary>
        /// Normalize a skill by making sure it is within the minimum (0) and maximum skill bounds.
        /// </summary>
        /// <param name="level"></param>
        /// <returns>level or closest bound for the skill</returns>
        protected float NormalizeSkillLevel(float level)
        {
            var maximum = SKILL_MAXIMUM;

            if (OverrideMaximumSkillLevel)
            {
                maximum = MaximumSkillLevel;
            }

            return Mathf.Clamp(level, SKILL_MINIMUM, maximum);
        }

        /// <summary>
        /// Calculates the minimum a skill can be lowered based off configurations.
        /// </summary>
        /// <returns></returns>
        private float GetSkillDrainFloor()
        {
            var minimum = SKILL_MINIMUM;

            if (OverrideMinimumSkillLevel)
            {
                minimum = MinimumSkillLevel;
            }
            else if (UseBossKeysForSkillLevel)
            {
                return _cachedSkillFloor;
            }

            return minimum;
        }

        /// <summary>
        /// Calculates the maximum a skill can be raised based off configurations.
        /// </summary>
        /// <returns></returns>
        private float GetSkillGainCeiling()
        {
            var maximum = SKILL_MAXIMUM;

            if (OverrideMaximumSkillLevel)
            {
                maximum = MaximumSkillLevel;
            }
            else if (UseBossKeysForSkillLevel)
            {
                return _cachedSkillCeiling;
            }

            return maximum;
        }

        #region Patches

        /// <summary>
        /// Changes how Skills are lowered based on the configured skill floor.
        /// </summary>
        [HarmonyPatch(typeof(Skills), nameof(Skills.LowerAllSkills))]
        public static class Patch_Skills_LowerAllSkills
        {
            private static bool Prefix(Skills __instance, float factor)
            {
                Instance.Update();

                if (!ProgressionPlugin.Instance.GetEnableSkillManager())
                {
                    return true; // Do nothing
                }

                if (ProgressionPlugin.Instance.GetAllowSkillDrain())
                {
                    foreach (KeyValuePair<Skills.SkillType, Skills.Skill> skillDatum in __instance.m_skillData)
                    {
                        var floor = Instance.GetSkillDrainFloor();
                        var skillDrain = Instance.GetSkillDrain(skillDatum.Value.m_level, floor, factor);
                        skillDatum.Value.m_level = Instance.NormalizeSkillLevel(skillDatum.Value.m_level - skillDrain);
                        skillDatum.Value.m_accumulator = 0f;
                    }

                    __instance.m_player.Message(MessageHud.MessageType.TopLeft, "$msg_skills_lowered");

                    return false; // Skip original method
                }

                return false; // Skip original method
            }
        }

        /// <summary>
        /// Changes how skills are raised based on the configured skill ceiling.
        /// </summary>
        [HarmonyPatch(typeof(Skills.Skill), nameof(Skills.Skill.Raise))]
        public static class Patch_Skills_Skill_Raise
        {
            private static bool Prefix(Skills.Skill __instance, float factor, ref bool __result)
            {
                Instance.Update();

                if (!ProgressionPlugin.Instance.GetEnableSkillManager())
                {
                    return true; // Do nothing
                }

                var increase = __instance.m_info.m_increseStep * factor;
                var ceiling = Instance.GetSkillGainCeiling();

                float accumulation = Instance.GetSkillAccumulationGain(__instance.m_level, ceiling, increase);

                if (__instance.m_accumulator + accumulation >= __instance.GetNextLevelRequirement())
                {
                    __instance.m_level = Instance.NormalizeSkillLevel(++__instance.m_level);
                    __instance.m_accumulator = 0f;
                    __result = true; // level up
                }
                else
                {
                    __instance.m_accumulator += accumulation;
                    __result = false; // no level up
                }

                return false; // Skip original method
            }
        }

        #endregion
    }
}