@tool
# Class representing a panel that manages a list of PLC items
class_name PlcList

extends Control

enum Status {
	CONNECTED,
	DISCONNECTED,
	UNKNOWN
}

#region Export Variables
# Container for the list of PLC items
@export var plc_list_container: VBoxContainer
# Button to add a new PLC item
@export var add_plc_button: Button
# Item list for PLC items.
@export var plc_items: ItemList
#endregion

#region Private Variables
var _plcs: Array
var _selected_plc: PlcData
#endregion

#region @onready Variables
@onready var icon: Texture2D = preload("res://addons/siemens_plugin/icons/Circle.svg")
#endregion

#region Godot Override Methods
# Function called when the panel is initialized
func _ready():
	EventBus.plc_updated.connect(_on_plc_updated)
	EventBus.ping_attempt_failed.connect(_on_ping_attempt_failed)
	EventBus.ping_completed.connect(_on_ping_completed)
	_plcs = PlcsController.Plcs
#endregion

#region Private Methods
# Function called when the add button is pressed
func _on_add_plc_item() -> void:
	var new_plc = PlcsController.CreatePlc()
	
	plc_items.add_item(new_plc.Name, icon)
	_update_status(new_plc.ConnectionStatus, _plcs.size() - 1)
	
	plc_items.select(plc_items.get_item_count() - 1)
	_selected_plc = new_plc
	EventBus.plc_selected.emit(new_plc)

# Function called when a PLC item is selected
func _on_plc_item_selected(index: int):
	_selected_plc = _plcs[index]
	EventBus.plc_selected.emit(_plcs[index])

# Function called when a PLC item is updated
func _on_plc_updated(plc_data: PlcData, property: String) -> void:
	match property:
		"Name":
			_update_name(plc_data.Name, _plcs.find(plc_data))
		"ConnectionStatus":
			_update_status(plc_data.ConnectionStatus, _plcs.find(plc_data))

# Function to remove a PLC item
func _on_remove_confirmed():
	var index = _plcs.find(_selected_plc)
	if PlcsController.RemovePlc(_selected_plc):
		plc_items.remove_item(index)
		EventBus.plc_deselect.emit()

# Function to show window to confirme deleting
func _on_remove_plc_item() -> void:
	var params = {
		"title": "Confirm Deletion",
		"message": "Are you sure you want to delete this PLC?",
		"ok_text": "Yes, delete",
		"cancel_text": "Cancel",
	}
	
	EventBus.confirm_popup_invoked.emit(params, _on_remove_confirmed)

# Function to show the ping dialog
func _on_ping_selected_plc() -> void:
	if _selected_plc:
		# Validate the IP address
		if !PlcsController.ValidateIP(_selected_plc.IPAddress):
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
			"cancel_callback": func(): PlcsController.CancelPing()
		}
		
		EventBus.confirm_popup_invoked.emit(params, func(): PlcsController.Ping(_selected_plc) )

# Function to modify content of the confirmation dialog
# when a ping attempt fails
func _on_ping_attempt_failed(plc_data: PlcData, attempt: int, max_attempts: int) -> void:
	var params = {
		"title": "PLC Ping",
		"message": "Attempt %d of %d: Testing connection to PLC..." % [attempt, max_attempts],
		"progress": true,
		"cancel_callback": func(): PlcsController.CancelPing()
	}
	EventBus.modify_content_popup_invoked.emit(params, func(): pass)

# Function to show the result of the ping attempt
func _on_ping_completed(plc_data: PlcData, success: bool) -> void:    
	var params = {
		"title": "Ping Result",
		"message": "Connection " + ("successful" if success else "failed after %d attempts" % 4) + " to PLC at " + plc_data.IPAddress,
		"ok_text": "OK",
		"cancel_text": "",
		"progress": false
	}
	
	EventBus.modify_content_popup_invoked.emit(params, func(): pass)

# Function to update the name of a PLC item
func _update_name(new_name: String, plc_index: int) -> void:
	plc_items.set_item_text(plc_index, new_name)

# Function to update the status of a PLC item
func _update_status(status: Status, plc_index: int) -> void:
	if not status:
		return
	
	match _plcs[plc_index].ConnectionStatus:
		Status.CONNECTED:
			plc_items.set_item_icon_modulate(plc_index, Color.LIME_GREEN)
		Status.DISCONNECTED:
			plc_items.set_item_icon_modulate(plc_index, Color.INDIAN_RED)
		Status.UNKNOWN:
			plc_items.set_item_icon_modulate(plc_index, Color.ORANGE)
#endregion
