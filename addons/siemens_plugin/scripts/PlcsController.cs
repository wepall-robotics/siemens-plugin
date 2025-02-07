using Godot;
using System;
using S7.Net;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Threading;
using System.Net.NetworkInformation;

[Tool]

/// <summary>
/// The PlcsController class manages a collection of PLCs (Programmable Logic Controllers).
/// It provides methods to create, update, remove, and manage PLCs, as well as to handle
/// communication and connection operations.
/// </summary>
public partial class PlcsController : Node
{
    #region Constants
    const string BASE_NAME = "Plc";
    const int MAX_PING_ATTEMPS = 4;
    const int PING_TIMEOUT = 1000;
    const int RETRY_BASE_DELAY = 500;
    #endregion

    #region Exports Variables
    [Export]
    public Godot.Collections.Array Plcs { get; private set; } = new Godot.Collections.Array();
    public static PlcsController Instance { get; private set; }
    #endregion

    #region Private Variables
    private GodotObject _eventBus;
    private Ping _ping = new Ping();
    private CancellationTokenSource _cts;
    #endregion

    #region Godot Override Methods
    /// <summary>
    /// Called when the node is initialized.
    /// 
    /// Clears the list of PLCs, sets the static instance reference, and retrieves the EventBus node.
    /// 
    /// <remarks>
    /// The EventBus node is necessary for communication between the PlcsController and other scripts.
    /// If the EventBus node cannot be found, an error message is printed to the console.
    /// </remarks>
    /// </summary>
    public override void _Ready()
    {
        ClearPlcs();
        Instance = this;
        _eventBus = GetNode<GodotObject>("/root/EventBus");

        if (_eventBus == null)
            GD.PrintErr("EventBus node not found!");
    }

    /// <summary>
    /// Clean up resources when the node is removed from the tree.
    /// </summary>
    public override void _ExitTree()
    {
        if (_cts != null)
            _cts.Dispose();
        _ping.Dispose();
    }

    #endregion

    #region Public Methods
    /// <summary>
    /// Cancels the ongoing ping operation if it has not been cancelled yet.
    /// </summary>
    /// <remarks>
    /// This method attempts to cancel the token source associated with the ping operation.
    /// It safely handles the case where the token source has already been disposed.
    /// /// </remarks>
    public void CancelPing()
    {
        try
        {
            if (_cts != null && !_cts.IsCancellationRequested)
                _cts.Cancel();
        }
        catch (ObjectDisposedException) { }
    }


    /// <summary>
    /// Clears the collection of PLCs.
    /// /// </summary>
    public void ClearPlcs()
    {
        Plcs.Clear();
    }

    /// <summary>
    /// Attempts to establish a connection to the PLC.
    /// <para>
    /// The function will implement the necessary logic to connect to the PLC
    /// using the available connection parameters. If the connection is successful,
    /// the function returns true; otherwise, it returns false.
    /// </para>
    /// </summary>
    /// /// <returns>True if the connection is successfully established; otherwise, false.</returns>
    public bool Connect()
    {
        // Implement PLC connection logic here
        return true;
    }

    /// <summary>
    /// Creates a new PLC instance, assigns it a unique name, and adds it to the collection.
    /// </summary>
    /// /// <returns>The newly created PlcData object.</returns>
    public PlcData CreatePlc()
    {
        var plc = new PlcData();
        plc.Name = GetUniqueName();
        Plcs.Add(plc);

        _eventBus.EmitSignal("plc_added", plc);
        return plc;
    }

    /// <summary>
    /// Disconnects the PLC with the given data.
    /// <para>
    /// The function will attempt to disconnect the PLC and update the connection status
    /// of the given PLC data accordingly. If the disconnection is successful,
    /// the "plc_updated" signal will be emitted with the given PLC data and
    /// a value of "ConnectionStatus".
    /// </para>
    /// <para>
    /// If an error occurs while disconnecting the PLC, an error message will
    /// be printed to the console.
    /// </para>
    /// /// </summary>
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

    /// <summary>
    /// Removes a PLC from the collection.
    /// 
    /// <param name="plc">The PLC to remove.</param>
    /// 
    /// <returns>True if the PLC was removed, false otherwise.</returns>
    /// /// </summary>
    public bool RemovePlc(PlcData plc)
    {
        if (Plcs.Contains(plc))
        {
            Plcs.Remove(plc);
            _eventBus.EmitSignal("plc_removed", plc);
            return true;
        }

        return false;
    }


    /// <summary>
    /// Updates the name of the specified PLC.
    /// 
    /// <param name="plc">The PLC to update.</param>
    /// <param name="newName">The new name.</param>
    /// 
    /// <returns>The updated name.</returns>
    /// </summary>
    public string UpdatePlcName(PlcData plc, string newName)
    {
        if (Plcs.Contains(plc))
        {
            if (PlcNameExists(newName))
                plc.Name = newName;
            else
                plc.Name = GetUniqueName(newName);

            _eventBus.EmitSignal("plc_updated", plc, "Name");
            return plc.Name;
        }

        return string.Empty;
    }

    /// <summary>
    /// Updates the type of the specified PLC.
    /// 
    /// <param name="plc">The PLC to update.</param>
    /// <param name="cpuType">The ID of the type to set.</param>
    /// 
    /// <remarks>
    /// The type is specified as an ID of the <see cref="CpuType"/> enum.
    /// /// </remarks>
    /// </summary>
    public void UpdatePlcType(PlcData plc, int cpuType)
    {
        if (Plcs.Contains(plc))
        {
            plc.Type = (CpuType)cpuType;
            _eventBus.EmitSignal("plc_updated", plc, "Type");
        }
    }

    /// <summary>
    /// Updates the IP address of the specified PLC.
    /// 
    /// <param name="plc">The PLC to update.</param>
    /// <param name="ipAddress">The new IP address.</param>
    /// 
    /// <returns>True if the IP address was updated successfully, false otherwise.</returns>
    /// /// </summary>
    public bool UpdatePlcIpAddress(PlcData plc, string ipAddress)
    {
        if (Plcs.Contains(plc))
        {
            if (!ValidateIP(ipAddress))
                return false;

            plc.IPAddress = ipAddress;
            _eventBus.EmitSignal("plc_updated", plc, "IpAddress");

            return true;
        }
        return false;
    }

    /// <summary>
    /// Updates the rack of the specified PLC.
    /// 
    /// <param name="plc">The PLC to update.</param>
    /// <param name="rack">The new rack value.</param>
    /// 
    /// <returns>The new rack value if the update was successful, -1 otherwise.</returns>
    /// /// </summary>
    public float UpdatePlcRack(PlcData plc, short rack)
    {
        if (Plcs.Contains(plc))
        {
            plc.Rack = rack;
            _eventBus.EmitSignal("plc_updated", plc, "Rack");
            return plc.Rack;
        }
        return -1;
    }

    /// <summary>
    /// Updates the slot of the given PLC.
    /// 
    /// <param name="plc">The PLC to update.</param>
    /// <param name="slot">The new slot value.</param>
    /// 
    /// <returns>The new slot value if the update was successful, -1 otherwise.</returns>
    /// </summary>
    public float UpdatePlcSlot(PlcData plc, short slot)
    {
        if (Plcs.Contains(plc))
        {
            plc.Slot = slot;
            _eventBus.EmitSignal("plc_updated", plc, "Slot");
            return plc.Slot;
        }
        return -1;
    }

    /// <summary>
    /// Validates an IP address. The validation is done by parsing the IP as an IPv4 address and
    /// verifying that the parsed address matches the given string exactly.
    /// </summary>
    /// <param name="ip">The IP address to validate.</param>
    /// <returns><c>true</c> if the IP is valid, <c>false</c> otherwise.</returns>
    public bool ValidateIP(string ip)
    {
        if (IPAddress.TryParse(ip, out var address))
        {
            // Verify that the IP is IPv4 and matches the given string exactly
            bool isValid = address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                        && address.ToString() == ip;

            return isValid;
        }
        return false;
    }

    #endregion

    #region Private Methods
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

    /// <summary>
    /// Pings the PLC using the given data.
    /// <para>
    /// The function will attempt to ping the PLC up to MAX_PING_ATTEMPTS times.
    /// If the ping is successful, the function will emit the "ping_completed" signal
    /// with the given PLC data and a value of true. If all attempts fail, the
    /// function will emit the "ping_completed" signal with the given PLC data and
    /// a value of false.
    /// </para>
    /// <para>
    /// The function may be cancelled using the CancelPing function.
    /// </para>
    /// </summary>
    /// <param name="plcData">The PLC data to use for the ping.</param>
    private async void Ping(PlcData plcData)
    {
        if (plcData == null || string.IsNullOrEmpty(plcData.IPAddress))
        {
            GD.PrintErr("PLC data or IP address is null");
            return;
        }

        CancelPing();

        _cts = new CancellationTokenSource();
        var success = false;

        try
        {
            for (int attempts = 1; attempts <= MAX_PING_ATTEMPS; attempts++)
            {
                if (!IsInstanceValid(this) || _cts.IsCancellationRequested)
                    break;

                GD.Print($"Ping attempt #{attempts} for {plcData.IPAddress}");

                try
                {
                    var reply = await _ping.SendPingAsync(plcData.IPAddress, PING_TIMEOUT)
                                            .WaitAsync(_cts.Token);

                    if (reply.Status == IPStatus.Success)
                    {
                        success = true;
                        break;
                    }

                    if (attempts < MAX_PING_ATTEMPS)
                    {
                        await Task.Delay(RETRY_BASE_DELAY * attempts, _cts.Token);
                        if (IsInstanceValid(this) && _eventBus != null)
                        {
                            _eventBus.EmitSignal("ping_attempt_failed", plcData, attempts + 1, MAX_PING_ATTEMPS);
                        }
                    }
                }
                catch (PingException ex)
                {
                    GD.PrintErr($"Error in attemp {attempts + 1}: {ex.Message}");
                    if (attempts == MAX_PING_ATTEMPS) throw;
                }
            }
        }
        catch (TaskCanceledException)
        {
            GD.Print("Ping cancelled by user");
            CancelPing();
        }
        finally
        {
            _eventBus.EmitSignal("ping_completed", plcData, success);
            CancelPing();
        }
    }

    /// <summary>
    /// Checks if a PLC with the given name already exists.
    /// </summary>
    /// <param name="uniqueName">The name to check for.</param>
    /// /// <returns><c>true</c> if a PLC with the given name exists, <c>false</c> otherwise.</returns>
    private bool PlcNameExists(string uniqueName)
    {
        return Plcs.Any(plc => ((PlcData)plc).Name == uniqueName);
    }
    #endregion
}
