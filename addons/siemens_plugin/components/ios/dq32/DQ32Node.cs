// ========================
// Digital Output 32-Channel
// ========================

using System;
using Godot;
using S7.Net;

/// <summary>
/// Node for writing 32 digital outputs to %Q area
/// </summary>
[Tool]
public partial class DQ32Node : Node, IWriteAction
{
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
    public Godot.Collections.Array<bool> Outputs { get; set; } = new Godot.Collections.Array<bool>(new bool[32]);
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
            GD.PrintErr("PLC instance is null. Cannot execute DQ32Node action.");
            return;
        }

        try
        {
            byte[] buffer = new byte[4];

            // Pack bits into bytes
            for (int i = 0; i < 32; i++)
            {
                int byteOffset = i / 8;
                int bitOffset = i % 8;
                if (Outputs[i])
                {
                    buffer[byteOffset] |= (byte)(1 << bitOffset);
                }
            }

            TargetPlc.WriteBytes(DataType.Output, 0, StartByte, buffer);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"DQ32 Write Error (StartByte={StartByte}): {ex.Message}");
        }
    }
}

/// <summary>
/// Data container for 32 digital outputs
/// </summary>
public class DQ32Data
{
    private readonly bool[] _values = new bool[32];

    public bool this[int index]
    {
        get => _values[index];
        set => _values[index] = value;
    }

    public void SetBit(int bytePos, int bitPos, bool value) => this[(bytePos * 8) + bitPos] = value;
}

