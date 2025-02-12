@tool
extends Node

class_name RowItem

enum FIELDS { NAME, DATATYPE, VARTYPE, DIRECTION, COMPONENT, PROPERTY, FORCE, VALUE }

# Summary:
# Types of memory area that can be read
enum DataType
{ 
	INPUT = 129,
	OUTPUT = 130,
	MEMORY = 131,
	DATA_BLOCK = 132,
	TIMER = 29,
	COUNTER = 28
}

enum VarType
{
	Bit,
	Byte,
	Word,
	DWord,
	Int,
	DInt,
	Real,
	LReal,
	String,
	S7String,
	S7WString,
	Timer,
	Counter,
	DateTime,
	DateTimeLong
}

enum CommsDirection
{
	Unknown,
	PlcToGodot,
	GodotToPlc,
	Bidirectional
}


var item: TreeItem:
	get: return item
	set(value):
		item = value

func _init(item: TreeItem) -> void:
	self.item = item

func _ready():
	_init_cell_name()
	_init_cell_datatype()
	_init_cell_vartype()
	_init_cell_direction()
	_init_cell_component()
	_init_cell_property()
	_init_cell_force()
	_init_cell_value()

func _get_field(field : FIELDS) -> Variant:
	return item.get_metadata(field)

func _create_cell(column: int, control: Control, item: TreeItem):
	# control.size_flags_horizontal = Control.SIZE_FILL
	# control.size_flags_vertical = Control.SIZE_FILL
	item.set_cell_mode(column, TreeItem.CELL_MODE_CUSTOM)
	item.set_custom_draw_callback(column, func (item : TreeItem, rect : Rect2):
			control.position.x = rect.position.x
			control.position.y = rect.position.y
			control.size = rect.size)
	item.get_tree().add_child(control)
	item.set_metadata(column, control)

func _init_cell_name():
	var cell_name = LineEdit.new()
	# cell_name.text_changed.connect()
	cell_name.placeholder_text = "Signal name"
	
	_create_cell(0, cell_name, item)

func _init_cell_datatype():
	var cell_datatype = OptionButton.new()

	cell_datatype.add_item("Input", DataType.INPUT)
	cell_datatype.add_item("Output", DataType.OUTPUT)
	cell_datatype.add_item("Memory", DataType.MEMORY)
	cell_datatype.add_item("Data Block", DataType.DATA_BLOCK)
	cell_datatype.add_item("Timer", DataType.TIMER)
	cell_datatype.add_item("Counter", DataType.COUNTER)

	_create_cell(1, cell_datatype, item)

func _init_cell_vartype():
	var cell_vartype = OptionButton.new()
	
	cell_vartype.add_item("Bit", VarType.Bit)
	cell_vartype.add_item("Byte", VarType.Byte)
	cell_vartype.add_item("Word", VarType.Word)
	cell_vartype.add_item("DWord", VarType.DWord)
	cell_vartype.add_item("Int", VarType.Int)
	cell_vartype.add_item("DInt", VarType.DInt)
	cell_vartype.add_item("Real", VarType.LReal)
	cell_vartype.add_item("LReal", VarType.LReal)
	cell_vartype.add_item("String", VarType.String)
	cell_vartype.add_item("S7String", VarType.S7String)
	cell_vartype.add_item("S7WString", VarType.S7WString)
	cell_vartype.add_item("Timer", VarType.Timer)
	cell_vartype.add_item("Counter", VarType.Counter)
	cell_vartype.add_item("DateTime", VarType.DateTime)
	cell_vartype.add_item("DateTimeLong", VarType.DateTimeLong)

	_create_cell(2, cell_vartype, item)

func _init_cell_direction():
	var cell_direction = OptionButton.new()

	cell_direction.add_item("Unknown", CommsDirection.Unknown)
	cell_direction.add_item("PlcToGodot", CommsDirection.PlcToGodot)
	cell_direction.add_item("GodotToPlc", CommsDirection.GodotToPlc)
	cell_direction.add_item("Bidirectional", CommsDirection.Bidirectional)

	_create_cell(3, cell_direction, item)

func _init_cell_component():
	var cell_component = OptionButton.new()

	_create_cell(4, cell_component, item)

func _init_cell_property():
	var cell_property = OptionButton.new()

	_create_cell(5, cell_property, item)

func _init_cell_force():
	var cell_force = CheckBox.new()

	_create_cell(6, cell_force, item)

func _init_cell_value():
	var cell_value = LineEdit.new()
	
	cell_value.editable = false

	_create_cell(7, cell_value, item)
