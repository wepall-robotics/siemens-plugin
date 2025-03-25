@tool

extends ConfirmationDialog

@export var progress_bar: ProgressBar

#region Godot Override Methods
func _ready() -> void:
	EventBus.confirm_popup_invoked.connect(_show_confirm_popup)
	EventBus.modify_content_popup_invoked.connect(_modify_content_popup)
	EventBus.close_confirm_popup.connect(hide)
#endregion

#region Private Methods

# Function to modify the content of the confirmation dialog
func _modify_content_popup(params: Dictionary, callback: Callable):
	# Fill the elements with params
	title = params.get("title", "Confirm dialog")
	dialog_text = params.get("message", "Are you sure?")

	var ok_text = params.get("ok_text", "")
	if ok_text != "":
		get_ok_button().visible = true
		get_ok_button().text = ok_text
		var ok_callback = params.get("ok_callback", func(): pass)
		# Connect confirmation button
		confirmed.connect(ok_callback, CONNECT_ONE_SHOT)
	else:
		get_ok_button().visible = false

	var cancel_text = params.get("cancel_text", "Cancel")
	if cancel_text:
		get_cancel_button().visible = true
		get_cancel_button().text = cancel_text
		var cancel_callback = params.get("cancel_callback", func(): pass)
		canceled.connect(cancel_callback, CONNECT_ONE_SHOT)
	else:
		get_cancel_button().visible = false

	if params.get("progress", false):
		progress_bar.visible = true
		callback.call()
	else:
		progress_bar.visible = false	

# Function to show the confirmation dialog
func _show_confirm_popup(params: Dictionary, callback: Callable):
	_modify_content_popup(params, callback)
	if is_inside_tree():
		popup_centered()
		
#endregion
