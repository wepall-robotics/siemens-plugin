@tool
extends Node

# Utils: A utility class providing helper functions for common operations.

class_name Utils

# Static Methods

## Generates a unique name by appending a number to the base name if it already exists in the provided list.
# @param name The base name to check for uniqueness.
# @param names An array of existing names to compare against.
# @return A unique name that does not exist in the provided list.
static func get_unique_name(name: String, names: Array) -> String:
    var unique_name = name
    var count = 1
    while names.has(unique_name):
        unique_name = name + " " + str(count)
        count += 1
    return unique_name