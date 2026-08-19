using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KarlBanan.PackageInstaller
{
    public sealed class GroupsTab
    {
        private readonly List<PackageEntry> members = new();
        private readonly List<PackageEntry> missingMembers = new();
        private readonly List<PackageEntry> installedMembers = new();

        public Vector2 Draw(Vector2 scrollPosition, IReadOnlyList<PackageGroup> groups, string searchText)
        {
            PackageInstallerUtility.DrawInfoPanel("Groups of packages you usually install together. Create one via Assets > Create > KarlBanan > Package Group");

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            int visibleCount = 0;

            foreach (PackageGroup group in groups)
            {
                if (!PackageInstallerUtility.PassesSearch(searchText, group.Label)) continue;

                visibleCount++;
                DrawGroupCard(group);
            }

            if (groups.Count == 0) PackageInstallerUtility.DrawInfoPanel("No package groups were found in the project");
            else if (visibleCount == 0) PackageInstallerUtility.DrawInfoPanel("No groups matched the current search");

            EditorGUILayout.EndScrollView();
            return scrollPosition;
        }

        private void DrawGroupCard(PackageGroup group)
        {
            RebuildMembers(group);

            int lineCount = Mathf.Max(1, members.Count);
            float height = 34f + lineCount * 16f + 8f;

            Rect rect = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            rect = PackageInstallerUtility.AddHorizontalPadding(rect, 10f);

            bool hover = rect.Contains(Event.current.mousePosition);
            bool complete = members.Count > 0 && missingMembers.Count == 0;

            Color accent = complete ? PackageInstallerStyles.InstalledColor : PackageInstallerStyles.NeutralBadgeColor;
            PackageInstallerUtility.DrawCard(rect, hover, accent);

            Rect content = PackageInstallerUtility.AddPadding(rect, 8f);

            GUI.Label(
                new(content.x, content.y, Mathf.Max(60f, content.width - 262f), 18f),
                group.Label,
                PackageInstallerStyles.BoldLabel
            );

            bool idle = !PackageOps.IsBusy;
            float y = content.y - 1f;

            if (PackageInstallerUtility.DrawActionButton(
                    new(content.xMax - 250f, y, 86f, 20f),
                    $"Install ({missingMembers.Count})",
                    PackageInstallerStyles.InstalledColor,
                    idle && missingMembers.Count > 0))
            {
                PackageOps.EnqueueInstall(missingMembers);
            }

            if (PackageInstallerUtility.DrawActionButton(
                    new(content.xMax - 158f, y, 86f, 20f),
                    $"Update ({installedMembers.Count})",
                    PackageInstallerStyles.UpdateColor,
                    idle && installedMembers.Count > 0))
            {
                PackageOps.EnqueueInstall(installedMembers);
            }

            if (PackageInstallerUtility.DrawTinyButton(new(content.xMax - 66f, y, 66f, 20f), "Select"))
            {
                Selection.activeObject = group;
                EditorGUIUtility.PingObject(group);
            }

            float memberY = content.y + 24f;

            if (members.Count == 0)
            {
                GUI.Label(new(content.x + 4f, memberY, content.width - 8f, 14f), "This group is empty", PackageInstallerStyles.TinyMutedLabel);
                return;
            }

            foreach (PackageEntry entry in members)
            {
                bool installed = PackageOps.IsInstalled(entry.PackageName);

                PackageInstallerUtility.DrawDot(
                    new(content.x + 4f, memberY + 4f, 7f, 7f),
                    installed ? PackageInstallerStyles.InstalledColor : PackageInstallerStyles.NeutralBadgeColor
                );

                GUI.Label(
                    new(content.x + 18f, memberY, content.width - 22f, 14f),
                    entry.Label,
                    PackageInstallerStyles.TinyMutedLabel
                );

                memberY += 16f;
            }
        }

        private void RebuildMembers(PackageGroup group)
        {
            members.Clear();
            missingMembers.Clear();
            installedMembers.Clear();

            foreach (PackageEntry entry in group.Resolve())
            {
                if (!entry.IsValid) continue;

                members.Add(entry);

                if (PackageOps.IsInstalled(entry.PackageName)) installedMembers.Add(entry);
                else missingMembers.Add(entry);
            }
        }
    }
}