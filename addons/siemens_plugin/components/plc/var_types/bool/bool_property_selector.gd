@tool
class_name BoolPropertySelector
extends EditorProperty

enum AccessMode { Read, Write,ReadWrite }

var mode_selector := OptionButton.new()
var prop_selector := OptionButton.new()
var bool_props: Array = []

func _init(visual_component: Node):
	mode_selector.add_item("Read", AccessMode.Read)
	mode_selector.add_item("Write", AccessMode.Write)
	mode_selector.add_item("Read/Write", AccessMode.ReadWrite)
	add_child(mode_selector)

	for prop in visual_component.get_property_list():
		#if prop.usage & PROPERTY_USAGE_SCRIPT_VARIABLE and prop.type == TYPE_BOOL:
		if prop.type == TYPE_BOOL:
			bool_props.append(prop.name)
	
	prop_selector.add_item("Select a property...")
	for prop in bool_props:
		prop_selector.add_item(prop)
	add_child(prop_selector)
	
	mode_selector.item_selected.connect(_update_mode)
	prop_selector.item_selected.connect(_update_visual_property)

func _ready():
	var current_prop = get_edited_object().VisualProperty
	if current_prop in bool_props:
		var prop_index = bool_props.find(current_prop)
		prop_selector.select(prop_index + 1)

func _update_mode(index: int) -> void:
	var mode = mode_selector.get_item_id(index)
	emit_changed("Mode", mode)

func _update_visual_property(index: int) -> void:
	if index > 0:
		var prop_name = prop_selector.get_item_text(index)
		emit_changed("VisualProperty", prop_name)
