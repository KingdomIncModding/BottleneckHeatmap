# Bottleneck Heatmap

[GitHub releases](https://github.com/urakkaamyx/KingdomInc_BottleneckHeatmap/releases) · [Nexus Mods](https://www.nexusmods.com/kingdominc/mods/21) · Version 1.2.6

Real-time diagnostic heatmap for Kingdom Inc workshops. Color-codes all workshop nodes and connecting pipes with semi-transparent overlays to instantly identify running lines, starved machines, clogged bottlenecks, and full storage containers.

## Features

- **Transparent Overlays:** Preserves full readability of node icons, recipe cards, progress bars, ports, and pipe routes without visual occlusion.
- **Node & Pipe State Evaluation:**
  - 🟢 **Running (Mint Green):** Actively processing recipes, producing resources, or transferring items through pipes.
  - 🟡 **Starved (Golden Amber):** Missing required recipe reagents, unfulfilled resource quotas, or pipes waiting for items from upstream.
  - 🔴 **Clogged (Crimson Coral):** Downstream output buffer full, item rejected, or jammed pipe connections.
  - 🟣 **Full Storage (Royal Purple):** Storage containers (Stockpiles and global Armories) that have reached maximum capacity.
  - ⚪ **Idle (Slate):** Unconfigured recipes, dormant machines, or empty/dead pipe branches.
- **Dynamic Color Variations Per State:**
  - Select base colors individually for each state: Running, Starved, Clogged, Full Storage, and Idle.
  - Automatically generates 5 surrounding hue variations (Base, Warm, Cool, Bright, Deep) selectable directly from Foundation ModConfig dropdowns or interactive cards in the Color Palette Wheel.
- **Color Palette Generator Wheel:**
  - Select any color using interactive Hue and Saturation/Brightness surfaces, preset swatches, or hex code.
  - Generates and previews surrounding hue variations for the selected state.
  - Accessible via Foundation Mod Config or by **Right-Clicking** the Heatmap button on the Mod Bar!
- **Foundation Mod Config Integration:**
  - **Percent Sliders** using Foundation's native `ModConfigSlider` (`section.AddSlider`) for Node Overlay Opacity (10%–100%) and Pipe Overlay Opacity (20%–100%).
  - **Width Slider** for Pipe Glow Width (1.0x–2.5x).
  - **Color Customization:** Dedicated color pickers and dynamic variation dropdowns for each state.
  - **Vanilla Hotkey Setup:** Interactive key rebinding button in Mod Config, also integrated into Kingdom Inc's native Key Bindings options.
- **Zero Save Footprint:** Visual-only runtime diagnostic; does not alter save files, recipe formulas, or vanilla mechanics.

## Hotkeys

- `H` — Toggle Bottleneck Heatmap overlay.

## Installation

1. Install [BepInEx 5 (Windows x64 Mono)](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.6).
2. Install [Foundation](https://www.nexusmods.com/kingdominc/mods/18) (0.5.0 or later, recommended).
3. Place `BottleneckHeatmap.dll` into `KingdomInc/BepInEx/plugins/BottleneckHeatmap/`.
4. Launch the game. Press `H` in any workshop tab to inspect your production lines!

