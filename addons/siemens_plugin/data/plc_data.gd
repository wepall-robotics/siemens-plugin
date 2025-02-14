@tool
class_name PlcData
extends Resource

## Types of S7 CPU supported by the library
enum CpuType {
	## S7-200 CPU type
	S7_200 = 0,
	## Siemens Logo 0BA8
	LOGO_0BA8 = 1,
	## S7-200 Smart
	S7_200_SMART = 2,
	## S7-300 CPU type
	S7_300 = 10,
	## S7-400 CPU type
	S7_400 = 20,
	## S7-1200 CPU type
	S7_1200 = 30,   
	## S7-1500 CPU type
	S7_1500 = 40
}

## The IP address of the [b]PLC[b/].
@export var ip_address: String:
	get:
		return ip_address
	set(value):
		ip_address = value
		emit_changed()

## The type of the CPU used by the [b]PLC[/b].
@export_enum("S7-200:0", "LOGO-0BA8:1", "S7-200-SMART:2", "S7-300:10", "S7-400:20", "S7-1200:30", "S7_1500:40")
var type: int:
	get:
		return type
	set(value):
		type = value

## The rack number where the [b]PLC[/b] is located.
## Valid range: [color=#70bafa][b]0[/b] - [b]3[/b][/color]
@export_range(0, 3) var rack: int:
	get:
		return rack
	set(value):
		rack = value

## The slot number where the [b]PLC[/b] is located.
## Valid range: [color=#70bafa][b]0[/b] - [b]31[/b][/color]
@export_range(0, 31) var slot: int:
	get:
		return slot
	set(value):
		slot = value
