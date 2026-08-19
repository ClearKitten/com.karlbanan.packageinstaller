using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace KarlBanan.PackageInstaller
{
    public static class PackageOps
    {
        private const string PENDING_KEY = "KarlBanan.PackageInstaller.Pending";

        private static Queue<PackageOperation> pending = new();
        private static Dictionary<string, string> installed = new();

        private static AddRequest add;
        private static RemoveRequest remove;
        private static ListRequest list;
        private static bool refreshQueued;
        private static PackageOperation? current;

        public static event Action OnChanged;

        public static bool IsBusy => add != null || remove != null || list != null || pending.Count > 0;
        public static int PendingCount => pending.Count;
        public static string CurrentTarget => current?.Identifier;
        public static PackageOperationKind? CurrentKind => current?.Kind;

        public static bool IsInstalled(string packageName) => !string.IsNullOrEmpty(packageName) && installed.ContainsKey(packageName);

        public static string VersionOf(string packageName) => packageName != null && installed.TryGetValue(packageName, out string value) ? value : null;

        [InitializeOnLoadMethod]
        private static void RestorePending()
        {
            string saved = SessionState.GetString(PENDING_KEY, string.Empty);
            if (string.IsNullOrEmpty(saved)) return;

            foreach (string line in saved.Split('\n'))
            {
                if (PackageOperation.TryParse(line, out PackageOperation operation)) pending.Enqueue(operation); 
            }

            if (pending.Count == 0) return;

            refreshQueued = true;
            Hook();
        }

        public static void Refresh()
        {
            if (list != null) return;
            list = Client.List(true, false);
            Hook();
        }

        public static void EnqueueInstall(PackageEntry entry)
        {
            if(entry == null || !entry.IsValid)
            {
                Debug.LogWarning("PackageInstaller: Skipped entry with missing name or git URL");
                return;
            }

            Enqueue(new(PackageOperationKind.Install, entry.GitUrl));
        }

        public static void EnqueueInstall(IEnumerable<PackageEntry> entries)
        {
            foreach (PackageEntry entry in entries) EnqueueInstall(entry);
        }

        public static void EnqueueRemove(PackageEntry entry)
        {
            if (entry == null || !entry.IsValid) return;

            if (!IsInstalled(entry.PackageName))
            {
                Debug.LogWarning($"PackageInstaller: '{entry.PackageName}' is not installed and cannot be removed.");
                return;
            }

            Enqueue(new(PackageOperationKind.Remove, entry.PackageName));
        }

        public static void EnqueueRemove(IEnumerable<PackageEntry> entries)
        {
            foreach (PackageEntry entry in entries) EnqueueRemove(entry);
        }

        public static void Enqueue(PackageOperation operation)
        {
            if (!operation.IsValid) return;
            if (pending.Contains(operation)) return;
            if (current.HasValue && current.Value.Equals(operation)) return;

            pending.Enqueue(operation);
            SavePending();

            Hook();
            OnChanged?.Invoke();
        }

        private static void SavePending()
        {
            if (pending.Count == 0)
            {
                SessionState.EraseString(PENDING_KEY);
                return;
            }

            List<string> lines = new();
            foreach (PackageOperation operation in pending) lines.Add(operation.Serialize());

            SessionState.SetString(PENDING_KEY, string.Join("\n", lines));
        }

        private static void Hook()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            if (list != null)
            {
                if (!list.IsCompleted) return;

                if (list.Status == StatusCode.Success)
                {
                    installed.Clear();
                    foreach (var p in list.Result) installed[p.name] = p.version;
                }
                else
                {
                    Debug.LogError($"PackageInstaller: List failed: {list.Error.message}");
                }

                list = null;
                OnChanged?.Invoke();
            }

            if (add != null)
            {
                if (!add.IsCompleted) return;

                if (add.Status == StatusCode.Success) Debug.Log($"PackageInstaller: Installed {add.Result.displayName} @ {add.Result.version}");
                else Debug.LogError($"PackageInstaller: Failed to install '{CurrentTarget}': {add.Error.message}");
               

                add = null;
                current = null;
                refreshQueued = true;
                OnChanged?.Invoke();
            }

            if(remove != null)
            {
                if (!remove.IsCompleted) return;

                if (remove.Status == StatusCode.Success) Debug.Log($"PackageInstaller: Removed {remove.PackageIdOrName}");
                else Debug.LogError($"PackageInstaller: Failed to remove '{CurrentTarget}': {remove.Error.message}");

                remove = null;
                current = null;
                refreshQueued = true;
                OnChanged?.Invoke();
            }

            if (pending.Count > 0)
            {
                PackageOperation operation = pending.Dequeue();
                SavePending();

                current = operation;

                if (operation.Kind == PackageOperationKind.Install) add = Client.Add(operation.Identifier);
                else remove = Client.Remove(operation.Identifier);

                OnChanged?.Invoke();
                return;
            }

            if (refreshQueued)
            {
                refreshQueued = false;
                Refresh();
                return;
            }

            EditorApplication.update -= Tick;
            OnChanged?.Invoke();
        }
    }
}