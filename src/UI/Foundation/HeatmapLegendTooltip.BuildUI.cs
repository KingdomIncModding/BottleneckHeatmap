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
        void BuildUI()
        {
            VanillaTheme.Ensure();

            var rt = (RectTransform)transform;
            rt.sizeDelta = new Vector2(330f, 218f);

            var cg = gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;

            var img = gameObject.AddComponent<Image>();
            img.raycastTarget = false;
            if (VanillaTheme.WindowSprite != null)
            {
                img.sprite = VanillaTheme.WindowSprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.12f, 0.10f, 0.14f, 0.98f);
            }

            // Title
            CreateText(transform, "Title", "Bottleneck Heatmap Legend", new Vector2(0f, -14f), new Vector2(330f, 20f), 14f, new Color(1f, 0.85f, 0.18f), FontStyles.Bold, TextAlignmentOptions.Center);

            // Separator 1
            CreateSeparator(transform, -36f);

            // 5 Status Rows
            float startY = -46f;
            float rowHeight = 26f;

            _swatchRunning = CreateLegendRow(transform, "Running", "Running", "Processing & transferring items", startY + 0 * -rowHeight);
            _swatchStarved = CreateLegendRow(transform, "Starved", "Starved", "Waiting for input reagents", startY + 1 * -rowHeight);
            _swatchClogged = CreateLegendRow(transform, "Clogged", "Clogged", "Output blocked / rejected", startY + 2 * -rowHeight);
            _swatchFullStorage = CreateLegendRow(transform, "FullStorage", "Full Storage", "Stockpiles & Armories at cap", startY + 3 * -rowHeight);
            _swatchIdle = CreateLegendRow(transform, "Idle", "Idle", "Dormant / unconfigured node", startY + 4 * -rowHeight);

            // Separator 2
            CreateSeparator(transform, -180f);

            // Footer tip
            CreateText(transform, "Tip", "Left-Click: Toggle  *  Right-Click: Colors", new Vector2(0f, -190f), new Vector2(330f, 18f), 11f, new Color(0.78f, 0.74f, 0.68f), FontStyles.Normal, TextAlignmentOptions.Center);

            gameObject.SetActive(false);
        }
    }
}
