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
