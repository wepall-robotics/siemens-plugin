@tool
class_name PropertySelector
extends EditorProperty

enum AccessMode { Read, Write,ReadWrite }

var prop_selector := OptionButton.new()
var props: Array = []

func _init(visual_component: Node, type: int):
	for prop in visual_component.get_property_list():
		if prop.type == type:
			props.append(prop.name)
	
	prop_selector.add_item("Select a property...")
	for prop in props:
		prop_selector.add_item(prop)
	add_child(prop_selector)
	
	prop_selector.item_selected.connect(_update_visual_property)

func _ready():
	var current_prop = get_edited_object().VisualProperty
	if current_prop in props:
		var prop_index = props.find(current_prop)
		prop_selector.select(prop_index + 1)

func _update_visual_property(index: int) -> void:
	if index > 0:
		var prop_name = prop_selector.get_item_text(index)
		emit_changed("VisualProperty", prop_name)
