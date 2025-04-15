using Godot;
using S7.Net;
using S7.Net.Types;
using System;

[Tool]
[Icon("uid://06dunvu30lio")]
[GlobalClass]
public partial class BoolItem : DataItem
{
    [Signal]
    public delegate void ValueChangedEventHandler(bool newValue);

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
    private bool _gdValue = false;

    [Export]
    public bool GDValue
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



    private void UpdateVisualComponent(bool value)
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
            if (Value is bool boolValue)
            {
                _gdValue = boolValue;
                UpdateVisualComponent(_gdValue);
                return;
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
                GDValue = value.AsBool();
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error updating Value: {ex.Message}");
        }
    }

    public BoolItem()
    {
        VarType = VarType.Bit;
        DataType = DataType.Input;
        Count = 1;
        Value = false;
    }

}
