using System;

namespace VentureValheim.TravelTotems;

[Flags]
public enum TravelTotemType : int
{
    Undefined = 0,
    AlwaysUnlock = 1,
    FindUnlock = 2,
    AlwaysLock = 3,
    ServerDefault = 4
}