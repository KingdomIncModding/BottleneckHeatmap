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
        Button CreateThemedButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Action onClick)
        {
            var btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);

            var rt = (RectTransform)btnObj.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = btnObj.GetComponent<Image>();
            if (VanillaTheme.ButtonSprite != null)
            {
                img.sprite = VanillaTheme.ButtonSprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.24f, 0.20f, 0.16f, 1f);
            }

            var btn = btnObj.GetComponent<Button>();
            btn.targetGraphic = img;

            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.10f, 1f);
            colors.pressedColor = new Color(0.80f, 0.75f, 0.70f, 1f);
            btn.colors = colors;

            var textObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(btnObj.transform, false);
            var tRt = (RectTransform)textObj.transform;
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = new Vector2(4, 2);
            tRt.offsetMax = new Vector2(-4, -2);

            var btnText = textObj.GetComponent<TextMeshProUGUI>();
            if (VanillaTheme.Font != null)
            {
                btnText.font = VanillaTheme.Font;
                btnText.fontSharedMaterial = VanillaTheme.FontMaterial;
            }
            btnText.text = label;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontSizeMin = 10;
            btnText.fontSizeMax = 14;
            btnText.enableAutoSizing = true;
            btnText.color = new Color(0.96f, 0.94f, 0.90f);
            btnText.raycastTarget = false;

            btn.onClick.AddListener(() =>
            {
                UISounds.TurnPage();
                onClick?.Invoke();
            });

            return btn;
        }
    }
}
