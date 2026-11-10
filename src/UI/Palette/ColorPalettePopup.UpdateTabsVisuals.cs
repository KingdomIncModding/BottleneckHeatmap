using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Behaviour.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    public sealed partial class ColorPalettePopup : MonoBehaviour
    {
        void UpdateTabsVisuals()
        {
            SetTabActive(_tabRunning, _currentState == BottleneckState.Running);
            SetTabActive(_tabStarved, _currentState == BottleneckState.Starved);
            SetTabActive(_tabClogged, _currentState == BottleneckState.Clogged);
            SetTabActive(_tabFullStorage, _currentState == BottleneckState.FullStorage);
            SetTabActive(_tabIdle, _currentState == BottleneckState.Idle);
        }
    }
}
