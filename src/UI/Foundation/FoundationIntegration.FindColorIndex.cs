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
        public static int FindColorIndex((string Label, string Hex)[] options, string hex)
        {
            if (options == null || options.Length == 0 || string.IsNullOrEmpty(hex)) return 0;
            for (int i = 0; i < options.Length; i++)
            {
                if (string.Equals(options[i].Hex, hex, StringComparison.OrdinalIgnoreCase)) return i;
            }
            return 0;
        }
    }
}
