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
    /// <summary>
    /// Custom row element displaying:
    /// [Toggle Checkbox] [Active Swatch] Label    [Pick Color Button]  |  [Var1][Var2][Var3][Var4][Var5]
    /// Completely matches user mockup image on a single unified line!
    /// </summary>
    internal sealed partial class ModConfigStateColorRow : IModConfigElement
    {
        public string Id { get; }
        public bool IsVisible { get; set; } = true;
        public BottleneckState State { get; }
        public string Label { get; }
        public Func<bool> EnabledGetter { get; }
        public Action<bool> EnabledSetter { get; }
        public Func<string> HexGetter { get; }
        public Action<string> HexSetter { get; }

        (string Label, string Hex)[] _variations;
        Image _leftSwatchImage;
        readonly Image[] _swatchBorders = new Image[5];
        readonly Image[] _swatchFills = new Image[5];


        public ModConfigStateColorRow(
            BottleneckState state,
            string label,
            (string Label, string Hex)[] initialVariations,
            Func<bool> enabledGetter,
            Action<bool> enabledSetter,
            Func<string> hexGetter,
            Action<string> hexSetter,
            string id = null)
        {
            State = state;
            Label = label;
            _variations = initialVariations;
            EnabledGetter = enabledGetter;
            EnabledSetter = enabledSetter;
            HexGetter = hexGetter;
            HexSetter = hexSetter;
            Id = id ?? Guid.NewGuid().ToString("N");
        }
    }
}
