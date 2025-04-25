using Godot;
using S7.Net;
using S7.Net.Types;
using System;

[Tool]
[Icon("uid://bkpi2sf11kd7y")]
[GlobalClass]
public partial class TimeItem : DataItem
{
    [Signal]
    public delegate void ValueChangedEventHandler(int newValue); // milisegundos

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
    private int _gdValue; // TIME en milisegundos (int32)

    [Export]
    public int GDValue
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

    private void UpdateVisualComponent(int value)
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
            if (Value is System.TimeSpan timeValue)
            {
                _gdValue = (int)timeValue.TotalMilliseconds;
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
                GDValue = value.AsInt32();
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error updating Value: {ex.Message}");
        }
    }

    // Métodos de utilidad para convertir entre formatos
    public System.TimeSpan AsTimeSpan()
    {
        return System.TimeSpan.FromMilliseconds(_gdValue);
    }

    public string FormatTime()
    {
        var ts = AsTimeSpan();
        return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds:D3}";
    }

    public TimeItem()
    {
        VarType = VarType.Time;
        DataType = DataType.Input;
        Count = 1;
        Value = 0; // milisegundos
    }
}
