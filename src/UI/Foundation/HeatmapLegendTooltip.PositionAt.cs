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
    internal sealed partial class HeatmapLegendTooltip : MonoBehaviour
    {
        void PositionAt(RectTransform anchor)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (!canvas) return;

            Vector3[] corners = new Vector3[4];
            anchor.GetWorldCorners(corners);
            Vector3 bottomCenter = (corners[0] + corners[3]) * 0.5f;
            Vector3 topCenter = (corners[1] + corners[2]) * 0.5f;

            bool inUpperHalf = bottomCenter.y > Screen.height * 0.5f;

            var rt = (RectTransform)transform;
            rt.pivot = inUpperHalf ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0f);
            rt.position = inUpperHalf ? bottomCenter + new Vector3(0, -6f, 0) : topCenter + new Vector3(0, 6f, 0);

            var canvasRt = (RectTransform)canvas.transform;
            float halfWidth = rt.rect.width * 0.5f;
            Vector3 local = rt.localPosition;
            float minX = -canvasRt.rect.width * 0.5f + halfWidth + 10f;
            float maxX = canvasRt.rect.width * 0.5f - halfWidth - 10f;
            local.x = Mathf.Clamp(local.x, minX, maxX);
            rt.localPosition = local;
        }
    }
}
