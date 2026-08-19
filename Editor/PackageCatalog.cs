using System.Collections.Generic;
using UnityEngine;

namespace KarlBanan.PackageInstaller
{
    [CreateAssetMenu(fileName = "PackageCatalog", menuName = "KarlBanan/Package Catalog")]
    public class PackageCatalog : ScriptableObject
    {
        [SerializeField] private List<PackageEntry> packages = new();

        public List<PackageEntry> Packages => packages;

        public PackageEntry Find(string packageName)
        {
            for(int i = 0; i < packages.Count; i++)
            {
                if (packages[i] != null && packages[i].PackageName == packageName) return packages[i];
            }
            return null;
        }
    }
}
