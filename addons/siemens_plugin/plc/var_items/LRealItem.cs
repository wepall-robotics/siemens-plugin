using Godot;
using S7.Net;
using S7.Net.Types;
using System;

[Tool]
[Icon("uid://btntvh24eorex")]
[GlobalClass]
public partial class LRealItem : DataItem
{
    [Signal]
    public delegate void ValueChangedEventHandler(double newValue);

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
    private double _gdValue;

    [Export]
    public double GDValue
    {
        get => _gdValue;
        set
        {
            if (Math.Abs(_gdValue - value) > 0.0000000001d) // Comparación de doubles
            {
                _gdValue = value;
                Value = value;
                EmitSignal(SignalName.ValueChanged, value);
            }
        }
    }

    private void UpdateVisualComponent(double value)
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
            if (Value is double doubleValue)
            {
                _gdValue = doubleValue;
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
                GDValue = value.AsDouble();
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error updating Value: {ex.Message}");
        }
    }

    public LRealItem()
    {
        VarType = VarType.LReal;
        DataType = DataType.Input;
        Count = 1;
        Value = 0.0d;
    }
}
