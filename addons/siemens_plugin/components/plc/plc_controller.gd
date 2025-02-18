@tool
extends EditorInspectorPlugin

const PLC_COMMANDS : PackedScene = preload("res://addons/siemens_plugin/components/controls/plc_commands.tscn")
var _plc: Plc

# Determines if the given object can be handled by this controller.
func _can_handle(object) -> bool:
	if object is Plc:
		_plc = object
		_connect_event_bus_signals()

	return object is Plc

## Method to connect [b]EventBus signals[/b] to their respective handlers.
func _connect_event_bus_signals() -> void:
	if not EventBus.ping_invoked.is_connected(_ping):
		EventBus.ping_invoked.connect(_ping, CONNECT_PERSIST)
		EventBus.ping_invoked.connect(_ping)
	if not EventBus.connect_invoked.is_connected(_connect):
		EventBus.connect_invoked.connect(_connect)
	if not EventBus.disconnect_invoked.is_connected(_disconnect):
		EventBus.disconnect_invoked.connect(_disconnect)
	if not EventBus.online_invoked.is_connected(_online):
		EventBus.online_invoked.connect(_online)
	if not EventBus.import_invoked.is_connected(_import):
		EventBus.import_invoked.connect(_import)
	if not EventBus.export_invoked.is_connected(_export):
		EventBus.export_invoked.connect(_export)
	if not EventBus.ping_attempt_failed.is_connected(_on_ping_attempt_failed):
		EventBus.ping_attempt_failed.connect(_on_ping_attempt_failed)
	if not EventBus.ping_completed.is_connected(_on_ping_completed):
		EventBus.ping_completed.connect(_on_ping_completed)

func _parse_category(object, category):
	if category=="PlcCommands":
		_create_command_tools()

func _parse_property(object, type, name, hint_type, hint_string, usage_flags, wide):
	if name == "ghost_prop":
		return true

## Creates command tools and adds them as custom controls.
func _create_command_tools():
	var plc_commands = PLC_COMMANDS.instantiate()
	add_custom_control(plc_commands)

## Connects to the [b]PLC[/b] and establishes communication.
## [b]Returns:[/b] [color=#70bafa]bool[/color] - [i]True if the connection is successful, false otherwise.[/i]
func _connect():
	print("Connect")

## Disconnects from the [b]PLC[/b].
## [b]Parameters:[/b]
## - [param force]: [color=#70bafa]bool[/color] - If true, forces the disconnection.
## [b]Returns:[/b] [color=#70bafa]void[/color]
func _disconnect():
	print("Disconnect")

## Sends a ping to the [b]PLC[/b].
## [b]Returns:[/b]
## - [color=#70bafa]string[/color]: The response from the PLC.
func _ping() -> void:
	# Validate the IP address
	if not _plc.data:
		return

	if !NetworkUtils.ValidateIP(_plc.data.ip_address):
		
		var params = {
			"title": "Invalid IP Address",
			"message": "The IP address is invalid. Please enter a valid IP address.",
			"ok_text": "OK",
			"cancel_text": "",
			"progress": false
		}
		
		EventBus.confirm_popup_invoked.emit(params, func(): pass)
		return

	# IP address is valid, start the ping process
	var params = {
		"title": "PLC Ping",
		"message": "Testing connection to PLC...",
		"progress": true,
		"ok_text": "",  # Hide the OK button
		"cancel_text": "Cancel",
		"cancel_callback": func(): NetworkUtils.CancelPing()
	}

	EventBus.confirm_popup_invoked.emit(params, func(): NetworkUtils.Ping(_plc.data.ip_address, EventBus))

# Function to modify content of the confirmation dialog
# when a ping attempt fails
func _on_ping_attempt_failed(ip: String, attempt: int, max_attempts: int) -> void:
	var params = {
		"title": "PLC Ping",
		"message": "Attempt %d of %d: Testing connection to PLC..." % [attempt, max_attempts],
		"progress": true,
		"cancel_callback": func():  NetworkUtils.CancelPing()
	}
	EventBus.modify_content_popup_invoked.emit(params, func(): pass)

# Function to show the result of the ping attempt
func _on_ping_completed(ip: String, success: bool) -> void:    
	var params = {
		"title": "Ping Result",
		"message": "Connection " + ("successful" if success else "failed after %d attempts" % 4) + " to PLC at " + ip,
		"ok_text": "OK",
		"cancel_text": "",
		"progress": false
	}
	
	EventBus.modify_content_popup_invoked.emit(params, func(): pass)

## Monitors live signals from the [b]PLC[/b].
## Displays real-time value changes and interactions.
## [b]Parameters:[/b]
## - [param duration]: [color=#70bafa]int[/color] - Duration in seconds to monitor signals.
func _online():
	print("Online")

## Handles the import event.
func _import():
	print("Import")

## Handles the export event.
func _export():
	print("Export")
