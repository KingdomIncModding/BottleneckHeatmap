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
    internal static partial class FoundationIntegration
    {
        public static string[] GetLabels((string Label, string Hex)[] options)
        {
            if (options == null) return new string[0];
            var labels = new string[options.Length];
            for (int i = 0; i < options.Length; i++) labels[i] = options[i].Label;
            return labels;
        }
    }
}
