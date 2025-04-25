using Godot;
using S7.Net;
using S7.Net.Types;
using System;

[Tool]
[Icon("uid://b1rv3alewpqrr")]
[GlobalClass]
public partial class RealItem : DataItem
{
    [Signal]
    public delegate void ValueChangedEventHandler(float newValue);

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
    private float _gdValue;

    [Export]
    public float GDValue
    {
        get => _gdValue;
        set
        {
            if (Math.Abs(_gdValue - value) > 0.00001f) // Comparación de floats
            {
                _gdValue = value;
                Value = value;
                EmitSignal(SignalName.ValueChanged, value);
            }
        }
    }

    private void UpdateVisualComponent(float value)
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
            if (Value is float floatValue)
            {
                _gdValue = floatValue;
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
                GDValue = value.AsSingle();
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error updating Value: {ex.Message}");
        }
    }

    public RealItem()
    {
        VarType = VarType.Real;
        DataType = DataType.Input;
        Count = 1;
        Value = 0.0f;
    }
}
