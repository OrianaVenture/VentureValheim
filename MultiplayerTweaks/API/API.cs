using JetBrains.Annotations;

namespace VentureValheim.MultiplayerTweaks;

[PublicAPI]
public class API
{
    public static int GetHaldorPinIndex() => TraderMapTweaks.HaldorIndex;
    public static int GetHildirPinIndex() => TraderMapTweaks.HildirIndex;
    public static int GetBogWitchPinIndex() => TraderMapTweaks.BogWitchIndex;
}