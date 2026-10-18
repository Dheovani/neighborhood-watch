extends SceneTree


func _initialize() -> void:
	call_deferred("_run")


func _run() -> void:
	var scene: PackedScene = load("res://scenes/gameplay/Main.tscn")
	if scene == null:
		_fail("Main scene did not load")
		return

	var main: Node3D = scene.instantiate()
	root.add_child(main)
	main.get_node("ConnectionMenu/Panel/Layout/Offline").pressed.emit()
	var player: CharacterBody3D = main.get_node("Player")
	var spots: Node3D = main.get_node("MissionSpots")
	var tasks: Label = main.get_node("MissionHud/Tasks")
	var prompt: Label = main.get_node("MissionHud/Prompt")
	await physics_frame
	await physics_frame
	if not tasks.text.contains("0/3") or not tasks.text.contains("Points: 0"):
		_fail("Missions did not start incomplete with zero points")
		return
	Input.action_press("interact")
	await physics_frame
	Input.action_release("interact")
	await physics_frame
	if not tasks.text.contains("0/3") or not tasks.text.contains("Points: 0"):
		_fail("Interaction away from a mission spot awarded progress")
		return

	var names: Array[String] = ["ParkLitter", "NeighborMail", "MarketSnack"]
	for index in range(names.size()):
		var spot: Node3D = spots.get_node(names[index])
		player.global_position = spot.global_position + Vector3(0, 0.05, 1.65)
		player.velocity = Vector3.ZERO
		await physics_frame
		await physics_frame
		if prompt.text.is_empty():
			_fail("No interaction prompt at " + names[index])
			return
		Input.action_press("interact")
		await physics_frame
		Input.action_release("interact")
		await physics_frame
		if not tasks.text.contains(str(index + 1) + "/3"):
			_fail("Interaction did not complete " + names[index])
			return

	if not tasks.text.contains("Points: 40"):
		_fail("Completed sample missions did not award configured points")
		return

	print("M03 mission smoke passed: three reachable, prompted, completed infractions.")
	quit(0)


func _fail(message: String) -> void:
	push_error(message)
	quit(1)
