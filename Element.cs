using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIKit
{
    // Lightweight UI element wrapper with fluent styling helpers and CSS-class support.
    [RequireComponent(typeof(RectTransform))]
    public class UIElement : MonoBehaviour
    {
        public Style Style = new Style();
        RectTransform rt;
        Image img;
        Text txt;

        // CSS-like class list that can be resolved with a StyleSheet
        private readonly List<string> classList = new List<string>();

        void Awake()
        {
            rt = GetComponent<RectTransform>();
        }

        public UIElement SetStyle(Style s)
        {
            Style = s;
            ApplyStyle();
            return this;
        }

        public UIElement AddClass(string className, StyleSheet sheet = null)
        {
            if (!classList.Contains(className)) classList.Add(className);
            if (sheet != null) ApplyClasses(sheet);
            else ApplyStyle();
            return this;
        }

        public UIElement RemoveClass(string className, StyleSheet sheet = null)
        {
            classList.Remove(className);
            if (sheet != null) ApplyClasses(sheet);
            else ApplyStyle();
            return this;
        }

        public void ApplyClasses(StyleSheet sheet)
        {
            if (sheet == null) { ApplyStyle(); return; }
            var merged = sheet.Merge(classList.ToArray());
            // create a merged style with current inline style overriding the class merged style
            var final = merged;
            // inline style should override class values, so merge inline into final
            sheet.Merge(); // no-op to keep flow readable
            // now merge inline Style into final by simple copy of non-defaults
            if (Style.Width.Kind != UnitValue.Unit.Auto) final.Width = Style.Width;
            if (Style.Height.Kind != UnitValue.Unit.Auto) final.Height = Style.Height;
            if (Style.Padding != Vector4.zero) final.Padding = Style.Padding;
            if (Style.Margin != Vector4.zero) final.Margin = Style.Margin;
            if (Style.Background.a > 0f) final.Background = Style.Background;
            if (Style.TextColor != Color.clear) final.TextColor = Style.TextColor;
            if (Style.FontSize != 0) final.FontSize = Style.FontSize;
            if (Style.FlexGrow != 0) final.FlexGrow = Style.FlexGrow;
            if (Style.FlexBasis.Kind != UnitValue.Unit.Auto) final.FlexBasis = Style.FlexBasis;
            final.Align = Style.Align;

            Style = final;
            ApplyStyle();
        }

        public void ApplyStyle()
        {
            if (rt == null) rt = GetComponent<RectTransform>();
            Canvas canvas = GetComponentInParent<Canvas>();
            float refW = Screen.width;
            float refH = Screen.height;
            if (canvas != null) { refW = canvas.pixelRect.width; refH = canvas.pixelRect.height; }

            if (Style.Width.Kind != UnitValue.Unit.Auto)
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Style.Width.ToPixels(refW, refH));
            if (Style.Height.Kind != UnitValue.Unit.Auto)
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Style.Height.ToPixels(refW, refH));

            if (Style.Background.a > 0f)
            {
                img = gameObject.GetComponent<Image>() ?? gameObject.AddComponent<Image>();
                img.color = Style.Background;
            }

            txt = gameObject.GetComponent<Text>();
            if (txt != null)
            {
                txt.color = Style.TextColor != Color.clear ? Style.TextColor : Color.white;
                txt.fontSize = Style.FontSize > 0 ? Style.FontSize : 14;
                txt.alignment = Style.Align;
            }
        }

        // Anchor & position helpers using UnitValue
        public UIElement AnchorTo(Vector2 anchor, Vector2 pivot)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            return this;
        }

        public UIElement SetPosition(UnitValue x, UnitValue y)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            float refW = Screen.width;
            float refH = Screen.height;
            if (canvas != null) { refW = canvas.pixelRect.width; refH = canvas.pixelRect.height; }

            float px = x.ToPixels(refW, refH);
            float py = y.ToPixels(refW, refH);
            rt.anchoredPosition = new Vector2(px, py);
            return this;
        }

        // Convenience setters
        public UIElement SetBackground(Color c) { Style.Background = c; ApplyStyle(); return this; }
        public UIElement SetSize(string w, string h) { Style.Width = UnitValue.Parse(w); Style.Height = UnitValue.Parse(h); ApplyStyle(); return this; }
        public UIElement SetFlex(int grow, string basis = null) { Style.FlexGrow = grow; if (basis != null) Style.FlexBasis = UnitValue.Parse(basis); return this; }

        // Add a child panel
        public UIElement AddPanel(string name = "Panel")
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UIElement));
            go.transform.SetParent(this.transform, false);
            var el = go.GetComponent<UIElement>();
            el.ApplyStyle();
            return el;
        }

        // Add a text label
        public Text AddLabel(string text, int fontSize = 14)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(this.transform, false);
            var t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Style.TextColor != Color.clear ? Style.TextColor : Color.white;
            return t;
        }

        // Add an image element
        public Image AddImage(Color col)
        {
            var go = new GameObject("Image", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(this.transform, false);
            var i = go.GetComponent<Image>();
            i.color = col;
            return i;
        }

        // Add a button with a label and click callback
        public Button AddButton(string label, System.Action onClick)
        {
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(this.transform, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.15f, 0.16f, 0.18f, 1f);
            var btn = go.GetComponent<Button>();
            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txtGo.transform.SetParent(go.transform, false);
            var t = txtGo.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.text = label;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            btn.onClick.AddListener(() => onClick?.Invoke());
            return btn;
        }
    }
}
