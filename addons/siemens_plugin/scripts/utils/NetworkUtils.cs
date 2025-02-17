using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Godot;

[GlobalClass]
public partial class NetworkUtils : RefCounted
{
    #region Constants
    const string BASE_NAME = "Plc";
    const int MAX_PING_ATTEMPTS = 4;
    const int PING_TIMEOUT = 1000;
    const int RETRY_BASE_DELAY = 500;
    #endregion

    #region Private Variables
    // private GodotObject _eventBus;
    // private Ping _ping = new Ping();
    // private CancellationTokenSource _cts;
    #endregion

    #region Exports Variables
    // public static NetworkUtils Instance { get; private set; }
    #endregion

#region Public API
    public static bool ValidateIP(string ip)
    {
        return IPAddress.TryParse(ip, out var address) 
            && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
            && address.ToString() == ip;
    }

    // public static async Task<bool> Ping(string ip, GodotObject eventBus = null)
    public static async void Ping(string ip, GodotObject eventBus = null)
    {
        if (!ValidateIP(ip)) return;
        
        using var cts = new CancellationTokenSource();
        using var ping = new Ping();
        
        try
        {
            for (int attempt = 1; attempt <= MAX_PING_ATTEMPTS; attempt++)
            {
                try
                {
                    var reply = await ping.SendPingAsync(ip, PING_TIMEOUT)
                        .WaitAsync(cts.Token);

                    if (reply.Status == IPStatus.Success) return;

                    if (attempt < MAX_PING_ATTEMPTS)
                    {
                        eventBus?.EmitSignal("ping_attempt_failed", attempt + 1, MAX_PING_ATTEMPTS);
                        await Task.Delay(RETRY_BASE_DELAY * attempt, cts.Token);
                    }
                }
                catch (PingException) when (attempt == MAX_PING_ATTEMPTS)
                {
                    return ;
                }
            }
            return ;
        }
        catch (TaskCanceledException)
        {
            GD.Print("Ping cancelled");
            return ;
        }
    }
    #endregion

    // #region Godot Override Methods
    // /// <summary>
    // /// Called when the node is initialized.
    // /// 
    // /// Clears the list of PLCs, sets the static instance reference, and retrieves the EventBus node.
    // /// 
    // /// <remarks>
    // /// The EventBus node is necessary for communication between the PlcsController and other scripts.
    // /// If the EventBus node cannot be found, an error message is printed to the console.
    // /// </remarks>
    // /// </summary>
    // public override void _Ready()
    // {
    //     Instance = this;
    //     _eventBus = GetNode<GodotObject>("/root/EventBus");

    //     if (_eventBus == null)
    //         GD.PrintErr("EventBus node not found!");
    // }

    // /// <summary>
    // /// Clean up resources when the node is removed from the tree.
    // /// </summary>
    // public override void _ExitTree()
    // {
    //     if (_cts != null)
    //         _cts.Dispose();
    //     _ping.Dispose();
    // }

    // #endregion

    // #region Public Methods
    // /// <summary>
    // /// Validates an IP address. The validation is done by parsing the IP as an IPv4 address and
    // /// verifying that the parsed address matches the given string exactly.
    // /// </summary>
    // /// <param name="ip">The IP address to validate.</param>
    // /// <returns><c>true</c> if the IP is valid, <c>false</c> otherwise.</returns>
    // public static bool ValidateIP(string ip)
    // {
    //     if (IPAddress.TryParse(ip, out var address))
    //     {
    //         // Verify that the IP is IPv4 and matches the given string exactly
    //         bool isValid = address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
    //                     && address.ToString() == ip;

    //         return isValid;
    //     }
    //     return false;
    // }

    // public async void Ping(string ip)
    // {
    //     if (!ValidateIP(ip))
    //     {
    //         // 
    //         GD.PrintErr("Ip is");
    //         return;
    //     }

    //     CancelPing();

    //     _cts = new CancellationTokenSource();
    //     var success = false;

    //     try
    //     {
    //         for (int attempts = 1; attempts <= MAX_PING_ATTEMPS; attempts++)
    //         {
    //             if (!IsInstanceValid(this) || _cts.IsCancellationRequested)
    //                 break;

    //             GD.Print($"Ping attempt #{attempts} for {ip}");

    //             try
    //             {
    //                 var reply = await _ping.SendPingAsync(ip, PING_TIMEOUT)
    //                                         .WaitAsync(_cts.Token);

    //                 if (reply.Status == IPStatus.Success)
    //                 {
    //                     success = true;
    //                     break;
    //                 }

    //                 if (attempts < MAX_PING_ATTEMPS)
    //                 {
    //                     await Task.Delay(RETRY_BASE_DELAY * attempts, _cts.Token);
    //                     if (IsInstanceValid(this) && _eventBus != null)
    //                     {
    //                         _eventBus.EmitSignal("ping_attempt_failed", plcData, attempts + 1, MAX_PING_ATTEMPS);
    //                     }
    //                 }
    //             }
    //             catch (PingException ex)
    //             {
    //                 GD.PrintErr($"Error in attemp {attempts + 1}: {ex.Message}");
    //                 if (attempts == MAX_PING_ATTEMPS) throw;
    //             }
    //         }
    //     }
    //     catch (TaskCanceledException)
    //     {
    //         GD.Print("Ping cancelled by user");
    //         CancelPing();
    //     }
    //     finally
    //     {
    //         _eventBus.EmitSignal("ping_completed", plcData, success);
    //         CancelPing();
    //     }
    // }

    // /// <summary>
    // /// Cancels the ongoing ping operation if it has not been cancelled yet.
    // /// </summary>
    // /// <remarks>
    // /// This method attempts to cancel the token source associated with the ping operation.
    // /// It safely handles the case where the token source has already been disposed.
    // /// /// </remarks>
    // public void CancelPing()
    // {
    //     try
    //     {
    //         if (_cts != null && !_cts.IsCancellationRequested)
    //             _cts.Cancel();
    //     }
    //     catch (ObjectDisposedException) { }
    // }
    // #endregion
}