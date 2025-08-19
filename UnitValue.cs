using System;
using UnityEngine;

namespace UIKit
{
    // Represents a length/size unit that can be px, vw, vh, or percent.
    [Serializable]
    public struct UnitValue
    {
        public enum Unit { Pixels, Vw, Vh, Percent, Auto }

        public float Value;
        public Unit Kind;

        public UnitValue(float value, Unit kind)
        {
            Value = value;
            Kind = kind;
        }

        public static UnitValue Pixels(float px) => new UnitValue(px, Unit.Pixels);
        public static UnitValue Vw(float v) => new UnitValue(v, Unit.Vw);
        public static UnitValue Vh(float v) => new UnitValue(v, Unit.Vh);
        public static UnitValue Percent(float p) => new UnitValue(p, Unit.Percent);
        public static UnitValue Auto() => new UnitValue(0f, Unit.Auto);

        // Parse strings like "100px", "10vw", "8vh", "50%"
        public static UnitValue Parse(string s)
        {
            if (string.IsNullOrEmpty(s)) return Auto();
            s = s.Trim().ToLowerInvariant();
            if (s.EndsWith("px")) return Pixels(float.Parse(s.Substring(0, s.Length - 2)));
            if (s.EndsWith("vw")) return Vw(float.Parse(s.Substring(0, s.Length - 2)));
            if (s.EndsWith("vh")) return Vh(float.Parse(s.Substring(0, s.Length - 2)));
            if (s.EndsWith("%")) return Percent(float.Parse(s.Substring(0, s.Length - 1)));
            // bare number = pixels
            if (float.TryParse(s, out var v)) return Pixels(v);
            return Auto();
        }

        // Convert to pixels using screen size (Canvas pixel size)
        public float ToPixels(float referenceWidth, float referenceHeight)
        {
            switch (Kind)
            {
                case Unit.Vw: return Value * referenceWidth / 100f;
                case Unit.Vh: return Value * referenceHeight / 100f;
                case Unit.Percent: return Value * referenceWidth / 100f; // percent of reference width by default
                case Unit.Pixels: return Value;
                default: return Value;
            }
        }

        public override string ToString() => $"{Value}{Kind}";
    }

    // Simple style container with common properties
    [Serializable]
    public class Style
    {
        public UnitValue Width = UnitValue.Auto();
        public UnitValue Height = UnitValue.Auto();

        public Vector4 Padding = Vector4.zero; // left, top, right, bottom (px)
        public Vector4 Margin = Vector4.zero;  // left, top, right, bottom (px)

        public Color Background = new Color(0, 0, 0, 0); // default transparent
        public Color TextColor = Color.white;
        public int FontSize = 14;

        // Flex properties
        public int FlexGrow = 0;
        public UnitValue FlexBasis = UnitValue.Auto();
        public TextAnchor Align = TextAnchor.MiddleCenter;

        public Style() { }

        public Style WidthPx(float px) { Width = UnitValue.Pixels(px); return this; }
        public Style HeightPx(float px) { Height = UnitValue.Pixels(px); return this; }
        public Style WidthVw(float vw) { Width = UnitValue.Vw(vw); return this; }
        public Style HeightVh(float vh) { Height = UnitValue.Vh(vh); return this; }
        public Style BackgroundColor(Color c) { Background = c; return this; }
        public Style TextColorIs(Color c) { TextColor = c; return this; }
        public Style FontSizeIs(int size) { FontSize = size; return this; }
        public Style Flex(int grow, string basis = null)
        {
            FlexGrow = grow;
            if (!string.IsNullOrEmpty(basis)) FlexBasis = UnitValue.Parse(basis);
            return this;
        }
    }
}
