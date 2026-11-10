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
    internal sealed partial class HeatmapLegendTooltip : MonoBehaviour
    {
        void UpdateColors()
        {
            if (_swatchRunning) _swatchRunning.color = HeatmapMaterials.GetEffectiveStateColor(BottleneckState.Running);
            if (_swatchStarved) _swatchStarved.color = HeatmapMaterials.GetEffectiveStateColor(BottleneckState.Starved);
            if (_swatchClogged) _swatchClogged.color = HeatmapMaterials.GetEffectiveStateColor(BottleneckState.Clogged);
            if (_swatchFullStorage) _swatchFullStorage.color = HeatmapMaterials.GetEffectiveStateColor(BottleneckState.FullStorage);
            if (_swatchIdle) _swatchIdle.color = HeatmapMaterials.GetEffectiveStateColor(BottleneckState.Idle);
        }
    }
}
