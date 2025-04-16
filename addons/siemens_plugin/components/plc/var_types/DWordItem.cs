using Godot;
using S7.Net;
using S7.Net.Types;
using System;

[Tool]
[Icon("uid://cylx5baefdiwx")]
[GlobalClass]
public partial class DWordItem : DataItem
{
    [Signal]
    public delegate void ValueChangedEventHandler(uint newValue);

    [Export]
    public override Node VisualComponent
    {
        get => _visualComponent;
        set
        {
            _visualComponent = value;
            NotifyPropertyListChanged();
        }
    }

    [Export]
    public override string VisualProperty
    {
        get => _visualProperty;
        set
        {
            if (_visualProperty == value) return;
            _visualProperty = value;
        }
    }

    private Node _visualComponent;
    private string _visualProperty;
    private uint _gdValue;

    [Export]
    public uint GDValue
    {
        get => _gdValue;
        set
        {
            if (_gdValue != value)
            {
                _gdValue = value;
                Value = value;
                EmitSignal(SignalName.ValueChanged, value);
            }
        }
    }

    private void UpdateVisualComponent(uint value)
    {
        if (VisualComponent != null && !string.IsNullOrEmpty(VisualProperty))
        {
            VisualComponent.Set(VisualProperty, value);
        }
    }

    public override void UpdateGDValue()
    {
        try
        {
            if (Value is uint uintValue)
            {
                _gdValue = uintValue;
                UpdateVisualComponent(_gdValue);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error updating GDValue: {ex.Message}");
        }
    }

    public override void UpdateValue()
    {
        try
        {
            if (VisualComponent != null && !string.IsNullOrEmpty(VisualProperty))
            {
                var value = VisualComponent.Get(VisualProperty);
                GDValue = value.AsUInt32();
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error updating Value: {ex.Message}");
        }
    }

    public DWordItem()
    {
        VarType = VarType.DWord;
        DataType = DataType.Input;
        Count = 1;
        Value = (uint)0;
    }
}
