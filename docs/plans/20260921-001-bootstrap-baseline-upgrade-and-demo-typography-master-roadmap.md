# Bootstrap baseline upgrade and demo typography master roadmap

> **For agentic workers:** execute one stage at a time. Do not skip a failing gate or opportunistically implement a later stage.

**Goal:** upgrade the Bootstrap5WinFormUI submodule to the latest approved `main` HEAD, prove BootstrapSourceGrid compatibility, then add PR #63-equivalent demo typography profiles without changing framework defaults or SourceGrid's programming model.

**Active spec:** `docs/BOOTSTRAP_BASELINE_UPGRADE_AND_DEMO_TYPOGRAPHY.md`.

**Current planning snapshot:** Bootstrap pin `cceba3c969e28726935793a1c6ca3772bed60a35`; Bootstrap `main` was `aba102e33c48937fd92468791c287afda0a59e77` when this roadmap was written. Re-resolve `main` immediately before Stage 0 moves the submodule.

## Global constraints

- Keep `BootstrapSourceGrid : SourceGrid.Grid`.
- Keep `net48;net8.0-windows`.
- SourceGrid baseline remains `f4e457b43582bf01892f50bdc74aa480531e5944`.
- No vendor source edits from this repository.
- Preserve SourceGrid behavior and consumer customization precedence.
- Preserve grid-owned shared Bootstrap editor adapters.
- Typography profiles belong to the sample/demo only.
- `Default` is the exact `BootstrapThemeTypography.Default` object.
- Do not introduce another global theme/typography manager.
- GUI tests must be STA, bounded, deterministic, and non-modal.
- Both vendor worktrees must be clean at every stage gate.

## Stage 0 — Upgrade Bootstrap baseline and prove compatibility

Plan: `docs/plans/20260921-002-bootstrap-baseline-upgrade-and-compatibility.md`

Acceptance:

- [ ] fetch Bootstrap vendor `origin/main` and record the actual latest target SHA;
- [ ] if target moved since roadmap creation, refresh old->target compare evidence before changing the pointer;
- [ ] run a current-baseline preflight so upgrade regressions can be distinguished from pre-existing failures;
- [ ] inspect the changed API/behavior surface used by BootstrapSourceGrid;
- [ ] move only `vendor/Bootstrap5WinFormUI`;
- [ ] restore/build/test both TFMs;
- [ ] run focused theme/font/editor/DPI/Designer-contract tests;
- [ ] run SourceGrid demo smoke for Light/Dark/editors/selection/scrolling;
- [ ] record compatibility evidence and blockers;
- [ ] do not proceed while any unexplained regression remains.

## Stage 1 — SourceGrid Demo typography profiles

Plan: `docs/plans/20260921-003-sourcegrid-demo-typography-profiles.md`

Acceptance:

- [ ] demo exposes `Default`, `Base 14px`, `Base 16px`;
- [ ] `Default` reuses framework default typography by reference;
- [ ] SourceGrid Demo startup behavior remains based on the already-installed `CurrentTheme`;
- [ ] Light/Dark, typography, and reduced motion compose independently;
- [ ] unknown external typography is preserved by reference during unrelated setting changes;
- [ ] one user setting action produces exactly one `ThemeChanged`;
- [ ] live MainForm/grid/editor instances are preserved during profile switching;
- [ ] consumer grid font opt-out remains authoritative;
- [ ] demo-owned native fonts refresh and dispose deterministically;
- [ ] toolbar/grid text containment is proven for all profiles at representative sizes;
- [ ] no core framework or product-level row/column sizing defaults are changed.

## Stage 2 — Regression matrix and canonical documentation sync

Plan: `docs/plans/20260921-004-regression-validation-and-doc-sync.md`

Acceptance:

- [ ] full dual-TFM build/test gate passes from a clean checkout/submodule state;
- [ ] manual profile/theme/reduced-motion/editor/DPI matrix is recorded;
- [ ] no new modal/hanging GUI test path exists;
- [ ] the accepted Bootstrap SHA is synchronized across canonical docs;
- [ ] `docs/UPSTREAM_API_SEAMS.md` is re-verified against the accepted baseline;
- [ ] release/package-gate documentation is updated only if the new baseline changes its evidence;
- [ ] both vendor worktrees are clean;
- [ ] active plan set is ready to move under `docs/archive/` after completion.

## Cross-stage regression checklist

At every stage boundary:

- [ ] SourceGrid data/cell/range/span APIs unchanged.
- [ ] Selection and active position semantics unchanged except previously approved behavior.
- [ ] Editor lifecycle/commit/cancel remains SourceGrid-owned.
- [ ] Existing Bootstrap editor logical-value bridges remain correct.
- [ ] Consumer custom Views are not replaced.
- [ ] Consumer grid Font is not replaced after opt-out.
- [ ] No per-cell Bootstrap composite-control allocation is introduced.
- [ ] Theme handler/font resources are disposed.
- [ ] No product-level automatic row/column resizing is introduced.
- [ ] Both TFMs remain first-class.
- [ ] Vendor worktrees are clean.

## Full validation

```powershell
git submodule update --init --recursive
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected final result: commands exit 0, the demo contract is covered by automated tests plus the documented manual matrix, and both vendor worktrees produce no status output.
