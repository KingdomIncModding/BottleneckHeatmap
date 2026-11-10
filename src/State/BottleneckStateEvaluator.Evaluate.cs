using System.Reflection;
using Assets.Behaviour.Flow;
using Assets.Source.Flow;
using Assets.Source.Flow.Elements;
using Assets.Source.Player;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace KingdomBottleneckHeatmap
{
    public static partial class BottleneckStateEvaluator
    {
        public static BottleneckState Evaluate(WorldFlowPipe worldPipe, float lastTransferTime, bool wasRejected)
        {
            if (!worldPipe || !worldPipe.gameObject.activeInHierarchy)
                return BottleneckState.Idle;

            var pipe = worldPipe.Pipe;
            if (pipe == null)
                return BottleneckState.Idle;

            // 1. Blocked / Rejected checks
            if (wasRejected || pipe.ItemRejected)
                return BottleneckState.Clogged;

            var output = pipe.ConnectedOutput;
            var input = pipe.ConnectedInput;

            if (output == null || input == null)
                return BottleneckState.Idle;

            var item = output.AvailableItem;
            if (item != null)
            {
                if (!input.CanAcceptItem(item) || !input.CanBuffer)
                    return BottleneckState.Clogged;
            }

            // 2. Active flow
            if (Time.time - lastTransferTime <= 1.5f)
                return BottleneckState.Running;

            // 3. Starved / Empty
            if (output.BufferedItems == null || output.BufferedItems.Count == 0)
                return BottleneckState.Starved;

            return BottleneckState.Running;
        }
    }
}
