# Bottleneck Heatmap

Find bottlenecks in your Kingdom Inc. workshop at a glance. Bottleneck Heatmap adds translucent overlays to machines and connecting pipes, making it easier to see which production lines are working, waiting for materials, or blocked.

## Status Colors

- **Running — Green:** A machine is processing or producing, or a pipe is transferring items.
- **Starved — Amber:** A machine is missing ingredients or resources, or a pipe is waiting for upstream supply.
- **Clogged — Red:** An output is full, an item is rejected, or a pipe connection is jammed.
- **Full Storage — Purple:** A supported storage container has reached capacity.
- **Idle — Gray:** A machine is not configured or active, or a pipe branch is empty.

## Features

- **Readable overlays:** Machine icons, recipe cards, progress bars, ports, and pipe routes remain visible.
- **Configurable appearance:** Adjust node and pipe opacity and pipe glow width in Foundation Mod Config.
- **Custom status colors:** Choose a separate base color for each state, with Base, Warm, Cool, Bright, and Deep variations.
- **Color Palette Wheel:** Select colors using a saturation/brightness area, hue strip, presets, or hex values.
- **Quick toggle:** Press **H** by default, or use the Heatmap button on the Foundation Mod Bar. The hotkey can be rebound through the game's controls and mod settings.
- **Visual-only:** The mod does not change recipes, production rules, or save data.

## Requirements

- Kingdom Inc. for Windows x64 (Unity Mono).
- [BepInEx 5](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.6).
- [Foundation](https://www.nexusmods.com/kingdominc/mods/18) is recommended for the Mod Bar button and Mod Config integration; the existing mod page describes it as optional.

## Installation

1. Install BepInEx 5 and, if desired, Foundation.
2. Close Kingdom Inc. before copying mod files.
3. Extract the mod's release ZIP into the game folder. The DLL should be at `BepInEx/plugins/BottleneckHeatmap/BottleneckHeatmap.dll`.
4. Launch Kingdom Inc., open a workshop flow diagram, and press **H** to toggle the overlay.

## Compatibility

Bottleneck Heatmap is a client-side visual diagnostic mod. It does not edit saved games or change production mechanics. The existing Nexus listing states Windows 10/11 x64 support.

## Removal

Close the game and delete `BepInEx/plugins/BottleneckHeatmap/`.

## Credits

Created by **Urakka**. [Source code on GitHub](https://github.com/urakkaamyx/KingdomInc_BottleneckHeatmap).
