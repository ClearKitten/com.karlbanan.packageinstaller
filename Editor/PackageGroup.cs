using System.Collections.Generic;
using UnityEngine;

namespace KarlBanan.PackageInstaller
{
    [CreateAssetMenu(fileName = "PackageGroup", menuName = "KarlBanan/Package Group")]
    public class PackageGroup : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField] private PackageCatalog catalog;

        [SerializeField] private List<string> packageNames = new();

        public string DisplayName
        {
            get => displayName;
            set => displayName = value;
        }

        public PackageCatalog Catalog
        {
            get => catalog;
            set => catalog = value;
        }

        public List<string> PackageNames => packageNames;

        public string Label => string.IsNullOrEmpty(displayName) ? name : displayName;

        public IEnumerable<PackageEntry> Resolve()
        {
            if (catalog == null) yield break;

            foreach(string name in packageNames)
            {
                PackageEntry entry = catalog.Find(name);
                if(entry != null) yield return entry;
            }
        }
    }
}
