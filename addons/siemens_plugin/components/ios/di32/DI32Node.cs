// ========================
// Digital Input 32-Channel
// ========================

using Godot;
using S7.Net;
using System;

/// <summary>
/// Node for reading 32 digital inputs from %I area
/// </summary>
[Tool]
public partial class DI32Node : Node, IReadAction
{
    /// <summary>
    /// Starting byte address in the PLC (e.g., 0 = I0.0, 2 = I2.0)
    /// </summary>
    [Export] public int StartByte { get; set; }
    [Export]
    public PlcNode PlcNode
    {
        get => _plc;
        set
        {
            // Solo actualizar si cambia la referencia
            if (_plc == value) return;

            // Desregistrar del PLC anterior
            if (_plc != null && _isRegistered)
            {
                _plc.Data.RemoveAction(this);
                _isRegistered = false;
            }

            _plc = value;

            // Registrar en el nuevo PLC si está en el árbol
            if (_plc != null && IsInsideTree())
            {
                _plc.Data.RegisterAction(this);
                _isRegistered = true;
            }
        }
    }
    public Plc TargetPlc { get => _plc?.Data; }

    [Export] 
    public Godot.Collections.Array<bool> Inputs { get; set; } = new Godot.Collections.Array<bool>(new bool[32]);
    private const int BLOCK_SIZE = 4; // 32 bits = 4 bytes
    private PlcNode _plc;
    private bool _isRegistered;

    public override void _Ready()
    {
        if (TargetPlc != null && !_isRegistered)
        {
            TargetPlc.RegisterAction(this);
            _isRegistered = true;
        }
    }

    public override void _ExitTree()
    {
        if (TargetPlc != null && _isRegistered)
        {
            TargetPlc.RemoveAction(this);
            _isRegistered = false;
        }
    }

    public void Execute()
    {
        if (TargetPlc == null)
        {
            GD.PrintErr("PLC instance is null. Cannot execute DI32Node action.");
            return;
        }

        try
        {
            // Read 4 consecutive bytes in single operation
            byte[] buffer = TargetPlc.ReadBytes(DataType.Input, 0, StartByte, BLOCK_SIZE);

            // Map bytes to individual bits
            for (int i = 0; i < 32; i++)
            {
                int byteOffset = i / 8;
                int bitOffset = i % 8;
                Inputs[i] = (buffer[byteOffset] & (1 << bitOffset)) != 0;
            }

        }
        catch (Exception ex)
        {
            GD.PrintErr($"DI32 Read Error (StartByte={StartByte}): {ex.Message}");
        }
    }
}
