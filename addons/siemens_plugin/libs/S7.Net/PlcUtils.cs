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
    [Icon("uid://dj2skfrj122mt")]
    public partial class Plc : Node
    {
        #region Enums
        public enum Status
        {
            Connected,
            Disconnected,
            Unknown
        }
        #endregion

        #region Constants
        const int MAX_PING_ATTEMPTS = 4;
        const int PING_TIMEOUT = 1000;
        const int RETRY_BASE_DELAY = 500;
        public const int DEFAULT_MONITOR_INTERVAL = 50;
        #endregion

        #region Private Fields
        private CancellationTokenSource _currentCts;
        private CancellationTokenSource _monitoringCts;
        private Plc _activePlc;
        private List<IPlcAction> _registeredActions = new List<IPlcAction>();
        private object _actionsLock = new object();
        private Status _currentStatus = Status.Unknown;
        private bool _validConfiguration;
        #endregion

        #region Export Properties
        /// <summary>
        /// IP address of the PLC
        /// </summary>
        [Export]
        public string IP { get => _ip; set { _ip = value; UpdateConfigurationWarnings(); } }

        /// <summary>
        /// CPU type of the PLC
        /// </summary>
        [Export]
        public CpuType CPU { get; set; } = CpuType.S71500;

        /// <summary>
        /// Rack of the PLC
        /// </summary>
        [Export]
        public Int16 Rack { get; set; } = 0;

        /// <summary>
        /// Slot of the CPU of the PLC
        /// </summary>
        [Export]
        public Int16 Slot { get; set; } = 1;

        /// <summary>
        /// Current status of the PLC
        /// </summary>
        [ExportCategory("PlcCommands")]
        [Export]
        public Status CurrentStatus
        {
            get => _currentStatus;
            set
            {
                _currentStatus = value;
                UpdateConfigurationWarnings();
            }
        }

        // Ghost property to organize the inspector
        private bool GhostProp { get; set; }

        /// <summary>
        /// Indicates if the configuration is valid
        /// </summary>
        [Export]
        public bool ValidConfiguration { get => _validConfiguration; set => _validConfiguration = value; }


        /// <summary>
        /// Online monitoring status of the PLC
        /// </summary>
        [Export]
        public bool IsOnline { get; set; } = false;
        #endregion

        #region Public API
        /// <summary>
        /// Connects to the PLC and starts monitoring its status.
        /// </summary>
        /// <param name="eventBus">The event bus used to emit connection-related signals.</param>
        public void ConnectPlc(GodotObject eventBus)
        {
            _ = ConnectAndMonitorInternal(eventBus, maxConnectRetries: 3);
        }

        /// <summary>
        /// Disconnects from the PLC and stops monitoring its status.
        /// </summary>
        /// <param name="eventBus">The event bus used to emit disconnection-related signals.</param>
        /// <remarks>
        /// If the PLC is not connected, this method will emit the "plc_already_disconnected" signal.
        /// If the disconnection is successful, this method will emit the "plc_disconnected" signal.
        /// If the disconnection fails, this method will emit the "plc_disconnection_failed" signal.
        /// </remarks>
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

        /// <summary>
        /// Validates the IP address.
        /// </summary>
        /// <remarks>
        /// Checks if the IP address is valid by trying to parse it into an <see cref="IPAddress"/>.
        /// Then checks if the parsed address is an IPv4 address and if the parsed address matches the input string.
        /// </remarks>
        /// <param name="ip">The IP address to validate.</param>
        /// <returns>True if the IP address is valid, false otherwise.</returns>
        public static bool ValidateIP(string ip)
        {
            return IPAddress.TryParse(ip, out var address)
                && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                && address.ToString() == ip;
        }

        /// <summary>
        /// Initiates a ping operation to the PLC.
        /// </summary>
        /// <param name="eventBus">The event bus used to emit signals related to the ping operation.</param>
        public void PingPlc(GodotObject eventBus)
        {
            _ = PingInternalWrapper(eventBus);
        }

        /// <summary>
        /// Cancels all ongoing operations.
        /// </summary>
        /// <remarks>
        /// This method will cancel any ongoing operations, including the ping operation
        /// and any monitoring operations. This method will not block and will return immediately.
        /// </remarks>
        public void CancelAllOperations()
        {
            _currentCts?.Cancel();
            _monitoringCts?.Cancel();
        }

        /// <summary>
        /// Registers a new action to be executed when the PLC is online.
        /// If the action is already registered, it is not added again.
        /// </summary>
        /// <param name="action">The action to register.</param>
        /// <remarks>
        /// This method is thread-safe.
        /// </remarks>
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

        /// <summary>
        /// Removes an action from the list of registered actions.
        /// If the action is not registered, this method does nothing.
        /// </summary>
        /// <param name="action">The action to remove.</param>
        /// <remarks>
        /// This method is thread-safe.
        /// </remarks>
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
        /// Processes all registered actions. If the PLC is not online, or there are no actions registered, this method does nothing.
        /// </summary>
        /// <remarks>
        /// This method is thread-safe.
        /// </remarks>
        private void ProcessRegisteredActions()
        {
            if (!IsOnline || _registeredActions.Count == 0) return;

            lock (_actionsLock)
            {
                try
                {
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
        /// <summary>
        /// Attempts to connect to the PLC and initiates a monitoring loop upon successful connection.
        /// </summary>
        /// <param name="eventBus">The event bus used to emit connection-related signals.</param>
        /// <param name="maxConnectRetries">The maximum number of connection attempts before failing.</param>
        /// <remarks>
        /// This method will attempt to connect to the PLC a specified number of times, emitting signals for each connection attempt,
        /// success, or failure. If a connection is established, it will start monitoring the PLC's status in the background.
        /// </remarks>
        /// <returns>A task representing the asynchronous connect and monitor operation.</returns>
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

        /// <summary>
        /// Checks if the PLC is available by sending a ping request.
        /// If the PLC does not respond, the method emits a "plc_connection_attempt_failed" signal
        /// and returns false.
        /// </summary>
        /// <param name="eventBus">The event bus to emit signals to.</param>
        /// <returns>True if the PLC is available, false otherwise.</returns>
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

        /// <summary>
        /// Internal wrapper for <see cref="PingAsync"/>.
        /// Handles task cancellation and emits the "ping_attempt", "ping_attempt_failed", "ping_cancelled" and "ping_completed" signals.
        /// </summary>
        /// <param name="eventBus">The event bus to emit signals to.</param>
        /// <returns>A task representing the asynchronous ping operation.</returns>
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

        /// <summary>
        /// Attempts to ping the PLC at the given IP address.
        /// </summary>
        /// <param name="attempt">The current attempt number.</param>
        /// <param name="eventBus">The event bus to emit signals to.</param>
        /// <returns>True if the PLC is pingable, false otherwise.</returns>
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


        /// <summary>
        /// Starts a monitoring loop in the background.
        /// </summary>
        /// <remarks>
        /// The loop will check every <see cref="DEFAULT_MONITOR_INTERVAL"/> ms if the PLC is online.
        /// If the PLC is online, registered actions will be processed and the <see cref="Plc.DataUpdated"/> signal will be emitted.
        /// If the PLC is not online, the loop will attempt to reconnect the PLC once and then emit the <see cref="Plc.ConnectionLost"/> signal.
        /// </remarks>
        private void StartMonitoring(GodotObject eventBus)
        {
            _monitoringCts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (!_monitoringCts.IsCancellationRequested)
                {
                    await Task.Delay(DEFAULT_MONITOR_INTERVAL);

                    if (IsOnline && this.IsConnected)
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

        #region Inspector Integration
        /// <summary>
        /// Returns warnings about invalid configurations.
        /// </summary>
        /// <remarks>
        /// Checks the IP address and whether there are any other PLCs with the same IP.
        /// </remarks>
        /// <returns>An array of strings containing the warnings.</returns>
        public override string[] _GetConfigurationWarnings()
        {
            List<string> warnings = [];
            _validConfiguration = false;

            if (string.IsNullOrEmpty(IP) || !Plc.ValidateIP(IP))
            {
                warnings.Add("Invalid IP address:\n1. Select this node in the Scene tree.\n" +
                            "2. In the Inspector, see Plc group.\n" +
                            "3. Enter a valid IP address in the field.");
            }
            else
            {
                var plcs = GetTree().GetNodesInGroup("Plcs");
                int counter = 0;

                foreach (var node in plcs)
                {
                    if (node is Plc plc && plc.IP == IP)
                    {
                        counter++;
                    }
                }

                if (counter > 1)
                {
                    warnings.Add("There is another PLC with this IP.");
                }
            }

            _validConfiguration = warnings.Count == 0;

            return warnings.ToArray();
        }

        /// <summary>
        /// Called when the Inspector needs to validate a property.
        /// This implementation makes the <see cref="CurrentStatus"/> property read-only and hides the ghost property.
        /// </summary>
        /// <param name="property">Dictionary containing the property name and value.</param>
        /// 
        public override void _ValidateProperty(Godot.Collections.Dictionary property)
        {
            // Acceder al nombre de la propiedad usando la key del diccionario
            StringName propertyName = property["name"].AsStringName();

            if (propertyName == nameof(CurrentStatus))
            {
                // Modificar el flag de uso (casting necesario)
                PropertyUsageFlags usage = property["usage"].As<PropertyUsageFlags>();
                usage = PropertyUsageFlags.NoEditor;
                property["usage"] = (int)usage;
            }
            else if (propertyName == nameof(GhostProp))
            {
                PropertyUsageFlags usage = property["usage"].As<PropertyUsageFlags>();
                usage = PropertyUsageFlags.NoEditor;
                property["usage"] = (int)usage;
            }
        }
        #endregion
    }
}