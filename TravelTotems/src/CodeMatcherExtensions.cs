using HarmonyLib;

namespace VentureValheim.TravelTotems;

public static class CodeMatcherExtensions
{
    public static CodeMatcher ExtractOperand(this CodeMatcher matcher, out object operand)
    {
        operand = matcher.Operand;
        return matcher;
    }
}