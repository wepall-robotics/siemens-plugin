@tool

extends Node

class_name Utils

static func get_unique_name(name: String, names: Array) -> String:
    var unique_name = name
    var count = 1
    while names.has(unique_name):
        unique_name = name + " " + count
        count += 1
    return unique_name
