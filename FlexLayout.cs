using System.Collections.Generic;
using UnityEngine;

namespace UIKit
{
    // Very small, easy-to-use flexbox-like layout for direct children.
    // Attach to a RectTransform and it will lay out direct children with UIElement wrappers / styles.
    public class FlexLayout : MonoBehaviour
    {
        public enum Direction { Row, Column }
        public Direction direction = Direction.Row;
        public TextAnchor alignItems = TextAnchor.MiddleCenter;
        public TextAnchor justifyContent = TextAnchor.MiddleLeft;
        public float gap = 4f; // pixels

        // If true, FlexLayout will resize its own RectTransform along the main axis to fit children.
        // Useful for scroll content: set FitContent = true on the content wrapper.
        public bool FitContent = false;

        RectTransform rt;
        void Awake() => rt = GetComponent<RectTransform>();

        void OnEnable() => Layout();
        void OnTransformChildrenChanged() => Layout();
#if UNITY_EDITOR
        void Update() { if (!Application.isPlaying) Layout(); }
#endif

        public void Layout()
        {
            if (rt == null) rt = GetComponent<RectTransform>();
            List<RectTransform> children = new List<RectTransform>();
            for (int i = 0; i < rt.childCount; i++)
            {
                var c = rt.GetChild(i) as RectTransform;
                if (c == null) continue;
                children.Add(c);
            }

            float width = rt.rect.width;
            float height = rt.rect.height;

            bool isRow = direction == Direction.Row;

            // Measure fixed sizes (basis) and flex grow total
            float totalFixed = 0f;
            int totalFlex = 0;
            var childInfos = new List<(RectTransform child, Style style, float basisPx)>();

            foreach (var c in children)
            {
                var uiWrap = c.GetComponent<UIElement>();
                Style s = uiWrap != null ? uiWrap.Style : new Style();
                float basis = 0f;
                if (s.FlexBasis.Kind != UnitValue.Unit.Auto)
                {
                    basis = s.FlexBasis.ToPixels(width, height);
                }
                else
                {
                    // fall back to explicit Width/Height if given
                    if (isRow && s.Width.Kind != UnitValue.Unit.Auto) basis = s.Width.ToPixels(width, height);
                    if (!isRow && s.Height.Kind != UnitValue.Unit.Auto) basis = s.Height.ToPixels(width, height);
                }

                childInfos.Add((c, s, basis));
                if (s.FlexGrow > 0) totalFlex += s.FlexGrow;
                else totalFixed += basis;
            }

            // Respect per-child order (if present)
            childInfos.Sort((a, b) => a.style.Order.CompareTo(b.style.Order));

            float available = (isRow ? width : height) - totalFixed - gap * (children.Count - 1);
            float perFlexUnit = totalFlex > 0 ? Mathf.Max(0f, available / totalFlex) : 0f;

            // compute total children size (fixed + total flex size) for justification
            float totalChildrenSize = totalFixed + (totalFlex > 0 ? perFlexUnit * totalFlex : 0f) + gap * (children.Count - 1);

            // If FitContent is true, resize our rect along main axis to fit totalChildrenSize
            if (FitContent)
            {
                if (isRow)
                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalChildrenSize);
                else
                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalChildrenSize);
                // update cached sizes
                width = rt.rect.width;
                height = rt.rect.height;
            }

            // Positioning
            float cursor = 0f;
            // justifyContent basic handling
            if (justifyContent == TextAnchor.MiddleCenter || justifyContent == TextAnchor.UpperCenter || justifyContent == TextAnchor.LowerCenter)
                cursor = ((isRow ? width : height) - totalChildrenSize) / 2f;

            if (justifyContent == TextAnchor.MiddleRight || justifyContent == TextAnchor.UpperRight || justifyContent == TextAnchor.LowerRight)
                cursor = (isRow ? width : height) - totalChildrenSize;

            for (int i = 0; i < childInfos.Count; i++)
            {
                var info = childInfos[i];
                float sizeAlong = info.basisPx;
                if (info.style.FlexGrow > 0) sizeAlong = perFlexUnit * info.style.FlexGrow;

                float crossSize = isRow ? (info.style.Height.Kind != UnitValue.Unit.Auto ? info.style.Height.ToPixels(width, height) : rt.rect.height) :
                                         (info.style.Width.Kind != UnitValue.Unit.Auto ? info.style.Width.ToPixels(width, height) : rt.rect.width);

                RectTransform c = info.child;

                // set size
                if (isRow)
                    c.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sizeAlong);
                else
                    c.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeAlong);

                if (isRow)
                    c.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, crossSize);
                else
                    c.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, crossSize);

                // pivot/anchor center-left to make positioning easier
                c.anchorMin = c.anchorMax = new Vector2(0f, 1f);
                c.pivot = new Vector2(0f, 1f);

                // compute x/y inside parent
                Vector2 anchoredPos;
                if (isRow)
                {
                    float x = cursor + info.style.Margin.x;
                    float y = -info.style.Margin.y; // anchoredPosition uses negative y downwards

                    // choose alignment per-child if specified, otherwise use container alignItems
                    TextAnchor align = info.style.AlignSelf; // style-level align-self
                    if (align == default(TextAnchor)) align = alignItems;

                    // vertical alignment
                    if (align == TextAnchor.UpperLeft || align == TextAnchor.UpperCenter || align == TextAnchor.UpperRight) y = -info.style.Margin.y;
                    if (align == TextAnchor.MiddleLeft || align == TextAnchor.MiddleCenter || align == TextAnchor.MiddleRight) y = -(rt.rect.height - crossSize) / 2f - info.style.Margin.y;
                    if (align == TextAnchor.LowerLeft || align == TextAnchor.LowerCenter || align == TextAnchor.LowerRight) y = - (rt.rect.height - crossSize) + info.style.Margin.y;

                    anchoredPos = new Vector2(x, y);
                    cursor += sizeAlong + gap;
                }
                else
                {
                    float y = -cursor - info.style.Margin.y;
                    float x = info.style.Margin.x;

                    TextAnchor align = info.style.AlignSelf;
                    if (align == default(TextAnchor)) align = alignItems;

                    // horizontal alignment
                    if (align == TextAnchor.UpperLeft || align == TextAnchor.MiddleLeft || align == TextAnchor.LowerLeft) x = info.style.Margin.x;
                    if (align == TextAnchor.UpperCenter || align == TextAnchor.MiddleCenter || align == TextAnchor.LowerCenter) x = (rt.rect.width - crossSize) / 2f + info.style.Margin.x;
                    if (align == TextAnchor.UpperRight || align == TextAnchor.MiddleRight || align == TextAnchor.LowerRight) x = rt.rect.width - crossSize - info.style.Margin.x;
                    anchoredPos = new Vector2(x, y);
                    cursor += sizeAlong + gap;
                }

                c.anchoredPosition = anchoredPos;
            }
        }
    }
}
