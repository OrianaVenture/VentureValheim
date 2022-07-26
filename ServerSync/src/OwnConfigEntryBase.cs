using BepInEx.Configuration;

namespace VentureValheim.ServerSync;

public abstract class OwnConfigEntryBase
{
    public object? LocalBaseValue;
    public abstract ConfigEntryBase BaseConfig { get; }

    public bool SynchronizedConfig = true;
}