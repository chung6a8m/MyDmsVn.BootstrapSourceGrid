# Regression validation and documentation synchronization plan

**Stage:** 2 of the active baseline/typography roadmap.

**Goal:** close the initiative with clean dual-TFM regression evidence, manual UI/DPI checks, and canonical documentation that matches the accepted Bootstrap baseline and demo behavior.

## Task 1 — Clean-checkout/full automated gate

From the final superproject state:

```powershell
git submodule update --init --recursive
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
```

Also run target-specific test commands if the combined run obscures a TFM-specific failure.

Record test counts/results in the dated verification record.

## Task 2 — Regression matrix

Automated/focused evidence must cover:

- construction/Dispose/theme subscription;
- grid theme font vs consumer font;
- consumer View preservation;
- runtime theme/profile changes;
- header/cell/selection rendering contracts;
- shared Bootstrap editor allocation and disposal;
- text/formatted/lookup editor commit/cancel/logical values;
- lookup focus/popup keyboard/mouse/deactivation scenarios;
- keyboard navigation and spans;
- reset/repopulate without grid/editor replacement;
- no modal WinForms failure UI.

## Task 3 — Manual demo matrix

Exercise:

```text
Typography: Default / Base 14px / Base 16px
Theme:      Light / Dark
Motion:     normal / Reduced motion
Font mode:  theme font / consumer grid font
```

At minimum sample representative combinations plus every typography profile in both Light and Dark.

For each profile verify:

- toolbar/control containment;
- diagnostics;
- header/cell text;
- selection/focus;
- scrolling;
- header sort;
- F2/typing/Enter/Escape/Tab/Shift+Tab;
- formatted value editing;
- lookup popup navigation/mouse/outside-click;
- profile/theme switch while editing or popup is open;
- spanned cell;
- Reset.

## Task 4 — DPI and Designer smoke

Where environment permits:

- 100%, 150%, 200% DPI;
- move between monitors with different DPI if available;
- open/reopen Designer for supported target workflows;
- ensure profile-related demo code does not require runtime-only bootstrap to construct.

Document any environment cell that could not be executed; do not silently drop it from release requirements.

## Task 5 — Synchronize canonical baseline facts

After the accepted gitlink and tests exist, update:

- `README.md`;
- `AGENTS.md`;
- `AI_CONTEXT.md`;
- `docs/DECISIONS.md` D-011;
- `docs/UPSTREAM.md`;
- `docs/UPSTREAM_API_SEAMS.md`.

Review and update only when materially affected:

- `docs/COMPATIBILITY.md`;
- `docs/TESTING.md`;
- `docs/RELEASE.md`;
- `docs/PENDING_DECISIONS.md`.

Record old -> new Bootstrap SHA and the compatibility outcome.

Do not change the SourceGrid baseline.

## Task 6 — Release/package gate re-check

The vendor source upgrade does not automatically unblock NuGet publication.

- [ ] verify whether package availability/license/equivalence evidence changed;
- [ ] if unchanged, keep the existing release block and merely update source-baseline references as needed;
- [ ] never claim package/source equivalence without an actual candidate package tied to the accepted source baseline.

## Task 7 — Final repository hygiene

```powershell
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected: no output.

Confirm no generated `bin/`, `obj/`, test-results, dumps, IDE files, or local package artifacts were committed.

## Completion and archive

When all acceptance criteria pass:

1. update `docs/DEVELOPMENT_PLAN.md` to mark the initiative complete;
2. promote durable behavior into canonical docs;
3. move this dated plan set under a new `docs/archive/20260921-bootstrap-baseline-demo-typography/` directory;
4. reset `docs/plans/README.md` to no active plan.

Do not archive while a compatibility or manual-matrix blocker remains.
