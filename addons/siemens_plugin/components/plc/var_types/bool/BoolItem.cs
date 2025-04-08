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
}
