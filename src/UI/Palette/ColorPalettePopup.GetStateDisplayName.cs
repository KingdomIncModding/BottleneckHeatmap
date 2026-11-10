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
        string GetStateDisplayName(BottleneckState state) => state switch
        {
            BottleneckState.Running => "Running",
            BottleneckState.Starved => "Starved",
            BottleneckState.Clogged => "Clogged",
            BottleneckState.FullStorage => "Full Storage",
            BottleneckState.Idle => "Idle",
            _ => state.ToString()
        };
    }
}
