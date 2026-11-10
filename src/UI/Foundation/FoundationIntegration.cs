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
        static FoundationModContext _context;
        static IModBarRegistration _barRegistration;
        static HeatmapBarView _activeView;
        static IModConfigButton _hotkeyButton;

        static ModConfigStateColorRow _runningRow;
        static ModConfigStateColorRow _starvedRow;
        static ModConfigStateColorRow _cloggedRow;
        static ModConfigStateColorRow _fullStorageRow;
        static ModConfigStateColorRow _idleRow;


        public static (string Label, string Hex)[] RunningVariations;
    }
}
