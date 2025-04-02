// ========================
// Digital Output 32-Channel
// ========================

using System;
using Godot;
using S7.Net;

/// <summary>
/// Node for writing 32 digital outputs to %Q area
/// </summary>
public partial class DQ32Node : Node, IWriteAction
{
    [Export] public int StartByte { get; set; }
    [Export] public Plc Plc { get; set; } // Reference to the PLC instance
    public DQ32Data Data { get; } = new DQ32Data();

    public void Execute()
    {
        if (Plc == null)
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
                if (Data[i]) 
                {
                    buffer[byteOffset] |= (byte)(1 << bitOffset);
                }
            }
            
            Plc.WriteBytes(DataType.Output, 0, StartByte, buffer);
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

