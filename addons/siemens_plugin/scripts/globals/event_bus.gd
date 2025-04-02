@tool
extends Node

#region Plc signals.
signal ping_attempt_failed(ip: String, attempt: int, max_attempts: int)
signal ping_completed(ip: String, success: bool)
signal ping_invoked()
signal plc_connect_invoked(plc: Plc)
signal plc_connection_failed(plc: Plc, error: String)
signal plc_connected(plc: Plc)
signal plc_disconnect_invoked()
signal online_invoked()
signal import_invoked()
signal export_invoked()
signal plc_connection_attempt(attempt: int, max_attempts: int)
signal plc_connection_cancelled(plc: Plc)
signal plc_connection_attempt_failed(plc: Plc, details: String)
signal plc_connection_lost(plc: Plc)
signal plc_disconnected(plc: Plc)
signal plc_disconnection_failed(reason: String)
signal plc_already_disconnected(plc: Plc)
signal plc_data_updated(plc: Plc)
signal ping_attempt(ip: String, attempt: int, max_attempts: int)
signal ping_cancelled(ip: String)
signal ping_error(ip: String)
#endregion

#region Windows and Popups signals.
signal close_confirm_popup
signal confirm_popup_invoked(params: Dictionary, callback: Callable)
signal modify_content_popup_invoked(params: Dictionary, callback: Callable)
#endregion
