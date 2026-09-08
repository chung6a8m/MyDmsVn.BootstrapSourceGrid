# MVP public API and compatibility review

Date: 2026-09-09

Reviewed integration assembly: `MyDmsVn.BootstrapSourceGrid` targeting `net48` and `net8.0-windows`.

## Public integration API inventory

Reflection over the Release assembly reports one exported integration type:

```text
MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid
    : SourceGrid.Grid

Public declared members:
    BootstrapSourceGrid()
    SourceGrid.Cells.ICellVirtual GetCell(int row, int column) override
    void ProcessSpecialGridKey(System.Windows.Forms.KeyEventArgs e) override
```

The control adds no public properties, events, row/column/cell/range/selection wrappers, theme manager, editor type, or virtual-grid type. The public overrides preserve existing SourceGrid method signatures. Protected declared members are limited to overrides of existing SourceGrid/WinForms lifecycle and input hooks: `CreateSelectionObject`, `OnFontChanged`, `OnDpiChangedAfterParent`, `OnHandleCreated`, `ProcessCmdKey`, `OnKeyDown`, and `Dispose`.

All integration types under `Theming`, `Views`, `Editors`, and `Internal` are non-exported. Some members are declared `public` inside internal types for ordinary C# implementation convenience; the containing type keeps them outside the consumer API.

## Wrapper and dependency audit

- No `BootstrapRow`, `BootstrapColumn`, `BootstrapCell`, `BootstrapRange`, or `BootstrapSelection` type exists.
- The product depends on Bootstrap5WinFormUI and SourceGrid through the two approved project references.
- SourceGrid contains no integration or Bootstrap5WinFormUI reference/import.
- Bootstrap5WinFormUI contains no SourceGrid or integration reference/import.
- Both vendor submodules were clean at their pinned commits during review.
- The inspection `.nupkg` contains only integration assemblies/XML docs; it does not embed either vendor DLL.

## Compatibility evidence

The dual-target automated suite covers:

| Scenario | Primary evidence |
|---|---|
| `Redim`, cell indexer, rows, columns | `BootstrapSourceGridCompatibilityTests` |
| spans and canonical covered positions | `BootstrapSourceGridCompatibilityTests`, `BootstrapSourceGridEditLifecycleTests`, `BootstrapSourceGridKeyboardTests` |
| selection, active position, multi-selection ownership | `BootstrapSourceGridSelectionStyleTests`, `BootstrapSourceGridKeyboardTests`, theme lifecycle tests |
| custom consumer View preservation | `BootstrapSourceGridRuntimeViewThemeTests`, `BootstrapSourceGridDemoTests` |
| edit activation, commit, cancel, active-theme refresh | editor and edit-lifecycle test suites |
| Tab, Shift+Tab, arrows, Home/End, PageUp/PageDown, F2 | `BootstrapSourceGridKeyboardTests` |
| scroll surface and sufficient demo data | compatibility tests plus the demo review surface |
| runtime theme switching without grid/data recreation | theme lifecycle/runtime View tests and `BootstrapSourceGridDemoTests` |
| consumer Font ownership | `BootstrapSourceGridFontTests`, `BootstrapSourceGridDemoTests` |
| DPI-owned metric mapping and lifecycle | DPI metric/lifecycle tests |
| Designer-safe construction and handle lifecycle | Designer contract, construction, and handle lifecycle tests |

The 2026-09-09 source-graph validation completed with zero build warnings/errors and 95 passing tests on each TFM. Existing manual evidence is retained in [Stage 3 DPI and Designer verification](20260907-stage-3-dpi-designer.md) and [Stage 4 editor verification](20260907-stage-4-editors.md). Physical 125%/150%/200% and mixed-monitor tests remain environment-limited as recorded there.

## Findings

1. The integration API is thin and conforms to the approved direct-inheritance model.
2. No accidental SourceGrid wrapper or forbidden dependency direction was found.
3. Package metadata correctly identifies the integration and MIT license, and local project-reference pack output is suitable for inspection.
4. Public NuGet publication remains blocked because exact-equivalent vendor packages and complete vendor license/notice evidence are unavailable. This is documented in [UPSTREAM.md](../UPSTREAM.md#11-public-package-verification-status-2026-09-09) and is not bypassed by embedding vendor binaries.
5. Known editor, scrollbar, custom View, concrete-grid, and Windows-only boundaries are documented in [KNOWN_LIMITATIONS.md](../KNOWN_LIMITATIONS.md).

## Verdict

The source-distributed MVP API and compatibility surface are accepted for continued development under pinned submodules plus `ProjectReference`. The public NuGet release gate is **blocked**, so this review does not authorize publication or claim Stage 5's public-dependency completion criterion.
