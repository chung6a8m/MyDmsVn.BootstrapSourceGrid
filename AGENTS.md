# AGENTS.md

Repository operating contract for Codex and other coding agents working on `MyDmsVn.BootstrapSourceGrid`.

For historical context, see [Archive](./docs/archive/). Archived plans are not part of the normal read order.

## 1. Read only what the task needs

Before product-code changes, always read:

1. `README.md`
2. `AI_CONTEXT.md`
3. the active spec for the task;
4. the active implementation plan under `docs/plans/`;
5. the relevant section of `docs/UPSTREAM_API_SEAMS.md`;
6. the exact pinned vendor source/tests touched by the task.

For Bootstrap editor work, the canonical spec is `docs/EDITOR_REPLACEMENT.md`. The completed implementation roadmap is archived under `docs/archive/20260909-bootstrap-editor-replacement/`.

Read other canonical documents only when their topic is relevant:

- architecture/ownership -> `docs/ARCHITECTURE.md`, `docs/DECISIONS.md`
- vendor upgrade/dependency -> `docs/UPSTREAM.md`
- compatibility -> `docs/COMPATIBILITY.md`
- GUI/test execution -> `docs/TESTING.md`
- packaging/release -> `docs/RELEASE.md`, `docs/PENDING_DECISIONS.md`

Do not load `docs/archive/` unless the user explicitly asks for historical reasoning or a regression requires it.

## 2. Fixed project constraints

Do not change without explicit user approval:

- Repository: `chung6a8m/MyDmsVn.BootstrapSourceGrid`
- Assembly/package: `MyDmsVn.BootstrapSourceGrid`
- Public control: `MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid`
- Direct base type: `SourceGrid.Grid`
- TFMs: `net48;net8.0-windows`
- UI: native Windows Forms
- License: MIT
- Bootstrap baseline: `cceba3c969e28726935793a1c6ca3772bed60a35`
- SourceGrid baseline: `f4e457b43582bf01892f50bdc74aa480531e5944`
- Development dependency model: pinned Git submodules + `ProjectReference`
- Public NuGet model: exact verified vendor packages; publication remains gated
- Scrollbars: retain SourceGrid/native subsystem unless separately approved

Dependency direction:

```text
MyDmsVn.BootstrapSourceGrid
    +--> MyDmsVn.Bootstrap5WinFormUI
    `--> SourceGrid
```

Forbidden: either vendor depending on this integration or on the other vendor because of this project.

## 3. SourceGrid compatibility boundary

SourceGrid owns:

- grid storage/virtual-concrete cell mechanics;
- rows, columns, ranges, positions, spans;
- selection and active position;
- keyboard/mouse navigation and controllers;
- editor lifecycle, placement, validation/conversion;
- scrolling;
- painting pipeline and cell Views/VisualModels.

Preserve SourceGrid's public programming model. Do not add Bootstrap-prefixed row/column/cell/range wrappers merely to rename existing APIs.

Prefer inheritance, Views, styles, adapters, and verified public/protected seams. Never invent an upstream seam from memory; verify `docs/UPSTREAM_API_SEAMS.md` and pinned source.

## 4. Bootstrap ownership boundary

Bootstrap5WinFormUI owns:

- `BootstrapThemeManager.CurrentTheme` / `ThemeChanged`;
- semantic colors, metrics, typography;
- `DpiScaler`;
- visual and interactive behavior of Bootstrap controls.

Do not create a second theme system or hard-code Bootstrap colors when theme tokens exist.

If this integration creates a `Font`/GDI object, it owns/disposes it. Never dispose consumer-owned resources.

## 5. Bootstrap editor integration rules

Bootstrap editor adapters follow these constraints:

```text
SourceGrid lifecycle + thin adapter + Bootstrap control
```

Mandatory rules:

- SourceGrid still owns start/commit/cancel, final validation/conversion, placement, and grid navigation.
- Bootstrap editor controls own theme/font/background/border/focus visuals.
- Bootstrap adapters default `UseCellViewProperties = false`.
- `BootstrapSourceGridEditorStyler` must not inject cell View properties into those adapters.
- Do not patch SourceGrid's static editor `Factory` for global replacement.
- Do not create one Bootstrap composite control per cell by default.
- Share adapters within one grid, normally one per column/configuration.
- Never share an adapter across grid instances.
- `BootstrapSourceGrid` owns/disposes adapters created by its editor registry, including never-started controls.
- Consumer custom editors remain untouched.

Logical values:

```text
BootstrapTextBox          -> Text
BootstrapFormattedTextBox -> RawValue
BootstrapLookupBox        -> SelectedValue
```

SourceGrid performs final conversion/validation after those values leave the adapter.

Lookup work must explicitly test popup/focus/`Validated` ordering, Enter/Escape/Tab/Shift+Tab, arrows/Page navigation, mouse selection, outside click, deactivation, theme switch, unmatched text, validation failure, and disposal.

## 6. Public API rules

- Keep integration API small and Bootstrap-specific.
- Do not shadow SourceGrid public members with different meanings.
- New public members require XML documentation and observable-behavior tests.
- Breaking changes require explicit approval and migration notes.
- Do not expose protected/native child controls just for convenience; add the narrowest integration seam required.

## 7. WinForms, DPI, and resource rules

Every UI change must consider both TFMs, handle lifecycle, Designer construction, runtime theme changes, DPI, custom fonts, enabled/read-only state, focus/navigation, and disposal.

Scale only Bootstrap-owned metrics through `DpiScaler`. Do not silently rescale SourceGrid/application row heights, column widths, or scrollbars.

Avoid per-cell/per-paint allocations when a shared immutable object is safe. Dispose every integration-owned GDI/WinForms resource and unsubscribe global events in `Dispose(bool)`.

## 8. Automated GUI test safety

Unattended tests must fail deterministically and never wait for a human/agent.

- GUI/handle tests run STA.
- No `MessageBox.Show`, default exception dialogs, unbounded `ShowDialog()`, or other modal waits.
- Configure WinForms test-side exception behavior to fail rather than display default dialogs.
- Keep message pumping finite and only at explicit synchronization points.
- Use bounded hang diagnostics for GUI tests.
- Do not add production fail-fast behavior merely to simplify tests.

GitHub Actions is disabled for this repository as of 2026-09-09. Local validation is the required automated gate until Actions is restored.

## 9. Task workflow

For each implementation task:

1. Read the active plan task and exact relevant upstream seams.
2. Inspect pinned vendor source/tests before coding.
3. Add a failing observable-behavior test where practical.
4. Run it and verify the intended failure.
5. Implement the minimum change.
6. Run focused tests.
7. Build both TFMs when affected.
8. Run relevant GUI/integration checks with hang protection.
9. Update canonical docs when a durable API/seam/behavior rule changes.
10. Verify both vendor worktrees are clean.
11. Commit one coherent change.

Do not skip a failing stage gate or opportunistically implement a later roadmap stage.

## 10. Validation gate

```powershell
git submodule update --init --recursive
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected: build/tests exit `0`; both vendor status commands produce no output.

Use target-specific `dotnet test -f net48` / `-f net8.0-windows` when isolating a framework-specific failure.

## 11. Repository hygiene

- Respect `.editorconfig` and `.gitattributes`.
- Markdown follows repository encoding/EOL policy.
- Do not commit `bin/`, `obj/`, packages, dumps, test results, or IDE-local files.
- Avoid formatting churn in vendor/imported code.
- Do not add third-party dependencies unless both TFMs remain supported and architecture requires them.
- `docs/plans/` is active work only; move completed plan sets to `docs/archive/`.

## 12. Definition of done

A task is done only when its acceptance criteria pass, both TFMs remain supported, SourceGrid behavior outside the intended integration change remains compatible, GUI tests are non-interactive, resource/event ownership is correct, public API/docs are synchronized, relevant upstream seam docs are current, and vendor worktrees are clean.
