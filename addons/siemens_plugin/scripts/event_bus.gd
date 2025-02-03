@tool
extends Node

signal plc_selected(plc_data : PlcData)
signal plc_deselect()
signal plc_added(plc_data: PlcData)
signal plc_removed(plc_data: PlcData)
signal plc_updated(plc_data: PlcData, property: String)