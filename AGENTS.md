# AGENTS.md

Repository-level operating contract for Codex and other coding agents working on `MyDmsVn.BootstrapSourceGrid`.

## 1. Mandatory read order

Before changing product code, read these files in order:

1. `README.md`
2. `AI_CONTEXT.md`
3. `docs/README.md`
4. `docs/DECISIONS.md`
5. `docs/PRD.md`
6. `docs/ARCHITECTURE.md`
7. `docs/UPSTREAM_API_SEAMS.md`
8. `docs/UPSTREAM.md`
9. `docs/COMPATIBILITY.md`
10. `docs/TESTING.md`
11. `docs/DEVELOPMENT_PLAN.md`
12. The active implementation plan under `docs/plans/`
13. Relevant upstream implementation/tests at the pinned vendor commits

Read `docs/PENDING_DECISIONS.md` before release/packaging work. The initial license and dependency-strategy choices are already resolved there; do not reopen them unless the user explicitly requests a change.

Do not start implementation from an isolated code snippet without reading the active plan and the upstream APIs it touches.

## 2. Fixed project constraints

Do not change these without explicit user approval:

- Repository: `chung6a8m/MyDmsVn.BootstrapSourceGrid`
- Product assembly/package name: `MyDmsVn.BootstrapSourceGrid`
- Public control name: `BootstrapSourceGrid`
- Public control namespace: `MyDmsVn.Bootstrap5WinFormUI.Controls`
- Direct base type: `SourceGrid.Grid`
- Target frameworks: `net48;net8.0-windows`
- UI technology: native Windows Forms
- Repository/package license: MIT
- Bootstrap vendor baseline: `chung6a8m/MyDmsVn.Bootstrap5WinFormUI@95077df0c8bad8593143c2190606d2f444bfc653`
- SourceGrid vendor baseline: `chung6a8m/sourcegrid@f4e457b43582bf01892f50bdc74aa480531e5944`
- Development dependency strategy: pinned Git submodules + `ProjectReference` (temporary 2B)
- Public NuGet dependency strategy: exact verified vendor NuGet packages via `PackageReference` (2A)
- Initial scrollbar policy: retain SourceGrid/native scrollbar behavior
- Initial editor policy: style/harden existing editors; do not replace the full SourceGrid editor model

## 3. Dependency direction and release dependency policy

Required dependency graph:

```text
MyDmsVn.BootstrapSourceGrid
    |-- MyDmsVn.Bootstrap5WinFormUI
    `-- SourceGrid
```

Forbidden dependency directions:

```text
SourceGrid -> MyDmsVn.Bootstrap5WinFormUI
MyDmsVn.Bootstrap5WinFormUI -> SourceGrid
Either vendor -> MyDmsVn.BootstrapSourceGrid
```

Development/pre-release builds use exact vendor submodules and project references. Public NuGet release must switch to resolvable exact-version vendor packages that are verified against the tested source baselines (or explicitly approved upgraded baselines) on both TFMs.

Do not publish `MyDmsVn.BootstrapSourceGrid` while either vendor package identity/version/feed/source correspondence is unresolved. Do not embed/copy vendor assemblies or source into the integration package merely to avoid a missing dependency. Never reference both a project and package copy of the same vendor assembly in one effective build graph.

Do not modify a vendor solely to make the integration easier unless the active plan explicitly identifies a proven blocker and the change is first isolated in the appropriate vendor repository.

## 4. SourceGrid compatibility rules

SourceGrid is the grid engine and retains ownership of:

- grid storage and virtual/concrete cell mechanics;
- rows, columns, ranges, positions, spans;
- selection and active-position behavior;
- keyboard/mouse navigation;
- controllers;
- editors;
- scrolling;
- painting pipeline and cell Views/VisualModels.

The integration must preserve SourceGrid's public programming model. Do not add wrappers such as `BootstrapRow`, `BootstrapColumn`, or `BootstrapCell` just to rename existing SourceGrid abstractions.

Prefer extension by inheritance, Views, styles, adapters, and documented public hooks. Avoid overriding broad painting/interaction behavior when a narrower SourceGrid extension point exists.

A verified SourceGrid seam matters here: normal `grid[row, column] = cell` assignment reaches a private `InsertCell`, so do not pretend a subclass can intercept every assignment through `SetCell`. The approved MVP plan lazily substitutes only known default View singletons through virtual `GetCell(...)`, preserving unknown/custom consumer Views. See `docs/UPSTREAM_API_SEAMS.md` and Stage 2 plan before changing this mechanism.

## 5. Bootstrap framework reuse rules

Bootstrap5WinFormUI owns the visual semantics. Reuse its existing infrastructure rather than duplicating it:

- `BootstrapThemeManager.CurrentTheme`
- `BootstrapThemeManager.ThemeChanged`
- `BootstrapTheme.Colors`
- `BootstrapTheme.Metrics`
- `BootstrapTheme.Typography`
- `DpiScaler`
- existing semantic color/variant helpers when applicable

Do not hard-code Bootstrap hex colors when a theme token exists. Do not create a second global theme manager in this repository.

Follow the same theme-font ownership rule as the framework: if this control creates a `Font`, it owns/disposes that instance; if a consumer explicitly assigns a font, do not dispose the consumer's font.

## 6. Public API rules

- Keep the integration API intentionally small.
- Bootstrap-specific properties/events must describe Bootstrap semantics, not duplicate SourceGrid APIs.
- Do not shadow a SourceGrid public member with a different meaning.
- Do not silently alter default SourceGrid editing, selection, keyboard, clipboard, or span behavior.
- New public members require XML documentation and tests for observable behavior.
- A breaking change requires explicit approval and a migration note.

## 7. Rendering and resource rules

- Prefer SourceGrid Views/VisualModels for cell/header rendering integration.
- Do not replace the complete SourceGrid painting engine to achieve theming.
- Use semantic theme tokens for colors/metrics/typography.
- Scale Bootstrap-owned pixel metrics using `DpiScaler`.
- Avoid repeated per-cell/per-paint allocation when a shared immutable View/style object is safe.
- Dispose every owned `Pen`, `Brush`, `Font`, `GraphicsPath`, `Bitmap`, `Region`, or other GDI resource.
- Do not dispose resources owned by SourceGrid or application code.
- Invalidate only the necessary surface when practical.

## 8. Theme lifecycle rules

- Subscribe to runtime theme changes only while the control is alive.
- Unsubscribe in `Dispose(bool)`.
- Theme changes must update already-created Bootstrap-owned/shared Views and repaint the grid without rebuilding application data.
- Designer construction must not require explicit application theme initialization.
- Theme handling must tolerate construction before a WinForms handle exists.

## 9. WinForms and DPI rules

Every UI-facing change must consider:

- .NET Framework 4.8 and .NET 8 Windows Forms differences;
- handle creation/destruction;
- runtime DPI changes;
- light/dark theme switching;
- custom consumer fonts;
- enabled/disabled/read-only states;
- focus and selection visibility;
- keyboard navigation and editing;
- disposal/recreation.

Prefer shared code across TFMs. Use conditional compilation only when a real platform/API difference requires it.

Do not use runtime APIs unavailable on `net48` unless guarded by compatibility code.

## 10. Automated WinForms test safety

Unattended tests must fail deterministically and must never wait for a human or coding agent.

- GUI/handle-based tests must run on STA.
- Never leave `MessageBox.Show`, exception dialogs, unbounded `ShowDialog()`, or other modal UI active.
- Configure test-side unhandled exception behavior so unexpected WinForms errors fail the test process instead of showing default dialogs.
- Raw `dotnet test` invocations for GUI tests must use bounded hang diagnostics such as `--blame-hang --blame-hang-timeout 5m`.
- Keep `Application.DoEvents()` finite and restricted to explicit synchronization points.
- Do not add production fail-fast behavior only to make tests easier.

## 11. Development workflow

For each task in an implementation plan:

1. Inspect the exact upstream APIs and nearby tests first.
2. Check `docs/UPSTREAM_API_SEAMS.md`; if the needed seam is absent or appears stale, verify it against the pinned vendor source before implementation.
3. Add a failing test for observable behavior where practical.
4. Run the focused test and verify the intended failure.
5. Implement the minimum change.
6. Run focused tests.
7. Build both TFMs when the task can affect both.
8. Run the relevant integration/GUI checks.
9. Update docs when behavior, a verified seam, public API, license metadata, or dependency packaging changes.
10. Commit a coherent change.

Do not jump to a later stage while the current stage's gate is failing.

## 12. Validation gates

Before reporting a stage complete:

```powershell
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
```

When target-specific validation is useful:

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --blame-hang --blame-hang-timeout 5m
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --blame-hang --blame-hang-timeout 5m
```

If scripts introduced later become canonical, update this section and `docs/TESTING.md` together.

## 13. Repository hygiene

- Respect `.editorconfig` and `.gitattributes`.
- Markdown is UTF-8 with CRLF working-tree EOL according to repository policy.
- Do not commit `bin/`, `obj/`, packages, test results, dumps, screenshots produced only for local debugging, or IDE-local state.
- Avoid unrelated formatting churn in imported/upstream code.
- Keep files focused by responsibility.
- Do not add third-party dependencies unless the architecture truly requires them and both target frameworks remain supported.

## 14. Definition of done

A task is done only when:

- its acceptance criteria pass;
- SourceGrid behavior outside the intended visual/integration change remains compatible;
- both TFMs build;
- relevant automated tests pass without interactive UI;
- designer/runtime lifecycle considerations were checked;
- theme/DPI/resource ownership is correct;
- public API and docs are synchronized;
- verified upstream seam documentation is synchronized when a seam changes;
- no forbidden vendor dependency or unnecessary upstream patch was introduced.

For release/packaging tasks additionally require:

- MIT package metadata matches D-016;
- vendor license/notice obligations are verified;
- public package dependencies satisfy D-017;
- a clean consumer can restore without vendor source checkout.
