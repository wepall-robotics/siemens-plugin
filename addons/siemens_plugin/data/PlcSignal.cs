using Godot;
using S7.Net;
using System;

/// <summary>
/// Represents a PLC signal resource with various properties and communication directions.
/// </summary>
[GlobalClass]
public partial class PlcSignal : Resource
{
    /// <summary>
    /// Specifies the communication direction of the PLC signal.
    /// </summary>
    public enum CommsDirection
    {
        /// <summary>
        /// The communication direction is unknown.
        /// </summary>
        Unknown,
        /// <summary>
        /// Communication from PLC to Godot.
        /// </summary>
        PlcToGodot,
        /// <summary>
        /// Communication from Godot to PLC.
        /// </summary>
        GodotToPlc,
        /// <summary>
        /// Bidirectional communication.
        /// </summary>
        Bidirectional
    }

    // /// <summary>
    // /// Gets or sets the name of the PLC signal.
    // /// </summary>
    // [Export]
    // public string Name { get; set; } = String.Empty;

    /// <summary>
    /// Gets or sets the data type of the PLC signal.
    /// </summary>
    [Export]
    public DataType DataType { get; set; } = DataType.Input;

    /// <summary>
    /// Gets or sets the variable type of the PLC signal.
    /// </summary>
    [Export]
    public VarType VarType { get; set; } = VarType.Bit;

    /// <summary>
    /// Gets or sets the start address of the PLC signal.
    /// </summary>
    [Export]
    public int StartAdr { get; set; } = 0;

    /// <summary>
    /// Gets or sets the length of the PLC signal.
    /// </summary>
    [Export]
    public int Length { get; set; } = 1;

    /// <summary>
    /// Gets or sets the communication direction of the PLC signal.
    /// </summary>
    [Export]
    public CommsDirection Direction { get; set; } = CommsDirection.Unknown;

    /// <summary>
    /// Gets or sets the component node associated with the PLC signal.
    /// </summary>
    // [Export]
    // public Node Component { get; set; } = null;

    /// <summary>
    /// Gets or sets the property name of the component associated with the PLC signal.
    /// </summary>
    [Export]
    public string Property { get; set; } = String.Empty;

    // /// <summary>
    // /// Gets or sets the forced value of the PLC signal.
    // /// </summary>
    // [Export]
    // public Variant ForceValue { get; set; }

    // /// <summary>
    // /// Gets or sets the current value of the PLC signal.
    // /// </summary>
    // [Export]
    // public Variant Value { get; set; }
}
