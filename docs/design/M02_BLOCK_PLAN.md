# M02 block plan

**Status:** Implemented as a primitive-geometry prototype. The player reported the first visible playtest working on 2026-10-17. Detailed route and art review remains. Coordinates are provisional tuning values, not final level dimensions.

## Footprint and circulation

The walkable ground is about 112 m east-west by 88 m north-south. One two-way street runs east-west through the center (`z = 0`), with sidewalks on both sides and a crosswalk near `x = 31`. Green perimeter hedges with collision mark the current map boundary. North is negative Z in the scene.

```text
                       NORTH (-Z)
  west (-X)                                              east (+X)
  +-------------------------------------------------------------+
  | house     house     house              market               |
  | gardens / front paths / north sidewalk                     |
  |================= road ======== crosswalk ===================|
  | south sidewalk / front paths                                |
  | house     house          gas station      park               |
  +-------------------------------------------------------------+
                       SOUTH (+Z)
```

The five houses use one scene. The market, gas station, park, trees, and parked cars are separate reusable scenes. Building shells have exterior collision; interiors are closed for this prototype. `Marker3D` nodes identify future entrances and possible mission locations, but have no interaction or scoring behavior.

## Gameplay readability

- The market sign, gas canopy, crosswalk, park trees, and contrasting roofs are orientation landmarks. Houses and shops are mixed into one ordinary U.S. neighborhood without a socioeconomic visual cue.
- The sidewalk and crosswalk form a direct public route from the park to the market. Front paths connect houses to sidewalks. The park has two crossing paths and a bench, allowing more than one viewing position.
- Cars, tree trunks, house corners, and the gas pumps create partial cover. The central crosswalk and sidewalks retain open sightlines that can later support observation. No evidence or visibility rules exist yet.
- The market checkout and gas station bathroom markers are outside closed shells. M03 must decide whether to add interiors or adapt those mission examples; neither mission is implemented here.

## Art and asset assessment

The current prototype uses built-in Godot meshes and materials, so no paid asset is needed to test layout or movement. The [Synty POLYGON Town Pack](https://syntystore.com/products/polygon-town-pack) lists houses, vehicles, props, a Godot 4.5.1 project, and FBX source files. It is a plausible visual source, but import and material behavior in this project's Godot 4.7.2 .NET version remain unverified until the files are available. [Godot recommends glTF/GLB for 3D import and also supports FBX](https://docs.godotengine.org/en/stable/tutorials/assets_pipeline/importing_3d_scenes/available_formats.html). Test a small subset and its license terms before replacing the placeholder scenes.

## Known limits and review

The level is a single rectangular block without interiors, fences between lots, traffic, mission interactions, or finished art. Automated checks cover scene loading, landmarks, sampled routes, ground, and major collisions. The first visible playtest worked; more detailed camera, route, and visual scale feedback can guide later M02 refinement.
