@tool
class_name Plc
extends Node

## Enum representing the connection status of the PLC.
enum Status {
	CONNECTED,    ## The PLC is connected.
	DISCONNECTED, ## The PLC is disconnected.
	UNKNOWN       ## The connection status of the PLC is unknown.
}

## This property holds the configuration for a [b]Programmable Logic Controller[/b].
## It is of type [code]PlcData[/code], which likely encapsulates essential [b]PLC[/b] settings such as
## [code]IP address[/code], [code]port numbers[/code], and other communication parameters.
@export var data: PlcData:
	get: return data
	set(value):
		data = value
		if data:
			data.changed.connect(update_configuration_warnings)
		update_configuration_warnings()

## Holds an array of [b]Plc[/b] Signal objects.
@export var signals: Array[PlcSignal]

@export_category("PlcCommands")
## Export the status property of type [b]Status[/b] enum, initialized to [b]UNKNOWN[/b].
@export var status: Status = Status.UNKNOWN:
	get: return status
	set(value):
		status = value

## Ghost property for export category organization.
## Its purpose is to control category header placement in the inspector.
@export var ghost_prop: bool

# Override function to provide configuration warnings for the node.
func _get_configuration_warnings():
	if not data:
		return ["PLC configuration required:\n1. Select this node in the Scene tree.\n2. In the Inspector, find the data property.\n3. Assign or create a new PLC configuration."]
	else:
		if data.ip_address.is_empty() or !NetworkUtils.ValidateIP(data.ip_address):
			return ["Invalid IP address:\n1. Select this node in the Scene tree.\n2. In the Inspector, expand the data property.\n3. Enter a valid IP address in the field."]
	return []

# Override function to validate and modify property attributes.
func _validate_property(property):
	if property.name == "status":
		property.usage |= PROPERTY_USAGE_READ_ONLY
	if property.name == "ghost_prop":
		property.usage |= PROPERTY_USAGE_NO_EDITOR
