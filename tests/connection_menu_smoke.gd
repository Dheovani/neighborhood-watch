extends SceneTree


func _initialize() -> void:
	call_deferred("_run")


func _run() -> void:
	var role := "host"
	var tested_recovery := false
	var port := "24567"
	for arg in OS.get_cmdline_user_args():
		if arg == "--ui-role=join":
			role = "join"
		if arg == "--ui-role=recover":
			role = "recover"
		if arg == "--ui-role=shutdown":
			role = "shutdown"
		if arg.begins_with("--ui-port="):
			port = arg.substr(10)
	var packed: PackedScene = load("res://scenes/gameplay/Main.tscn")
	var main: Node3D = packed.instantiate()
	root.add_child(main)
	current_scene = main
	var menu: CanvasLayer = main.get_node("ConnectionMenu")
	var panel: Control = menu.get_node("Panel")
	var session: Node = main.get_node("NetworkSession")
	var players: Node3D = main.get_node("NetworkPlayers")
	if not panel.visible:
		_fail("Connection menu did not open by default")
		return
	var offline_player: CharacterBody3D = main.get_node("Player")
	var start_position: Vector3 = offline_player.global_position
	Input.action_press("move_forward")
	for frame in range(20):
		await physics_frame
	Input.action_release("move_forward")
	if offline_player.global_position.distance_to(start_position) > 0.1:
		_fail("Menu input moved the offline player")
		return
	var port_field: LineEdit = panel.get_node("Layout/Port")
	port_field.text = "invalid"
	panel.get_node("Layout/Host").pressed.emit()
	await process_frame
	await process_frame
	if not panel.get_node("Layout/Status").text.contains("Port must be"):
		_fail("Invalid port was not rejected in the menu")
		return
	port_field.text = port
	if role == "recover":
		tested_recovery = true
		panel.get_node("Layout/Join").pressed.emit()
		var failed := false
		var failure_deadline := Time.get_ticks_msec() + 8000
		while Time.get_ticks_msec() < failure_deadline:
			await process_frame
			if current_scene != main and current_scene != null:
				failed = true
				break
		if not failed:
			_fail("Unavailable server did not return to the connection menu")
			return
		main = current_scene
		menu = main.get_node("ConnectionMenu")
		panel = menu.get_node("Panel")
		session = main.get_node("NetworkSession")
		players = main.get_node("NetworkPlayers")
		await process_frame
		await process_frame
		var failure_status: String = panel.get_node("Layout/Status").text
		if not panel.visible or not (failure_status.contains("Connection timed out") or failure_status.contains("Connection failed")):
			_fail("Connection failure was not shown after returning to the menu")
			return
		panel.get_node("Layout/Port").text = port
		role = "host"
	panel.get_node("Layout/" + ("Host" if role == "host" else "Join")).pressed.emit()
	var connected := false
	for frame in range(720):
		await process_frame
		if main.get_multiplayer().multiplayer_peer == null:
			continue
		var local_id: int = main.get_multiplayer().get_unique_id()
		if local_id > 1 and players.has_node("Peer_" + str(local_id)):
			connected = true
			break
	await process_frame
	await process_frame
	if not connected or panel.visible:
		_fail("Menu did not join a session and hide after spawning")
		return
	if role == "shutdown":
		print("M04 shutdown client ready")
		var disconnected := false
		var disconnect_deadline := Time.get_ticks_msec() + 20000
		while Time.get_ticks_msec() < disconnect_deadline:
			await process_frame
			if current_scene != main and current_scene != null:
				disconnected = true
				break
		if not disconnected:
			_fail("Server shutdown did not return the client to the connection menu")
			return
		await process_frame
		await process_frame
		var shutdown_panel: Control = current_scene.get_node("ConnectionMenu/Panel")
		if not shutdown_panel.visible or not shutdown_panel.get_node("Layout/Status").text.contains("Server disconnected"):
			_fail("Server shutdown did not show a recoverable disconnection status")
			return
		print("M04 server shutdown recovery passed: client returned to the connection menu.")
		quit(0)
		return
	var tab := InputEventKey.new()
	tab.keycode = KEY_TAB
	tab.pressed = true
	Input.parse_input_event(tab)
	await process_frame
	if not panel.visible or not panel.get_node("Layout/Leave").visible:
		_fail("Connected player could not reopen the menu to leave")
		return
	panel.get_node("Layout/Leave").pressed.emit()
	var returned := false
	for frame in range(60):
		await process_frame
		if current_scene != main and current_scene != null:
			returned = true
			break
	if not returned or not current_scene.get_node("ConnectionMenu/Panel").visible:
		_fail("Leave did not return to the connection menu")
		return
	if role == "join":
		main = current_scene
		panel = main.get_node("ConnectionMenu/Panel")
		players = main.get_node("NetworkPlayers")
		panel.get_node("Layout/Port").text = port
		panel.get_node("Layout/Join").pressed.emit()
		var reconnected := false
		for frame in range(720):
			await process_frame
			if main.get_multiplayer().multiplayer_peer == null:
				continue
			var local_id: int = main.get_multiplayer().get_unique_id()
			if local_id > 1 and players.has_node("Peer_" + str(local_id)):
				reconnected = true
				break
		await process_frame
		await process_frame
		if not reconnected or panel.visible:
			_fail("Joining again after leaving did not spawn a local player")
			return
	print("M04 connection menu " + ("recovery" if tested_recovery else role) + " passed: port validation, connection, player spawn, leave" + (", and reconnect" if role == "join" else "") + ".")
	quit(0)


func _fail(message: String) -> void:
	push_error(message)
	quit(1)
