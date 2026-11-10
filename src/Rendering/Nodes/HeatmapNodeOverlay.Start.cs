using System.Reflection;
using Assets.Behaviour.Flow;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    public sealed partial class HeatmapNodeOverlay : MonoBehaviour
    {
        void Start()
        {
            RefreshVisuals(true);
        }
    }
}
