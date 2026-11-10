using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Behaviour.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    public sealed partial class ColorPalettePopup : MonoBehaviour
    {
        void ApplyCurrentColor()
        {
            var plugin = BottleneckHeatmapPlugin.Instance;
            if (plugin != null)
            {
                Color baseColor = Color.HSVToRGB(_baseHue, Mathf.Clamp01(_baseSaturation), Mathf.Clamp01(_baseValue));
                string hex = "#" + ColorUtility.ToHtmlStringRGB(baseColor);

                switch (_currentState)
                {
                    case BottleneckState.Running:
                        plugin.RunningColorConfig.Value = hex;
                        FoundationIntegration.RunningVariations = _currentGeneratedVariations;
                        FoundationIntegration.UpdateDropdownOptions(BottleneckState.Running, _currentGeneratedVariations);
                        break;
                    case BottleneckState.Starved:
                        plugin.StarvedColorConfig.Value = hex;
                        FoundationIntegration.StarvedVariations = _currentGeneratedVariations;
                        FoundationIntegration.UpdateDropdownOptions(BottleneckState.Starved, _currentGeneratedVariations);
                        break;
                    case BottleneckState.Clogged:
                        plugin.CloggedColorConfig.Value = hex;
                        FoundationIntegration.CloggedVariations = _currentGeneratedVariations;
                        FoundationIntegration.UpdateDropdownOptions(BottleneckState.Clogged, _currentGeneratedVariations);
                        break;
                    case BottleneckState.FullStorage:
                        plugin.FullStorageColorConfig.Value = hex;
                        FoundationIntegration.FullStorageVariations = _currentGeneratedVariations;
                        FoundationIntegration.UpdateDropdownOptions(BottleneckState.FullStorage, _currentGeneratedVariations);
                        break;
                    case BottleneckState.Idle:
                        plugin.IdleColorConfig.Value = hex;
                        FoundationIntegration.IdleVariations = _currentGeneratedVariations;
                        FoundationIntegration.UpdateDropdownOptions(BottleneckState.Idle, _currentGeneratedVariations);
                        break;
                }

                plugin.Config.Save();
                plugin.UpdateMaterialColors();
                plugin.RefreshAllOverlays();

                try
                {
                    string stateName = GetStateDisplayName(_currentState);
                    var msg = Assets.Behaviour.UI.UIStatusMessage.Show(
                        $"Bottleneck Heatmap: Updated {stateName} Color",
                        "Icons_17",
                        false
                    );
                    if (msg != null && msg.StatusText != null)
                    {
                        msg.StatusText.text = $"Bottleneck Heatmap: Updated {stateName} (<color={hex}>{hex}</color>)";
                    }
                }
                catch { }
            }

            Close();
        }
    }
}
