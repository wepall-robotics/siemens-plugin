@tool
extends EditorInspectorPlugin

var PLC_COMMANDS : PackedScene
var _plc: PlcNode
var BoolPropertySelector = preload("res://addons/siemens_plugin/components/plc/var_types/bool/bool_property_selector.gd")

func _init():
	PLC_COMMANDS = load("res://addons/siemens_plugin/components/controls/plc_commands.tscn")

# Determines if the given object can be handled by this controller.
func _can_handle(object) -> bool:
	if object is PlcNode:
		_plc = object
		_connect_event_bus_signals()
		return true
	elif object is BoolItem:
		return true

	return false

## Method to connect [b]EventBus signals[/b] to their respective handlers.
func _connect_event_bus_signals() -> void:
	await EventBus

	if not EventBus.ping_invoked.is_connected(_ping):
		EventBus.ping_invoked.connect(_ping)
	if not EventBus.plc_connect_invoked.is_connected(_connect_plc):
		EventBus.plc_connect_invoked.connect(_connect_plc)
	if not EventBus.plc_connection_failed.is_connected(_on_plc_connection_failed):
		EventBus.plc_connection_failed.connect(_on_plc_connection_failed)
	if not EventBus.plc_connected.is_connected(_on_plc_connected):
		EventBus.plc_connected.connect(_on_plc_connected)
	if not EventBus.plc_disconnect_invoked.is_connected(_disconnect):
		EventBus.plc_disconnect_invoked.connect(_disconnect)
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
	if not EventBus.plc_connection_attempt.is_connected(_on_plc_connection_attempt):
		EventBus.plc_connection_attempt.connect(_on_plc_connection_attempt)
	if not EventBus.plc_connection_lost.is_connected(_on_plc_connection_lost):
		EventBus.plc_connection_lost.connect(_on_plc_connection_lost)
	if not EventBus.plc_disconnected.is_connected(_on_plc_disconnected):
		EventBus.plc_disconnected.connect(_on_plc_disconnected)

func _parse_category(object, category):
	if category=="PlcCommands":
		_create_command_tools()

func _parse_property(object, type, name, hint_type, hint_string, usage_flags, wide):
	if name == "ghost_prop":
		return true
	if name == "VisualProperty" and object.VisualComponent != null:
		# Crea un selector personalizado
		var selector = BoolPropertySelector.new(object.VisualComponent)
		add_property_editor(name, selector)
		return true  # Reemplaza el editor por defecto
	return false

## Creates command tools and adds them as custom controls.
func _create_command_tools():
	var plc_commands = PLC_COMMANDS.instantiate()
	plc_commands.set_up(_plc)
	add_custom_control(plc_commands)

## Connects to the [b]PLC[/b] and establishes communication.
## [b]Returns:[/b] [color=#70bafa]bool[/color] - [i]True if the connection is successful, false otherwise.[/i]
func _connect_plc():
	# Validate the IP address
	if not _plc.Data or not _plc.ValidConfiguration:
		return
	
	if !Plc.ValidateIP(_plc.Data.IP):
		var params = {
			"title": "Invalid IP Address",
			"message": "The IP address is invalid. Please enter a valid IP address.",
			"ok_text": "OK",
			"cancel_text": "",
			"progress": false
		}
		
		EventBus.confirm_popup_invoked.emit(params, func(): pass)
		return

	# IP address is valid, start the connection process.
	var params = {
		"title": "Connection",
		"message": "Connecting to PLC...",
		"progress": true,
		"ok_text": "",  # Hide the OK button
		"cancel_text": "Cancel",
		"cancel_callback": func():  NetworkUtils.CancelAllOperations()
	}

	print("Antes de conectar.")
	EventBus.confirm_popup_invoked.emit(params, func(): 
		print("Salta el evento de connect")
		_plc.Data.ConnectPlc(EventBus))

func _on_plc_connection_attempt(attempt: int, max_attempts: int):
	var params = {
		"title": "Connecting to PLC",
		"message": "Attempt %d/%d: Establishing connection..." % [attempt, max_attempts],
		"progress": true,
		"cancel_text": "Cancel",
		"cancel_callback": func():  _plc.Data.CancelAllOperations()
	}
	EventBus.modify_content_popup_invoked.emit(params, func(): pass)

## Disconnects from the [b]PLC[/b].
## [b]Parameters:[/b]
## - [param force]: [color=#70bafa]bool[/color] - If true, forces the disconnection.
## [b]Returns:[/b] [color=#70bafa]void[/color]
func _disconnect():
	if not _plc.Data or _plc.CurrentStatus != 0:
		EventBus.plc_disconnection_failed.emit("PLC not configured or connected.", func(): pass)
		return

	EventBus.confirm_popup_invoked.emit({
		"title": "Disconnect PLC",
		"message": "Are you sure you want to disconnect the PLC?",
		"ok_text": "Yes",
		"cancel_text": "No",
		"ok_callback": func(): _plc.Data.Disconnect(EventBus)
	}, func(): pass)

func _on_plc_disconnected(plc):
	_plc.CurrentStatus = 1
	_plc.Data.IsOnline = false
	print("PLC successfully disconnected.")

func _on_plc_disconnection_failed(reason):
	print("PLC disconnection failed: %s" % reason)
	
	var params = {
		"title": "Disconnection Error",
		"message": "Failed to disconnect PLC:\n%s" % reason,
		"ok_text": "OK"
	}
	EventBus.modify_content_popup_invoked.emit(params, func(): pass)

func _on_plc_already_disconnected(plc):
	print("PLC was already disconnected.")

## Sends a ping to the [b]PLC[/b].
## [b]Returns:[/b]
## - [color=#70bafa]string[/color]: The response from the PLC.
func _ping() -> void:
	# Validate the IP address
	if not _plc.Data or _plc.CurrentStatus == 0 or not _plc.ValidConfiguration:
		return
	if !Plc.ValidateIP(_plc.Data.IP):
		
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
		"cancel_callback": func(): _plc.Data.CancelAllOperations()
	}

	EventBus.confirm_popup_invoked.emit(params, func(): _plc.Data.PingPlc(EventBus))

# Function to modify content of the confirmation dialog
# when a connection to plc is accomplish.
func _on_plc_connected(plcData):
	_plc.CurrentStatus = 0
	print("Plc connected.")
	EventBus.close_confirm_popup.emit()
	
	var params = {
		"title": "Connection Successful",
		"message": "PLC connected successfully!",
		"ok_text": "OK",
		"progress": false
	}
	EventBus.modify_content_popup_invoked.emit(params, func(): pass)

func _on_plc_connection_lost(plcData):
	print("Connection lost.")
	_plc.CurrentStatus = 1
	_plc.Data.IsOnline = false
	
	var params = {
		"title": "Connection Lost",
		"message": "The connection to the PLC was lost. Attempting to reconnect...",
		"progress": true,
		"cancel_text": "Cancel"
	}
	EventBus.modify_content_popup_invoked.emit(params, func(): pass)

# Function to modify content of the confirmation dialog
# when a connection to plc failed.
func _on_plc_connection_failed(plcData: Plc, error: String) -> void:
	var params = {
		"title": "Plc connection failed",
		"message": error,
		"progress": false,
		"ok_text": "Exit",
		"cancel_text": "",
	}
	EventBus.modify_content_popup_invoked.emit(params, func(): pass)

# Function to modify content of the confirmation dialog
# when a ping attempt fails
func _on_ping_attempt_failed(ip: String, attempt: int, max_attempts: int) -> void:
	var params = {
		"title": "PLC Ping",
		"message": "Attempt %d of %d: Testing connection to PLC..." % [attempt, max_attempts],
		"progress": true,
		"cancel_callback": func():  _plc.Data.CancelAllOperations()
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
	_plc.Data.IsOnline = !_plc.Data.IsOnline

## Handles the import event.
func _import():
	print("Import")

## Handles the export event.
func _export():
	print("Export")
