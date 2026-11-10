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
        void CreateVariationCard(Transform parent, string name, Vector2 pos, out VariationCard card, Action onCardClicked)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            root.transform.SetParent(parent, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(108f, 150f);

            var bg = root.GetComponent<Image>();
            if (VanillaTheme.ButtonSprite != null)
            {
                bg.sprite = VanillaTheme.ButtonSprite;
                bg.type = Image.Type.Sliced;
                bg.color = new Color(0.85f, 0.82f, 0.78f, 1f);
            }
            else
            {
                bg.color = new Color(0.18f, 0.16f, 0.20f, 0.98f);
            }

            var btn = root.GetComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() =>
            {
                UISounds.TurnPage();
                onCardClicked?.Invoke();
            });

            // Card Swatch Box with inset border
            var sBorder = new GameObject("SwatchBorder", typeof(RectTransform), typeof(Image));
            sBorder.transform.SetParent(root.transform, false);
            var sbRt = (RectTransform)sBorder.transform;
            sbRt.anchorMin = new Vector2(0.5f, 1f);
            sbRt.anchorMax = new Vector2(0.5f, 1f);
            sbRt.pivot = new Vector2(0.5f, 1f);
            sbRt.anchoredPosition = new Vector2(0, -8);
            sbRt.sizeDelta = new Vector2(92, 54);
            var sbImg = sBorder.GetComponent<Image>();
            sbImg.color = new Color(0.32f, 0.28f, 0.22f, 1f);

            var sObj = new GameObject("Swatch", typeof(RectTransform), typeof(Image));
            sObj.transform.SetParent(sBorder.transform, false);
            var sRt = (RectTransform)sObj.transform;
            sRt.anchorMin = Vector2.zero;
            sRt.anchorMax = Vector2.one;
            sRt.offsetMin = new Vector2(2, 2);
            sRt.offsetMax = new Vector2(-2, -2);
            var swatch = sObj.GetComponent<Image>();
            swatch.color = Color.white;

            // Variation Title
            var title = CreateText(root.transform, "Title", "Variation", new Vector2(0, -68), new Vector2(108, 30), 11, new Color(0.92f, 0.90f, 0.85f), FontStyles.Bold, TextAlignmentOptions.Center);
            title.textWrappingMode = TextWrappingModes.Normal;

            // Hex Code
            var hex = CreateText(root.transform, "Hex", "#FFFFFF", new Vector2(0, -120), new Vector2(108, 18), 11, new Color(1f, 0.85f, 0.18f), FontStyles.Bold, TextAlignmentOptions.Center);

            card = new VariationCard
            {
                Root = root,
                Border = sbImg,
                Swatch = swatch,
                Title = title,
                Hex = hex,
                Color = Color.white
            };
        }
    }
}
