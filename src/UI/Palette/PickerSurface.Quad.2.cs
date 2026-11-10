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
    }
}
