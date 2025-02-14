@tool
extends EditorPlugin

var plugin

func _enter_tree():
	plugin = preload("res://addons/siemens_plugin/components/plc/plc_controller.gd").new()
	add_inspector_plugin(plugin)

func _enable_plugin():
	add_autoload_singleton("Globals", "res://addons/siemens_plugin/scripts/globals/globals.gd")

func _exit_tree():
	remove_inspector_plugin(plugin)

func _disable_plugin():
	remove_autoload_singleton("Globals")
