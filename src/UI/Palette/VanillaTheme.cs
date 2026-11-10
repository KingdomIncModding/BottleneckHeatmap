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
    internal static partial class VanillaTheme
    {
        public static Sprite WindowSprite;
        public static Sprite ButtonSprite;
        public static Sprite CloseButtonSprite;
        public static TMP_FontAsset Font;
        public static Material FontMaterial;
        public static bool Initialized;


        public static void Ensure()
        {
            if (Initialized && WindowSprite != null && ButtonSprite != null && Font != null) return;
            Initialized = true;

            try
            {
                // 1. Text / Font from any active TextMeshPro in the game
                var texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
                foreach (var t in texts)
                {
                    if (t != null && t.font != null && t.font.name != "LiberationSans SDF" && t.font.name != "Arial")
                    {
                        Font = t.font;
                        FontMaterial = t.fontSharedMaterial;
                        break;
                    }
                }

                // 2. OptionsUI elements
                var options = Resources.FindObjectsOfTypeAll<Assets.Behaviour.UI.MainMenu.OptionsUI>().FirstOrDefault();
                if (options != null)
                {
                    var optImg = options.GetComponent<Image>();
                    if (optImg != null && optImg.sprite != null)
                    {
                        WindowSprite = optImg.sprite;
                    }

                    var buttons = options.GetComponentsInChildren<Button>(true);
                    var apply = buttons.FirstOrDefault(b => b.name == "Apply" || b.name == "Cancel" || b.name == "KeyBindings");
                    if (apply != null)
                    {
                        var bImg = apply.GetComponent<Image>();
                        if (bImg != null && bImg.sprite != null) ButtonSprite = bImg.sprite;
                    }

                    var close = buttons.FirstOrDefault(b => b.name == "Close" || b.name == "Back" || b.name.StartsWith("Close"));
                    if (close != null)
                    {
                        var cImg = close.GetComponent<Image>();
                        if (cImg != null && cImg.sprite != null) CloseButtonSprite = cImg.sprite;
                    }
                }

                // 3. Fallback to popups across the game
                if (WindowSprite == null)
                {
                    var popups = Resources.FindObjectsOfTypeAll<Assets.Behaviour.UI.Shared.UIPopupMenu>();
                    foreach (var p in popups)
                    {
                        var img = p.GetComponent<Image>() ?? p.GetComponentInChildren<Image>(true);
                        if (img != null && img.sprite != null)
                        {
                            WindowSprite = img.sprite;
                            break;
                        }
                    }
                }

                // 4. Fallback search all sliced images
                if (WindowSprite == null || ButtonSprite == null || CloseButtonSprite == null)
                {
                    var allImgs = Resources.FindObjectsOfTypeAll<Image>();
                    foreach (var img in allImgs)
                    {
                        if (img == null || img.sprite == null) continue;
                        string s = img.sprite.name.ToLowerInvariant();
                        if (WindowSprite == null && (s.Contains("window") || s.Contains("dialog") || s.Contains("modal") || s.Contains("frame_bg")))
                            WindowSprite = img.sprite;
                        if (ButtonSprite == null && (s.Contains("button") || s.Contains("btn")) && img.type == Image.Type.Sliced)
                            ButtonSprite = img.sprite;
                        if (CloseButtonSprite == null && (s.Contains("close") || s.Contains("exit") || s.Contains("cross")))
                            CloseButtonSprite = img.sprite;
                    }
                }
            }
            catch { }
        }
    }
}
