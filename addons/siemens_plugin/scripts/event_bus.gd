@tool
extends Node

# Plc signals.
signal plc_selected(plc_data : PlcData)
signal plc_deselect()
signal plc_added(plc_data: PlcData)
signal plc_removed(plc_data: PlcData)
signal plc_updated(plc_data: PlcData, property: String)

# Windows and Popups signals._add_constant_central_force
signal show_confirm_popup(params: Dictionary, callback: Callable)