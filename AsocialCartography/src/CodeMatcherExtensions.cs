using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace VentureValheim.AsocialCartography;

public static class CodeMatcherExtensions
{
    public static CodeMatcher ExtractLabels(this CodeMatcher matcher, out List<Label> labels)
    {
        Label[] array = new Label[matcher.Labels.Count];
        matcher.Labels.CopyTo(array);
        matcher.Labels.Clear();
        labels = array.ToList();

        return matcher;
    }
}