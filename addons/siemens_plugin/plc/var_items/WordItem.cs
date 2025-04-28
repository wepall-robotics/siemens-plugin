using Godot;
using S7.Net;
using S7.Net.Types;
using System;

[Tool]
[Icon("uid://blr1imivpjslt")]
[GlobalClass]
/// <summary>
/// Represents a Word (16-bit unsigned integer) data item that interacts with a visual component in the Godot engine.
/// This class provides functionality to synchronize a Word value between the PLC and a visual property.
/// </summary>
public partial class WordItem : DataItem
{
    #region Signals

    /// <summary>
    /// Signal emitted when the Word value changes.
    /// </summary>
    [Signal]
    public delegate void ValueChangedEventHandler(ushort newValue);

    #endregion

    #region Fields

    /// <summary>
    /// Cached value of the Word data item in Godot.
    /// </summary>
    private ushort _gdValue;

    /// <summary>
    /// Reference to the visual component associated with this data item.
    /// </summary>
    private Node _visualComponent;

    /// <summary>
    /// Name of the property in the visual component to bind to.
    /// </summary>
    private string _visualProperty;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the Word value in Godot.
    /// When set, it updates the PLC value and emits a signal.
    /// </summary>
    [Export]
    public ushort GDValue
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

    /// <summary>
    /// Gets or sets the visual component associated with this data item.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the name of the visual property to bind to.
    /// </summary>
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

    #endregion

    #region Private Methods

    /// <summary>
    /// Updates the visual component's property with the given Word value.
    /// </summary>
    /// <param name="value">The Word value to set in the visual component.</param>
    private void UpdateVisualComponent(ushort value)
    {
        if (VisualComponent != null && !string.IsNullOrEmpty(VisualProperty))
            VisualComponent.Set(VisualProperty, value);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates the Godot value based on the PLC value and synchronizes it with the visual component.
    /// </summary>
    public override void UpdateGDValue()
    {
        try
        {
            if (Value is ushort ushortValue)
            {
                _gdValue = ushortValue;
                UpdateVisualComponent(_gdValue);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error updating GDValue: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the PLC value based on the visual component's property value.
    /// </summary>
    public override void UpdateValue()
    {
        try
        {
            if (VisualComponent != null && !string.IsNullOrEmpty(VisualProperty))
            {
                var value = VisualComponent.Get(VisualProperty);
                GDValue = value.AsUInt16();
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error updating Value: {ex.Message}");
        }
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="WordItem"/> class with default values.
    /// </summary>
    public WordItem()
    {
        VarType = VarType.Word;
        DataType = DataType.Input;
        Count = 1;
        Value = (ushort)0;
    }

    #endregion
}