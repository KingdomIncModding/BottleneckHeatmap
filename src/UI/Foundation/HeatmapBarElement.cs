using System;
using System.Collections.Generic;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    internal sealed class HeatmapBarElement : ModBarElement
    {
        public override IModBarView CreateView(IModBarContext context)
        {
            var view = new HeatmapBarView(_plugin, context);
            FoundationIntegration.SetActiveView(view);
            return view;
        }

        readonly BottleneckHeatmapPlugin _plugin;
        public HeatmapBarElement(BottleneckHeatmapPlugin plugin) : base(BottleneckHeatmapPlugin.Id, "heatmap_toggle", 160, 20, ModBarAlignment.Right)
        {
            _plugin = plugin;
        }
    }
}
