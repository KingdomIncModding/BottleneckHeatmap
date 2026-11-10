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
        public static void Release()
        {
            try
            {
                _context?.Dispose();
                _context = null;
                _barRegistration = null;
                _activeView = null;
            }
            catch
            {
            }
        }
    }
}
