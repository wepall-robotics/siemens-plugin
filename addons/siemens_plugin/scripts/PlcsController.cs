using Godot;
using System;
using S7.Net;
using System.Threading.Tasks;
using System.Linq;
using System.Net;

[Tool]
public partial class PlcsController : Node
{
    [Signal]
    public delegate void PingCompletedEventHandler(bool success);

    const string BASE_NAME = "Plc";
    const int MAX_PING_ATTEMPS = 3;
    const float PING_TIMEOUT_S = 1f;

    [Export]
    public Godot.Collections.Array Plcs { get; private set; } = new Godot.Collections.Array();
    public static PlcsController Instance { get; private set; }

    private GodotObject eventBus;

    public override void _Ready()
    {
        ClearPlcs();
        Instance = this;
        eventBus = GetNode<GodotObject>("/root/EventBus");
    }

    public PlcData CreatePlc()
    {
        var plc = new PlcData();
        plc.Name = GetUniqueName();
        Plcs.Add(plc);

        eventBus.EmitSignal("plc_added", plc);
        return plc;
    }

    public bool RemovePlc(PlcData plc)
    {
        if (Plcs.Contains(plc))
        {
            Plcs.Remove(plc);
            eventBus.EmitSignal("plc_removed", plc);
            return true;
        }

        return false;
    }

    public void ClearPlcs()
    {
        Plcs.Clear();
    }

    public string UpdatePlcName(PlcData plc, string newName)
    {
        if (Plcs.Contains(plc))
        {
            if (PlcNameExists(newName))
                plc.Name = newName;
            else
                plc.Name = GetUniqueName(newName);

            eventBus.EmitSignal("plc_updated", plc, "Name");
            return plc.Name;
        }

        return string.Empty;
    }

    public void UpdatePlcType(PlcData plc, int cpuType)
    {
        if (Plcs.Contains(plc))
        {
            plc.Type = (CpuType)cpuType;
            eventBus.EmitSignal("plc_updated", plc, "Type");
        }
    }

    public bool UpdatePlcIpAddress(PlcData plc, string ipAddress)
    {
        if (Plcs.Contains(plc))
        {
            if (!ValidateIP(ipAddress))
                return false;

            plc.IPAddress = ipAddress;
            eventBus.EmitSignal("plc_updated", plc, "IpAddress");

            return true;
        }
        return false;
    }

    public float UpdatePlcRack(PlcData plc, short rack)
    {
        if (Plcs.Contains(plc))
        {
            plc.Rack = rack;
            eventBus.EmitSignal("plc_updated", plc, "Rack");
            return plc.Rack;
        }
        return -1;
    }

    public float UpdatePlcSlot(PlcData plc, short slot)
    {
        if (Plcs.Contains(plc))
        {
            plc.Slot = slot;
            eventBus.EmitSignal("plc_updated", plc, "Slot");
            return plc.Slot;
        }
        return -1;
    }

    /// <summary>
    /// Generates a unique name based on the given base name.
    /// The function will append a counter starting from 1 if the name already exists.
    /// </summary>
    /// <param name="baseName">The base name to generate the unique name from.</param>
    /// <returns>A unique name.</returns>
    private string GetUniqueName(String baseName = BASE_NAME)
    {
        var counter = 1;
        var uniqueName = baseName;
        while (PlcNameExists(uniqueName))
        {
            uniqueName = baseName + " " + counter;
            counter++;
        }
        return uniqueName;
    }

    private bool PlcNameExists(string uniqueName)
    {
        return Plcs.Any(plc => ((PlcData)plc).Name == uniqueName);
    }

    public bool ValidateIP(string ip)
    {
        if (IPAddress.TryParse(ip, out var address))
        {
            // Verificar que sea IPv4 y que el formato coincida exactamente
            bool isValid = address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                        && address.ToString() == ip;

            GD.Print($"IP validada: {isValid} -> {address}");
            return isValid;
        }
        return false;
    }


    public bool Connect()
    {
        // Implement PLC connection logic here
        return true;
    }

    public void Disconnect(PlcData plcData)
    {
        using (var plc = new Plc(plcData.Type, plcData.IPAddress, plcData.Rack, plcData.Slot))
        {
            try
            {
                if (plc.IsConnected)
                {
                    plc.Close();
                    plcData.ConnectionStatus = PlcData.Status.Disconnected;
                    GD.Print("PLC disconnected successfully.");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error disconnecting PLC: {ex.Message}");
            }
        }
    }
    private async Task<bool> AsyncPing(PlcData plcData)
    {
        using var plc = new Plc(plcData.Type, plcData.IPAddress, plcData.Rack, plcData.Slot);
        try
        {
            await plc.OpenAsync();
            EmitSignal(nameof(PingCompleted), plc.IsConnected);
            return plc.IsConnected;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error pinging PLC: {ex.Message}");
            EmitSignal(nameof(PingCompleted), false);
            return false;
        }
    }

}
