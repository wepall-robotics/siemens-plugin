@tool

extends ConfirmationDialog

func _ready() -> void:
	EventBus.show_confirm_popup.connect(_on_show_confirm_popup)

func _on_show_confirm_popup(params: Dictionary, callback: Callable):	
	# Fill the elements with params
	title = params.title
	dialog_text = params.message
	get_ok_button().text = params.ok_text
	get_cancel_button().text = params.cancel_text
	
	# Connect buttons
	confirmed.connect(callback, CONNECT_ONE_SHOT)
	
	popup_centered()
