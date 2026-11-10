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
    public sealed partial class PickerSurface : MaskableGraphic, IPointerDownHandler, IDragHandler
    {
        internal PickerMode Mode;
        internal float Hue, Saturation = 1f, Brightness = 1f;
        internal Action<Vector2> Changed;


        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var rect = rectTransform.rect;
            int columns = Mode == PickerMode.SaturationValue ? 20 : 1;
            int rows = Mode == PickerMode.SaturationValue ? 16 : 24;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    float x0 = (float)x / columns, x1 = (float)(x + 1) / columns;
                    float y0 = (float)y / rows, y1 = (float)(y + 1) / rows;
                    Quad(mesh, new Rect(rect.xMin + x0 * rect.width, rect.yMin + y0 * rect.height,
                        (x1 - x0) * rect.width, (y1 - y0) * rect.height),
                        At(x0, y0), At(x1, y0), At(x1, y1), At(x0, y1));
                }
            }

            Vector2 marker;
            if (Mode == PickerMode.SaturationValue)
                marker = new Vector2(rect.xMin + Saturation * rect.width, rect.yMin + Brightness * rect.height);
            else
                marker = new Vector2(rect.center.x, rect.yMax - Hue * rect.height);

            if (Mode == PickerMode.SaturationValue)
            {
                Ring(mesh, marker, 6, 4, Color.black, Color.white);
            }
            else
            {
                var line = new Rect(rect.xMin - 2, marker.y - 3, rect.width + 4, 6);
                Quad(mesh, line, Color.black);
                line.xMin += 1; line.xMax -= 1; line.yMin += 1; line.yMax -= 1;
                Quad(mesh, line, Color.white);
            }
        }
    }
}
