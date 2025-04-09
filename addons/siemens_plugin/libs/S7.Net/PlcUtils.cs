using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using S7.Net;

namespace S7.Net
{
    public partial class Plc : Resource
    {
        #region Constants
        const int MAX_PING_ATTEMPTS = 4;
        const int PING_TIMEOUT = 1000;
        const int RETRY_BASE_DELAY = 500;
        const int DEFAULT_MONITOR_INTERVAL = 50;
        #endregion

        #region Private Fields
        private CancellationTokenSource _currentCts;
        private CancellationTokenSource _monitoringCts;
        private Plc _activePlc;
        private List<IPlcAction> _registeredActions = new List<IPlcAction>();
        private object _actionsLock = new object();
        #endregion

        /// <summary>
        /// Online monitoring status of the PLC
        /// </summary>
        [Export]
        public bool IsOnline { get; set; } = false;


        #region Public API
        public void ConnectPlc(GodotObject eventBus)
        {
            _ = ConnectAndMonitorInternal(eventBus, maxConnectRetries: 3);
        }

        public void Disconnect(GodotObject eventBus = null)
        {
            GD.Print("Disconnecting PLC...");
            if (this == null)
            {
                eventBus?.EmitSignal("plc_disconnection_failed", "PLC not valid.");
                return;
            }

            try
            {
                // Cancel any ongoing operation
                CancelAllOperations();

                // Check if the PLC is connected before attempting to disconnect
                if (this.IsConnected)
                {
                    this.Close(); // Closes the connection with the PLC
                    eventBus?.EmitSignal("plc_disconnected", this);
                }
                else
                {
                    eventBus?.EmitSignal("plc_already_disconnected", this);
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

        public void PingPlc(GodotObject eventBus)
        {
            _ = PingInternalWrapper(eventBus);
        }

        public void CancelAllOperations()
        {
            _currentCts?.Cancel();
            _monitoringCts?.Cancel();
        }

        /// <summary>
        /// Registra una acción para ejecución automática
        /// </summary>
        public void RegisterAction(IPlcAction action)
        {
            lock (_actionsLock)
            {
                if (!_registeredActions.Contains(action))
                {
                    _registeredActions.Add(action);
                    GD.Print($"Action registered: {action.GetType().Name}");
                }
            }
        }

        public void RemoveAction(IPlcAction action)
        {
            lock (_actionsLock)
            {
                if (_registeredActions.Contains(action))
                {
                    _registeredActions.Remove(action);
                    GD.Print($"Action removed: {action.GetType().Name}");
                }
            }
        }

        /// <summary>
        /// Ejecuta todas las acciones registradas en orden seguro
        /// </summary>
        private void ProcessRegisteredActions()
        {
            if (!IsOnline || _registeredActions.Count == 0) return;

            lock (_actionsLock)
            {
                try
                {
                    // Ejecutar lecturas primero
                    foreach (var action in _registeredActions.OfType<IPlcAction>())
                    {
                        action.Execute();
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Action processing failed: {ex.Message}");
                }
            }
        }
        #endregion

        #region Core Implementation
        private async Task ConnectAndMonitorInternal(GodotObject eventBus, int maxConnectRetries)
        {
            CancelAllOperations();
            _currentCts = new CancellationTokenSource();
            _activePlc = this;

            try
            {
                bool success = false;
                int attempts = 0;

                while (!success && attempts < maxConnectRetries)
                {
                    attempts++;
                    eventBus.CallDeferred("emit_signal", "plc_connection_attempt", attempts, maxConnectRetries);

                    // Enhanced status verification
                    bool plcAvailable = await CheckPlcAvailability(eventBus);

                    if (plcAvailable)
                    {
                        try
                        {
                            await this.OpenAsync(_currentCts.Token);
                            if (this.IsConnected)
                            {
                                success = true;
                                eventBus.EmitSignal("plc_connected", this);
                                StartMonitoring(eventBus);
                            }
                        }
                        catch (Exception e)
                        {
                            GD.PrintErr($"Connection error: {e.Message}");
                            eventBus.EmitSignal("plc_connection_attempt_failed",
                                this, $"Attempt {attempts}/{maxConnectRetries}: {e.Message}");
                        }
                    }

                    await Task.Delay(RETRY_BASE_DELAY);
                }

                if (!success)
                {
                    eventBus.CallDeferred("emit_signal", "plc_connection_failed", this, $"Failed after {attempts} attempts");
                }
            }
            catch (TaskCanceledException)
            {
                eventBus.EmitSignal("plc_connection_cancelled", this);
            }
        }

        private async Task<bool> CheckPlcAvailability(GodotObject eventBus)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(this.IP, PING_TIMEOUT);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                eventBus.EmitSignal("plc_connection_attempt_failed",
                    this, "PLC not responding to ping");
                return false;
            }
        }

        private async Task PingInternalWrapper(GodotObject eventBus)
        {
            CancelAllOperations();
            _currentCts = new CancellationTokenSource();

            try
            {
                bool success = false;
                for (int attempt = 1; attempt <= MAX_PING_ATTEMPTS; attempt++)
                {
                    eventBus.EmitSignal("ping_attempt", this.IP, attempt, MAX_PING_ATTEMPTS);

                    if (await AttemptPing(attempt, eventBus))
                    {
                        success = true;
                        break;
                    }
                    else
                    {
                        eventBus.EmitSignal("ping_attempt_failed", this.IP, attempt, MAX_PING_ATTEMPTS);
                    }
                }
                eventBus.EmitSignal("ping_completed", this.IP, success);
            }
            catch (TaskCanceledException)
            {
                eventBus.EmitSignal("ping_cancelled", this.IP);
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Checks whether the PLC is pingable.
        /// </summary>
        /// <param name="eventBus">The event bus to emit signals to.</param>
        /// <returns>True if the PLC is pingable, false otherwise.</returns>
        private async Task<bool> IsPingable(GodotObject eventBus)
        {
            using var ping = new Ping();
            try
            {
                var reply = await ping.SendPingAsync(this.IP, PING_TIMEOUT);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                eventBus.EmitSignal("ping_error", this.IP);
                return false;
            }
        }

        private async Task<bool> AttemptPing(int attempt, GodotObject eventBus)
        {
            using var ping = new Ping();
            try
            {
                var reply = await ping.SendPingAsync(this.IP, PING_TIMEOUT);
                if (reply.Status == IPStatus.Success) return true;

                await Task.Delay(RETRY_BASE_DELAY * attempt);
                return false;
            }
            catch
            {
                eventBus.EmitSignal("ping_error", this.IP);
                return false;
            }
        }

        private void StartMonitoring(GodotObject eventBus)
        {
            _monitoringCts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (!_monitoringCts.IsCancellationRequested)
                {
                    await Task.Delay(DEFAULT_MONITOR_INTERVAL);

                    if (IsOnline)
                    {
                        ProcessRegisteredActions(); // Process registered actions
                        eventBus.CallDeferred("emit_signal", "plc_data_updated", this);
                    }

                    else if (!_monitoringCts.IsCancellationRequested && (!this.IsConnected || !await IsPingable(eventBus)))
                    {
                        eventBus.CallDeferred("emit_signal", "plc_connection_lost", this);
                        await ConnectAndMonitorInternal(eventBus, maxConnectRetries: 1);
                    }
                }
            });
        }
        #endregion
    }
}