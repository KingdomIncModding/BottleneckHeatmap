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
    internal sealed partial class HeatmapBarElement : ModBarElement
    {
        readonly BottleneckHeatmapPlugin _plugin;


        public HeatmapBarElement(BottleneckHeatmapPlugin plugin)
            : base(BottleneckHeatmapPlugin.Id, "heatmap_toggle", 160, 20, ModBarAlignment.Right)
        {
            _plugin = plugin;
        }
    }
}
