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
    public sealed partial class PickerSurface : MaskableGraphic, IPointerDownHandler, IDragHandler
    {
        void Select(PointerEventData data)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, data.position, data.pressEventCamera, out var local))
            {
                var rect = rectTransform.rect;
                Changed?.Invoke(new Vector2(Mathf.Clamp01((local.x - rect.xMin) / rect.width), Mathf.Clamp01((local.y - rect.yMin) / rect.height)));
            }
        }
    }
}
