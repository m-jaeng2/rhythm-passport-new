using UnityEngine;

namespace RhythmPassport.UI
{
    public static class SimpleScreenStyles
    {
        private static GUIStyle titleStyle;
        private static GUIStyle bodyStyle;
        private static GUIStyle accentStyle;
        private static GUIStyle panelStyle;
        private static GUIStyle centeredBodyStyle;
        private static GUIStyle statusStyle;
        private static GUIStyle subtitleStyle;
        private static GUIStyle scoreStyle;

        public static GUIStyle TitleStyle => titleStyle ??= CreateStyle(34, FontStyle.Bold, TextAnchor.UpperCenter, Color.white);

        public static GUIStyle BodyStyle => bodyStyle ??= CreateStyle(20, FontStyle.Normal, TextAnchor.UpperLeft, Color.white);

        public static GUIStyle AccentStyle => accentStyle ??= CreateStyle(24, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.53f, 0.9f, 0.72f));

        public static GUIStyle CenteredBodyStyle => centeredBodyStyle ??= CreateStyle(20, FontStyle.Normal, TextAnchor.UpperCenter, Color.white);

        public static GUIStyle StatusStyle => statusStyle ??= CreateStyle(18, FontStyle.Bold, TextAnchor.MiddleCenter, Color.black);

        public static GUIStyle SubtitleStyle => subtitleStyle ??= CreateStyle(18, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.86f, 0.92f, 0.98f));

        public static GUIStyle ScoreStyle => scoreStyle ??= CreateStyle(34, FontStyle.Bold, TextAnchor.UpperCenter, Color.white);

        public static GUIStyle PanelStyle
        {
            get
            {
                if (panelStyle != null)
                {
                    return panelStyle;
                }

                var texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, new Color(0.08f, 0.12f, 0.18f, 0.88f));
                texture.Apply();

                panelStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = texture },
                    padding = new RectOffset(24, 24, 24, 24),
                };
                return panelStyle;
            }
        }

        private static GUIStyle CreateStyle(int fontSize, FontStyle fontStyle, TextAnchor alignment, Color color)
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                fontStyle = fontStyle,
                alignment = alignment,
                wordWrap = true,
                normal = { textColor = color },
            };
        }

        public static void DrawProgressBar(Rect rect, float progress)
        {
            var clamped = Mathf.Clamp01(progress);
            var previousColor = GUI.color;

            GUI.color = new Color(0.12f, 0.16f, 0.2f, 1f);
            GUI.Box(rect, GUIContent.none);

            GUI.color = new Color(0.53f, 0.9f, 0.72f, 1f);
            GUI.Box(new Rect(rect.x, rect.y, rect.width * clamped, rect.height), GUIContent.none);

            GUI.color = previousColor;
        }

        public static void DrawStatusBadge(Rect rect, string text, Color backgroundColor)
        {
            var previousColor = GUI.color;
            GUI.color = backgroundColor;
            GUI.Box(rect, GUIContent.none);
            GUI.color = previousColor;
            GUI.Label(rect, text, StatusStyle);
        }

        public static void DrawInfoCard(Rect rect, string title, string body)
        {
            GUI.Box(rect, GUIContent.none, PanelStyle);
            GUI.Label(new Rect(rect.x + 16, rect.y + 14, rect.width - 32, 24), title, SubtitleStyle);
            GUI.Label(new Rect(rect.x + 16, rect.y + 44, rect.width - 32, rect.height - 56), body, BodyStyle);
        }

        public static void DrawCameraFrame(Rect rect, Texture texture, string title, string fallbackText)
        {
            GUI.Box(rect, GUIContent.none, PanelStyle);
            GUI.Label(new Rect(rect.x + 16, rect.y + 14, rect.width - 32, 24), title, SubtitleStyle);

            var contentRect = new Rect(rect.x + 16, rect.y + 48, rect.width - 32, rect.height - 64);

            if (texture != null)
            {
                GUI.DrawTexture(contentRect, texture, ScaleMode.ScaleToFit, false);
                return;
            }

            GUI.Label(contentRect, fallbackText, CenteredBodyStyle);
        }
    }
}
