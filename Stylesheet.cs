using System.Collections.Generic;
using UnityEngine;

namespace UIKit
{
    // Simple StyleSheet: store named styles and merge them for UIElements.
    public class StyleSheet
    {
        private readonly Dictionary<string, Style> classes = new Dictionary<string, Style>();

        public StyleSheet() { }

        public void AddClass(string name, Style style)
        {
            classes[name] = style;
        }

        public bool HasClass(string name) => classes.ContainsKey(name);

        public Style GetClass(string name)
        {
            if (classes.TryGetValue(name, out var s)) return s;
            return new Style();
        }

        // Merge multiple classes in order (later classes override earlier ones)
        public Style Merge(params string[] classNames)
        {
            var result = new Style();
            for (int i = 0; i < classNames.Length; i++)
            {
                var n = classNames[i];
                if (!classes.TryGetValue(n, out var s)) continue;
                MergeInto(result, s);
            }
            return result;
        }

        private void MergeInto(Style target, Style src)
        {
            // Only copy values that are not defaults to allow overriding.
            if (src.Width.Kind != UnitValue.Unit.Auto) target.Width = src.Width;
            if (src.Height.Kind != UnitValue.Unit.Auto) target.Height = src.Height;

            if (src.Padding != Vector4.zero) target.Padding = src.Padding;
            if (src.Margin != Vector4.zero) target.Margin = src.Margin;

            if (src.Background.a > 0f) target.Background = src.Background;
            if (src.TextColor != Color.clear) target.TextColor = src.TextColor;
            if (src.FontSize != 0) target.FontSize = src.FontSize;

            if (src.FlexGrow != 0) target.FlexGrow = src.FlexGrow;
            if (src.FlexBasis.Kind != UnitValue.Unit.Auto) target.FlexBasis = src.FlexBasis;

            target.Align = src.Align;
        }
    }
}
