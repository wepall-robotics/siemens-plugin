@tool
extends Node

class_name Plc

@export var data: PlcData:
	get: return data
	set(value):
		data = value
		if data:
			data.changed.connect(update_configuration_warnings)
		update_configuration_warnings()

@export var signals: Array[PlcSignal]

func _get_configuration_warnings():
	if not data:
		return ["Necesitas configurar el plc."]
	if data.ip_address.is_empty() or !NetworkUtils.ValidateIP(data.ip_address):
		return ["No tienes bien la IP joperra"]
	return []
