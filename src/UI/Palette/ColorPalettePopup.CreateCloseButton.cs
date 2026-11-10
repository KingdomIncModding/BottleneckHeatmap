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
        void CreateCloseButton(Transform parent, Vector2 pos, Action onClick)
        {
            var closeObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            closeObj.transform.SetParent(parent, false);

            var rt = (RectTransform)closeObj.transform;
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(28f, 28f);

            var img = closeObj.GetComponent<Image>();
            if (VanillaTheme.CloseButtonSprite != null)
            {
                img.sprite = VanillaTheme.CloseButtonSprite;
                img.color = Color.white;
            }
            else if (VanillaTheme.ButtonSprite != null)
            {
                img.sprite = VanillaTheme.ButtonSprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.35f, 0.18f, 0.18f, 1f);
            }

            var btn = closeObj.GetComponent<Button>();
            btn.targetGraphic = img;

            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 0.8f, 0.8f, 1f);
            colors.pressedColor = new Color(0.8f, 0.5f, 0.5f, 1f);
            btn.colors = colors;

            var textObj = new GameObject("X", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(closeObj.transform, false);
            var t = textObj.GetComponent<TextMeshProUGUI>();
            if (VanillaTheme.Font != null)
            {
                t.font = VanillaTheme.Font;
                t.fontSharedMaterial = VanillaTheme.FontMaterial;
            }
            t.text = "✕";
            t.fontSize = 16;
            t.fontStyle = FontStyles.Bold;
            t.color = new Color(0.95f, 0.85f, 0.65f);
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            var trt = (RectTransform)textObj.transform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;

            btn.onClick.AddListener(() =>
            {
                UISounds.TurnPage();
                onClick?.Invoke();
            });
        }
    }
}
