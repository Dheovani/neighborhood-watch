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
	await physics_frame
	await physics_frame

	var neighborhood: Node3D = main.get_node("Neighborhood")
	var ground: StaticBody3D = neighborhood.get_node("Ground")
	var player: CharacterBody3D = main.get_node("Player")
	var space: PhysicsDirectSpaceState3D = main.get_world_3d().direct_space_state
	var landmarks: Array[String] = [
		"Sites/NorthHouseWest/Entrance",
		"Sites/NorthHouseCenter/Entrance",
		"Sites/NorthHouseEast/Entrance",
		"Sites/SouthHouseWest/Entrance",
		"Sites/SouthHouseCenter/Entrance",
		"Sites/Market/Entrance",
		"Sites/Market/FutureCheckout",
		"Sites/GasStation/Entrance",
		"Sites/GasStation/BathroomEntrance",
		"Sites/Park/Entrance",
		"Sites/Park/FutureDogWalk",
	]
	for path in landmarks:
		if not neighborhood.has_node(path):
			_fail("Missing landmark: " + path)
			return
		var marker: Marker3D = neighborhood.get_node(path)
		if not _has_ground(space, ground, marker.global_position):
			_fail("No walkable ground at " + path)
			return

	var routes: Array[PackedVector3Array] = [
		PackedVector3Array([Vector3(-8, 0, 8), Vector3(31, 0, 8), Vector3(31, 0, -8), Vector3(31, 0, -18)]),
		PackedVector3Array([Vector3(31, 0, 8), Vector3(35, 0, 15.5), Vector3(35, 0, 27.5)]),
		PackedVector3Array([Vector3(-8, 0, 8), Vector3(-20, 0, 8), Vector3(-20, 0, 17.5)]),
		PackedVector3Array([Vector3(-8, 0, 8), Vector3(-40, 0, 8), Vector3(-40, 0, -8), Vector3(-40, 0, -17.5)]),
		PackedVector3Array([Vector3(-8, 0, 8), Vector3(12, 0, 8), Vector3(12, 0, 21), Vector3(2.6, 0, 21)]),
	]
	for route in routes:
		for i in range(route.size()):
			if not _has_ground(space, ground, route[i]):
				_fail("No ground on route at " + str(route[i]))
				return
			if i > 0 and not _is_clear(space, player, route[i - 1], route[i]):
				_fail("Route blocked between " + str(route[i - 1]) + " and " + str(route[i]))
				return
	if not _is_clear(space, player, Vector3(31, 0.5, -11), Vector3(35, 0.5, 11)):
		_fail("Market-to-park sightline is blocked")
		return

	var house: StaticBody3D = neighborhood.get_node("Sites/NorthHouseWest")
	var wall_hit: Dictionary = space.intersect_ray(PhysicsRayQueryParameters3D.create(Vector3(-40, 2, -18), Vector3(-40, 2, -25)))
	if wall_hit.get("collider") != house:
		_fail("House wall has no collision")
		return
	var boundary: StaticBody3D = neighborhood.get_node("Boundaries")
	var edge_hit: Dictionary = space.intersect_ray(PhysicsRayQueryParameters3D.create(Vector3(53, 1, 0), Vector3(57, 1, 0)))
	if edge_hit.get("collider") != boundary:
		_fail("Neighborhood boundary has no collision")
		return
	var car: StaticBody3D = neighborhood.get_node("StreetProps/ParkedCarWest")
	var car_hit: Dictionary = space.intersect_ray(PhysicsRayQueryParameters3D.create(Vector3(-35, 0.8, 4.5), Vector3(-27, 0.8, 4.5)))
	if car_hit.get("collider") != car:
		_fail("Parked car has no collision")
		return
	var pump: StaticBody3D = neighborhood.get_node("Sites/GasStation/PumpLeft")
	var pump_hit: Dictionary = space.intersect_ray(PhysicsRayQueryParameters3D.create(Vector3(6.7, 0.9, 16), Vector3(6.7, 0.9, 20)))
	if pump_hit.get("collider") != pump:
		_fail("Gas pump has no collision")
		return

	print("M02 neighborhood smoke passed: landmarks, ground, routes, sightline, and solid collision.")
	quit(0)


func _has_ground(space: PhysicsDirectSpaceState3D, ground: StaticBody3D, point: Vector3) -> bool:
	var ray: PhysicsRayQueryParameters3D = PhysicsRayQueryParameters3D.create(point + Vector3.UP * 1.8, point + Vector3.DOWN * 1.8)
	var hit: Dictionary = space.intersect_ray(ray)
	return hit.get("collider") == ground


func _is_clear(space: PhysicsDirectSpaceState3D, player: CharacterBody3D, start: Vector3, finish: Vector3) -> bool:
	var ray: PhysicsRayQueryParameters3D = PhysicsRayQueryParameters3D.create(start + Vector3.UP, finish + Vector3.UP)
	ray.exclude = [player.get_rid()]
	return space.intersect_ray(ray).is_empty()


func _fail(message: String) -> void:
	push_error(message)
	quit(1)
