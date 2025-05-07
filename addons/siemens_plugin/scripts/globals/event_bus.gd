@tool
extends Node

# EventBus: A global node for managing and emitting signals across the application.

#region Plc signals

# Emitted when a ping attempt fails.
# @param ip The IP address being pinged.
# @param attempt The current attempt number.
# @param max_attempts The maximum number of attempts allowed.
signal ping_attempt_failed(ip: String, attempt: int, max_attempts: int)

# Emitted when a ping operation completes.
# @param ip The IP address being pinged.
# @param success Whether the ping was successful.
signal ping_completed(ip: String, success: bool)

# Emitted when a ping operation is invoked.
signal ping_invoked()

# Emitted when a PLC connection is invoked.
# @param plc The PLC instance being connected.
signal plc_connect_invoked(plc: Plc)

# Emitted when a PLC connection fails.
# @param plc The PLC instance.
# @param error The error message.
signal plc_connection_failed(plc: Plc, error: String)

# Emitted when a PLC is successfully connected.
# @param plc The PLC instance.
signal plc_connected(plc: Plc)

# Emitted when a PLC disconnection is invoked.
signal plc_disconnect_invoked()

# Emitted when an online operation is invoked.
signal online_invoked()

# Emitted when an import operation is invoked.
signal import_invoked()

# Emitted when an export operation is invoked.
signal export_invoked()

# Emitted during a PLC connection attempt.
# @param attempt The current attempt number.
# @param max_attempts The maximum number of attempts allowed.
signal plc_connection_attempt(attempt: int, max_attempts: int)

# Emitted when a PLC connection attempt is cancelled.
# @param plc The PLC instance.
signal plc_connection_cancelled(plc: Plc)

# Emitted when a PLC connection attempt fails.
# @param plc The PLC instance.
# @param details Additional details about the failure.
signal plc_connection_attempt_failed(plc: Plc, details: String)

# Emitted when a PLC connection is lost.
# @param plc The PLC instance.
signal plc_connection_lost(plc: Plc)

# Emitted when a PLC is disconnected.
# @param plc The PLC instance.
signal plc_disconnected(plc: Plc)

# Emitted when a PLC disconnection fails.
# @param reason The reason for the failure.
signal plc_disconnection_failed(reason: String)

# Emitted when a PLC is already disconnected.
# @param plc The PLC instance.
signal plc_already_disconnected(plc: Plc)

# Emitted when PLC data is updated.
# @param plc The PLC instance.
signal plc_data_updated(plc: Plc)

# Emitted during a ping attempt.
# @param ip The IP address being pinged.
# @param attempt The current attempt number.
# @param max_attempts The maximum number of attempts allowed.
signal ping_attempt(ip: String, attempt: int, max_attempts: int)

# Emitted when a ping operation is cancelled.
# @param ip The IP address being pinged.
signal ping_cancelled(ip: String)

# Emitted when a ping operation encounters an error.
# @param ip The IP address being pinged.
signal ping_error(ip: String)

#endregion

#region Windows and Popups signals

# Emitted to close a confirmation popup.
signal close_confirm_popup

# Emitted to invoke a confirmation popup.
# @param params A dictionary containing popup parameters.
# @param callback A callable to execute after confirmation.
signal confirm_popup_invoked(params: Dictionary, callback: Callable)

# Emitted to invoke a content modification popup.
# @param params A dictionary containing popup parameters.
# @param callback A callable to execute after modification.
signal modify_content_popup_invoked(params: Dictionary, callback: Callable)

#endregion
