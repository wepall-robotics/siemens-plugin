using S7.Net;
using Godot;
using System;
using System.Net.NetworkInformation;

[GlobalClass]
public partial class PlcData : Resource
{
	public enum Status
	{
		Connected,
		Disconnected,
		Unknown
	}

	[Export]
	public string IPAddress { get; set; } = String.Empty;
	[Export]
	public string Name { get; set; } = String.Empty;
	[Export]
	public CpuType Type { get; set; } = CpuType.S71500;
	[Export]
	public short Rack { get; set; } = 0;
	[Export]
	public short Slot { get; set; } = 1;
	[Export]
	public Status ConnectionStatus { get; set; } = Status.Unknown;


	public bool Connect()
	{
		// Implement PLC connection logic here
		return true;
	}

	public void Disconnect()
	{
		using (var plc = new Plc(Type, IPAddress, Rack, Slot))
		{
			try
			{
				if (plc.IsConnected)
				{
					plc.Close();
					ConnectionStatus = Status.Disconnected;
					GD.Print("PLC disconnected successfully.");
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"Error disconnecting PLC: {ex.Message}");
			}
		}
	}
	public bool Ping()
	{
        using var plc = new Plc(Type, IPAddress, Rack, Slot);
        try
        {
            plc.Open();
            return plc.IsConnected;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error pinging PLC: {ex.Message}");
            return false;
        }
    }
}

