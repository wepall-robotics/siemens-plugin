using Godot;
using S7.Net;
using S7.Net.Types;
using System;

[Tool]
[Icon("uid://q1g1q5gfjrw6")]
[GlobalClass]
public partial class StringExItem : DataItem
{
    [Signal]
    public delegate void ValueChangedEventHandler(string newValue);

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
    private string _gdValue = string.Empty;

    [Export]
    public string GDValue
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

    // Opcional: Tamaño máximo de la cadena extendida
    [Export]
    public int MaxLength { get; set; } = 1024; // Puedes ajustar el valor por defecto según tu PLC

    private void UpdateVisualComponent(string value)
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
            if (Value is string stringValue)
            {
                _gdValue = stringValue;
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
                GDValue = value.AsString();
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error updating Value: {ex.Message}");
        }
    }

    public StringExItem()
    {
        VarType = VarType.S7WString;
        DataType = DataType.Input;
        Count = 1;
        Value = string.Empty;
    }
}
