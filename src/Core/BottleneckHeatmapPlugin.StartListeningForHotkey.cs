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
        public void StartListeningForHotkey()
        {
            _isListeningForHotkey = true;
            FoundationIntegration.UpdateHotkeyButtonText("[ Press Any Key... ]");
        }
    }
}
