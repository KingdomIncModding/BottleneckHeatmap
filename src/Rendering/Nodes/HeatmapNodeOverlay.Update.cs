using System.Reflection;
using Assets.Behaviour.Flow;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    public sealed partial class HeatmapNodeOverlay : MonoBehaviour
    {
        void Update()
        {
            if (Time.time < _nextCheckTime) return;
            _nextCheckTime = Time.time + 0.12f;

            RefreshVisuals(false);
        }
    }
}
