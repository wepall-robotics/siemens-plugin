using S7.Net;
using Godot;
using System;

/// <summary>
/// Represents the data related to a PLC (Programmable Logic Controller).
/// </summary>
[GlobalClass]
public partial class PlcData : Resource
{
	/// <summary>
	/// Enum representing the connection status of the PLC.
	/// </summary>
	public enum Status
	{
		/// <summary>
		/// The PLC is connected.
		/// </summary>
		Connected,
		
		/// <summary>
		/// The PLC is disconnected.
		/// </summary>
		Disconnected,
		
		/// <summary>
		/// The connection status of the PLC is unknown.
		/// </summary>
		Unknown
	}

	/// <summary>
	/// Gets or sets the IP address of the PLC.
	/// </summary>
	[Export]
	public string IPAddress { get; set; } = String.Empty;

	/// <summary>
	/// Gets or sets the name of the PLC.
	/// </summary>
	[Export]
	public string Name { get; set; } = String.Empty;

	/// <summary>
	/// Gets or sets the type of the CPU used by the PLC.
	/// </summary>
	[Export]
	public CpuType Type { get; set; } = CpuType.S71500;

	/// <summary>
	/// Gets or sets the rack number where the PLC is located.
	/// </summary>
	[Export]
	public short Rack { get; set; } = 0;

	/// <summary>
	/// Gets or sets the slot number where the PLC is located.
	/// </summary>
	[Export]
	public short Slot { get; set; } = 1;

	/// <summary>
	/// Gets or sets the connection status of the PLC.
	/// </summary>
	[Export]
	public Status ConnectionStatus { get; set; } = Status.Unknown;
}
