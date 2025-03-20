@tool
extends HFlowContainer

class_name PlcCommands

func _ready() -> void:
	$Connect.pressed.connect(EventBus.plc_connect_invoked.emit)
	$Disconnect.pressed.connect(EventBus.plc_disconnect_invoked.emit)
	$Ping.pressed.connect(EventBus.ping_invoked.emit)
	$Online.pressed.connect(EventBus.online_invoked.emit)
	$Import.pressed.connect(EventBus.import_invoked.emit)
	$Export.pressed.connect(EventBus.export_invoked.emit)

	EventBus.plc_connected.connect(func(plc):
		$Connect.disabled = true
		$Disconnect.disabled = false)

	EventBus.plc_connection_lost.connect(func(plc):
		$Connect.disabled = false
		$Disconnect.disabled = true)

func set_up(plc: PlcNode) -> void:
		$Connect.disabled = plc.status == PlcNode.Status.CONNECTED
		$Disconnect.disabled  = plc.status == PlcNode.Status.DISCONNECTED
