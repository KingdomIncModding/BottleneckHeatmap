# Bottleneck Heatmap

**Current release: 1.2.7 — for Kingdom Inc game version 1.0.14.**

[GitHub releases](https://github.com/KingdomIncModding/KingdomInc_BottleneckHeatmap/releases) · [Nexus Mods](https://www.nexusmods.com/kingdominc/mods/21) · Version 1.2.7

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
  - Automatically generates 5 surrounding hue variations (Base, Warm, Cool, Bright, Deep) selectable directly from Foundation Mod Config dropdowns or interactive cards in the Color Palette Wheel.
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

- `H` — Toggle Bottleneck Heatmap overlay on/off (rebindable in Controls or Foundation Mod Config).

## Installation

1. Install [BepInEx 5 (Windows x64 Mono)](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.6).
2. Install [Foundation](https://www.nexusmods.com/kingdominc/mods/18) (0.14.0 or newer).
3. Place `BottleneckHeatmap.dll` into `KingdomInc/BepInEx/plugins/BottleneckHeatmap/`.
4. Launch the game. Press `H` in any workshop tab to inspect your production lines!

## Compatibility

| Class.Method | Hook Type | Impact |
|---|---|---|
| WorldFlowElement.SetElement | Foundation hook (postfix) | Attaches diagnostic overlay to newly created or loaded workshop nodes. |
| WorldFlowPipe.SetFlowPipe | Foundation hook (postfix) | Attaches diagnostic overlay to newly created or loaded pipes. |
| WorldFlowPipe.UpdateLinePositions | Foundation hook (postfix) | Updates pipe overlay geometry when pipes are bent or rerouted. |
| WorldFlowPipe.ItemTransferred | Foundation hook (postfix) | Tracks active throughput for pipe running status. |
| WorldFlowPipe.ItemRejected | Foundation hook (postfix) | Detects blocked downstream inputs and full buffers to flag clogged pipe status. |
| WorldFlowManager.ShowFlow | Foundation hook (postfix) | Refreshes node and pipe overlays when switching workshop tabs. |
| PlayerInput.Awake | Foundation hook (postfix) | Registers the configurable Bottleneck Heatmap toggle action into Kingdom Inc's native input rebinding system. |
| KeyBindingRow.SetBinding | Foundation hook (postfix) | Displays "Bottleneck Heatmap" as the human-readable label in the in-game key binding menu. |
