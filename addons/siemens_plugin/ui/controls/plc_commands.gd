@tool
extends Control

# PlcCommands: A UI control for managing PLC-related commands and their states.

class_name PlcCommands

# Called when the node is added to the scene.
func _ready() -> void:
    # Wait for the EventBus to be ready.
    await EventBus
    
    # Connect button signals to EventBus signals.
    $HFlowContainer/Connect.pressed.connect(EventBus.plc_connect_invoked.emit)
    $HFlowContainer/Disconnect.pressed.connect(EventBus.plc_disconnect_invoked.emit)
    $HFlowContainer/Ping.pressed.connect(EventBus.ping_invoked.emit)
    $HFlowContainer/Online.pressed.connect(EventBus.online_invoked.emit)
    $HFlowContainer/Import.pressed.connect(EventBus.import_invoked.emit)
    $HFlowContainer/Export.pressed.connect(EventBus.export_invoked.emit)
    
    # Connect EventBus signals to local methods for handling PLC connection changes.
    EventBus.plc_connected.connect(func(plc: Plc): _on_plc_connection_changed(true))
    EventBus.plc_connection_lost.connect(func(plc: Plc): _on_plc_connection_changed(false))
    EventBus.plc_disconnected.connect(func(plc: Plc): _on_plc_connection_changed(false))

# Sets up the UI state based on the current PLC status.
# @param plc The PLC instance to configure the UI for.
func set_up(plc: Plc) -> void:
    # Enable or disable buttons based on the PLC's current status.
    $HFlowContainer/Connect.disabled = plc.CurrentStatus == 0
    $HFlowContainer/Ping.disabled = plc.CurrentStatus == 0
    $HFlowContainer/Disconnect.disabled = plc.CurrentStatus != 0
    $HFlowContainer/Online.disabled = plc.CurrentStatus != 0
    
    # Set the "Online" button state without emitting signals.
    $HFlowContainer/Online.set_pressed_no_signal(plc.IsOnline)
    
    # Update the status color based on the PLC's current status.
    _set_color(plc.CurrentStatus)

# Handles changes in the PLC connection state.
# @param connected Whether the PLC is connected.
func _on_plc_connection_changed(connected: bool):
    # Update button states based on the connection status.
    $HFlowContainer/Connect.disabled = connected
    $HFlowContainer/Ping.disabled = connected
    $HFlowContainer/Disconnect.disabled = not connected
    $HFlowContainer/Online.disabled = not connected
    
    # Update the status color based on the connection state.
    if connected:
        _set_color(0)  # Connected
    else:
        _set_color(1)  # Disconnected

# Updates the status color and text based on the PLC status.
# @param status The current status of the PLC (0 = Connected, 1 = Disconnected, 2 = Unknown).
func _set_color(status: int) -> void:
    match status:
        1:  # Disconnected
            $StatusContainer.color = Color("#ff786b")
            $StatusContainer/Status.text = "Disconnected"
            $StatusContainer/Status.set("theme_override_colors/font_color", Color("#95463f"))
            $HFlowContainer/Online.set_pressed_no_signal(false)
        0:  # Connected
            $StatusContainer.color = Color("#8eef97")
            $StatusContainer/Status.text = "Connected"
            $StatusContainer/Status.set("theme_override_colors/font_color", Color("#5c9a62"))
        2:  # Unknown
            $StatusContainer.color = Color("#ffde66")
            $StatusContainer/Status.text = "Unknown"
            $StatusContainer/Status.set("theme_override_colors/font_color", Color("#927f3b"))
            $HFlowContainer/Online.set_pressed_no_signal(false)
        _:  # Default (Unknown)
            $StatusContainer.color = Color("#ffde66")
            $StatusContainer/Status.text = "Unknown"
            $StatusContainer/Status.set("theme_override_colors/font_color", Color("#927f3b"))
            $HFlowContainer/Online.set_pressed_no_signal(false)