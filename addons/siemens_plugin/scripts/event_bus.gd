@tool
extends Node

# Plc signals.
signal ping_attempt_failed(plc_data: PlcData, attempt: int, max_attempts: int)
signal ping_completed(plc_data: PlcData, success: bool)
signal ping_started(plc_data: PlcData)
signal plc_added(plc_data: PlcData)
signal plc_deselect()
signal plc_removed(plc_data: PlcData)
signal plc_selected(plc_data : PlcData)
signal plc_updated(plc_data: PlcData, property: String)

# Windows and Popups signals.
signal close_confirm_popup
signal confirm_popup_invoked(params: Dictionary, callback: Callable)
signal modify_content_popup_invoked(params: Dictionary, callback: Callable)