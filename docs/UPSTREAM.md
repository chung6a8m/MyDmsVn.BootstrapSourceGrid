# Upstream and vendor policy

## 1. Purpose

This repository integrates two independently maintained vendors. This document pins the initial baselines and defines how they are consumed, patched, upgraded, and distributed.

## 2. Pinned baselines

### Bootstrap framework

- Repository: `chung6a8m/MyDmsVn.Bootstrap5WinFormUI`
- Baseline commit: `cceba3c969e28726935793a1c6ca3772bed60a35`
- Role: theme, colors, metrics, typography, rendering helpers, DPI helpers, design conventions
- Required TFM compatibility: `net48;net8.0-windows`

Important integration APIs at this baseline include:

- `BootstrapThemeManager.CurrentTheme`
- `BootstrapThemeManager.ThemeChanged`
- `BootstrapTheme.Colors`
- `BootstrapTheme.Metrics`
- `BootstrapTheme.Typography`
- `BootstrapFontToken`
- `DpiScaler`
- `ColorUtil`

`BootstrapDataGridView` is a useful reference for theme subscription, theme-owned font lifetime, contrast color use, DPI scaling, and disposal patterns. It is not the base class for this project.

### SourceGrid

- Repository: `chung6a8m/sourcegrid`
- Baseline commit: `f4e457b43582bf01892f50bdc74aa480531e5944`
- Product line: SourceGrid 5.0 modernization
- Role: grid engine, concrete/virtual grid foundation, cells, Views, Editors, Controllers, selection, spans, scrolling
- Required TFM compatibility: `net48;net8.0-windows`

SourceGrid 5.0 explicitly treats its historical public API as a compatibility constraint. This project must preserve that posture.

## 3. Repository layout

Development/pre-release implementation uses Git submodules:

```text
vendor/
  Bootstrap5WinFormUI/
  sourcegrid/
```

Expected setup:

```powershell
git submodule add https://github.com/chung6a8m/MyDmsVn.Bootstrap5WinFormUI.git vendor/Bootstrap5WinFormUI
git -C vendor/Bootstrap5WinFormUI checkout cceba3c969e28726935793a1c6ca3772bed60a35

git submodule add https://github.com/chung6a8m/sourcegrid.git vendor/sourcegrid
git -C vendor/sourcegrid checkout f4e457b43582bf01892f50bdc74aa480531e5944
```

After cloning this repository:

```powershell
git submodule update --init --recursive
```

The superproject commit records the submodule commit IDs; no script should silently move submodules to vendor default branches.

## 4. Project references during development

Development/pre-release product builds reference vendor projects directly so compile-time API mismatches are visible. From `src/MyDmsVn.BootstrapSourceGrid/MyDmsVn.BootstrapSourceGrid.csproj` the expected relative paths are:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\vendor\Bootstrap5WinFormUI\src\MyDmsVn.Bootstrap5WinFormUI\MyDmsVn.Bootstrap5WinFormUI.csproj" />
  <ProjectReference Include="..\..\vendor\sourcegrid\SourceGrid\SourceGrid.csproj" />
</ItemGroup>
```

Paths must be verified during Stage 0 after submodules are initialized. Do not copy vendor source files into `src/MyDmsVn.BootstrapSourceGrid`.

This ProjectReference/submodule mode is the approved temporary development strategy (decision 2B / D-017). It is not the final public NuGet dependency graph.

## 5. Vendor modification policy

Default: **no modifications inside submodules from this repository**.

If implementation reveals a blocker:

1. create a focused failing integration test;
2. identify the missing/incorrect vendor hook;
3. demonstrate why inheritance, SourceGrid Views, public/protected hooks, or Bootstrap helper APIs cannot solve it externally;
4. open/implement the smallest patch in the owning vendor repository;
5. add vendor-side regression tests;
6. update the vendor commit in this repository only after the patch exists in the vendor repository;
7. update this document with old/new baseline and rationale.

Never commit a dirty submodule state as an undocumented integration patch.

## 6. Upgrade procedure

Vendor upgrades are explicit maintenance tasks, not incidental updates.

For either vendor:

1. record current baseline;
2. inspect vendor release/commit changes affecting public APIs used by this integration;
3. move only one vendor baseline at a time when practical;
4. restore/build integration for both TFMs;
5. run all automated tests;
6. run demo/manual matrix for theme, editing, selection, scrolling, spans, keyboard, Designer, and DPI;
7. compare public package/API behavior;
8. record new baseline and any required migration note;
9. commit submodule pointer plus documentation changes together.

## 7. Bootstrap vendor upgrade watch list

Pay special attention to changes in:

- `BootstrapThemeManager` event semantics;
- `BootstrapTheme` shape;
- color token names/meaning;
- typography/font ownership patterns;
- `DpiScaler` behavior;
- `ColorUtil` accessibility/contrast logic;
- WinForms lifecycle conventions in framework controls.

## 8. SourceGrid vendor upgrade watch list

Pay special attention to changes in:

- `Grid` / `GridVirtual` inheritance or lifecycle;
- `CustomScrollControl` behavior;
- cell View/VisualModel APIs;
- editor lifecycle and control ownership;
- selection/active position semantics;
- controller ordering;
- row/column/span behavior;
- painting invalidation and shared View assumptions;
- project TFM/dependency changes.

## 9. Approved public NuGet dependency strategy

Public distribution uses strategy **2A** from project-owner decision D-017.

Before publishing `MyDmsVn.BootstrapSourceGrid`, both vendor dependencies must be available as resolvable NuGet packages with exact versions and verified equivalence to the tested source baselines, or to explicitly approved upgraded baselines.

For each vendor package record:

```text
PackageId
Version
Feed
Supported TFMs
Verified source commit/baseline
Public API/behavior equivalence result
Transitive dependency review
License/notice review
```

The public release build must use those approved `PackageReference` dependencies rather than unresolved vendor `ProjectReference` dependencies.

A package transition is accepted only when:

- the published package corresponds to a tested vendor baseline;
- the package supports both `net48` and `net8.0-windows`;
- public APIs used by integration match the verified source baseline;
- transitive dependencies are acceptable;
- demo, Designer, and automated tests pass against package references;
- reproducible version pins and feeds are documented;
- a clean consumer restore succeeds without checking out the vendor source repositories.

Do not mix source project references and package references to different versions of the same vendor in one build graph. Do not reference both project and package copies of the same vendor assembly simultaneously.

If either vendor package is unavailable or unverified, remain in temporary strategy **2B** for development/source distribution and do **not** publish a public BootstrapSourceGrid NuGet package yet.

Never solve missing package dependencies by silently embedding/copying vendor source or assemblies into `MyDmsVn.BootstrapSourceGrid.nupkg`.

## 10. Licensing

The integration repository/package uses the MIT license (D-016); see root `LICENSE`.

This license choice applies to this repository's own integration source only. Before first public package release, verify and document redistribution/dependency obligations for both vendor packages/repositories.

Do not copy vendor license text into this repository unless the packaging/legal requirement actually calls for it. Retain required notices through the normal dependency/package mechanism or an explicitly documented notice file where required.

The release is blocked if vendor license/notice obligations for the approved package dependency graph have not been verified.

## 11. Public package verification status (2026-09-09)

Public publication is currently **blocked**. Verification against the configured feeds and the official repository state produced this evidence:

| Vendor | Source metadata at pinned commit | Feed availability | Commit/package correspondence | License/notice result |
|---|---|---|---|---|
| Bootstrap5WinFormUI | `PackageId=MyDmsVn.Bootstrap5WinFormUI`; `Version=1.0.0-rc.1`; `net48;net8.0-windows` | No exact package on NuGet.org or the configured internal feed; no GitHub release | Cannot verify because no candidate package is available | Blocked: the pinned repository has no license file and the project has no package license metadata |
| SourceGrid | SDK-default `PackageId=SourceGrid`; `Version=5.0.0`; `net48;net8.0-windows` | NuGet.org exposes only legacy `SourceGrid 4.4.0`; neither `5.0.0` nor another exact candidate exists on the configured internal feed; no GitHub release | `4.4.0` predates and is not equivalent to fork commit `f4e457b43582bf01892f50bdc74aa480531e5944` | Source baseline includes `SourceGrid/SourceGrid.License.txt`, an MIT-style license requiring preservation of its copyright and permission notice; package-level notice handling remains unverified without a 5.0.0 candidate |

Official feed evidence:

- `https://api.nuget.org/v3-flatcontainer/mydmsvn.bootstrap5winformui/index.json` returned 404.
- `https://api.nuget.org/v3-flatcontainer/sourcegrid/index.json` listed only `4.4.0`.
- `https://www.nuget.org/packages/SourceGrid/` identifies `4.4.0` as the sole gallery version and as a .NET Framework 3.5 asset, not the dual-target pinned fork.
- `https://github.com/chung6a8m/MyDmsVn.Bootstrap5WinFormUI/releases` and `https://github.com/chung6a8m/sourcegrid/releases` contained no releases.

The SourceGrid source baseline declares `Microsoft.Data.SqlClient`, `Microsoft.Windows.Compatibility`, and `System.Text.Json`, plus `System.Resources.Extensions` for `net48`. Bootstrap5WinFormUI declares no package dependency at its pinned project file. These source-graph observations do not substitute for review of an actual release package's transitive graph.

Required unblock action: publish or otherwise provide exact vendor package candidates tied to the approved commits (or explicitly approve upgraded baselines), add/clarify the Bootstrap vendor license, then repeat TFM/API/behavior/transitive/license verification and clean-consumer validation. Until then, submodule plus `ProjectReference` remains the only approved build graph and local `.nupkg` files are inspection artifacts only.
