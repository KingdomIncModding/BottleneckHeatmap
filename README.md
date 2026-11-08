# Bottleneck Heatmap

[GitHub releases](https://github.com/urakkaamyx/KingdomInc_BottleneckHeatmap/releases) · [Nexus Mods](https://www.nexusmods.com/kingdominc/mods/21) · Version 1.0.0

Real-time diagnostic heatmap for Kingdom Inc workshops. Color-codes all workshop nodes and connecting pipes with semi-transparent overlays to instantly identify running lines, starved machines, and clogged bottlenecks.

## Features

- **Transparent Overlays:** Preserves full readability of node icons, recipe cards, progress bars, ports, and pipe routes without visual occlusion.
- **Node & Pipe State Evaluation:**
  - 🟢 **Running (Mint Green):** Actively processing recipes, producing resources, or transferring items through pipes.
  - 🟡 **Starved (Golden Amber):** Missing required recipe reagents, unfulfilled resource quotas, or pipes waiting for items from upstream.
  - 🔴 **Clogged (Crimson Coral):** Downstream output buffer full, item rejected, or jammed pipe connections.
  - ⚪ **Idle (Slate):** Unconfigured recipes, dormant machines, or empty/dead pipe branches.
- **Instant Hotkey Toggle:** Press `H` (configurable) at any time to toggle the heatmap on or off.
- **Foundation Integration:**
  - Automatically adds an aligned **Heatmap: ON / OFF** toggle button directly onto the in-game **Mod Bar**.
  - Adds full settings under **Mod Config**: adjust node overlay opacity, pipe glow opacity, pipe line width multiplier, toggle idle element visibility, and configure notifications.
- **Zero Save Footprint:** Visual-only runtime diagnostic; does not alter save files, recipe formulas, or vanilla mechanics.

## Hotkeys

- `H` — Toggle Bottleneck Heatmap overlay.

## Installation

1. Install [BepInEx 5 (Windows x64 Mono)](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.6).
2. Install [Foundation](https://www.nexusmods.com/kingdominc/mods/18) (0.5.0 or later, recommended).
3. Place `BottleneckHeatmap.dll` into `KingdomInc/BepInEx/plugins/BottleneckHeatmap/`.
4. Launch the game. Press `H` in any workshop tab to inspect your production lines!

