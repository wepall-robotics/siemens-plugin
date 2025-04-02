@tool
extends Control

class_name PlcCommands

func _ready() -> void:
	await EventBus
	$HFlowContainer/Connect.pressed.connect(EventBus.plc_connect_invoked.emit)
	$HFlowContainer/Disconnect.pressed.connect(EventBus.plc_disconnect_invoked.emit)
	$HFlowContainer/Ping.pressed.connect(EventBus.ping_invoked.emit)
	$HFlowContainer/Online.pressed.connect(EventBus.online_invoked.emit)
	$HFlowContainer/Import.pressed.connect(EventBus.import_invoked.emit)
	$HFlowContainer/Export.pressed.connect(EventBus.export_invoked.emit)
	
	EventBus.plc_connected.connect(func(plc: Plc): _on_plc_connection_changed(true))
	EventBus.plc_connection_lost.connect(func(plc: Plc): _on_plc_connection_changed(false))
	EventBus.plc_disconnected.connect(func(plc: Plc): _on_plc_connection_changed(false))

func set_up(plc: PlcNode) -> void:
	$HFlowContainer/Connect.disabled = plc.CurrentStatus == 0
	$HFlowContainer/Ping.disabled = plc.CurrentStatus == 0
	$HFlowContainer/Disconnect.disabled  = plc.CurrentStatus != 0
	$HFlowContainer/Online.disabled  = plc.CurrentStatus != 0
	
	_set_color(plc.CurrentStatus)

func _on_plc_connection_changed(connected: bool):
	$HFlowContainer/Connect.disabled = connected
	$HFlowContainer/Ping.disabled = connected
	$HFlowContainer/Disconnect.disabled = not connected
	$HFlowContainer/Online.disabled = not connected
	
	if connected:
		_set_color(0)
	else:
		_set_color(1)

func _set_color(status: int) -> void:
	match status:
		1:
			$StatusContainer.color = Color("#ff786b")
			$StatusContainer/Status.text = "Disconnected"
			$StatusContainer/Status.set("theme_override_colors/font_color", Color("#95463f"))
		0:
			$StatusContainer.color = Color("#8eef97")
			$StatusContainer/Status.text = "Connected"
			$StatusContainer/Status.set("theme_override_colors/font_color", Color("#5c9a62"))
		2:
			$StatusContainer.color = Color("#ffde66")
			$StatusContainer/Status.text = "Unknown"
			$StatusContainer/Status.set("theme_override_colors/font_color", Color("#927f3b"))
		_:
			$StatusContainer.color = Color("#ffde66")
			$StatusContainer/Status.text = "Unknown"
			$StatusContainer/Status.set("theme_override_colors/font_color", Color("#927f3b"))
