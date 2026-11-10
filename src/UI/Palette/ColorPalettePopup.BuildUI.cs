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
        void BuildUI()
        {
            VanillaTheme.Ensure();

            var rt = (RectTransform)transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // 1. Blocker backdrop
            var backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
            backdrop.transform.SetParent(transform, false);
            var bRt = (RectTransform)backdrop.transform;
            bRt.anchorMin = Vector2.zero;
            bRt.anchorMax = Vector2.one;
            bRt.offsetMin = Vector2.zero;
            bRt.offsetMax = Vector2.zero;
            var bImg = backdrop.GetComponent<Image>();
            bImg.color = new Color(0f, 0f, 0f, 0.65f);
            var bBtn = backdrop.GetComponent<Button>();
            bBtn.onClick.AddListener(Close);

            // 2. Main dialog frame (matching Kingdom Inc window frame)
            var dialog = new GameObject("DialogFrame", typeof(RectTransform), typeof(Image));
            dialog.transform.SetParent(transform, false);
            var dRt = (RectTransform)dialog.transform;
            dRt.anchorMin = new Vector2(0.5f, 0.5f);
            dRt.anchorMax = new Vector2(0.5f, 0.5f);
            dRt.pivot = new Vector2(0.5f, 0.5f);
            dRt.sizeDelta = new Vector2(620f, 470f);

            var dImg = dialog.GetComponent<Image>();
            if (VanillaTheme.WindowSprite != null)
            {
                dImg.sprite = VanillaTheme.WindowSprite;
                dImg.type = Image.Type.Sliced;
                dImg.color = Color.white;
            }
            else
            {
                dImg.color = new Color(0.14f, 0.12f, 0.16f, 0.98f);
                // Outer gold border fallback
                var border = new GameObject("Border", typeof(RectTransform), typeof(Image));
                border.transform.SetParent(dialog.transform, false);
                var brt = (RectTransform)border.transform;
                brt.anchorMin = Vector2.zero;
                brt.anchorMax = Vector2.one;
                brt.offsetMin = new Vector2(-2, -2);
                brt.offsetMax = new Vector2(2, 2);
                var bBorder = border.GetComponent<Image>();
                bBorder.color = new Color(0.48f, 0.40f, 0.28f, 1f);
                border.transform.SetAsFirstSibling();
            }

            // Header Title
            CreateText(dialog.transform, "Title", "Heatmap Color Variations Generator", new Vector2(24, -18), new Vector2(480, 26), 18, new Color(1f, 0.85f, 0.18f), FontStyles.Bold);
            _headerSubtitle = CreateText(dialog.transform, "Subtitle", "Select base color to auto-generate slight variation hues", new Vector2(24, -42), new Vector2(500, 20), 12, new Color(0.78f, 0.74f, 0.68f), FontStyles.Normal);

            // Close button [X]
            CreateCloseButton(dialog.transform, new Vector2(-16, -16), Close);

            // State Selection Tabs
            float tabStartX = 24f;
            float tabWidth = 110f;
            float tabY = -68f;

            _tabRunning = CreateTabButton(dialog.transform, "Tab_Running", "Running", new Vector2(tabStartX + 0 * (tabWidth + 5), tabY), new Vector2(tabWidth, 26), () => SelectState(BottleneckState.Running));
            _tabStarved = CreateTabButton(dialog.transform, "Tab_Starved", "Starved", new Vector2(tabStartX + 1 * (tabWidth + 5), tabY), new Vector2(tabWidth, 26), () => SelectState(BottleneckState.Starved));
            _tabClogged = CreateTabButton(dialog.transform, "Tab_Clogged", "Clogged", new Vector2(tabStartX + 2 * (tabWidth + 5), tabY), new Vector2(tabWidth, 26), () => SelectState(BottleneckState.Clogged));
            _tabFullStorage = CreateTabButton(dialog.transform, "Tab_Full", "Full Storage", new Vector2(tabStartX + 3 * (tabWidth + 5), tabY), new Vector2(tabWidth, 26), () => SelectState(BottleneckState.FullStorage));
            _tabIdle = CreateTabButton(dialog.transform, "Tab_Idle", "Idle", new Vector2(tabStartX + 4 * (tabWidth + 5), tabY), new Vector2(tabWidth, 26), () => SelectState(BottleneckState.Idle));

            // Color Picker Area Frame
            // SV Surface (210 x 110)
            var svBorder = CreatePanelBox(dialog.transform, "SVBorder", new Vector2(24, -102), new Vector2(212, 112));
            var svObj = new GameObject("SVSurface", typeof(RectTransform), typeof(PickerSurface));
            svObj.transform.SetParent(svBorder.transform, false);
            var svRt = (RectTransform)svObj.transform;
            svRt.anchorMin = Vector2.zero;
            svRt.anchorMax = Vector2.one;
            svRt.offsetMin = new Vector2(1, 1);
            svRt.offsetMax = new Vector2(-1, -1);
            _svSurface = svObj.GetComponent<PickerSurface>();
            _svSurface.Mode = PickerMode.SaturationValue;
            _svSurface.Changed = p =>
            {
                _baseSaturation = p.x;
                _baseValue = p.y;
                UpdateSurfaces();
                UpdateGeneratedVariations();
            };

            // Hue Strip (24 x 110)
            var hueBorder = CreatePanelBox(dialog.transform, "HueBorder", new Vector2(244, -102), new Vector2(26, 112));
            var hueObj = new GameObject("HueStrip", typeof(RectTransform), typeof(PickerSurface));
            hueObj.transform.SetParent(hueBorder.transform, false);
            var hueRt = (RectTransform)hueObj.transform;
            hueRt.anchorMin = Vector2.zero;
            hueRt.anchorMax = Vector2.one;
            hueRt.offsetMin = new Vector2(1, 1);
            hueRt.offsetMax = new Vector2(-1, -1);
            _hueSurface = hueObj.GetComponent<PickerSurface>();
            _hueSurface.Mode = PickerMode.Hue;
            _hueSurface.Changed = p =>
            {
                _baseHue = 1f - p.y;
                UpdateSurfaces();
                UpdateGeneratedVariations();
            };

            // Selected Base Color Preview Box
            var bpBorder = CreatePanelBox(dialog.transform, "BaseColorBorder", new Vector2(280, -102), new Vector2(52, 50));
            var basePrevObj = new GameObject("BaseColorPreview", typeof(RectTransform), typeof(Image));
            basePrevObj.transform.SetParent(bpBorder.transform, false);
            var bpRt = (RectTransform)basePrevObj.transform;
            bpRt.anchorMin = Vector2.zero;
            bpRt.anchorMax = Vector2.one;
            bpRt.offsetMin = new Vector2(2, 2);
            bpRt.offsetMax = new Vector2(-2, -2);
            _baseColorPreview = basePrevObj.GetComponent<Image>();
            _baseColorPreview.color = Color.green;

            _baseHexText = CreateText(dialog.transform, "BaseHex", "#26FA73", new Vector2(280, -156), new Vector2(62, 20), 12, new Color(1f, 0.85f, 0.18f), FontStyles.Bold, TextAlignmentOptions.Left);

            // Quick Presets
            CreateText(dialog.transform, "PresetsLabel", "Quick Presets:", new Vector2(352, -102), new Vector2(100, 16), 11, new Color(0.85f, 0.82f, 0.76f), FontStyles.Bold);

            float pX = 352f;
            float pY = -122f;
            for (int i = 0; i < Presets.Length; i++)
            {
                string hex = Presets[i];
                ColorUtility.TryParseHtmlString("#" + hex, out var pColor);
                CreateColorButton(dialog.transform, $"Preset_{hex}", pColor, new Vector2(pX + (i % 5) * 44f, pY - (i / 5) * 32f), new Vector2(38f, 26f), () =>
                {
                    Color.RGBToHSV(pColor, out _baseHue, out _baseSaturation, out _baseValue);
                    UpdateSurfaces();
                    UpdateGeneratedVariations();
                });
            }

            // Section 2: Surrounding Hues Cards (Slight Variations)
            _variationsTitle = CreateText(dialog.transform, "VariationsLabel", "Auto-Generated Surrounding Hues:", new Vector2(24, -228), new Vector2(480, 20), 13, new Color(1f, 0.85f, 0.18f), FontStyles.Bold);

            float cardStartX = 24f;
            float cardSpacing = 116f;
            float cardY = -252f;

            for (int i = 0; i < 5; i++)
            {
                int cardIdx = i;
                CreateVariationCard(dialog.transform, $"VarCard_{i}", new Vector2(cardStartX + i * cardSpacing, cardY), out _cards[i], () =>
                {
                    Color.RGBToHSV(_cards[cardIdx].Color, out _baseHue, out _baseSaturation, out _baseValue);
                    UpdateSurfaces();
                    UpdateGeneratedVariations();
                });
            }

            // Section 3: Bottom Action Buttons
            _applyButton = CreateThemedButton(dialog.transform, "ApplyBtn", "Apply to State", new Vector2(24, -420), new Vector2(184, 36), ApplyCurrentColor);
            CreateThemedButton(dialog.transform, "ResetBtn", "Reset State Default", new Vector2(220, -420), new Vector2(174, 36), ResetStateDefault);
            CreateThemedButton(dialog.transform, "CancelBtn", "Close", new Vector2(406, -420), new Vector2(174, 36), Close);
        }
    }
}
