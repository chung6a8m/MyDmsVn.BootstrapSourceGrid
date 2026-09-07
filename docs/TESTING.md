# Testing strategy

## 1. Goals

Testing must prove both integration correctness and preservation of SourceGrid behavior. Because this is WinForms code, automated tests must also be safe for unattended execution.

## 2. Test project

Planned project:

```text
tests/MyDmsVn.BootstrapSourceGrid.Tests/
```

Target frameworks:

```xml
<TargetFrameworks>net48;net8.0-windows</TargetFrameworks>
```

Use the same test framework/version strategy as the pinned vendors unless Stage 0 discovers a concrete compatibility reason to choose differently. The test project must support STA tests on both TFMs.

## 3. Test layers

### Layer A — pure theme mapping tests

No WinForms handles required.

Test:

- Bootstrap theme -> integration theme snapshot mapping;
- normal/alternate/header/selection colors;
- selection contrast calculation delegation/behavior;
- DPI metric calculation helpers;
- theme-owned vs consumer-owned font state decisions;
- ownership detection for integration-created Views.

These should be fast and deterministic.

### Layer B — control construction/lifecycle tests

STA tests that instantiate `BootstrapSourceGrid`.

Test:

- construction before handle creation;
- default base type is `SourceGrid.Grid`;
- theme subscription exists only while alive;
- dispose unsubscribes and releases owned fonts/resources;
- explicit consumer `Font` survives theme changes and disposal;
- handle create/destroy cycles do not duplicate subscriptions.

### Layer C — SourceGrid compatibility tests

Exercise common SourceGrid usage through `BootstrapSourceGrid`:

- `Redim`;
- cell assignment/indexing;
- row height and column width;
- spans;
- selection ranges/active position;
- editor activation/commit/cancel for representative editors;
- keyboard movement;
- scrolling where practical;
- consumer custom View assignment.

The purpose is not to duplicate SourceGrid's whole suite. Cover behaviors the integration could accidentally disturb.

### Layer D — runtime theme tests

Create the grid and data, establish selection and representative customizations, then change Bootstrap theme.

Verify:

- integration-owned colors/views update;
- data/cell values remain unchanged;
- selected ranges/active position remain unchanged;
- consumer View remains assigned;
- consumer Font remains assigned after opt-out;
- control remains usable after multiple theme switches.

### Layer E — editor appearance tests

For each default editor explicitly supported in MVP:

- activate editor;
- verify editor control receives safe Bootstrap font/color styling;
- type/edit and commit;
- type/edit and cancel;
- verify navigation/focus semantics remain SourceGrid-compatible;
- switch theme while editor is active if SourceGrid supports that scenario safely.

Do not force a visual property if SourceGrid's editor does not expose a safe hook.

### Layer F — demo/manual verification

Use the demo application for behaviors that are expensive or brittle to assert pixel-perfectly:

- visual comparison in light/dark;
- headers and alternate rows;
- focus rectangle/active cell clarity;
- editor visual continuity;
- native scrollbar appearance with themed surface;
- DPI scaling at 100/125/150/200%;
- Designer toolbox/drag-drop/property serialization;
- long scrolling/rapid navigation;
- custom SourceGrid Views coexisting with Bootstrap defaults.

## 4. Unattended WinForms safety

Automated tests must never display UI that waits for a person.

Mandatory rules:

- run handle-based tests on STA;
- configure unhandled WinForms exceptions to throw/fail instead of opening default dialogs;
- never use an unbounded `ShowDialog()`;
- never leave `MessageBox.Show` reachable in an automated failure path;
- if a vendor failure can trigger modal UI, add test-side guards/adapters rather than production fail-fast code;
- keep `Application.DoEvents()` finite and only at known synchronization points;
- use bounded hang diagnostics for raw GUI test runs.

Recommended command:

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --blame-hang --blame-hang-timeout 5m
```

Target-specific:

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --blame-hang --blame-hang-timeout 5m
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --blame-hang --blame-hang-timeout 5m
```

## 5. Theme test fixtures

Tests should construct known light/dark/custom themes using public Bootstrap5WinFormUI APIs at the pinned baseline. Do not depend on ambient desktop theme or registry state.

Each expected color assertion should derive from explicit theme tokens, not screenshots or machine defaults.

## 6. Font ownership tests

Minimum cases:

1. default construction creates/adopts the theme body font;
2. theme switch changes integration-owned font;
3. assigning a consumer font disables automatic replacement;
4. later theme switches retain the exact consumer font instance;
5. disposing the grid does not dispose consumer font;
6. disposing the grid releases the integration-owned font.

When checking disposal, use deterministic operations that throw on use of a disposed GDI object only when reliable across both TFMs; otherwise expose/test internal ownership state through `InternalsVisibleTo` rather than public test-only APIs.

## 7. View ownership tests

Minimum cases:

- default/integration-owned View updates on theme change;
- consumer assigns a different SourceGrid View;
- theme switch does not replace the consumer View;
- newly created default cells after a theme switch receive current theme visuals;
- shared integration Views do not leak cell-specific state across cells.

## 8. Selection/focus regression tests

Test styling does not alter:

- active position after arrow navigation;
- multi-selection ranges;
- selection after theme switch;
- focus transfer into/out of editor;
- Tab/Shift+Tab behavior supported by SourceGrid;
- PageUp/PageDown and Home/End where integration changes can affect input/focus.

## 9. Span regression tests

Create representative row/column spans and verify:

- covered positions still resolve according to SourceGrid behavior;
- themed Views render on span owner without creating duplicate semantic cells;
- theme switch does not mutate span definitions.

## 10. DPI manual matrix

At minimum verify 100%, 150%, and 200%; include 125% when practical.

Inspect:

- text clipping;
- row/header content alignment;
- integration padding;
- focus/selection thickness;
- editor bounds;
- scrollbar/layout edge alignment;
- runtime move between monitors with different DPI when hardware/environment permits.

## 11. Designer manual matrix

For both framework target workflows:

1. open demo/test form in Visual Studio Designer;
2. place `BootstrapSourceGrid` from toolbox or create it in designer code;
3. resize/dock/anchor;
4. edit safe public properties;
5. close/reopen designer;
6. build/run form;
7. confirm no theme subscription/resource leak after repeated design sessions when observable.

## 12. Full validation gate

After each implementation stage:

```powershell
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
```

A stage with UI changes also requires its documented demo/manual checks before it is considered complete.

## 13. What not to test

Do not duplicate SourceGrid's complete upstream test suite. Integration tests should focus on:

- APIs/behavior touched by subclassing or theming;
- compatibility seams;
- lifecycle/resource ownership;
- runtime theme/DPI behavior;
- representative SourceGrid features likely to regress through the integration.
