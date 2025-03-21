@tool
extends Control

class_name PlcCommands

func _ready() -> void:
	$HFlowContainer/Connect.pressed.connect(EventBus.plc_connect_invoked.emit)
	$HFlowContainer/Disconnect.pressed.connect(EventBus.plc_disconnect_invoked.emit)
	$HFlowContainer/Ping.pressed.connect(EventBus.ping_invoked.emit)
	$HFlowContainer/Online.pressed.connect(EventBus.online_invoked.emit)
	$HFlowContainer/Import.pressed.connect(EventBus.import_invoked.emit)
	$HFlowContainer/Export.pressed.connect(EventBus.export_invoked.emit)
	
	EventBus.plc_connected.connect(func(plc: Plc):
		$HFlowContainer/Connect.disabled = true
		$HFlowContainer/Disconnect.disabled = false
		$HFlowContainer/Online.disabled = false
		$HFlowContainer/Ping.disabled = true
		_set_color(PlcNode.Status.CONNECTED))

	EventBus.plc_connection_lost.connect(func(plc: Plc):
		$HFlowContainer/Connect.disabled = false
		$HFlowContainer/Ping.disabled = false
		$HFlowContainer/Disconnect.disabled = true
		$HFlowContainer/Online.disabled = true
		_set_color(PlcNode.Status.DISCONNECTED))

func set_up(plc: PlcNode) -> void:
	$HFlowContainer/Connect.disabled = plc.status == PlcNode.Status.CONNECTED
	$HFlowContainer/Ping.disabled = plc.status == PlcNode.Status.CONNECTED
	$HFlowContainer/Disconnect.disabled  = plc.status == PlcNode.Status.DISCONNECTED
	$HFlowContainer/Online.disabled  = plc.status == PlcNode.Status.DISCONNECTED
	
	_set_color(plc.status)

func _set_color(status: PlcNode.Status) -> void:
	match status:
		PlcNode.Status.DISCONNECTED:
			$StatusContainer.color = Color("#ff786b")
			$StatusContainer/Status.text = "Disconnected"
			$StatusContainer/Status.set("theme_override_colors/font_color", Color("#95463f"))
		PlcNode.Status.CONNECTED:
			$StatusContainer.color = Color("#8eef97")
			$StatusContainer/Status.text = "Connected"
			$StatusContainer/Status.set("theme_override_colors/font_color", Color("#5c9a62"))
		PlcNode.Status.UNKNOWN:
			$StatusContainer.color = Color("#ffde66")
			$StatusContainer/Status.text = "Unknown"
			$StatusContainer/Status.set("theme_override_colors/font_color", Color("#927f3b"))
		_:
			$StatusContainer.color = Color("#ffde66")
			$StatusContainer/Status.text = "Unknown"
			$StatusContainer/Status.set("theme_override_colors/font_color", Color("#927f3b"))
