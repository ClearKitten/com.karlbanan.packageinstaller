# Changelog

All notable changes to this package will be documented in this file.

## [1.0.0] - 2026-08-19

### Added

- Inital release of KarlBanan Package Installer.
- Added `PackageEntry` for single catalog entry with display name, package name, git URL, icon and description.
- Added `PackageCatalog` ScriptableObject for storing package entries.
- Added `PackageGroup` ScriptableObject for defining sets of packages installed together.
- Added `PackageGroupEditor` for picking group members from the assigned catalog as a checklist.
- Added `PackageInstallerWindow` with Packages and Groups tabs, opened from `Tools > KarlBanan > Package Installer`.
- Added `PackagesTab` for listing catalog entries with per row install, update and remove actions.
- Added `GroupsTab` for listing package groups with per group install and update actions.
- Added `PackageOps` for running package operations against the Unity Package Manager.
- Added `PackageOperation` and `PackageOperationKind` for representing queued install and remove opertations.
- Added sequential operation queueing so bulk actions run one request at a time.
- Added queue persistence through `SessionState` so bulk operation continue across domain reloads.
- Added installed state and version detection through an offline package list request.
- Added Install All and Update All actions for the whole catalog.
- Added checkbox selection with Select All, Clear, Install Selected and Remove Selected actions.
- Added removal confirmation before any remove operation is queued.
- Added search filtering across display name, package name and description.
- Added description display on package rows with the full description as a tooltip.
- Added a status bar showing the current operation and the number of queued operations. 
- Added `PackageInstallerStyles` for shared colors and GUI styles.
- Added `PackageInstallerUtility` for shared card, badge, button, checkbox and dot drawing.
- Added `PackageInstallerTab` for the available window tabs. 