@tool
extends EditorPlugin

var plugin
var popup_window

func _enter_tree():
	plugin = preload("res://addons/siemens_plugin/components/plc/plc_controller.gd").new()
	add_inspector_plugin(plugin)

func _enable_plugin():
	var base_control = get_editor_interface().get_base_control()
	add_autoload_singleton("Globals", "res://addons/siemens_plugin/scripts/globals/globals.gd")

	popup_window = load("res://addons/siemens_plugin/components/windows/confirm_window.tscn").instantiate()
	base_control.add_child(popup_window)

	popup_window.visible = false 
	popup_window.transient = true
	popup_window.exclusive = true

func _exit_tree():
	remove_inspector_plugin(plugin)

func _disable_plugin():
	remove_autoload_singleton("Globals")
	
	if popup_window:
		popup_window.queue_free()
