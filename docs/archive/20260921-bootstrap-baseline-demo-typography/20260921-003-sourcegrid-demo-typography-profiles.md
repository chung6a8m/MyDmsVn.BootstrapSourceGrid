# SourceGrid Demo typography profiles plan

**Stage:** 1 of the active baseline/typography roadmap.

**Goal:** add a demo-only Base font profile selector equivalent to Bootstrap5WinFormUI PR #63 while preserving BootstrapSourceGrid-specific font ownership, live grid/editor state, and framework defaults.

**Prerequisite:** Stage 0 accepted the new Bootstrap vendor baseline.

## Target behavior

Add three known profiles:

```text
Default
Base 14px
Base 16px
```

Definitions:

- `Default` -> exact `BootstrapThemeTypography.Default` reference.
- `Base 14px` -> Segoe UI: Body 10.5pt; BodySmall 9.1875pt; Label 10.5pt Bold; HeadingSmall 13.125pt Bold; HeadingMedium 15.75pt Bold.
- `Base 16px` -> Segoe UI: Body 12pt; BodySmall 10.5pt; Label 12pt Bold; HeadingSmall 15pt Bold; HeadingMedium 18pt Bold.

SourceGrid Demo startup remains based on the already-installed `BootstrapThemeManager.CurrentTheme`. Do not force Base 16px at startup.

## File map

Create, unless implementation proves a smaller equivalent structure:

- `samples/MyDmsVn.BootstrapSourceGrid.Demo/DemoTypographyPreset.cs`;
- `samples/MyDmsVn.BootstrapSourceGrid.Demo/DemoTypography.cs`;
- `samples/MyDmsVn.BootstrapSourceGrid.Demo/DemoThemeFactory.cs`;
- `samples/MyDmsVn.BootstrapSourceGrid.Demo/Properties/AssemblyInfo.cs` for test friend access if internal helpers require it.

Modify:

- `samples/MyDmsVn.BootstrapSourceGrid.Demo/MainForm.cs`;
- `samples/MyDmsVn.BootstrapSourceGrid.Demo/MainForm.Designer.cs`;
- `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridDemoTests.cs`.

Create a separate demo typography test file only if it keeps the existing demo tests clearer; do not create a competing test harness.

## Task 1 — Lock profile/factory behavior with tests

Add failing tests first for:

- [ ] `Default` is `SameAs(BootstrapThemeTypography.Default)`;
- [ ] Base 14px exact token sizes/styles;
- [ ] Base 16px exact token sizes/styles;
- [ ] invalid enum value throws;
- [ ] arbitrary custom typography can be composed/preserved by reference;
- [ ] constructing `MainForm` does not overwrite an already-installed theme.

Helper/profile types stay internal to the demo assembly.

## Task 2 — Add centralized demo profiles

Implement a single profile-definition location.

Required helpers:

- map preset -> immutable typography object;
- try map known typography object -> preset;
- create `Font` from a `BootstrapFontToken`;
- compare an existing Font with a token without replacing it unnecessarily.

Do not calculate 14/16 ratios in controls.

## Task 3 — Compose complete themes

Provide a demo-only factory/helper that creates one complete `BootstrapTheme` from:

- requested mode;
- requested/preserved typography;
- requested reduced-motion value.

Use framework default colors/metrics for the chosen mode unless a later explicit requirement says otherwise.

Do not use `BootstrapTheme.CreateDefault(mode)` in a way that discards the active typography or reduced-motion state.

## Task 4 — Extend MainForm settings UI

Keep current Reset and Consumer font scenarios.

Add:

- label `Base font`;
- DropDownList ComboBox with exact item order;
- CheckBox `Reduced motion`.

The existing Light/Dark buttons may remain to minimize demo churn.

Give the Base font ComboBox a stable `Name` and an accessibility name suitable for tests/tooling.

On construction:

- synchronize from the existing `CurrentTheme`;
- select a known preset when the typography object is one of the known profile objects;
- use `SelectedIndex == -1` for unknown custom typography;
- do not publish a replacement theme just to synchronize controls.

## Task 5 — Independent settings behavior

Lock by tests:

- [ ] Light/Dark preserves known profile + reduced motion;
- [ ] profile switch preserves mode + reduced motion;
- [ ] reduced-motion switch preserves mode + typography;
- [ ] Light/Dark and reduced-motion changes preserve unknown custom typography by exact reference;
- [ ] choosing a known preset exits custom typography state;
- [ ] each user setting action raises exactly one `ThemeChanged`;
- [ ] synchronization after external theme change does not republish.

## Task 6 — Live font/resource behavior

MainForm native shell controls should follow `Typography.Body`.

- [ ] switch Default -> Base14 -> Base16 on the same live form;
- [ ] avoid replacing owned form font when token is unchanged;
- [ ] replace/assign/dispose owned Font deterministically;
- [ ] unsubscribe `ThemeChanged` in Dispose.

BootstrapSourceGrid itself must remain governed by its existing font ownership contract.

Add a regression proving:

1. click `Use consumer font`;
2. switch typography profile and theme;
3. `grid.Font` remains the same consumer-owned Font instance.

Also prove the existing grid and the three shared Bootstrap editor adapters are not recreated by profile changes.

## Task 7 — Layout and clipping coverage

Automated checks should exercise all three profiles at representative shell sizes, at least the existing 960x640 plus one smaller supported client size.

Verify:

- toolbar controls remain contained/usable;
- diagnostics/help labels have positive usable bounds;
- grid header text is not visibly/measureably clipped;
- representative ordinary cell text remains usable;
- Bootstrap editor preferred heights remain compatible with the demo row layout;
- existing fixed column widths are not silently rewritten by the product control.

If a demo-only row/header size needs adjustment for Base16, make that explicit in `DemoGridContent` or another demo-only helper and test it. Do not add product-level auto-resizing.

## Task 8 — Diagnostics

Extend diagnostics so manual reviewers can see:

- current Theme mode;
- current profile name or `Custom`;
- current Body size;
- Reduced motion state;
- existing DPI/font-ownership/logical-value information.

## Focused validation

Run relevant demo tests for both TFMs with hang protection, then the full test project.

Manual smoke must include profile switching while:

- an editor is active;
- lookup popup is open;
- consumer grid font mode is active;
- Dark mode is active.

## Stage gate

- [ ] all profile/state/publication tests pass on both TFMs;
- [ ] no framework core default changed;
- [ ] no grid data/editor/selection reconstruction is required for profile changes;
- [ ] no consumer View/font precedence regression;
- [ ] no unbounded/modal GUI path;
- [ ] vendor worktrees remain clean.
