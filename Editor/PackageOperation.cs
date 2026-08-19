using System;
using UnityEngine;

namespace KarlBanan.PackageInstaller
{
    public readonly struct PackageOperation : IEquatable<PackageOperation>
    {
        private const char SEPERATOR = '|';

        public readonly PackageOperationKind Kind;
        public readonly string Identifier;

        public PackageOperation(PackageOperationKind kind, string identifier)
        {
            Kind = kind;
            Identifier = identifier;
        }

        public bool IsValid => !string.IsNullOrEmpty(Identifier);

        public string Serialize() => $"{(int)Kind}{SEPERATOR}{Identifier}";

        public static bool TryParse(string text, out PackageOperation operation)
        {
            operation = default;

            if (string.IsNullOrEmpty(text)) return false;

            int index = text.IndexOf(SEPERATOR);
            if (index <= 0 || index >= text.Length - 1) return false;

            if (!int.TryParse(text[..index], out int kind)) return false;
            if (!Enum.IsDefined(typeof(PackageOperationKind), kind)) return false;

            operation = new((PackageOperationKind)kind, text[(index + 1)..]);
            return true;
        }

        public bool Equals(PackageOperation other) => Kind == other.Kind && Identifier == other.Identifier;
        public override bool Equals(object obj) => obj is PackageOperation other && Equals(other);
        public override int GetHashCode() => (Identifier?.GetHashCode() ?? 0) ^ (int)Kind;
    }
}
