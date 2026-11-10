using System;
using System.Collections.Generic;
using Assets.Behaviour.UI;
using Assets.Source.Util;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    internal sealed partial class HeatmapBarView : IModBarView
    {
        string GetLabel() => _plugin.IsHeatmapEnabled ? "Heatmap: ON" : "Heatmap: OFF";
    }
}
