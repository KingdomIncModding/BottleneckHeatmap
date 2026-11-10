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
        void CreateColorButton(Transform parent, string name, Color color, Vector2 pos, Vector2 size, Action onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var borderImg = go.GetComponent<Image>();
            borderImg.color = new Color(0.32f, 0.28f, 0.22f, 1f);

            var innerObj = new GameObject("Inner", typeof(RectTransform), typeof(Image));
            innerObj.transform.SetParent(go.transform, false);
            var irt = (RectTransform)innerObj.transform;
            irt.anchorMin = Vector2.zero;
            irt.anchorMax = Vector2.one;
            irt.offsetMin = new Vector2(2, 2);
            irt.offsetMax = new Vector2(-2, -2);
            var innerImg = innerObj.GetComponent<Image>();
            innerImg.color = color;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = borderImg;
            btn.onClick.AddListener(() =>
            {
                UISounds.TurnPage();
                onClick?.Invoke();
            });
        }
    }
}
