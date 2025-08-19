using UnityEngine;

namespace UIKit
{
    public static class UIFactory
    {
        // Create a root Canvas and wrapper UIElement and optionally parent it
        public static UIElement CreateRoot(string name = "UIRoot", Transform parent = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            if (parent != null) go.transform.SetParent(parent, false);

            var wrapper = new GameObject("RootWrapper", typeof(UIElement));
            wrapper.transform.SetParent(go.transform, false);
            var element = wrapper.GetComponent<UIElement>();
            element.ApplyStyle(); // ensure defaults take effect
            return element;
        }

        // Attach a UI root underneath the in-game HUD canvas (HUDElements[2])
        public static UIElement CreateHudRoot(string name = "HudUI")
        {
            var hud = UnityEngine.Object.FindObjectOfType<HUDManager>();
            Transform parent = null;
            if (hud != null && hud.HUDElements != null && hud.HUDElements.Length > 2)
            {
                parent = hud.HUDElements[2].canvasGroup.transform.parent;
            }

            return CreateRoot(name, parent);
        }
    }
}
