using System;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Source.Flow;
using Assets.Source.Items;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomBottleneckHeatmap
{
    public sealed partial class BottleneckHeatmapPlugin : BaseUnityPlugin
    {
        void Update()
        {
            if (_isListeningForHotkey)
            {
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.escapeKey.wasPressedThisFrame)
                    {
                        StopListeningForHotkey(true);
                    }
                    else
                    {
                        foreach (var key in Keyboard.current.allKeys)
                        {
                            if (key.wasPressedThisFrame && key.keyCode != Key.None && key.keyCode != Key.Escape)
                            {
                                ToggleKeyConfig.Value = key.keyCode;
                                Config.Save();
                                SyncVanillaActionBinding();
                                StopListeningForHotkey(false);
                                break;
                            }
                        }
                    }
                }
                return;
            }

            // If native InputAction is active, it handles hotkey via performed callback
            if (Patches.HeatmapAction == null || !Patches.HeatmapAction.enabled)
            {
                if (Keyboard.current != null && ToggleKeyConfig != null)
                {
                    var keyControl = Keyboard.current[ToggleKeyConfig.Value];
                    if (keyControl != null && keyControl.wasPressedThisFrame)
                    {
                        ToggleHeatmapFromHotkey();
                    }
                }
            }

            // Periodic sweep for any nodes or pipes created outside hooked paths
            if (Time.time >= _nextSweepTime)
            {
                _nextSweepTime = Time.time + 3.0f;
                SweepAndAttachOverlays();
            }
        }
    }
}
