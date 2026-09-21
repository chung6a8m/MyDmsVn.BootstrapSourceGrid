# Bootstrap baseline upgrade and compatibility plan

**Stage:** 0 of the active baseline/typography roadmap.

**Goal:** move `vendor/Bootstrap5WinFormUI` from the current pin to the latest approved `main` HEAD and produce evidence that BootstrapSourceGrid still behaves correctly before any new demo feature is added.

**Spec:** `docs/BOOTSTRAP_BASELINE_UPGRADE_AND_DEMO_TYPOGRAPHY.md`.

## Known planning facts

Current pin:

```text
cceba3c969e28726935793a1c6ca3772bed60a35
```

Planning-time `main` HEAD:

```text
aba102e33c48937fd92468791c287afda0a59e77
```

Planning-time compare: 106 commits / 197 files. A preliminary scan found no direct changes to the framework Theme classes, `DpiScaler`, the three Bootstrap editor control implementations currently integrated here, or the framework project TFM definition.

Treat that only as risk triage.

## Task 1 — Resolve and freeze the actual target

- [ ] `git -C vendor/Bootstrap5WinFormUI fetch origin main`.
- [ ] Record `git -C vendor/Bootstrap5WinFormUI rev-parse origin/main`.
- [ ] If it differs from the planning snapshot, compare current pin to the newer SHA and update the active spec/verification evidence.
- [ ] Record commit date/message and whether current pin is a direct ancestor.
- [ ] Do not use a floating branch in the superproject; the final result remains a pinned commit.

## Task 2 — Establish pre-upgrade evidence

Run on the existing pin before moving it:

```powershell
git submodule update --init --recursive
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
```

- [ ] Record pass/fail and test counts by TFM when available.
- [ ] If the old pin is already failing, classify/fix that separately before attributing anything to the upgrade.

## Task 3 — Re-verify Bootstrap integration seams

Inspect old vs target vendor source for every Bootstrap API actually consumed by this repository, including:

- `BootstrapThemeManager.CurrentTheme` / `ThemeChanged`;
- `BootstrapTheme` constructor and immutable token properties;
- `BootstrapThemeColors`;
- `BootstrapThemeMetrics`;
- `BootstrapThemeTypography` / `BootstrapFontToken`;
- `DpiScaler`;
- `BootstrapTextBox`;
- `BootstrapFormattedTextBox`;
- `BootstrapLookupBox` and popup/keyboard/focus behavior;
- any public/protected member used by editor adapters.

- [ ] Update `docs/UPSTREAM_API_SEAMS.md` only after the target baseline is accepted.
- [ ] Add a focused regression test before changing integration code if a semantic difference is discovered.

## Task 4 — Move only the Bootstrap submodule

- [ ] Checkout the resolved target SHA inside `vendor/Bootstrap5WinFormUI`.
- [ ] Confirm `vendor/sourcegrid` remains at `f4e457b43582bf01892f50bdc74aa480531e5944`.
- [ ] Do not edit files inside either vendor.
- [ ] Superproject diff at this point should contain the Bootstrap gitlink change plus integration/docs/tests only when necessary.

## Task 5 — Compile and focused compatibility tests

Run both target frameworks explicitly for focused failures.

At minimum cover:

- construction and handle lifecycle;
- theme adapter/snapshot;
- runtime theme switching;
- theme-owned vs consumer-owned grid font;
- cell/header/selection Views;
- DPI metrics and DPI lifecycle;
- legacy editor styling;
- BootstrapTextBox editor;
- BootstrapFormattedTextBox editor;
- BootstrapLookupBox editor and interaction tests;
- editor registry ownership/disposal;
- keyboard/span compatibility;
- demo construction and theme switching.

Then run the full solution build and full test project.

Any failure caused by the vendor target must be resolved with the smallest integration-side change consistent with the stable architecture. Do not patch vendor source locally.

## Task 6 — Manual compatibility smoke

Run the demo at least on `net8.0-windows`, and on `net48` where the environment supports execution.

Verify:

- Light -> Dark -> Light;
- selection/range/focus;
- header sort;
- scrolling;
- spanned editable cell;
- text/formatted/lookup Bootstrap editors;
- lookup keyboard, mouse, outside-click and popup/theme-switch scenarios;
- Reset preserves grid instance/editor sharing;
- Consumer font survives later theme changes.

No typography-profile feature is implemented in this stage.

## Task 7 — Compatibility disposition

Create/update a dated verification record under `docs/verification/` with:

- old SHA;
- target SHA;
- compare summary;
- API/seam review notes;
- build/test commands and results;
- manual checks;
- any integration change required;
- unresolved observations.

Stage outcome must be one of:

- **Accepted:** all required gates pass and target can become the canonical Bootstrap baseline.
- **Blocked:** a reproducible regression remains; do not start Stage 1.

## Stage gate

```powershell
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected: exit 0 and no vendor worktree modifications. The Bootstrap gitlink itself is expected to differ in the superproject.
