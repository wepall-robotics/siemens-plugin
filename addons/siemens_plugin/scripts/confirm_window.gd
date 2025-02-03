@tool

extends ConfirmationDialog

@export var progress_bar: ProgressBar

func _ready() -> void:
	EventBus.show_confirm_popup.connect(_on_show_confirm_popup)
	EventBus.close_confirm_popup.connect(hide)

func _on_show_confirm_popup(params: Dictionary, callback: Callable):	
	# Fill the elements with params
	title = params.get("title", "Confirm dialog")
	dialog_text = params.get("message", "Are you sure?")

	var ok_text = params.get("ok_text", "")
	if ok_text != "":
		get_ok_button().visible = true
		get_ok_button().text = ok_text
		# Connect confirmation button
		confirmed.connect(callback, CONNECT_ONE_SHOT)
	else:
		get_ok_button().visible = false

	var cancel_text = params.get("cancel_text", "Cancel")
	if cancel_text:
		get_cancel_button().visible = true
		get_cancel_button().text = cancel_text
	else:
		get_cancel_button().visible = false

	if params.get("progress", false):
		progress_bar.visible = true
		callback.call()
	else:
		progress_bar.visible = false
	
	
	popup_centered()
