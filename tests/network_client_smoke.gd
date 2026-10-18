extends SceneTree


func _initialize() -> void:
	call_deferred("_run")


func _run() -> void:
	var packed: PackedScene = load("res://scenes/gameplay/Main.tscn")
	if packed == null:
		_fail("Main scene did not load")
		return
	var main: Node3D = packed.instantiate()
	root.add_child(main)
	var players: Node3D = main.get_node("NetworkPlayers")
	var session: Node = main.get_node("NetworkSession")
	var tasks: Label = main.get_node("MissionHud/Tasks")
	var feedback: Label = main.get_node("MissionHud/Feedback")
	var local_id: int = 0
	for frame in range(300):
		await physics_frame
		local_id = main.get_multiplayer().get_unique_id()
		if local_id > 1 and players.has_node("Peer_" + str(local_id)) and players.get_child_count() == 2:
			break
	if local_id <= 1 or players.get_child_count() != 2:
		_fail("Two network players did not spawn")
		return

	var role: String = "observer"
	for arg in OS.get_cmdline_user_args():
		if arg == "--smoke-role=actor":
			role = "actor"

	if role == "actor":
		session.rpc_id(1, "SubmitInteraction", "ParkLitter")
		if not await _wait_for_text(feedback, "Mission completed", 120):
			_fail("Server did not confirm a valid nearby mission interaction")
			return
		if not tasks.text.contains("1/3"):
			_fail("Mission completion was not reflected on the client")
			return
		var paper: MeshInstance3D = main.get_node("MissionSpots/ParkLitter/Paper")
		for frame in range(60):
			await physics_frame
			if paper.visible:
				break
		if not paper.visible:
			_fail("Validated paper drop did not change the replicated world")
			return
		session.rpc_id(1, "SubmitInteraction", "ParkLitter")
		if not await _wait_for_text(feedback, "Action rejected: AlreadyCompleted", 120):
			_fail("Repeated use of the same one-time spot was not rejected")
			return
		if not tasks.text.contains("1/3"):
			_fail("Repeated action duplicated mission progress")
			return
		var player: CharacterBody3D = players.get_node("Peer_" + str(local_id))
		var start_z: float = player.global_position.z
		Input.action_press("move_forward")
		for frame in range(45):
			await physics_frame
		Input.action_release("move_forward")
		for frame in range(15):
			await physics_frame
		if player.global_position.z >= start_z - 1.0:
			_fail("Server movement snapshots did not move the local player")
			return
		for frame in range(120):
			await physics_frame
		print("M04 actor smoke passed: connected, validated mission, duplicate rejection, server movement.")
	else:
		var other_player: CharacterBody3D = null
		for child in players.get_children():
			if child.name != "Peer_" + str(local_id):
				other_player = child
		if other_player == null:
			_fail("Remote player was not spawned")
			return
		var remote_start: Vector3 = other_player.global_position
		session.rpc_id(1, "SubmitInteraction", "MarketSnack")
		if not await _wait_for_text(feedback, "Action rejected: TooFar", 120):
			_fail("Server did not reject a distant interaction")
			return
		if not tasks.text.contains("0/2"):
			_fail("Rejected interaction changed mission progress")
			return
		session.rpc_id(1, "SubmitInteraction", "BogusSpot")
		if not await _wait_for_text(feedback, "Action rejected: UnknownMission", 120):
			_fail("Server did not reject an unknown target")
			return
		for frame in range(90):
			await physics_frame
		if not is_instance_valid(other_player):
			_fail("Remote player disconnected before movement could be checked")
			return
		if other_player.global_position.distance_to(remote_start) < 0.5:
			_fail("Remote player movement was not replicated")
			return
		var player: CharacterBody3D = players.get_node("Peer_" + str(local_id))
		Input.action_press("move_right")
		for frame in range(45):
			await physics_frame
		Input.action_release("move_right")
		for frame in range(15):
			await physics_frame
		var park: Node3D = main.get_node("MissionSpots/ParkLitter")
		if player.global_position.distance_to(park.global_position) > 2.6:
			_fail("Server movement did not bring the observer near the park")
			return
		session.rpc_id(1, "SubmitInteraction", "ParkLitter")
		if not await _wait_for_text(feedback, "mission unchanged (UnknownMission)", 120):
			_fail("A valid action without an assignment was not recorded")
			return
		if not tasks.text.contains("0/2"):
			_fail("Spontaneous action awarded unassigned mission progress")
			return
		var state_label: Label3D = park.get_node("ParkLabel")
		for frame in range(60):
			await physics_frame
			if state_label.text.contains("(2)"):
				break
		if not state_label.text.contains("(2)"):
			_fail("Second validated action did not update shared park state")
			return
		Input.action_press("move_left")
		for frame in range(24):
			await physics_frame
		Input.action_release("move_left")
		var market: Node3D = main.get_node("MissionSpots/MarketSnack")
		Input.action_press("move_forward")
		var reached_market := false
		for frame in range(650):
			await physics_frame
			if player.global_position.distance_to(market.global_position) <= 2.3:
				reached_market = true
				break
		Input.action_release("move_forward")
		if not reached_market:
			_fail("Second player could not reach the market using server-owned movement")
			return
		session.rpc_id(1, "SubmitInteraction", "MarketSnack")
		if not await _wait_for_text(feedback, "Mission completed", 120):
			_fail("Second player could not complete an assigned market mission")
			return
		if not tasks.text.contains("1/2"):
			_fail("Second player's mission progress was not updated")
			return
		var stock_label: Label3D = market.get_node("SnackLabel")
		for frame in range(60):
			await physics_frame
			if stock_label.text.contains("7 left"):
				break
		if not stock_label.text.contains("7 left"):
			_fail("Server-owned market stock was not replicated after use")
			return
		print("M04 observer smoke passed: remote movement, spontaneous infraction, own mission, and shared stock.")
	quit(0)


func _wait_for_text(label: Label, expected: String, frames: int) -> bool:
	for frame in range(frames):
		await physics_frame
		if label.text.contains(expected):
			return true
	return false


func _fail(message: String) -> void:
	push_error(message)
	quit(1)
