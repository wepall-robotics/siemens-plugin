@tool
extends EditorPlugin

var plugin
var popup_window

func _enter_tree():
	add_autoload_singleton("Globals", "res://addons/siemens_plugin/scripts/globals/globals.gd")
	add_autoload_singleton("EventBus", "res://addons/siemens_plugin/scripts/globals/event_bus.gd")
	
	_initialize()

func _initialize():
	var base_control = EditorInterface.get_base_control()

	plugin = load("uid://dlpgaxruwgn7h").new()
	add_inspector_plugin(plugin)

	if not popup_window:
		popup_window = load("uid://0bcym7qxne18").instantiate()
		base_control.add_child(popup_window)

	popup_window.visible = false 
	popup_window.transient = true
	popup_window.exclusive = true

func _exit_tree():
	remove_autoload_singleton("Globals")
	remove_autoload_singleton("EventBus")
	
	if popup_window:
		popup_window.queue_free()
	if plugin:
		remove_inspector_plugin(plugin)
