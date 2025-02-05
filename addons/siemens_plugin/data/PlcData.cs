using S7.Net;
using Godot;
using System;
using System.Threading.Tasks;

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

}
