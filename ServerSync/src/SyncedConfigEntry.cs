using BepInEx.Configuration;
using JetBrains.Annotations;

namespace VentureValheim.ServerSync;

[PublicAPI]
public class SyncedConfigEntry<T> : OwnConfigEntryBase
{
    public override ConfigEntryBase BaseConfig => SourceConfig;
    public readonly ConfigEntry<T> SourceConfig;

    public SyncedConfigEntry(ConfigEntry<T> sourceConfig)
    {
        SourceConfig = sourceConfig;
    }

    public T Value
    {
        get => SourceConfig.Value;
        set => SourceConfig.Value = value;
    }

    public void AssignLocalValue(T value)
    {
        if (LocalBaseValue == null)
        {
            Value = value;
        }
        else
        {
            LocalBaseValue = value;
        }
    }
}
