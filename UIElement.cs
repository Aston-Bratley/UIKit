using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIKit
{
    // Consolidated UIElement combining features from Wrapper.cs and Element.cs
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
            rt = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
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

        // Anchor / pivot helpers
        public UIElement SetAnchors(Vector2 min, Vector2 max) { rt.anchorMin = min; rt.anchorMax = max; return this; }
        public UIElement SetPivot(Vector2 p) { rt.pivot = p; return this; }
        public UIElement SetAnchorsFill() { rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; return this; }
        public UIElement SetAnchorsCenter(float w, float h) { rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w); rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h); return this; }

        // Anchor presets
        public UIElement AnchorTopLeft() { rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f); rt.pivot = new Vector2(0f, 1f); return this; }
        public UIElement AnchorTopCenter() { rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f); rt.pivot = new Vector2(0.5f, 1f); return this; }
        public UIElement AnchorTopRight() { rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f); rt.pivot = new Vector2(1f, 1f); return this; }
        public UIElement AnchorMiddleCenter() { rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f); return this; }
        public UIElement AnchorBottomLeft() { rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f); rt.pivot = new Vector2(0f, 0f); return this; }
        public UIElement AnchorStretchHorizontal() { rt.anchorMin = new Vector2(0f, rt.anchorMin.y); rt.anchorMax = new Vector2(1f, rt.anchorMax.y); return this; }
        public UIElement AnchorStretchVertical() { rt.anchorMin = new Vector2(rt.anchorMin.x, 0f); rt.anchorMax = new Vector2(rt.anchorMax.x, 1f); return this; }

        // Convenience setters
        public UIElement SetBackground(Color c) { Style.Background = c; ApplyStyle(); return this; }
        public UIElement SetSize(string w, string h) { Style.Width = UnitValue.Parse(w); Style.Height = UnitValue.Parse(h); ApplyStyle(); return this; }
        public UIElement SetSizePx(float w, float h) { Style.Width = UnitValue.Pixels(w); Style.Height = UnitValue.Pixels(h); ApplyStyle(); return this; }
        public UIElement SetRelativeSize(float wPercent, float hPercent) { Style.Width = UnitValue.Percent(wPercent); Style.Height = UnitValue.Percent(hPercent); ApplyStyle(); return this; }
        public UIElement SetFlex(int grow, string basis = null) { Style.FlexGrow = grow; if (basis != null) Style.FlexBasis = UnitValue.Parse(basis); return this; }
        public UIElement FillParent() { SetAnchorsFill(); return this; }
        public UIElement CenterInParent(float w, float h) { SetAnchorsCenter(w, h); return this; }

        // Click and z-order helpers
        public UIElement OnClick(System.Action action)
        {
            var image = gameObject.GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0); // invisible but needed for Button hit area
            var btn = gameObject.GetComponent<Button>() ?? gameObject.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => action?.Invoke());
            return this;
        }

        public UIElement BringToFront() { transform.SetAsLastSibling(); return this; }
        public UIElement SendToBack() { transform.SetAsFirstSibling(); return this; }

        // Add a child element
        public UIElement AddPanel(string name = "Panel")
        {
            var go = new GameObject(name, typeof(UIElement));
            go.transform.SetParent(this.transform, false);
            var el = go.GetComponent<UIElement>();
            return el;
        }

        // Create a label as a child and return the Text component (keeps existing behavior)
        public Text AddLabel(string text, int fontSize = 14)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(this.transform, false);
            var t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Style.TextColor;
            return t;
        }

        // Create a label but return the UIElement wrapper for fluent styling
        public UIElement AddLabelElement(string text, int fontSize = 14)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(UIElement));
            go.transform.SetParent(this.transform, false);
            var el = go.GetComponent<UIElement>();
            var t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = el.Style.TextColor;
            el.ApplyStyle();
            return el;
        }

        public Image AddImage(Color col)
        {
            var go = new GameObject("Image", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(this.transform, false);
            var i = go.GetComponent<Image>();
            i.color = col;
            return i;
        }

        // Returns the Button component (existing behavior)
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

        // AddButton that returns the wrapper for the created button so you can chain styles
        public UIElement AddButtonElement(string label, System.Action onClick)
        {
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(UIElement));
            go.transform.SetParent(this.transform, false);
            var el = go.GetComponent<UIElement>();
            var img = go.GetComponent<Image>();
            img.color = new Color(0.15f, 0.16f, 0.18f, 1f);
            var btn = go.GetComponent<Button>();
            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txtGo.transform.SetParent(go.transform, false);
            var t = txtGo.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.text = label;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = el.Style.TextColor;
            t.fontSize = el.Style.FontSize;
            btn.onClick.AddListener(() => onClick?.Invoke());
            el.ApplyStyle();
            return el;
        }

        // Add a ScrollView as a child and return the content UIElement for populating.
        // vertical: true => vertical scrolling, content uses Column flex direction by default.
        public UIElement AddScrollView(string name = "ScrollView", bool vertical = true)
        {
            // Root scroll container (ScrollRect lives here)
            var go = new GameObject(name, typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            go.transform.SetParent(this.transform, false);
            var goRT = go.GetComponent<RectTransform>();
            goRT.anchorMin = new Vector2(0f, 1f);
            goRT.anchorMax = new Vector2(1f, 1f);
            goRT.pivot = new Vector2(0.5f, 1f);

            var bgImg = go.GetComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0); // transparent background

            var scroll = go.GetComponent<ScrollRect>();
            scroll.horizontal = !vertical;
            scroll.vertical = vertical;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = true;

            // Viewport (mask)
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(go.transform, false);
            var vpRT = viewport.GetComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;

            var vpImg = viewport.GetComponent<Image>();
            vpImg.color = new Color(1, 1, 1, 0); // invisible but needed by Mask

            var vpMask = viewport.GetComponent<Mask>();
            vpMask.showMaskGraphic = false;

            // Content (where items go)
            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(UIElement));
            contentGo.transform.SetParent(viewport.transform, false);
            var contentRT = contentGo.GetComponent<RectTransform>();

            // Content anchors/pivot: top-left anchored so vertical scroll expands downwards
            if (vertical)
            {
                contentRT.anchorMin = new Vector2(0f, 1f);
                contentRT.anchorMax = new Vector2(1f, 1f);
                contentRT.pivot = new Vector2(0.5f, 1f);
                contentRT.anchoredPosition = Vector2.zero;
            }
            else
            {
                contentRT.anchorMin = new Vector2(0f, 0f);
                contentRT.anchorMax = new Vector2(0f, 1f);
                contentRT.pivot = new Vector2(0f, 0.5f);
                contentRT.anchoredPosition = Vector2.zero;
            }

            // Attach FlexLayout to the content by default for easy list creation
            var flex = contentGo.AddComponent<FlexLayout>();
            flex.direction = vertical ? FlexLayout.Direction.Column : FlexLayout.Direction.Row;
            flex.gap = 4f;
            flex.FitContent = true; // content will size to children so ScrollRect can work
            var el = contentGo.GetComponent<UIElement>();
            el.ApplyStyle();

            // Wire up ScrollRect
            scroll.viewport = vpRT;
            scroll.content = contentRT;

            return el;
        }
    }
}
