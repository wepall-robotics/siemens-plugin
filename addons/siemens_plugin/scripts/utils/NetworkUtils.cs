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
    const int MAX_PING_ATTEMPTS = 4;
    const int PING_TIMEOUT = 1000;
    const int RETRY_BASE_DELAY = 500;
    const int DEFAULT_MONITOR_INTERVAL = 50;
    #endregion

    #region Internal State
    private static CancellationTokenSource _currentCts;
    private static CancellationTokenSource _monitoringCts;
    private static Plc _activePlc;
    #endregion

    #region Public API
    public static void ConnectPlc(Plc plc, GodotObject eventBus)
    {
        _ = ConnectAndMonitorInternal(plc, eventBus, maxConnectRetries: 3);
    }

    public static void Disconnect(Plc plc, GodotObject eventBus = null)
    {
        GD.Print("Disconnecting PLC...");
        if (plc == null)
        {
            eventBus?.EmitSignal("plc_disconnection_failed", "PLC not valid.");
            return;
        }

        try
        {
            // Cancel any ongoing operation
            CancelAllOperations();

            // Check if the PLC is connected before attempting to disconnect
            if (plc.IsConnected)
            {
                plc.Close(); // Closes the connection with the PLC
                eventBus?.EmitSignal("plc_disconnected", plc);
                GD.Print("PLC disconnected.");
            }
            else
            {
                GD.Print("El PLC already disconnected.");
                eventBus?.EmitSignal("plc_already_disconnected", plc);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Plc disconnection failed: {e.Message}");
            eventBus?.EmitSignal("plc_disconnection_failed", $"Error: {e.Message}");
        }
    }

    public static bool ValidateIP(string ip)
    {
        return IPAddress.TryParse(ip, out var address)
            && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
            && address.ToString() == ip;
    }

    public static void PingPlc(string ip, GodotObject eventBus)
    {
        _ = PingInternalWrapper(ip, eventBus);
    }

    public static void CancelAllOperations()
    {
        _currentCts?.Cancel();
        _monitoringCts?.Cancel();
    }
    #endregion

    #region Core Implementation
    private static async Task ConnectAndMonitorInternal(Plc plc, GodotObject eventBus, int maxConnectRetries)
    {
        CancelAllOperations();
        _currentCts = new CancellationTokenSource();
        _activePlc = plc;

        try
        {
            bool success = false;
            int attempts = 0;

            while (!success && attempts < maxConnectRetries)
            {
                attempts++;
                eventBus.CallDeferred("emit_signal", "plc_connection_attempt", attempts, maxConnectRetries);

                // Enhanced status verification
                bool plcAvailable = await CheckPlcAvailability(plc.IP, eventBus);

                if (plcAvailable)
                {
                    try
                    {
                        await plc.OpenAsync(_currentCts.Token);
                        if (plc.IsConnected)
                        {
                            success = true;
                            eventBus.EmitSignal("plc_connected", plc);
                            StartMonitoring(plc, eventBus);
                        }
                    }
                    catch (Exception e)
                    {
                        GD.PrintErr($"Connection error: {e.Message}");
                        eventBus.EmitSignal("plc_connection_attempt_failed",
                            plc, $"Attempt {attempts}/{maxConnectRetries}: {e.Message}");
                    }
                }

                await Task.Delay(RETRY_BASE_DELAY);
            }

            if (!success)
            {
                eventBus.CallDeferred("emit_signal", "plc_connection_failed", plc, $"Failed after {attempts} attempts");
            }
        }
        catch (TaskCanceledException)
        {
            eventBus.EmitSignal("plc_connection_cancelled", plc);
        }
    }

    private static async Task<bool> CheckPlcAvailability(string ip, GodotObject eventBus)
    {
        try
        {
            using (var ping = new Ping())
            {
                var reply = await ping.SendPingAsync(ip, PING_TIMEOUT);
                return reply.Status == IPStatus.Success;
            }
        }
        catch
        {
            eventBus.EmitSignal("plc_connection_attempt_failed",
                _activePlc, "PLC not responding to ping");
            return false;
        }
    }

    private static async Task PingInternalWrapper(string ip, GodotObject eventBus)
    {
        CancelAllOperations();
        _currentCts = new CancellationTokenSource();

        try
        {
            bool success = false;
            for (int attempt = 1; attempt <= MAX_PING_ATTEMPTS; attempt++)
            {
                eventBus.EmitSignal("ping_attempt", ip, attempt, MAX_PING_ATTEMPTS);

                if (await AttemptPing(ip, attempt, eventBus))
                {
                    success = true;
                    break;
                }
                else
                {
                    eventBus.EmitSignal("ping_attempt_failed", ip, attempt, MAX_PING_ATTEMPTS);
                }
            }
            eventBus.EmitSignal("ping_completed", ip, success);
        }
        catch (TaskCanceledException)
        {
            eventBus.EmitSignal("ping_cancelled", ip);
        }
    }
    #endregion

    #region Helper Methods
    private static async Task<bool> IsPingable(string ip, GodotObject eventBus)
    {
        using var ping = new Ping();
        try
        {
            var reply = await ping.SendPingAsync(ip, PING_TIMEOUT);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            eventBus.EmitSignal("ping_error", ip);
            return false;
        }
    }

    private static async Task<bool> AttemptPing(string ip, int attempt, GodotObject eventBus)
    {
        using var ping = new Ping();
        try
        {
            var reply = await ping.SendPingAsync(ip, PING_TIMEOUT);
            if (reply.Status == IPStatus.Success) return true;

            await Task.Delay(RETRY_BASE_DELAY * attempt);
            return false;
        }
        catch
        {
            eventBus.EmitSignal("ping_error", ip);
            return false;
        }
    }

    private static void StartMonitoring(Plc plc, GodotObject eventBus)
    {
        _monitoringCts = new CancellationTokenSource();
        Task.Run(async () =>
        {
            while (!_monitoringCts.IsCancellationRequested)
            {
                await Task.Delay(DEFAULT_MONITOR_INTERVAL);

                if (!_monitoringCts.IsCancellationRequested && (!plc.IsConnected || !await IsPingable(plc.IP, eventBus)))
                {
                    eventBus.CallDeferred("emit_signal", "plc_connection_lost", plc);
                    await ConnectAndMonitorInternal(plc, eventBus, maxConnectRetries: 1);
                }
            }
        });
    }
    #endregion
}
