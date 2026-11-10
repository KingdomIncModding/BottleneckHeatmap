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
        public static void Register(BottleneckHeatmapPlugin plugin)
        {
            try
            {
                _context = FoundationApi.ForMod(BottleneckHeatmapPlugin.Id);
                RegisterConfigInternal(plugin);
                RegisterModBarInternal(plugin);
            }
            catch
            {
                _context?.Dispose();
                _context = null;
                throw;
            }
        }
    }
}
