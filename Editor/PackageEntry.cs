using System;
using UnityEngine;

namespace KarlBanan.PackageInstaller
{
    [Serializable]
    public class PackageEntry 
    {
        [SerializeField] private string displayName;
        [SerializeField] private string packageName;
        [SerializeField] private string gitUrl;
        [SerializeField] private Sprite icon;

        [TextArea(1, 3)]
        [SerializeField] private string description;

        public string DisplayName => displayName;
        public string PackageName => packageName;
        public string GitUrl => gitUrl;
        public Texture2D Icon => icon != null ? icon.texture : null;
        public string Description => description;

        public bool IsValid => !string.IsNullOrEmpty(packageName) && !string.IsNullOrEmpty(gitUrl);
        public string Label => string.IsNullOrEmpty(displayName) ? packageName : displayName;
    }
}
