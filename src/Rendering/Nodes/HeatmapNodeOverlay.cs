using System.Reflection;
using Assets.Behaviour.Flow;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    public sealed partial class HeatmapNodeOverlay : MonoBehaviour
    {
        static readonly FieldInfo BackgroundField = AccessTools.Field(typeof(WorldFlowElement), "_background");

        WorldFlowElement _world;
        Image _overlayImage;
        float _nextCheckTime;
        BottleneckState _currentState = BottleneckState.Idle;

        public BottleneckState CurrentState => _currentState;


        public static HeatmapNodeOverlay Ensure(WorldFlowElement world)
        {
            if (!world) return null;
            var overlay = world.GetComponent<HeatmapNodeOverlay>();
            if (!overlay)
            {
                overlay = world.gameObject.AddComponent<HeatmapNodeOverlay>();
            }
            return overlay;
        }
    }
}
