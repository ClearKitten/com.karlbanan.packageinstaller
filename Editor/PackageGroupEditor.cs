using UnityEditor;
using UnityEngine;

namespace KarlBanan.PackageInstaller
{
    [CustomEditor(typeof(PackageGroup))]
    public class PackageGroupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            PackageGroup group = (PackageGroup)target;

            EditorGUI.BeginChangeCheck();

            string displayName = EditorGUILayout.TextField("Display Name", group.DisplayName);
            PackageCatalog catalog = (PackageCatalog)EditorGUILayout.ObjectField("Catalog", group.Catalog, typeof(PackageCatalog), false);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(group, "Edit Package Group");
                group.DisplayName = displayName;
                group.Catalog = catalog;
                EditorUtility.SetDirty(group);
            }

            if(group.Catalog == null)
            {
                EditorGUILayout.HelpBox("Assign a catalog to pick packages", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Packages", EditorStyles.boldLabel);

            foreach(PackageEntry entry in group.Catalog.Packages)
            {
                if (entry == null || string.IsNullOrEmpty(entry.PackageName)) continue;

                bool wasIn = group.PackageNames.Contains(entry.PackageName);
                bool isIn = EditorGUILayout.ToggleLeft(entry.Label, wasIn);

                if (isIn == wasIn) continue;

                Undo.RecordObject(group, "Toggle Package In Group");
                if (isIn) group.PackageNames.Add(entry.PackageName);
                else group.PackageNames.Remove(entry.PackageName);
                EditorUtility.SetDirty(group);
            }
        }
    }
}
