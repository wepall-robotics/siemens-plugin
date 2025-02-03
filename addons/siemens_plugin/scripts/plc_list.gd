@tool
# Class representing a panel that manages a list of PLC items
class_name PlcList

extends Control

enum Status {
    CONNECTED,
    DISCONNECTED,
    UNKNOWN
}

# Base name for PLC items
const BASE_NAME = "Plc"

# Container for the list of PLC items
@export var plc_list_container: VBoxContainer
# Button to add a new PLC item
@export var add_plc_button: Button
# Item list for PLC items.
@export var plc_items: ItemList

var _plcs: Array[PlcData] = []
var _selected_plc: PlcData

@onready var icon: Texture2D = preload("res://addons/siemens_plugin/icons/Circle.svg")

# Function called when the panel is initialized
func _init():
    EventBus.plc_updated.connect(_on_plc_updated)
    _plcs = PlcController.plcs

# Function called when the add button is pressed
func _on_add_plc_item() -> void:
    var new_plc = PlcController.create_plc()
    
    plc_items.add_item(new_plc.Name, icon)
    _update_status(new_plc.ConnectionStatus, _plcs.size() - 1)
    
    plc_items.select(plc_items.get_item_count() - 1)
    _selected_plc = new_plc
    EventBus.plc_selected.emit(new_plc)

func _on_plc_item_selected(index:int):
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
func _on_remove_plc_item() -> void:
    var index = _plcs.find(_selected_plc)
    if PlcController.remove_plc(_selected_plc):
        plc_items.remove_item(index)
        EventBus.plc_deselect.emit()


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
