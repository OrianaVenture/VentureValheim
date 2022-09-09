using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.Progression
{
    public class SkillsManager
    {
        public static bool AllowSkillDrain { get; private set; }
        public static bool UseAbsoluteSkillDrain { get; private set; }
        public static float AbsoluteSkillDrain { get; private set; }
        public static bool CompareAndSelectDrain { get; private set; }
        public static bool CompareUseMinimumDrain { get; private set; }

        private SkillsManager() {}
        private static readonly SkillsManager _instance = new SkillsManager();

        public static SkillsManager Instance
        {
            get => _instance;
        }

        public void Initialize(bool allowSkillDrain, bool useAbsoluteSkillDrain, float absoluteSkillDrain,
            bool compareAndSelectDrain, bool compareUseMinimumDrain)
        {
            AllowSkillDrain = allowSkillDrain;
            UseAbsoluteSkillDrain = useAbsoluteSkillDrain;
            AbsoluteSkillDrain = absoluteSkillDrain;
            CompareAndSelectDrain = compareAndSelectDrain;
            CompareUseMinimumDrain = compareUseMinimumDrain;
        }

        /// <summary>
        /// Changes how Skills are lowered based on the configured skill floor.
        /// </summary>
        [HarmonyPatch(typeof(Skills), nameof(Skills.LowerAllSkills))]
        public static class Patch_Skills_LowerAllSkills
        {
            private static bool Prefix(Skills __instance, float factor)
            {
                if (AllowSkillDrain)
                {
                    foreach (KeyValuePair<Skills.SkillType, Skills.Skill> skillDatum in __instance.m_skillData)
                    {
                        var floor = ProgressionManager.Instance.GetSkillDrainFloor(skillDatum.Value.m_level);
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
                var increase = __instance.m_info.m_increseStep * factor;
                var ceiling = ProgressionManager.Instance.GetSkillGainCeiling(__instance.m_level);

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
                if (UseAbsoluteSkillDrain)
                {
                    if (CompareAndSelectDrain)
                    {
                        if (CompareUseMinimumDrain)
                        {
                            return Mathf.Min(level * factor, AbsoluteSkillDrain);
                        }
                        else
                        {
                            return Mathf.Max(level * factor, AbsoluteSkillDrain);
                        }
                    }

                    return AbsoluteSkillDrain;
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
        /// Normalize a skill by making sure it is within the minimum and maximum skill bounds.
        /// </summary>
        /// <param name="level"></param>
        /// <returns>level or closest bound for the skill</returns>
        public float NormalizeSkillLevel(float level)
        {
            // TODO: Set min/max level
            return Mathf.Clamp(level, 0f, 100f);
        }
    }
}