using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using S7.Net;

[GlobalClass]
public partial class NetworkUtils : RefCounted
{
    #region Constants
    /// <summary>
    /// The maximum number of ping attempts to make before giving up.
    /// </summary>
    const int MAX_PING_ATTEMPTS = 4;
    /// <summary>
    /// The timeout in milliseconds for each ping attempt.
    /// </summary>
    const int PING_TIMEOUT = 1000;
    /// <summary>
    /// The base delay in milliseconds between retry attempts.
    /// </summary>
    const int RETRY_BASE_DELAY = 500;
    #endregion

    #region Private Variables
    private static CancellationTokenSource _currentCts;
    #endregion

    #region Exports Variables
    #endregion

    #region Public API
    /// <summary>
    /// Connects to the PLC and establishes communication.
    /// </summary>
    /// <param name="plc">The PLC object containing connection data.</param>
    /// <param name="eventBus">The event bus to emit signals to. If null, no signals will be emitted.</param>
    public static async void Connect(Plc plc, GodotObject eventBus = null)
    {
        GD.Print("Aquí el connect");
        if (plc == null)
        {
            if (eventBus != null)
            {
                eventBus.EmitSignal("plc_connection_failed", plc, "Invalid PLC data.");
            }
            return;
        }

        CancellationTokenSource cts = new CancellationTokenSource();
        var success = false;

        try
        {
            await plc.OpenAsync(cts.Token).WaitAsync(cts.Token);
            success = true;
        }
        catch (Exception e)
        {
            GD.PrintErr("Failed to connect to PLC:", e.Message);
        }
        finally
        {
            if (success)
            {
                if (eventBus != null)
                {
                    // GD.Print("jaaaaaaa ", plc.IsConnected);
                    eventBus.EmitSignal("plc_connected", plc);
                }
            }
            else
            {
                if (eventBus != null)
                {
                    eventBus.EmitSignal("plc_connection_failed", plc, "Failed to connect to PLC.");
                }
            }
        }
    }

    /// <summary>
    /// Validates an IP address. The validation is done by parsing the IP as an IPv4 address and
    /// verifying that the parsed address matches the given string exactly.
    /// </summary>
    /// <param name="ip">The IP address to validate.</param>
    /// <returns><c>true</c> if the IP is valid, <c>false</c> otherwise.</returns>
    public static bool ValidateIP(string ip)
    {
        return IPAddress.TryParse(ip, out var address)
            && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
            && address.ToString() == ip;
    }

    /// <summary>
    /// Pings the PLC using the given IP address.
    /// The function will attempt to ping the PLC up to MAX_PING_ATTEMPTS times.
    /// If the ping is successful, the function will emit the "ping_completed" signal
    /// with the given IP address and a value of true. If all attempts fail, the
    /// function will emit the "ping_completed" signal with the given IP address and
    /// a value of false.
    /// <para>
    /// The function may be cancelled using the CancelPing function.
    /// </para>
    /// </summary>
    /// <param name="ip">The IP address to ping.</param>
    /// <param name="eventBus">The event bus to emit signals to. If null, no signals will be emitted.</param>
    public static async void Ping(string ip, GodotObject eventBus = null)
    {
        var success = false;

        if (!ValidateIP(ip)) return;

        _currentCts = new CancellationTokenSource();
        using var ping = new Ping();

        try
        {
            for (int attempt = 1; attempt <= MAX_PING_ATTEMPTS; attempt++)
            {
                try
                {
                    var reply = await ping.SendPingAsync(ip, PING_TIMEOUT)
                        .WaitAsync(_currentCts.Token);

                    if (reply.Status == IPStatus.Success)
                    {
                        success = true;
                        break;
                    }

                    if (attempt < MAX_PING_ATTEMPTS)
                    {
                        eventBus?.EmitSignal("ping_attempt_failed", ip, attempt + 1, MAX_PING_ATTEMPTS);
                        await Task.Delay(RETRY_BASE_DELAY * attempt, _currentCts.Token);
                    }
                }
                catch (PingException) when (attempt == MAX_PING_ATTEMPTS)
                {
                    success = false;
                }
            }
            return;
        }
        catch (TaskCanceledException)
        {
            return;
        }
        finally
        {
            eventBus?.EmitSignal("ping_completed", ip, success);
            CancelPing();
        }
    }

    /// <summary>
    /// Cancels the ongoing ping operation if it has not been cancelled yet.
    /// 
    /// This method attempts to cancel the token source associated with the ping operation.
    /// It safely handles the case where the token source has already been disposed.
    /// /// </summary>
    public static void CancelPing()
    {
        try
        {
            if (_currentCts != null && !_currentCts.IsCancellationRequested)
                _currentCts.Cancel();
        }
        catch (ObjectDisposedException) { }
    }
    #endregion

}