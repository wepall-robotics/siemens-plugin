@tool
extends Control

class_name PlcConfig

@export_group("UI Elements")
@export var name_edit: LineEdit
@export var cpu_type_option: OptionButton
@export var ip_edit: LineEdit
@export var rack_spin: SpinBox
@export var slot_spin: SpinBox

var _current_plc: PlcData

@onready var error_icon: Texture2D = preload("res://addons/siemens_plugin/icons/StatusError.svg")

# Function called when the panel is initialized
func _init():
	EventBus.plc_selected.connect(_load_plc)
	EventBus.plc_deselect.connect(_on_plc_deselect)

# Function to load the PLC data into the UI elements
func _load_plc(plc: PlcData) -> void:
	_current_plc = plc
	
	if not _current_plc:
		hide()
		return
	show()
	
	# Load the PLC data into the UI elements
	name_edit.text = _current_plc.Name
	# Set the selected option in the OptionButton based on the PLC type
	var selected_id = cpu_type_option.get_item_index(_current_plc.Type)
	cpu_type_option.select(selected_id)

	# Load the IP address into the IP edit field
	_show_error_ip_icon(false)
	ip_edit.text = _current_plc.IPAddress

	# Set the values of the SpinBoxes based on the PLC rack and slot
	rack_spin.value = _current_plc.Rack
	slot_spin.value = _current_plc.Slot

# Function called when the name edit field loses focus
func _on_name_focus_exited():
	if name_edit.text != _current_plc.Name:
		name_edit.text = PlcsController.UpdatePlcName(_current_plc, name_edit.text)

# Function called when the name edit field is submitted
func _on_name_text_submitted(new_text: String):
	if name_edit.text != _current_plc.Name:
		name_edit.text = PlcsController.UpdatePlcName(_current_plc, new_text)

# Function called when the CPU type option is selected
func _on_cpu_type_option_selected(index: int):
	var selected_id = cpu_type_option.get_item_id(index)

	if selected_id != _current_plc.Type:
		PlcsController.UpdatePlcType(_current_plc, selected_id)

# Function called when the IP edit field loses focus
func _on_ip_focus_exited():
	if ip_edit.text != _current_plc.IPAddress:
		if not PlcsController.UpdatePlcIpAddress(_current_plc, ip_edit.text):
			ip_edit.text = ""
			_show_error_ip_icon(true)
		else:
			_current_plc.IPAddress = ip_edit.text
			_show_error_ip_icon(false)

# Function called when the IP edit field is submitted
func _on_ip_text_submitted(new_text: String):
	if ip_edit.text != _current_plc.IPAddress:
		if not PlcsController.UpdatePlcIpAddress(_current_plc, new_text):
			ip_edit.text = ""
			_show_error_ip_icon(true)
		else:
			_current_plc.IPAddress = new_text
			_show_error_ip_icon(false)

# Function called when Event Bus signals that a PLC is deselected
func _on_plc_deselect():
	_current_plc = null
	hide()

# Function called when the rack spin box value changes
func _on_rack_value_changed(value: float):
	if value != _current_plc.Rack:
		rack_spin.value = PlcsController.UpdatePlcRack(_current_plc, value)

# Function called when the slot spin box value changes
func _on_slot_value_changed(value: float):
	if value != _current_plc.Slot:
		slot_spin.value = PlcsController.UpdatePlcSlot(_current_plc, value)

# Function to show or hide the error icon next to the IP edit field
func _show_error_ip_icon(show: bool) -> void:
	if show:
		ip_edit.set("right_icon", error_icon)
	else:
		ip_edit.set("right_icon", null)
