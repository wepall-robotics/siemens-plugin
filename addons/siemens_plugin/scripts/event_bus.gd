@tool
extends Node

# Plc signals.
signal plc_selected(plc_data : PlcData)
signal plc_deselect()
signal plc_added(plc_data: PlcData)
signal plc_removed(plc_data: PlcData)
signal plc_updated(plc_data: PlcData, property: String)
signal ping_started(plc_data: PlcData)
signal ping_completed(plc_data: PlcData, success: bool)
signal ping_attempt_failed(plc_data: PlcData, attempt: int, max_attempts: int)

# Windows and Popups signals._add_constant_central_force
signal show_confirm_popup(params: Dictionary, callback: Callable)
signal close_confirm_popup