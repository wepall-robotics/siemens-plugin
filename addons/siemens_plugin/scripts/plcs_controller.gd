@tool
extends Node

enum Status {
	CONNECTED,
	DISCONNECTED,
	UNKNOWN
}

# Base name for PLC items
const BASE_NAME = "Plc"

var plcs: Array[PlcData] = []:
	get:
		return plcs

# Function to create a new PLC item
func create_plc() -> PlcData:
	# Initialize plc.
	var new_plc = PlcData.new()
	new_plc.Name = _generate_unique_name()
	plcs.append(new_plc)
	# Emit an event indicating a new PLC has been added.
	EventBus.plc_added.emit(new_plc)

	return new_plc

# Function to clear the list of PLC items
func clear_plcs() -> void:
	plcs.clear()

const MAX_PING_ATTEMPTS = 3
const PING_TIMEOUT_MS = 1000  # 1 segundo por intento

func ping_plc(selected_plc: PlcData) -> void:
	var thread = Thread.new()
	thread.start(Callable(self, "_ping_thread").bind(selected_plc))
	thread.wait_to_finish()

func _ping_thread(selected_plc: PlcData) -> void:
	var success = false
	
	for attempt in range(MAX_PING_ATTEMPTS):
		if attempt > 0:
			OS.delay_msec(PING_TIMEOUT_MS)
		
		success = selected_plc.Ping()
		if success:
			break
			
		call_deferred("_on_ping_attempt_failed", selected_plc, attempt + 1)
	
	call_deferred("_on_ping_completed", selected_plc, success)

func _on_ping_attempt_failed(plc_data: PlcData, attempt: int) -> void:
	EventBus.ping_attempt_failed.emit(plc_data, attempt, MAX_PING_ATTEMPTS)


func _on_ping_completed(plc_data: PlcData, success: bool) -> void:
	if plc_data:  # Verificar que plc_data no sea null
		print("Ping completed ", plc_data.Name, " ", success)
		EventBus.ping_completed.emit(plc_data, success)


# Function to remove a Plc item
func remove_plc(plc: PlcData) -> bool:
	if plcs.has(plc):
		plcs.erase(plc)
		EventBus.plc_removed.emit(plc)
		return true
	
	return false

func update_plc_name(plc: PlcData, new_name: String) -> String:
	if plcs.has(plc):
		if not _plc_name_exists(new_name):
			plc.Name = new_name
		else:
			var unique_name = _generate_unique_name(new_name)
			plc.Name = unique_name
		
		EventBus.plc_updated.emit(plc, "Name")
		
		return plc.Name
	
	return "Invalid plc"

func update_plc_type(plc: PlcData, cpu_type: int) -> void:
	if plcs.has(plc):
		plc.Type = cpu_type
		EventBus.plc_updated.emit(plc, "Type")

# Function to update the IP address of a PLC item
func update_plc_ip(plc: PlcData, new_ip: String) -> bool:
	if plcs.has(plc):
		if not validate_ip_address(new_ip):
			return false
		
		plc.IPAddress = new_ip
		EventBus.plc_updated.emit(plc, "IPAddress")

		return true
	return false

# Function to update the rack number of a PLC item
func update_plc_rack(plc: PlcData, new_rack: float) -> float:
	if plcs.has(plc):
		plc.Rack = new_rack
		EventBus.plc_updated.emit(plc, "Rack")
		return plc.Rack
	return -1

# Function to update the slot number of a PLC item
func update_plc_slot(plc: PlcData, new_slot: float) -> float:
	if plcs.has(plc):
		plc.Slot = new_slot
		EventBus.plc_updated.emit(plc, "Slot")
		return plc.Slot
	return -1

# Function to generate a unique name for a PLC item
func _generate_unique_name(base_name: String = BASE_NAME) -> String:
	var unique_name = base_name
	var counter = 1
	# Loop until a unique name is found
	while _plc_name_exists(unique_name):
		unique_name = base_name + " " + str(counter)
		counter += 1
	return unique_name

# Function to check if a PLC with the given name already exists
func _plc_name_exists(name: String) -> bool:
	return plcs.any(func(plc): return plc.Name == name)

# Function to validate an IP address
func validate_ip_address(ip_address: String) -> bool:
	var regex = RegEx.new()
	regex.compile("^(\\d{1,3}\\.){3}\\d{1,3}$")
	
	if regex.search(ip_address):
		# Split the IP address into octets
		var octets = ip_address.split(".")
		for octet in octets:
			var num = octet.to_int()
			if num < 0 or num > 255:
				return false
		return true
	
	return false
