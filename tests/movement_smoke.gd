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
	var camera: Camera3D = player.get_node("CameraYaw/CameraPitch/SpringArm3D/Camera3D")
	await physics_frame
	await physics_frame

	var start_z: float = player.global_position.z
	Input.action_press("move_forward")
	for frame in range(20):
		await physics_frame
	Input.action_release("move_forward")
	if player.global_position.z >= start_z - 0.5:
		_fail("Move forward input did not move the player")
		return

	for frame in range(20):
		await physics_frame
	if not player.is_on_floor():
		_fail("Player did not settle on the ground")
		return

	var start_yaw: float = player.get_node("CameraYaw").rotation.y
	player.ApplyMouseLook(Vector2(40, 0))
	if is_equal_approx(start_yaw, player.get_node("CameraYaw").rotation.y):
		_fail("Mouse motion did not rotate the camera")
		return
	if not camera.current:
		_fail("Follow camera is not current")
		return

	print("M01 movement smoke passed: movement, ground, camera.")
	quit(0)


func _fail(message: String) -> void:
	push_error(message)
	quit(1)
