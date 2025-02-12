@tool
extends Resource
class_name PlcData

## Enum representing the connection status of the PLC.
enum Status {
	CONNECTED,    ## The PLC is connected.
	DISCONNECTED, ## The PLC is disconnected.
	UNKNOWN       ## The connection status of the PLC is unknown.
}


## The IP address of the PLC.
@export var ip_address: String:
	get:
		return ip_address
	set(value):
		ip_address = value
		emit_changed()

## The name of the PLC.
@export var name: String:
	get:
		return name
	set(value):
		name = value

## The type of the CPU used by the PLC.
@export var type: int:
	get:
		return type
	set(value):
		type = value

## The rack number where the PLC is located.
@export var rack: int:
	get:
		return rack
	set(value):
		rack = value

## The slot number where the PLC is located.
@export var slot: int:
	get:
		return slot
	set(value):
		slot = value

@export var is_number_editable: bool:
	set(value):
		is_number_editable = value
		notify_property_list_changed()
@export var number: int

func _validate_property(property: Dictionary):
	if property.name == "number" and not is_number_editable:
		property.usage |= PROPERTY_USAGE_READ_ONLY
