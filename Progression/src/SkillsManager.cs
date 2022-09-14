using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.Progression
{
    public class SkillsManager
    {
        public const float SKILL_MINIMUM = 0f;
        public const float SKILL_MAXIMUM = 100f;

        private SkillsManager() {}
        private static readonly SkillsManager _instance = new SkillsManager();

        public static SkillsManager Instance
        {
            get => _instance;
        }

        public void Initialize()
        {
        }

        /// <summary>
        /// Changes how Skills are lowered based on the configured skill floor.
        /// </summary>
        [HarmonyPatch(typeof(Skills), nameof(Skills.LowerAllSkills))]
        public static class Patch_Skills_LowerAllSkills
        {
            private static bool Prefix(Skills __instance, float factor)
            {
                if (!ProgressionPlugin.Instance.GetEnableSkillManager())
                {
                    return true; // Do nothing
                }

                if (ProgressionPlugin.Instance.GetAllowSkillDrain())
                {
                    foreach (KeyValuePair<Skills.SkillType, Skills.Skill> skillDatum in __instance.m_skillData)
                    {
                        var floor = Instance.GetSkillDrainFloor();
                        var skillDrain = _instance.GetSkillDrain(skillDatum.Value.m_level, floor, factor);
                        skillDatum.Value.m_level = _instance.NormalizeSkillLevel(skillDatum.Value.m_level - skillDrain);
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
                if (!ProgressionPlugin.Instance.GetEnableSkillManager())
                {
                    return true; // Do nothing
                }

                var increase = __instance.m_info.m_increseStep * factor;
                var ceiling = _instance.GetSkillGainCeiling();

                float accumulation = _instance.GetSkillAccumulationGain(__instance.m_level, ceiling, increase);

                if (__instance.m_accumulator + accumulation >= __instance.GetNextLevelRequirement())
                {
                    __instance.m_level = _instance.NormalizeSkillLevel(++__instance.m_level);
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

        /// <summary>
        /// Returns the total skill drain based off configurations.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="floor"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public float GetSkillDrain(float level, float floor, float factor)
        {
            if (floor < level)
            {
                if (ProgressionPlugin.Instance.GetUseAbsoluteSkillDrain())
                {
                    var drain = ProgressionPlugin.Instance.GetAbsoluteSkillDrain();

                    if (ProgressionPlugin.Instance.GetCompareAndSelectDrain())
                    {
                        if (ProgressionPlugin.Instance.GetCompareUseMinimumDrain())
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
        public float GetSkillAccumulationGain(float level, float ceiling, float increase)
        {
            return ceiling > level ? increase : 0f;
        }

        /// <summary>
        /// Normalize a skill by making sure it is within the minimum (0) and maximum skill bounds.
        /// </summary>
        /// <param name="level"></param>
        /// <returns>level or closest bound for the skill</returns>
        public float NormalizeSkillLevel(float level)
        {
            var maximum = SKILL_MAXIMUM;

            if (ProgressionPlugin.Instance.GetOverrideMaximumSkillLevel())
            {
                maximum = ProgressionPlugin.Instance.GetMaximumSkillLevel();
            }

            return Mathf.Clamp(level, SKILL_MINIMUM, maximum);
        }

        public float GetSkillDrainFloor()
        {
            // TODO: calculate skill floor based on global and player keys
            var minimum = SKILL_MINIMUM;

            if (ProgressionPlugin.Instance.GetOverrideMinimumSkillLevel())
            {
                minimum = ProgressionPlugin.Instance.GetMinimumSkillLevel();
            }

            return minimum;
        }

        public float GetSkillGainCeiling()
        {
            // TODO: calculate skill ceiling based on global and player keys
            var maximum = SKILL_MAXIMUM;

            if (ProgressionPlugin.Instance.GetOverrideMaximumSkillLevel())
            {
                maximum = ProgressionPlugin.Instance.GetMaximumSkillLevel();
            }

            return maximum;
        }
    }
}