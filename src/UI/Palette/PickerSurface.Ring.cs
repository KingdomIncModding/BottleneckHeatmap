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
        static void Ring(VertexHelper mesh, Vector2 center, float outer, float inner, Color outline, Color fill)
        {
            for (int layer = 0; layer < 2; layer++)
            {
                float a = layer == 0 ? outer : outer - 1, b = layer == 0 ? inner - 1 : inner;
                for (int n = 0; n < 24; n++)
                {
                    float start = n * Mathf.PI / 12, end = (n + 1) * Mathf.PI / 12;
                    Vector2 d0 = new Vector2(Mathf.Cos(start), Mathf.Sin(start)), d1 = new Vector2(Mathf.Cos(end), Mathf.Sin(end));
                    int i = mesh.currentVertCount; Color c = layer == 0 ? outline : fill;
                    mesh.AddVert(center + d0 * a, c, Vector2.zero);
                    mesh.AddVert(center + d1 * a, c, Vector2.zero);
                    mesh.AddVert(center + d1 * b, c, Vector2.zero);
                    mesh.AddVert(center + d0 * b, c, Vector2.zero);
                    mesh.AddTriangle(i, i + 1, i + 2);
                    mesh.AddTriangle(i, i + 2, i + 3);
                }
            }
        }
    }
}
