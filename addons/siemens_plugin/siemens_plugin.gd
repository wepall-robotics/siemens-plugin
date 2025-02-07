@tool
extends EditorPlugin

var panel: Control

#region Private Methods
# Function to add a control to the bottom panel once the plugin is enabled
func _enable_plugin():
	add_autoload_singleton("EventBus", "res://addons/siemens_plugin/scripts/event_bus.gd")
	# wait for autoload to be ready
	await get_tree().physics_frame
	add_autoload_singleton("PlcsController", "res://addons/siemens_plugin/scripts/PlcsController.cs")
	await get_tree().physics_frame
	panel = load("res://addons/siemens_plugin/scenes/main.tscn").instantiate()
	add_control_to_bottom_panel(panel, "Siemens Manager")

# Function to remove the control from the bottom panel once the plugin is disabled
func _disable_plugin():
	remove_autoload_singleton("EventBus")
	remove_autoload_singleton("PlcsController")

	if panel:
		remove_control_from_bottom_panel(panel)
		panel.queue_free()
		panel = null
#endregion
