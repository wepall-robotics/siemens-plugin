@tool
extends Control

class_name PlcSignals

var current_plc: PlcData
var signals_list: Dictionary = {}

func load_plc_signals(plc: PlcData) -> void:
    current_plc = plc
    _update_signals_view()

func _update_signals_view() -> void:
    pass
    # Implementar vista de señales
