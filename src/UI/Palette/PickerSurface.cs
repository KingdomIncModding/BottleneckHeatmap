using KingdomFoundation.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    public sealed class PickerSurface : MaskableGraphic, IPointerDownHandler, IDragHandler
    {
        Color At(float x, float y)
        {
            if (Mode == PickerMode.SaturationValue)
                return Color.HSVToRGB(Hue, x, y);
            return Color.HSVToRGB(1 - y, 1, 1);
        }

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
                    Quad(mesh, new Rect(rect.xMin + x0 * rect.width, rect.yMin + y0 * rect.height, (x1 - x0) * rect.width, (y1 - y0) * rect.height), At(x0, y0), At(x1, y0), At(x1, y1), At(x0, y1));
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
                line.xMin += 1;
                line.xMax -= 1;
                line.yMin += 1;
                line.yMax -= 1;
                Quad(mesh, line, Color.white);
            }
        }

        public void OnDrag(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left)
                Select(data);
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left)
                Select(data);
        }

        static void Quad(VertexHelper mesh, Rect r, Color bl, Color br, Color tr, Color tl)
        {
            int i = mesh.currentVertCount;
            mesh.AddVert(new Vector3(r.xMin, r.yMin), bl, Vector2.zero);
            mesh.AddVert(new Vector3(r.xMax, r.yMin), br, Vector2.zero);
            mesh.AddVert(new Vector3(r.xMax, r.yMax), tr, Vector2.zero);
            mesh.AddVert(new Vector3(r.xMin, r.yMax), tl, Vector2.zero);
            mesh.AddTriangle(i, i + 1, i + 2);
            mesh.AddTriangle(i, i + 2, i + 3);
        }

        static void Quad(VertexHelper mesh, Rect r, Color color) => Quad(mesh, r, color, color, color, color);
        static void Ring(VertexHelper mesh, Vector2 center, float outer, float inner, Color outline, Color fill)
        {
            for (int layer = 0; layer < 2; layer++)
            {
                float a = layer == 0 ? outer : outer - 1, b = layer == 0 ? inner - 1 : inner;
                for (int n = 0; n < 24; n++)
                {
                    float start = n * Mathf.PI / 12, end = (n + 1) * Mathf.PI / 12;
                    Vector2 d0 = new Vector2(Mathf.Cos(start), Mathf.Sin(start)), d1 = new Vector2(Mathf.Cos(end), Mathf.Sin(end));
                    int i = mesh.currentVertCount;
                    Color c = layer == 0 ? outline : fill;
                    mesh.AddVert(center + d0 * a, c, Vector2.zero);
                    mesh.AddVert(center + d1 * a, c, Vector2.zero);
                    mesh.AddVert(center + d1 * b, c, Vector2.zero);
                    mesh.AddVert(center + d0 * b, c, Vector2.zero);
                    mesh.AddTriangle(i, i + 1, i + 2);
                    mesh.AddTriangle(i, i + 2, i + 3);
                }
            }
        }

        void Select(PointerEventData data)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, data.position, data.pressEventCamera, out var local))
            {
                var rect = rectTransform.rect;
                Changed?.Invoke(new Vector2(Mathf.Clamp01((local.x - rect.xMin) / rect.width), Mathf.Clamp01((local.y - rect.yMin) / rect.height)));
            }
        }
    }
}
