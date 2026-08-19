using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KarlBanan.PackageInstaller
{
    public sealed class PackageInstallerWindow : EditorWindow
    {
        private const string CATALOG_PREF_KEY = "KarlBanan.PackageInstaller.Catalog";

        private PackageInstallerTab currentTab = PackageInstallerTab.Packages;

        private PackageCatalog catalog;
        private PackageGroup[] groups = Array.Empty<PackageGroup>();

        private string searchText;

        private Vector2 packagesScrollPosition;
        private Vector2 groupsScrollPosition;

        private readonly PackagesTab packagesTab = new();
        private readonly GroupsTab groupsTab = new();

        private readonly HashSet<string> selectedPackages = new();

        [MenuItem("Tools/KarlBanan/Package Installer")]
        public static void Open()
        {
            PackageInstallerWindow window = GetWindow<PackageInstallerWindow>("Package Installer");
            window.minSize = new(690f, 320f);
        }

        private void OnEnable()
        {
            PackageOps.OnChanged += Repaint;

            LoadCatalog();
            ReloadGroups();
            PackageOps.Refresh();

            wantsMouseMove = true;
        }

        private void OnDisable()
        {
            PackageOps.OnChanged -= Repaint;
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.MouseMove) Repaint();

            PackageInstallerStyles.Initialize();

            PackageInstallerUtility.DrawWindowBackground(position);
            DrawTopArea();

            GUILayout.Space(PackageInstallerUtility.TOP_AREA_HEIGHT + 8f);

            if (catalog == null)
            {
                PackageInstallerUtility.DrawInfoPanel("Assign a package catalog in the toolbar to get started");
                return;
            }

            switch (currentTab)
            {
                case PackageInstallerTab.Packages:
                    packagesScrollPosition = packagesTab.Draw(packagesScrollPosition, catalog, selectedPackages, searchText);
                    break;

                case PackageInstallerTab.Groups:
                    groupsScrollPosition = groupsTab.Draw(groupsScrollPosition, groups, searchText);
                    break;
            }

            DrawStatusBar();
        }

        private void DrawTopArea()
        {
            DrawHeader();
            DrawToolbar();
        }

        private void DrawHeader()
        {
            Rect rect = new(0f, 0f, position.width, PackageInstallerUtility.HEADER_HEIGHT);

            EditorGUI.DrawRect(rect, PackageInstallerStyles.HeaderBackground);
            EditorGUI.DrawRect(new(rect.x, rect.yMax - 2f, rect.width, 2f), PackageInstallerStyles.AccentDarkRed);

            GUI.Label(new(12f, 8f, 280f, 18f), "Package Installer", PackageInstallerStyles.Title);
            GUI.Label(new(12f, 26f, 460f, 14f), "Install and update your git packages without leaving the editor", PackageInstallerStyles.SubTitle);

            CountPackages(out int installedCount, out int totalCount);

            bool complete = totalCount > 0 && installedCount == totalCount;

            PackageInstallerUtility.DrawBadge(
                new(position.width - 108f, 12f, 92f, 20f),
                $"{installedCount} / {totalCount} installed",
                complete ? PackageInstallerStyles.InstalledColor : PackageInstallerStyles.NeutralBadgeColor
            );
        }

        private void DrawToolbar()
        {
            Rect rect = new(0f, PackageInstallerUtility.HEADER_HEIGHT, position.width, PackageInstallerUtility.TOOLBAR_HEIGHT);
            EditorGUI.DrawRect(rect, PackageInstallerStyles.ToolbarBackground);

            float x = 12f;
            float y = rect.y + 7f;
            float tabHeight = 20f;

            DrawTabButton(new(x, y, 78f, tabHeight), "Packages", PackageInstallerTab.Packages);
            x += 84f;

            DrawTabButton(new(x, y, 78f, tabHeight), "Groups", PackageInstallerTab.Groups);
            x += 84f;

            GUI.Label(new(x, y + 2f, 42f, 18f), "Search", PackageInstallerStyles.MutedLabel);
            x += 46f;

            float catalogLabelX = position.width - 286f;
            float searchWidth = Mathf.Max(100f, catalogLabelX - 8f - x);

            Rect searchRect = new(x, y, searchWidth, tabHeight);
            PackageInstallerUtility.DrawDarkFieldBackground(searchRect);

            searchText = EditorGUI.TextField(searchRect, searchText, EditorStyles.toolbarSearchField);

            GUI.Label(new(catalogLabelX, y + 2f, 48f, 18f), "Catalog", PackageInstallerStyles.MutedLabel);

            EditorGUI.BeginChangeCheck();

            PackageCatalog newCatalog = (PackageCatalog)EditorGUI.ObjectField(
                new(position.width - 234f, y, 150f, tabHeight),
                catalog,
                typeof(PackageCatalog),
                false
            );

            if (EditorGUI.EndChangeCheck()) SetCatalog(newCatalog);

            if (PackageInstallerUtility.DrawActionButton(
                    new(position.width - 76f, y, 64f, tabHeight),
                    "Refresh",
                    PackageInstallerStyles.InfoAccent,
                    !PackageOps.IsBusy))
            {
                ReloadGroups();
                PackageOps.Refresh();
            }
        }

        private void DrawTabButton(Rect rect, string label, PackageInstallerTab tab)
        {
            bool selected = currentTab == tab;
            bool hovered = rect.Contains(Event.current.mousePosition);

            Color background;

            if (selected) background = new(0.28f, 0.28f, 0.31f, 1f);
            else if (hovered) background = new(0.25f, 0.25f, 0.28f, 1f);
            else background = new(0.21f, 0.21f, 0.23f, 1f);

            EditorGUI.DrawRect(rect, background);

            if (selected) EditorGUI.DrawRect(new(rect.x, rect.yMax - 2f, rect.width, 2f), PackageInstallerStyles.AccentRed);
            else EditorGUI.DrawRect(new(rect.x, rect.yMax - 1f, rect.width, 1f), PackageInstallerStyles.CardBorder);

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) currentTab = tab;
            GUI.Label(rect, label, PackageInstallerStyles.BadgeText);
        }

        private void DrawStatusBar()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 26f, GUILayout.ExpandWidth(true));

            EditorGUI.DrawRect(rect, PackageInstallerStyles.ToolbarBackground);
            EditorGUI.DrawRect(new(rect.x, rect.y, rect.width, 1f), PackageInstallerStyles.CardBorder);

            bool busy = PackageOps.IsBusy;

            PackageInstallerUtility.DrawDot(
                new(rect.x + 12f, rect.y + 9f, 8f, 8f),
                busy ? PackageInstallerStyles.UpdateColor : PackageInstallerStyles.InstalledColor
            );

            string message;

            if (!busy) message = "Idle";
            else if (PackageOps.CurrentKind == null) message = "Reading installed packages";
            else if (PackageOps.CurrentKind == PackageOperationKind.Remove) message = $"Removing {PackageOps.CurrentTarget}  -  {PackageOps.PendingCount} queued";
            else message = $"Installing {PackageOps.CurrentTarget}  -  {PackageOps.PendingCount} queued";
            

            GUI.Label(
                new(rect.x + 26f, rect.y + 5f, rect.width - 38f, 16f),
                message,
                busy ? PackageInstallerStyles.SecondaryLabel : PackageInstallerStyles.MutedLabel
            );
        }

        private void CountPackages(out int installedCount, out int totalCount)
        {
            installedCount = 0;
            totalCount = 0;

            if (catalog == null) return;

            foreach (PackageEntry entry in catalog.Packages)
            {
                if (entry == null || !entry.IsValid) continue;

                totalCount++;
                if (PackageOps.IsInstalled(entry.PackageName)) installedCount++;
            }
        }

        private void SetCatalog(PackageCatalog newCatalog)
        {
            catalog = newCatalog;
            selectedPackages.Clear();

            if (catalog == null)
            {
                EditorPrefs.DeleteKey(CATALOG_PREF_KEY);
                return;
            }

            string path = AssetDatabase.GetAssetPath(catalog);
            EditorPrefs.SetString(CATALOG_PREF_KEY, AssetDatabase.AssetPathToGUID(path));
        }

        private void LoadCatalog()
        {
            string guid = EditorPrefs.GetString(CATALOG_PREF_KEY, null);

            if (!string.IsNullOrEmpty(guid))
            {
                catalog = AssetDatabase.LoadAssetAtPath<PackageCatalog>(AssetDatabase.GUIDToAssetPath(guid));
            }

            if (catalog != null) return;

            string[] found = AssetDatabase.FindAssets("t:PackageCatalog");
            if (found.Length > 0) catalog = AssetDatabase.LoadAssetAtPath<PackageCatalog>(AssetDatabase.GUIDToAssetPath(found[0]));
        }

        private void ReloadGroups()
        {
            string[] guids = AssetDatabase.FindAssets("t:PackageGroup");
            List<PackageGroup> found = new();

            foreach (string guid in guids)
            {
                PackageGroup group = AssetDatabase.LoadAssetAtPath<PackageGroup>(AssetDatabase.GUIDToAssetPath(guid));
                if (group != null) found.Add(group);
            }

            groups = found.ToArray();
        }
    }
}