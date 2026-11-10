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
    internal sealed partial class ModConfigStateColorRow : IModConfigElement
    {
        public GameObject CreateView(IModConfigContext context)
        {
            VanillaTheme.Ensure();

            var row = context.Templates.CreateEmptyRow(context.Container, 46f);
            row.name = $"StateColorRow_{State}";

            bool isEnabled = EnabledGetter != null ? EnabledGetter() : true;
            string currentHex = HexGetter != null ? HexGetter() : "#FFFFFF";
            Color curColor = ColorUtility.TryParseHtmlString(currentHex, out var c) ? c : Color.white;

            // 1. Toggle Checkbox cloned from vanilla template
            var dummyToggleRow = context.Templates.CreateToggleRow(context.Container, "", isEnabled, null, out var toggle);
            var toggleObj = toggle.gameObject;
            toggleObj.transform.SetParent(row.transform, false);
            var tRt = (RectTransform)toggleObj.transform;
            tRt.anchorMin = new Vector2(0f, 0.5f);
            tRt.anchorMax = new Vector2(0f, 0.5f);
            tRt.pivot = new Vector2(0f, 0.5f);
            tRt.anchoredPosition = new Vector2(6f, 0f);
            tRt.sizeDelta = new Vector2(22f, 22f);

            toggle.onValueChanged = new Toggle.ToggleEvent();
            toggle.SetIsOnWithoutNotify(isEnabled);
            toggle.onValueChanged.AddListener(val =>
            {
                UISounds.CraftStep();
                EnabledSetter?.Invoke(val);
            });
            UnityEngine.Object.Destroy(dummyToggleRow);

            // 2. Left Swatch Border (20x20)
            var swatchBorderObj = new GameObject("SwatchBorder", typeof(RectTransform), typeof(Image));
            swatchBorderObj.transform.SetParent(row.transform, false);
            var sbRt = (RectTransform)swatchBorderObj.transform;
            sbRt.anchorMin = new Vector2(0f, 0.5f);
            sbRt.anchorMax = new Vector2(0f, 0.5f);
            sbRt.pivot = new Vector2(0f, 0.5f);
            sbRt.anchoredPosition = new Vector2(32f, 0f);
            sbRt.sizeDelta = new Vector2(20f, 20f);
            var sbImg = swatchBorderObj.GetComponent<Image>();
            sbImg.color = new Color(0.36f, 0.32f, 0.24f, 1f);

            // Left Swatch Image
            var swatchObj = new GameObject("Swatch", typeof(RectTransform), typeof(Image));
            swatchObj.transform.SetParent(swatchBorderObj.transform, false);
            var sRt = (RectTransform)swatchObj.transform;
            sRt.anchorMin = Vector2.zero;
            sRt.anchorMax = Vector2.one;
            sRt.offsetMin = new Vector2(2f, 2f);
            sRt.offsetMax = new Vector2(-2f, -2f);
            _leftSwatchImage = swatchObj.GetComponent<Image>();
            _leftSwatchImage.color = curColor;

            // 3. Row Label (Single line, strictly no wrap)
            var labelObj = new GameObject("RowLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObj.transform.SetParent(row.transform, false);
            var lRt = (RectTransform)labelObj.transform;
            lRt.anchorMin = new Vector2(0f, 0f);
            lRt.anchorMax = new Vector2(0.40f, 1f);
            lRt.offsetMin = new Vector2(56f, 0f);
            lRt.offsetMax = new Vector2(-4f, 0f);
            var tmp = labelObj.GetComponent<TextMeshProUGUI>();
            if (VanillaTheme.Font != null)
            {
                tmp.font = VanillaTheme.Font;
                tmp.fontSharedMaterial = VanillaTheme.FontMaterial;
            }
            tmp.fontSize = 13.5f;
            tmp.fontSizeMin = 9f;
            tmp.fontSizeMax = 15f;
            tmp.enableAutoSizing = true;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.raycastTarget = false;
            tmp.text = Label ?? "";

            // 4. "Pick Color" Button cloned from vanilla (Widened with horizontal margins for text)
            var dummyBtnRow = context.Templates.CreateButtonRow(context.Container, "", "Pick Color", null, out var button, out var btnText);

            var btnObj = button.gameObject;
            btnObj.transform.SetParent(row.transform, false);
            var bRt = (RectTransform)btnObj.transform;
            bRt.anchorMin = new Vector2(0.41f, 0.10f);
            bRt.anchorMax = new Vector2(0.66f, 0.90f);
            bRt.offsetMin = Vector2.zero;
            bRt.offsetMax = Vector2.zero;
            if (btnText != null)
            {
                btnText.text = "Pick Color";
                btnText.alignment = TextAlignmentOptions.Center;
                btnText.textWrappingMode = TextWrappingModes.NoWrap;
                btnText.overflowMode = TextOverflowModes.Ellipsis;
                btnText.fontSize = 13f;
                btnText.fontSizeMin = 8f;
                btnText.fontSizeMax = 14f;
                btnText.enableAutoSizing = true;
                btnText.rectTransform.anchorMin = Vector2.zero;
                btnText.rectTransform.anchorMax = Vector2.one;
                btnText.rectTransform.offsetMin = new Vector2(10f, 0f);
                btnText.rectTransform.offsetMax = new Vector2(-10f, 0f);
            }

            // Explicitly rebind onClick to guarantee it opens ColorPalettePopup
            var targetState = State;
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() =>
            {
                UISounds.TurnPage();
                ColorPalettePopup.Open(targetState);
            });

            UnityEngine.Object.Destroy(dummyBtnRow);

            // 5. Subtle Vertical Divider "|"
            var divObj = new GameObject("Divider", typeof(RectTransform), typeof(Image));
            divObj.transform.SetParent(row.transform, false);
            var divRt = (RectTransform)divObj.transform;
            divRt.anchorMin = new Vector2(0.68f, 0.14f);
            divRt.anchorMax = new Vector2(0.68f, 0.86f);
            divRt.sizeDelta = new Vector2(1.5f, 0f);
            var divImg = divObj.GetComponent<Image>();
            divImg.color = new Color(0.42f, 0.36f, 0.28f, 0.85f);

            // 6. 5 Variation Swatches Container with HorizontalLayoutGroup
            var swatchesContainer = new GameObject("SwatchesContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            swatchesContainer.transform.SetParent(row.transform, false);
            var scRt = (RectTransform)swatchesContainer.transform;
            scRt.anchorMin = new Vector2(0.70f, 0.12f);
            scRt.anchorMax = new Vector2(0.99f, 0.88f);
            scRt.offsetMin = Vector2.zero;
            scRt.offsetMax = Vector2.zero;

            var hlg = swatchesContainer.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            for (int i = 0; i < 5; i++)
            {
                int varIdx = i;
                var sBtnObj = new GameObject($"VarSwatch_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
                sBtnObj.transform.SetParent(swatchesContainer.transform, false);

                var borderImg = sBtnObj.GetComponent<Image>();
                borderImg.color = new Color(0.28f, 0.24f, 0.18f, 0.9f);
                _swatchBorders[i] = borderImg;

                var fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                fillObj.transform.SetParent(sBtnObj.transform, false);
                var fRt = (RectTransform)fillObj.transform;
                fRt.anchorMin = Vector2.zero;
                fRt.anchorMax = Vector2.one;
                fRt.offsetMin = new Vector2(2f, 2f);
                fRt.offsetMax = new Vector2(-2f, -2f);
                var fillImg = fillObj.GetComponent<Image>();
                fillImg.raycastTarget = false;
                _swatchFills[i] = fillImg;

                if (_variations != null && i < _variations.Length)
                {
                    ColorUtility.TryParseHtmlString(_variations[i].Hex, out var varCol);
                    fillImg.color = varCol;
                }

                var btn = sBtnObj.GetComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    UISounds.TurnPage();
                    SelectVariation(varIdx);
                });
            }

            HighlightSelected(currentHex);

            row.SetActive(IsVisible);
            return row;
        }
    }
}
