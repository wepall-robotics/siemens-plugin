using Godot;
using S7.Net;
using S7.Net.Types;
using System;

[Tool]
[Icon("uid://bbvxn2ddshx62")]
[GlobalClass]
public partial class IntItem : DataItem
{
    [Signal]
    public delegate void ValueChangedEventHandler(short newValue);

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
    private short _gdValue;

    [Export]
    public short GDValue
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

    private void UpdateVisualComponent(short value)
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
            if (Value is short shortValue)
            {
                _gdValue = shortValue;
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
                GDValue = value.AsInt16();
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error updating Value: {ex.Message}");
        }
    }

    public IntItem()
    {
        VarType = VarType.Int;
        DataType = DataType.Input;
        Count = 1;
        Value = (short)0;
    }
}
