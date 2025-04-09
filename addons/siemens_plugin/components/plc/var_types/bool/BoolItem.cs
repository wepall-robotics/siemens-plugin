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
    public Node VisualComponent
    {
        get => _visualComponent;
        set
        {
            _visualComponent = value;
            NotifyPropertyListChanged();
        }
    }

    [Export]
    public string VisualProperty { get; set; }

    private Node _visualComponent;
    private bool _gdValue;

    [Export]
    public bool GDValue
    {
        get => _gdValue;
        set
        {
            if (_gdValue != value)
            {
                _gdValue = value;
                // Value = value; // Sincronizar con el valor base
                EmitSignal(SignalName.ValueChanged, value);
                UpdateVisualComponent(value);
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
            if (Value is byte byteValue)
            {
                int bitPosition = BitAdr % 8;
                bool bitValue = (byteValue & (1 << bitPosition)) != 0;
                _gdValue = bitValue;
                UpdateVisualComponent(_gdValue);
                return;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error updating GDValue: {ex.Message}");
        }
    }
}
