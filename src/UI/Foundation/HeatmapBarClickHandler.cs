using System;
using System.Collections.Generic;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    internal sealed class HeatmapBarClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Action OnRightClick;
        public Action OnHoverEnter;
        public Action OnHoverExit;
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnRightClick?.Invoke();
            }
        }

        void OnDisable()
        {
            OnHoverExit?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHoverEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHoverExit?.Invoke();
        }
    }
}
