# Upstream and vendor policy

## 1. Purpose

This repository integrates two independently maintained vendors. This document pins the initial baselines and defines how they are consumed, patched, and upgraded.

## 2. Pinned baselines

### Bootstrap framework

- Repository: `chung6a8m/MyDmsVn.Bootstrap5WinFormUI`
- Baseline commit: `95077df0c8bad8593143c2190606d2f444bfc653`
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

Initial implementation uses Git submodules:

```text
vendor/
  Bootstrap5WinFormUI/
  sourcegrid/
```

Expected setup:

```powershell
git submodule add https://github.com/chung6a8m/MyDmsVn.Bootstrap5WinFormUI.git vendor/Bootstrap5WinFormUI
git -C vendor/Bootstrap5WinFormUI checkout 95077df0c8bad8593143c2190606d2f444bfc653

git submodule add https://github.com/chung6a8m/sourcegrid.git vendor/sourcegrid
git -C vendor/sourcegrid checkout f4e457b43582bf01892f50bdc74aa480531e5944
```

After cloning this repository:

```powershell
git submodule update --init --recursive
```

The superproject commit records the submodule commit IDs; no script should silently move submodules to vendor default branches.

## 4. Project references

Initial product project references vendor projects directly so compile-time API mismatches are visible:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\..\vendor\Bootstrap5WinFormUI\src\MyDmsVn.Bootstrap5WinFormUI\MyDmsVn.Bootstrap5WinFormUI.csproj" />
  <ProjectReference Include="..\..\..\vendor\sourcegrid\SourceGrid\SourceGrid.csproj" />
</ItemGroup>
```

Paths must be verified during Stage 0 after submodules are initialized. Do not copy vendor source files into `src/MyDmsVn.BootstrapSourceGrid`.

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

## 9. Migration to NuGet packages

A future release may replace one or both project references with NuGet dependencies. This is allowed only when:

- a published package corresponds to a tested vendor baseline;
- package supports both required TFMs;
- public APIs used by integration match the verified source baseline;
- transitive dependencies are acceptable;
- demo, Designer, and automated tests pass against package references;
- reproducible version pins are documented.

Do not mix source project references and package references to different versions of the same vendor in one build graph.

## 10. Licensing

Before first public package release, verify and document redistribution/license obligations for both vendor packages/repositories. Do not copy vendor license text into this repository unless the packaging/legal requirement actually calls for it; retain required notices through the normal dependency/package mechanism.
