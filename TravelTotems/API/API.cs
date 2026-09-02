using JetBrains.Annotations;

namespace VentureValheim.TravelTotems;

[PublicAPI]
public class API
{
    public static int GetTotemPinIndex() => TravelTotemMapPin.TotemIndex;
}