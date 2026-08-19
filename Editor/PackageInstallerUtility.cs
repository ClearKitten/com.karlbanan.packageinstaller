using System;
using UnityEditor;
using UnityEngine;

namespace KarlBanan.PackageInstaller
{
    public static class PackageInstallerUtility
    {
        public const float HEADER_HEIGHT = 48f;
        public const float TOOLBAR_HEIGHT = 36f;
        public const float TOP_AREA_HEIGHT = HEADER_HEIGHT + TOOLBAR_HEIGHT;

        private static Texture2D dotTexture;

        public static void DrawWindowBackground(Rect windowRect)
        {
            EditorGUI.DrawRect(new(0f, 0f, windowRect.width, windowRect.height), PackageInstallerStyles.WindowBackground);
        }

        public static void DrawCard(Rect rect, bool hover, Color color)
        {
            EditorGUI.DrawRect(rect, PackageInstallerStyles.CardBorder);

            Rect innerRect = new(rect.x + 1f, rect.y + 1f, rect.width - 2f, rect.height - 2f);

            EditorGUI.DrawRect(innerRect, hover ? PackageInstallerStyles.CardBackgroundHover : PackageInstallerStyles.CardBackground);
            EditorGUI.DrawRect(new(rect.x, rect.y, 3f, rect.height), color);
        }

        public static bool DrawTinyButton(Rect rect, string text)
        {
            bool hovered = rect.Contains(Event.current.mousePosition);

            EditorGUI.DrawRect(rect, hovered ? new(0.34f, 0.34f, 0.38f, 1f) : new(0.28f, 0.28f, 0.31f, 1f));
            GUI.Label(rect, text, PackageInstallerStyles.BadgeText);
            return GUI.Button(rect, GUIContent.none, GUIStyle.none);
        }

        public static bool DrawActionButton(Rect rect, string label, Color accentColor, bool enabled = true)
        {
            bool hovered = enabled && rect.Contains(Event.current.mousePosition);

            Color fill;

            if (!enabled) fill = new(0.2f, 0.2f, 0.22f, 1f);
            else if (hovered) fill = new(0.31f, 0.31f, 0.35f, 1f);
            else fill = new(0.26f, 0.26f, 0.29f, 1f);

            EditorGUI.DrawRect(rect, fill);
            EditorGUI.DrawRect(
                new(rect.x, rect.yMax - 1f, rect.width, 1f),
                enabled ? accentColor : PackageInstallerStyles.CardBorder
            );

            GUI.Label(rect, label, enabled ? PackageInstallerStyles.BadgeText : PackageInstallerStyles.DisabledBadgeText);

            if (!enabled) return false;
            return GUI.Button(rect, GUIContent.none, GUIStyle.none);
        }

        public static bool DrawCheckbox(Rect rect, bool value)
        {
            bool hovered = rect.Contains(Event.current.mousePosition);

            EditorGUI.DrawRect(rect, hovered ? new(0.3f, 0.3f, 0.34f, 1f) : new(0.16f, 0.16f, 0.18f, 1f));
            DrawOutline(rect, value ? PackageInstallerStyles.SelectionColor : PackageInstallerStyles.CardBorder, 1f);

            if (value) EditorGUI.DrawRect(AddPadding(rect, 4f), PackageInstallerStyles.SelectionColor);

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) return !value;
            return value;
        }

        public static void DrawBadge(Rect rect, string text, Color color)
        {
            EditorGUI.DrawRect(rect, color);
            GUI.Label(rect, text, PackageInstallerStyles.BadgeText);
        }

        public static void DrawDarkFieldBackground(Rect rect)
        {
            EditorGUI.DrawRect(rect, new(0.13f, 0.13f, 0.15f, 1f));
            EditorGUI.DrawRect(new(rect.x, rect.yMax - 1f, rect.width, 1f), PackageInstallerStyles.CardBorder);
        }

        public static void DrawOutline(Rect rect, Color color, float thickness)
        {
            EditorGUI.DrawRect(new(rect.x, rect.y, rect.width, thickness), color);
            EditorGUI.DrawRect(new(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new(rect.x, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        public static void DrawInfoPanel(string message)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 28f, GUILayout.ExpandWidth(true));
            rect = AddHorizontalPadding(rect, 10f);

            EditorGUI.DrawRect(rect, PackageInstallerStyles.InfoPanelBackground);
            EditorGUI.DrawRect(new(rect.x, rect.y, 3f, rect.height), PackageInstallerStyles.InfoAccent);

            GUI.Label(new(rect.x + 10f, rect.y + 6f, rect.width - 20f, 16f), message, PackageInstallerStyles.SecondaryLabel);
        }

        public static Rect AddHorizontalPadding(Rect rect, float padding)
            => new(rect.x + padding, rect.y, rect.width - padding * 2f, rect.height);

        public static Rect AddPadding(Rect rect, float padding)
            => new(rect.x + padding, rect.y + padding, rect.width - padding * 2f, rect.height - padding * 2f);

        public static bool PassesSearch(string searchText, string text)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return true;
            if (string.IsNullOrWhiteSpace(text)) return false;

            return text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static void DrawDot(Rect rect, Color color)
        {
            if (dotTexture == null) dotTexture = CreateCircleTexture(16);

            Color previousColor = GUI.color;

            GUI.color = color;
            GUI.DrawTexture(rect, dotTexture);
            GUI.color = previousColor;
        }

        private static Texture2D CreateCircleTexture(int size)
        {
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear
            };

            float radius = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new(x + 0.5f, y + 0.5f), new(radius, radius));
                    float alpha = Mathf.Clamp01(radius - distance);

                    texture.SetPixel(x, y, new(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return texture;
        }
    }
}