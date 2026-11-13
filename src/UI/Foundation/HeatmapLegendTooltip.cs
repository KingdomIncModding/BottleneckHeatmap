using System;
using System.Collections.Generic;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    internal sealed class HeatmapLegendTooltip : MonoBehaviour
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

        Image CreateLegendRow(Transform parent, string name, string label, string desc, float y)
        {
            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rRt = (RectTransform)row.transform;
            rRt.anchorMin = new Vector2(0f, 1f);
            rRt.anchorMax = new Vector2(1f, 1f);
            rRt.pivot = new Vector2(0f, 1f);
            rRt.anchoredPosition = new Vector2(16f, y);
            rRt.sizeDelta = new Vector2(-32f, 24f);
            // Swatch Border
            var sBorder = new GameObject("SwatchBorder", typeof(RectTransform), typeof(Image));
            sBorder.transform.SetParent(row.transform, false);
            var sbRt = (RectTransform)sBorder.transform;
            sbRt.anchorMin = new Vector2(0f, 0.5f);
            sbRt.anchorMax = new Vector2(0f, 0.5f);
            sbRt.pivot = new Vector2(0f, 0.5f);
            sbRt.anchoredPosition = Vector2.zero;
            sbRt.sizeDelta = new Vector2(18f, 18f);
            sBorder.GetComponent<Image>().color = new Color(0.35f, 0.30f, 0.24f, 1f);
            // Swatch Fill
            var sFill = new GameObject("SwatchFill", typeof(RectTransform), typeof(Image));
            sFill.transform.SetParent(sBorder.transform, false);
            var sfRt = (RectTransform)sFill.transform;
            sfRt.anchorMin = Vector2.zero;
            sfRt.anchorMax = Vector2.one;
            sfRt.offsetMin = new Vector2(2f, 2f);
            sfRt.offsetMax = new Vector2(-2f, -2f);
            var swatchImg = sFill.GetComponent<Image>();
            // Label
            var lObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lObj.transform.SetParent(row.transform, false);
            var lRt = (RectTransform)lObj.transform;
            lRt.anchorMin = new Vector2(0f, 0f);
            lRt.anchorMax = new Vector2(0f, 1f);
            lRt.pivot = new Vector2(0f, 0.5f);
            lRt.anchoredPosition = new Vector2(24f, 0f);
            lRt.sizeDelta = new Vector2(85f, 24f);
            var lText = lObj.GetComponent<TextMeshProUGUI>();
            if (VanillaTheme.Font != null)
            {
                lText.font = VanillaTheme.Font;
                lText.fontSharedMaterial = VanillaTheme.FontMaterial;
            }

            lText.text = label;
            lText.fontSize = 12f;
            lText.fontStyle = FontStyles.Bold;
            lText.color = Color.white;
            lText.alignment = TextAlignmentOptions.MidlineLeft;
            // Description
            var dObj = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
            dObj.transform.SetParent(row.transform, false);
            var dRt = (RectTransform)dObj.transform;
            dRt.anchorMin = new Vector2(0f, 0f);
            dRt.anchorMax = new Vector2(1f, 1f);
            dRt.pivot = new Vector2(0f, 0.5f);
            dRt.offsetMin = new Vector2(112f, 0f);
            dRt.offsetMax = Vector2.zero;
            var dText = dObj.GetComponent<TextMeshProUGUI>();
            if (VanillaTheme.Font != null)
            {
                dText.font = VanillaTheme.Font;
                dText.fontSharedMaterial = VanillaTheme.FontMaterial;
            }

            dText.text = desc;
            dText.fontSize = 10f;
            dText.color = new Color(0.80f, 0.77f, 0.72f, 0.95f);
            dText.alignment = TextAlignmentOptions.MidlineLeft;
            return swatchImg;
        }

        void CreateSeparator(Transform parent, float y)
        {
            var sep = new GameObject("Sep", typeof(RectTransform), typeof(Image));
            sep.transform.SetParent(parent, false);
            var srt = (RectTransform)sep.transform;
            srt.anchorMin = new Vector2(0f, 1f);
            srt.anchorMax = new Vector2(1f, 1f);
            srt.anchoredPosition = new Vector2(0f, y);
            srt.sizeDelta = new Vector2(-28f, 1.5f);
            sep.GetComponent<Image>().color = new Color(0.38f, 0.32f, 0.24f, 0.85f);
        }

        TMP_Text CreateText(Transform parent, string name, string text, Vector2 pos, Vector2 size, float fontSize, Color color, FontStyles style, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (VanillaTheme.Font != null)
            {
                tmp.font = VanillaTheme.Font;
                tmp.fontSharedMaterial = VanillaTheme.FontMaterial;
            }

            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.raycastTarget = false;
            return tmp;
        }

        static HeatmapLegendTooltip _instance;
        Image _swatchRunning;
        Image _swatchStarved;
        Image _swatchClogged;
        Image _swatchFullStorage;
        Image _swatchIdle;
        public static void Show(RectTransform anchor)
        {
            if (!anchor)
                return;
            EnsureInstance(anchor);
            if (!_instance)
                return;
            _instance.UpdateColors();
            _instance.PositionAt(anchor);
            _instance.gameObject.SetActive(true);
            _instance.transform.SetAsLastSibling();
        }

        static void EnsureInstance(RectTransform anchor)
        {
            if (_instance)
                return;
            var canvas = anchor.GetComponentInParent<Canvas>();
            if (!canvas)
                canvas = global::UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (!canvas)
                return;
            var go = new GameObject("HeatmapLegendTooltip", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            _instance = go.AddComponent<HeatmapLegendTooltip>();
            _instance.BuildUI();
        }

        public static void Hide()
        {
            if (_instance)
            {
                _instance.gameObject.SetActive(false);
            }
        }

        void PositionAt(RectTransform anchor)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (!canvas)
                return;
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

        void UpdateColors()
        {
            if (_swatchRunning)
                _swatchRunning.color = HeatmapMaterials.GetEffectiveStateColor(BottleneckState.Running);
            if (_swatchStarved)
                _swatchStarved.color = HeatmapMaterials.GetEffectiveStateColor(BottleneckState.Starved);
            if (_swatchClogged)
                _swatchClogged.color = HeatmapMaterials.GetEffectiveStateColor(BottleneckState.Clogged);
            if (_swatchFullStorage)
                _swatchFullStorage.color = HeatmapMaterials.GetEffectiveStateColor(BottleneckState.FullStorage);
            if (_swatchIdle)
                _swatchIdle.color = HeatmapMaterials.GetEffectiveStateColor(BottleneckState.Idle);
        }
    }
}
