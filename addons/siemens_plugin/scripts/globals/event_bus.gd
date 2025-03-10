@tool
extends Node

#region Plc signals.
signal ping_attempt_failed(ip: String, attempt: int, max_attempts: int)
signal ping_completed(ip: String, success: bool)
signal ping_invoked()
signal plc_connect_invoked(plc: PlcData)
signal plc_connection_failed(plc: Dictionary, error: String)
signal plc_connected(plc: PlcData)
signal plc_disconnect_invoked()
signal online_invoked()
signal import_invoked()
signal export_invoked()
#endregion

#region Windows and Popups signals.
signal close_confirm_popup
signal confirm_popup_invoked(params: Dictionary, callback: Callable)
signal modify_content_popup_invoked(params: Dictionary, callback: Callable)
#endregion
