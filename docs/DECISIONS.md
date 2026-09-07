# Architectural decisions

This document records approved project decisions. Changes require explicit user approval and must update dependent docs/plans in the same change.

## D-001 — Direct inheritance from SourceGrid.Grid

**Decision:** `BootstrapSourceGrid` derives directly from `SourceGrid.Grid`.

```csharp
namespace MyDmsVn.Bootstrap5WinFormUI.Controls;

public class BootstrapSourceGrid : SourceGrid.Grid
{
}
```

**Rationale:** SourceGrid is already a complete WinForms grid control with a broad public API. Direct inheritance preserves existing rows, columns, cells, selection, controllers, editors, spans, clipboard, keyboard, and scrolling contracts. A composition wrapper would require forwarding a large API surface and would weaken SourceGrid compatibility.

**Exception rule:** A wrapper/composition model may be reconsidered only if a concrete blocker is demonstrated, documented, and approved.

## D-002 — One-way integration dependency

**Decision:** Dependency direction is:

```text
MyDmsVn.BootstrapSourceGrid
    +--> MyDmsVn.Bootstrap5WinFormUI
    `--> SourceGrid
```

Neither vendor may acquire a dependency on the other because of this project.

**Rationale:** Keeps both vendors independently maintainable and makes future upstream synchronization tractable.

## D-003 — Preserve SourceGrid's public programming model

**Decision:** Do not create Bootstrap-prefixed duplicates of SourceGrid abstractions such as rows, columns, cells, ranges, selection, editors, or controllers merely to provide naming consistency.

**Rationale:** The integration should be substitutable for common SourceGrid usage, not force consumers to rewrite their grid code.

## D-004 — Theme through adapters and SourceGrid visual extension points

**Decision:** Bootstrap semantic tokens are mapped into SourceGrid Views/VisualModels/styles through a dedicated adapter. Broad replacement of SourceGrid's painting engine is not the default strategy.

Preferred flow:

```text
BootstrapThemeManager.CurrentTheme
        -> BootstrapSourceGridThemeAdapter
        -> SourceGrid cell/header/selection visual objects
```

**Rationale:** SourceGrid already separates cell View from Model/Editor/Controller concerns. Reusing that architecture minimizes behavior regression and upstream divergence.

## D-005 — Vendor patches remain zero or near zero

**Decision:** Initial implementation must not modify vendor source. If a blocker is discovered:

1. prove the blocker with a focused regression test or minimal reproduction;
2. document why public/protected SourceGrid or Bootstrap hooks are insufficient;
3. patch the appropriate vendor repository, not this integration by copy/paste;
4. keep the patch minimal and behavior-compatible;
5. update the pinned baseline only after the vendor patch is accepted/committed;
6. record the change in `UPSTREAM.md`.

## D-006 — First-class dual target support

**Decision:** Product, tests, demo, and packaging support:

```text
net48;net8.0-windows
```

A stage is not complete if only one TFM works unless the user explicitly approves a temporary exception.

## D-007 — Initial visual scope

**Decision:** Initial implementation covers:

- grid background/foreground/border;
- normal and alternating cells;
- column headers;
- row headers;
- selection/focus visual integration where SourceGrid exposes safe hooks;
- read-only/disabled visual integration where safe;
- Bootstrap typography defaults;
- runtime theme changes;
- Bootstrap-owned DPI-scaled metrics;
- Designer-safe construction;
- default editor appearance hardening.

## D-008 — Scrollbars remain native/SourceGrid in the initial release

**Decision:** Do not replace SourceGrid's scrollbar subsystem in the initial release.

**Rationale:** `CustomScrollControl` owns scrolling behavior and replacement would expand risk into wheel handling, PageUp/PageDown, thumb tracking, Win32 focus, accessibility, DPI, and layout with limited MVP value.

## D-009 — Do not replace the full editor architecture initially

**Decision:** Preserve SourceGrid editors and edit lifecycle. Initial work may align font/colors/border/selection for default editors only where this can be done without changing editor semantics.

A future Bootstrap-specific editor package/adapter may be designed separately after the base integration is stable.

## D-010 — Assembly/package and namespace are intentionally different

**Decision:**

- Assembly/package: `MyDmsVn.BootstrapSourceGrid`
- Primary public namespace: `MyDmsVn.Bootstrap5WinFormUI.Controls`

**Rationale:** The namespace provides a coherent consumer experience with the Bootstrap control family while the separate assembly preserves dependency isolation.

## D-011 — Pinned vendor source via Git submodules plus ProjectReference during bootstrap

**Decision:** Initial repository implementation uses pinned Git submodules:

```text
vendor/Bootstrap5WinFormUI
vendor/sourcegrid
```

and product/test projects use project references to the appropriate vendor projects.

Pinned commits:

- Bootstrap5WinFormUI: `95077df0c8bad8593143c2190606d2f444bfc653`
- SourceGrid: `f4e457b43582bf01892f50bdc74aa480531e5944`

**Rationale:** This combines direct project-reference development with exact commit reproducibility and avoids copying vendor code.

A later switch to published package references is allowed if the packages expose equivalent APIs/behavior on both TFMs and the migration is documented.

## D-012 — Consumer font override wins

**Decision:** By default the grid adopts Bootstrap body typography. Once application code explicitly assigns `Font`, the control must treat it as consumer-owned and stop replacing/disposal of that font during theme changes.

## D-013 — Runtime theme changes are automatic

**Decision:** The control subscribes to `BootstrapThemeManager.ThemeChanged` and updates existing visual objects/repaints automatically. Application code should not need to call a custom `RefreshTheme()` after every theme change.

## D-014 — Automated GUI tests must be non-interactive

**Decision:** Any WinForms test that can create handles or trigger default WinForms UI must use STA execution, bounded hang detection, and deterministic exception handling. Modal UI waiting for a human is forbidden in automated tests.

## D-015 — SourceGrid behavior changes require explicit intent

**Decision:** Styling is not permission to redefine SourceGrid editing, selection, focus, navigation, spans, clipboard, scrolling, or event ordering. Any intentional behavioral change requires a separate approved decision and regression coverage.
