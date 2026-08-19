using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KarlBanan.PackageInstaller
{
    public sealed class PackagesTab
    {
        private const float ROW_HEIGHT = 46f;
        private const float ROW_HEIGHT_WITH_DESCRIPTION = 62f;
        private const float RIGHT_COLUMN_WIDTH = 216f;

        private readonly List<PackageEntry> validEntries = new();
        private readonly List<PackageEntry> missingEntries = new();
        private readonly List<PackageEntry> installedEntries = new();
        private readonly List<PackageEntry> selectedEntries = new();
        private readonly List<PackageEntry> selectedInstalledEntries = new();

        private readonly List<PackageEntry> visibleEntries = new();

        public Vector2 Draw(Vector2 scrollPosition, PackageCatalog catalog, HashSet<string> selectedPackages, string searchText)
        {
            RebuildBuckets(catalog, selectedPackages, searchText);
            DrawBulkActions(selectedPackages);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (PackageEntry entry in visibleEntries) DrawPackageRow(entry, selectedPackages);

            if (validEntries.Count == 0)
            {
                PackageInstallerUtility.DrawInfoPanel("The catalog has no usable entries. Every entry needs a package name and a git URL");
            }
            else if (visibleEntries.Count == 0)
            {
                PackageInstallerUtility.DrawInfoPanel("No packages matched the current search");
            }

            EditorGUILayout.EndScrollView();
            return scrollPosition;
        }

        private void RebuildBuckets(PackageCatalog catalog, HashSet<string> selectedPackages, string searchText)
        {
            validEntries.Clear();
            visibleEntries.Clear();
            missingEntries.Clear();
            installedEntries.Clear();
            selectedEntries.Clear();
            selectedInstalledEntries.Clear();

            foreach (PackageEntry entry in catalog.Packages)
            {
                if (entry == null || !entry.IsValid) continue;

                validEntries.Add(entry);

                bool installed = PackageOps.IsInstalled(entry.PackageName);

                if (installed) installedEntries.Add(entry);
                else missingEntries.Add(entry);

                if (selectedPackages.Contains(entry.PackageName))
                {
                    selectedEntries.Add(entry);
                    if (installed) selectedInstalledEntries.Add(entry);
                }

                if(PackageInstallerUtility.PassesSearch(searchText, entry.Label)
                    || PackageInstallerUtility.PassesSearch(searchText, entry.PackageName)
                    || PackageInstallerUtility.PassesSearch(searchText, entry.Description))
                {
                    visibleEntries.Add(entry);
                }
            }
        }

        private void DrawBulkActions(HashSet<string> selectedPackages)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 32f, GUILayout.ExpandWidth(true));
            rect = PackageInstallerUtility.AddHorizontalPadding(rect, 10f);

            bool idle = !PackageOps.IsBusy;

            float y = rect.y + 6f;
            float x = rect.x;

            if (PackageInstallerUtility.DrawActionButton(
                    new(x, y, 108, 20f),
                    $"Install All ({missingEntries.Count})",
                    PackageInstallerStyles.InstalledColor,
                    idle && missingEntries.Count > 0))
            {
                PackageOps.EnqueueInstall(missingEntries);
            }

            x += 114f;

            if (PackageInstallerUtility.DrawActionButton(
                    new(x, y, 108, 20f),
                    $"Update All ({installedEntries.Count})",
                    PackageInstallerStyles.UpdateColor,
                    idle && installedEntries.Count > 0))
            {
                PackageOps.EnqueueInstall(installedEntries);
            }

            x += 114f;

            if (PackageInstallerUtility.DrawActionButton(
                    new(x, y, 132, 20f),
                    $"Install Selected ({selectedEntries.Count})",
                    PackageInstallerStyles.SelectionColor,
                    idle && selectedEntries.Count > 0))
            {
                PackageOps.EnqueueInstall(selectedEntries);
                selectedPackages.Clear();
            }

            x += 138f;

            if(PackageInstallerUtility.DrawActionButton(
                    new(x, y, 138f, 20f),
                    $"Remove Selected ({selectedInstalledEntries.Count})",
                    PackageInstallerStyles.AccentRed,
                    idle && selectedInstalledEntries.Count > 0))
            {
                if (ConfirmRemoval(selectedInstalledEntries.Count))
                {
                    PackageOps.EnqueueRemove(selectedInstalledEntries);
                    selectedPackages.Clear();
                }
            }

            bool canSelectMore = false;

            foreach(PackageEntry entry in visibleEntries)
            {
                if (selectedPackages.Contains(entry.PackageName)) continue;

                canSelectMore = true;
                break;
            }

            if(PackageInstallerUtility.DrawActionButton(
                new(rect.xMax - 152f, y, 86f, 20f),
                $"Select All ({visibleEntries.Count})",
                PackageInstallerStyles.SelectionColor,
                canSelectMore))
            {
                foreach (PackageEntry entry in visibleEntries) selectedPackages.Add(entry.PackageName);
            }

            if(PackageInstallerUtility.DrawActionButton(
                new(rect.xMax - 60f, y, 60f, 20f),
                "Clear",
                PackageInstallerStyles.NeutralBadgeColor,
                selectedEntries.Count > 0))
            {
                selectedPackages.Clear();
            }

        }

        private static bool ConfirmRemoval(int count)
        {
            return EditorUtility.DisplayDialog(
                "Remove Packages",
                count == 1
                    ? "Remove the selected package from this project?"
                    : $"Remove {count} selected packages from this project?",
                "Remove",
                "Cancel"
            );
        }

        private static void DrawPackageRow(PackageEntry entry, HashSet<string> selectedPackages)
        {
            bool hasDescription = !string.IsNullOrWhiteSpace(entry.Description);
            float height = hasDescription ? ROW_HEIGHT_WITH_DESCRIPTION : ROW_HEIGHT;

            Rect rect = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            rect = PackageInstallerUtility.AddHorizontalPadding(rect, 10f);

            bool installed = PackageOps.IsInstalled(entry.PackageName);
            bool hover = rect.Contains(Event.current.mousePosition);

            Color accent = installed ? PackageInstallerStyles.InstalledColor : PackageInstallerStyles.NeutralBadgeColor;
            PackageInstallerUtility.DrawCard(rect, hover, accent);

            Rect content = PackageInstallerUtility.AddPadding(rect, 8f);

            bool wasSelected = selectedPackages.Contains(entry.PackageName);
            bool isSelected = PackageInstallerUtility.DrawCheckbox(new(content.x, content.y + 7f, 16f, 16f), wasSelected);

            if (isSelected != wasSelected)
            {
                if (isSelected) selectedPackages.Add(entry.PackageName);
                else selectedPackages.Remove(entry.PackageName);
            }

            float textX = content.x + 26f;

            if (entry.Icon != null)
            {
                GUI.DrawTexture(new(content.x + 24f, content.y + 1f, 28f, 28f), entry.Icon, ScaleMode.ScaleToFit);
                textX = content.x + 58f;
            }

            float textWidth = Mathf.Max(60f, content.xMax - textX - 142f);

            GUI.Label(new(textX, content.y, textWidth, 18f), entry.Label, PackageInstallerStyles.BoldLabel);

            string subText = installed
                ? $"{entry.PackageName}  -  {PackageOps.VersionOf(entry.PackageName)}"
                : entry.PackageName;

            GUI.Label(new(textX, content.y + 16f, textWidth, 14f), subText, PackageInstallerStyles.TinyMutedLabel);

            if (hasDescription)
            {
                GUI.Label(new(textX, content.y + 30f, textWidth, 14f),
                    new GUIContent(entry.Description, entry.Description),
                    PackageInstallerStyles.DescriptionLabel
                );
            }

            PackageInstallerUtility.DrawBadge(
                new(content.xMax - 136f, content.y + 6f, 62f, 18f),
                installed ? "Installed" : "Missing",
                accent
            );

            bool idle = !PackageOps.IsBusy;

            if (PackageInstallerUtility.DrawActionButton(
                    new(content.xMax - 142f, content.y + 5f, 68f, 20f),
                    installed ? "Update" : "Install",
                    installed ? PackageInstallerStyles.UpdateColor : PackageInstallerStyles.InstalledColor,
                    idle))
            {
                PackageOps.EnqueueInstall(entry);
            }

            if(PackageInstallerUtility.DrawActionButton(
                new(content.xMax- 68f, content.y + 5f, 68f, 20f),
                "Remove",
                PackageInstallerStyles.AccentRed,
                idle && installed))
            {
                PackageOps.EnqueueRemove(entry);
                selectedPackages.Remove(entry.PackageName);
            }
        }
    }
}