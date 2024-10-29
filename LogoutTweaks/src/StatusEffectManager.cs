using System.Collections.Generic;

namespace VentureValheim.LogoutTweaks
{
    public class StatusEffectManager
    {
        private static readonly int Poison = "Poison".GetStableHashCode();
        private static readonly int Burning = "Burning".GetStableHashCode();
        private static readonly int Spirit = "Spirit".GetStableHashCode();

        private static HashSet<int> BasicStatusEffects = new HashSet<int>
        {
            "Rested".GetStableHashCode(),
            "Wet".GetStableHashCode(),
            "GP_Eikthyr".GetStableHashCode(),
            "GP_TheElder".GetStableHashCode(),
            "GP_Bonemass".GetStableHashCode(),
            "GP_Moder".GetStableHashCode(),
            "GP_Yagluth".GetStableHashCode(),
            "GP_Queen".GetStableHashCode(),
            "GP_Fader".GetStableHashCode(),
            "Potion_tasty".GetStableHashCode(),
            "Potion_barleywine".GetStableHashCode(),
            "Potion_frostresist".GetStableHashCode(),
            "Potion_poisonresist".GetStableHashCode(),
            "Potion_health_major".GetStableHashCode(),
            "Potion_health_medium".GetStableHashCode(),
            "Potion_health_minor".GetStableHashCode(),
            "Potion_health_lingering".GetStableHashCode(),
            "Potion_stamina_lingering".GetStableHashCode(),
            "Potion_stamina_medium".GetStableHashCode(),
            "Potion_stamina_minor".GetStableHashCode(),
            "Potion_eitr_minor".GetStableHashCode(),
            "Potion_eitr_lingering".GetStableHashCode(),
            "Potion_BugRepellent".GetStableHashCode(),
            "Potion_bzerker".GetStableHashCode(),
            "Potion_hasty".GetStableHashCode(),
            "Potion_LightFoot".GetStableHashCode(),
            "Potion_strength".GetStableHashCode(),
            "Potion_swimmer".GetStableHashCode(),
            "Potion_tamer".GetStableHashCode(),
            "Potion_TrollPheromones".GetStableHashCode(),
            "CorpseRun".GetStableHashCode(),
            "SoftDeath".GetStableHashCode(),
            "Slimed".GetStableHashCode(),
            "Tared".GetStableHashCode(),
            "Lightning".GetStableHashCode(),
            "Frost".GetStableHashCode(),
            "Immobilized".GetStableHashCode(),
            "ImmobilizedAshlands".GetStableHashCode(),
            "ImmobilizedLong".GetStableHashCode()
        };

        public static bool SupportedStatusEffect(int name)
        {
            return BasicStatusEffects.Contains(name) ||
                name == Poison ||
                name == Burning ||
                name == Spirit;
        }

        public static StatusEffect BuildStatusEffect(StatusEffectData data)
        {
            var original = ObjectDB.instance.GetStatusEffect(data.Name);

            if (original == null)
            {
                return null;
            }

            var se = original.Clone();

            if (BasicStatusEffects.Contains(data.Name))
            {
                se.m_ttl = data.Ttl;
                se.m_time = data.Time;
                return se;
            }
            else if (data.Name == Poison)
            {
                var sePoison = (SE_Poison)se;
                sePoison.m_ttl = data.Ttl;
                sePoison.m_time = data.Time;
                sePoison.m_damageLeft = data.Value1;
                sePoison.m_damagePerHit = data.Value2;
                return sePoison;
            }
            else if (data.Name == Burning || data.Name == Spirit)
            {
                var seBurning = (SE_Burning)se;
                seBurning.m_ttl = data.Ttl;
                seBurning.m_time = data.Time;
                seBurning.m_fireDamageLeft = data.Value1;
                seBurning.m_fireDamagePerHit = data.Value2;
                seBurning.m_spiritDamageLeft = data.Value3;
                seBurning.m_spiritDamagePerHit = data.Value4;
                return seBurning;
            }

            return null;
        }

        public readonly struct StatusEffectData
        {
            public int Name { get; }
            public float Ttl { get; }
            public float Time { get; }

            public float Value1 { get; }
            public float Value2 { get; }
            public float Value3 { get; }
            public float Value4 { get; }

            public StatusEffectData(StatusEffect se)
            {
                Name = se.NameHash();
                Ttl = se.m_ttl;
                Time = se.m_time;
                Value1 = 0;
                Value2 = 0;
                Value3 = 0;
                Value4 = 0;

                if (Name == Poison)
                {
                    var sePosion = (SE_Poison)se;
                    Value1 = sePosion.m_damageLeft;
                    Value2 = sePosion.m_damagePerHit;
                }
                else if (Name == Burning || Name == Spirit)
                {
                    var seBurining = (SE_Burning)se;
                    Value1 = seBurining.m_fireDamageLeft;
                    Value2 = seBurining.m_fireDamagePerHit;
                    Value3 = seBurining.m_spiritDamageLeft;
                    Value4 = seBurining.m_spiritDamagePerHit;
                }
            }

            public StatusEffectData(string data)
            {
                Name = 0;
                Ttl = 0;
                Time = 0;
                Value1 = 0;
                Value2 = 0;
                Value3 = 0;
                Value4 = 0;
                var effectData = data.Split(':');

                if (effectData.Length >= 3 &&
                    int.TryParse(effectData[0], out int name) &&
                    float.TryParse(effectData[1], out float ttl) &&
                    float.TryParse(effectData[2], out float time))
                {
                    Name = name;
                    Ttl = ttl;
                    Time = time;

                    if (effectData.Length >= 5)
                    {
                        if (float.TryParse(effectData[3], out float value1) &&
                            float.TryParse(effectData[4], out float value2))
                        {
                            Value1 = value1;
                            Value2 = value2;
                        }
                    }
                    
                    if (effectData.Length >= 7)
                    {
                        if (float.TryParse(effectData[5], out float value3) &&
                            float.TryParse(effectData[6], out float value4))
                        {
                            Value3 = value3;
                            Value4 = value4;
                        }
                    }
                }
            }

            public override string ToString()
            {
                if (Name == Poison)
                {
                    return $"{Name}:{Ttl}:{Time}:{Value1}:{Value2}";
                }
                else if (Name == Burning || Name == Spirit)
                {
                    return $"{Name}:{Ttl}:{Time}:{Value1}:{Value2}:{Value3}:{Value4}";
                }

                return $"{Name}:{Ttl}:{Time}";
            }
        }
    }
}
