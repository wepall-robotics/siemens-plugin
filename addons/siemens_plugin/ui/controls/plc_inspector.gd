@tool
extends EditorInspectorPlugin

# PlcInspector: A plugin for managing and inspecting PLC-related objects in the Godot editor.

#region Constants
var PLC_COMMANDS : PackedScene
var PropertySelector = preload("uid://k20fn1xhvv4k")
#endregion

#region Public Variables
var _plc: Plc
var item_type
#endregion

#region Built-in virtual Init
func _init():
	# Load the PLC commands scene.
	PLC_COMMANDS = load("uid://c8an8h7jgu8of")
#endregion

#region Built-in functions
# Determines if the given object can be handled by this controller.
func _can_handle(object) -> bool:
	if object is Plc:
		_plc = object
		_connect_event_bus_signals()
		return true
	elif object is DataItem:
		return true

	return false

# Parses a category in the inspector.
func _parse_category(object, category):
	if category=="PlcCommands":
		_create_command_tools()

# Parses a property in the inspector.
func _parse_property(object, type, name, hint_type, hint_string, usage_flags, wide):
	if name == "ghost_prop":
		return true
	if name == "VisualProperty" and object.VisualComponent != null:
		var t
		match object:
			var x when x is BoolItem: t = TYPE_BOOL
			var x when x is ByteItem: t = TYPE_INT
			var x when x is DIntItem: t = TYPE_INT
			var x when x is DWordItem: t = TYPE_INT
			var x when x is IntItem: t = TYPE_INT
			var x when x is RealItem: t = TYPE_FLOAT
			var x when x is LRealItem: t = TYPE_FLOAT
			var x when x is StringItem: t = TYPE_STRING
			var x when x is StringExItem: t = TYPE_STRING
			var x when x is WordItem: t = TYPE_INT
			#var x when x is CounterItem: t = TYPE_INT
			#var x when x is TimerItem: t = TYPE_INT
			
		var selector = PropertySelector.new(object.VisualComponent, t)
		add_property_editor(name, selector)
		return true
	return false

#endregion

#region Private Functions
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
	if not EventBus.online_invoked.is_connected(_toggle_online):
		EventBus.online_invoked.connect(_toggle_online)
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
	if not EventBus.plc_already_disconnected.is_connected(_on_plc_already_disconnected):
		EventBus.plc_already_disconnected.connect(_on_plc_already_disconnected)

## Creates command tools and adds them as custom controls.
func _create_command_tools():
	var plc_commands = PLC_COMMANDS.instantiate()
	plc_commands.set_up(_plc)
	add_custom_control(plc_commands)

## Connects to the [b]PLC[/b] and establishes communication.
## [b]Returns:[/b] [color=#70bafa]bool[/color] - [i]True if the connection is successful, false otherwise.[/i]
func _connect_plc():
	_plc.ConnectPlc(EventBus)
	# Validate the IP address
	if not _plc or not _plc.ValidConfiguration:
		return
	
	if !Plc.ValidateIP(_plc.IP):
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
		"cancel_callback": func():  _plc.CancelAllOperations()
	}
	
	EventBus.confirm_popup_invoked.emit(params, func(): 
		_plc.ConnectPlc(EventBus))

func _on_plc_connection_attempt(attempt: int, max_attempts: int):
	var params = {
		"title": "Connecting to Plc",
		"message": "Attempt %d/%d: Establishing connection..." % [attempt, max_attempts],
		"progress": true,
		"cancel_text": "Cancel",
		"cancel_callback": func():  _plc.CancelAllOperations()
	}
	EventBus.modify_content_popup_invoked.emit(params, func(): pass)

## Disconnects from the [b]PLC[/b].
## [b]Parameters:[/b]
## - [param force]: [color=#70bafa]bool[/color] - If true, forces the disconnection.
## [b]Returns:[/b] [color=#70bafa]void[/color]
func _disconnect():
	if not _plc or _plc.CurrentStatus != 0:
		EventBus.plc_disconnection_failed.emit("Plc not configured or connected.", func(): pass)
		return

	EventBus.confirm_popup_invoked.emit({
		"title": "Disconnect Plc",
		"message": "Are you sure you want to disconnect the Plc?",
		"ok_text": "Yes",
		"cancel_text": "No",
		"ok_callback": func(): _plc.Disconnect(EventBus)
	}, func(): pass)

func _on_plc_disconnected(plc):
	_set_status(1)
	#_plc.CurrentStatus = 1
	_online(false)
	print_rich("[color=#fb7e7e]Plc successfully disconnected.[/color]")

func _on_plc_disconnection_failed(reason):
	print_rich("[color=#fb7e7e]Plc disconnection failed: %s[/color]" % reason)
	
	var params = {
		"title": "Disconnection Error",
		"message": "Failed to disconnect Plc:\n%s" % reason,
		"ok_text": "OK"
	}
	EventBus.modify_content_popup_invoked.emit(params, func(): pass)

func _on_plc_already_disconnected(plc):
	_set_status(1)
	#_plc.CurrentStatus = 1
	_online(false)
	print_rich("[color=#fb7e7e]Plc was already disconnected.[/color]")

## Sends a ping to the [b]PLC[/b].
## [b]Returns:[/b]
## - [color=#70bafa]string[/color]: The response from the PLC.
func _ping() -> void:
	# Validate the IP address
	if not _plc or _plc.CurrentStatus == 0 or not _plc.ValidConfiguration:
		return
	if !Plc.ValidateIP(_plc.IP):
		
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
		"title": "Plc Ping",
		"message": "Testing connection to PLC...",
		"progress": true,
		"ok_text": "",  # Hide the OK button
		"cancel_text": "Cancel",
		"cancel_callback": func(): _plc.CancelAllOperations()
	}

	EventBus.confirm_popup_invoked.emit(params, func(): _plc.PingPlc(EventBus))

# Function to modify content of the confirmation dialog
# when a connection to plc is accomplish.
func _on_plc_connected(plcData):
	_set_status(0)
	#_plc.CurrentStatus = 0
	print_rich("[color=#70e03d]Plc connected.[/color]")
	EventBus.close_confirm_popup.emit()
	
	var params = {
		"title": "Connection Successful",
		"message": "Plc connected successfully!",
		"ok_text": "OK",
		"progress": false
	}
	EventBus.modify_content_popup_invoked.emit(params, func(): pass)

func _on_plc_connection_lost(plcData):
	print_rich("[color=#fb7e7e]Connection lost.[/color]")
	_set_status(1)
	#_plc.CurrentStatus = 1
	_online(false)
	
	var params = {
		"title": "Connection Lost",
		"message": "The connection to the Plc was lost. Attempting to reconnect...",
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
		"title": "Plc Ping",
		"message": "Attempt %d of %d: Testing connection to Plc..." % [attempt, max_attempts],
		"progress": true,
		"cancel_callback": func():  _plc.CancelAllOperations()
	}
	EventBus.modify_content_popup_invoked.emit(params, func(): pass)

# Function to show the result of the ping attempt
func _on_ping_completed(ip: String, success: bool) -> void:    
	var params = {
		"title": "Ping Result",
		"message": "Connection " + ("successful" if success else "failed after %d attempts" % 4) + " to Plc at " + ip,
		"ok_text": "OK",
		"cancel_text": "",
		"progress": false
	}

	EventBus.modify_content_popup_invoked.emit(params, func(): pass)

func _toggle_online():
	_online(!_plc.IsOnline)

## Monitors live signals from the [b]PLC[/b].
## Displays real-time value changes and interactions.
## [b]Parameters:[/b]
## - [param duration]: [color=#70bafa]int[/color] - Duration in seconds to monitor signals.
func _online(_value: bool) -> void:
	var undo_redo = EditorInterface.get_editor_undo_redo()
	undo_redo.create_action("Set Online")
	undo_redo.add_do_property(_plc, "IsOnline", _value)
	undo_redo.add_undo_property(_plc, "IsOnline", !_value)
	undo_redo.commit_action()

func _set_status(_value: int) -> void:
	var undo_redo = EditorInterface.get_editor_undo_redo()
	undo_redo.create_action("Set Status")
	undo_redo.add_do_property(_plc, "CurrentStatus", _value)
	undo_redo.add_undo_property(_plc, "CurrentStatus", _plc.CurrentStatus)
	undo_redo.commit_action()

## Handles the import event.
func _import():
	print("Import")

## Handles the export event.
func _export():
	print("Export")
#endregion
