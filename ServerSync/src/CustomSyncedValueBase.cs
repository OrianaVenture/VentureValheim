using System;

namespace VentureValheim.ServerSync;

public abstract class CustomSyncedValueBase
{
    public event Action? ValueChanged;

    public object? LocalBaseValue;

    public readonly string Identifier;
    public readonly Type Type;

    private object? boxedValue;

    public object? BoxedValue
    {
        get => boxedValue;
        set
        {
            boxedValue = value;
            ValueChanged?.Invoke();
        }
    }

    protected bool localIsOwner;

    protected CustomSyncedValueBase(ConfigSync configSync, string identifier, Type type)
    {
        Identifier = identifier;
        Type = type;
        configSync.AddCustomValue(this);
        localIsOwner = configSync.IsSourceOfTruth;
        configSync.SourceOfTruthChanged += truth => localIsOwner = truth;
    }
}