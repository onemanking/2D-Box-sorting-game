# Big Frogg Box Sorting

This is a simple Unity game where you watch a frog character automatically sort different types of boxes into the correct areas.

## What it does

The NPC will moving around the scene looking for boxes that fall from above. When it finds one, it will pick it up and carries it to the matching sorting area based on box type. Red type boxes go to red areas, blue type boxes go to blue areas.

## Prerequisites

- Unity 6.0 (6000.0.53f1)

## Features

- Boxes spawn randomly at intervals
- NPC has a simple state machine (Idle, Search, Found, Collect, Sorting)
- Each Sorting area only accept certain types of boxes
- Sorted boxes stack up top each other in each area
- Easy to add new types of boxes and areas using ScriptableObjects

## Implementation Details

This project uses a data-driven approach with ScriptableObjects to make it extremely easy to add new content:

### Adding New Box Types

1. Add the new box type to the `BoxType` enum in `BoxData.cs` (this is the only code change needed)
2. Create a new `BoxData` ScriptableObject
3. Set the `BoxType` enum value, color, and prefab reference
4. Add it to the `BoxSpawnConfigData` array to include it in spawning

### Setting Up Sorting Areas

1. Create a `SortingAreaData` ScriptableObject
2. Add the accepted `BoxData` references to the AcceptedBoxes
3. Assign this data to a SortingArea component in the scene

### NPC Behavior Configuration

- `NPCBaseStateConfigData` controls all timing and speed values for the npc's behavior
- Easily adjust idle times, movement speeds, and state transitions without touching code

## Architecture

The system is built with:

- **State Machine**: Clean separation of NPC behaviors (Idle, Search, Found, Collect, Sorting)
- **ScriptableObjects**: Data-driven configuration for easy content creation
- **Component-based**: Modular design for boxes, sorting areas, and NPC behavior

Almost no code changes needed to add new box types or sorting rules - just update the enum once and create new ScriptableObject assets
