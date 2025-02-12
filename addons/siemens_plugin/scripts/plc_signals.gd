@tool
extends Tree

class_name PlcSignals

var _root

func _ready():
	clear()
	_create_headers()
	_root = create_item()
	
func _on_add_signal_pressed():
	var item = create_item(_root)
	var row = RowItem.new(item)
	
	get_tree().root.add_child(row)

func _create_headers():
	set_column_title_alignment(0, HORIZONTAL_ALIGNMENT_LEFT)
	set_column_title_alignment(1, HORIZONTAL_ALIGNMENT_LEFT)
	set_column_title_alignment(2, HORIZONTAL_ALIGNMENT_LEFT)
	set_column_title_alignment(3, HORIZONTAL_ALIGNMENT_LEFT)
	set_column_title_alignment(4, HORIZONTAL_ALIGNMENT_LEFT)
	set_column_title_alignment(5, HORIZONTAL_ALIGNMENT_LEFT)
	set_column_title_alignment(6, HORIZONTAL_ALIGNMENT_LEFT)
	set_column_title_alignment(7, HORIZONTAL_ALIGNMENT_LEFT)

	set_column_title(0, "Name")
	set_column_title(1, "Type")
	set_column_title(2, "Address")
	set_column_title(3, "Component")
	set_column_title(4, "Property")
	set_column_title(5, "Comms")
	set_column_title(6, "Value")
	set_column_title(7, "Force")
