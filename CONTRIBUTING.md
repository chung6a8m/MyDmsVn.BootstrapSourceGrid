# Contributing

Contributions to `MyDmsVn.BootstrapSourceGrid` must preserve the architecture and compatibility contracts documented in this repository.

## Start here

Before changing product code, read:

1. `README.md`
2. `AGENTS.md`
3. `AI_CONTEXT.md`
4. `docs/README.md`
5. `docs/DECISIONS.md`
6. `docs/PRD.md`
7. `docs/ARCHITECTURE.md`
8. `docs/UPSTREAM_API_SEAMS.md`
9. `docs/UPSTREAM.md`
10. `docs/COMPATIBILITY.md`
11. `docs/TESTING.md`
12. the active task plan under `docs/plans/`

## Architecture rules

The current design is intentional:

```text
BootstrapSourceGrid : SourceGrid.Grid
```

and dependency direction is:

```text
MyDmsVn.BootstrapSourceGrid
    +--> MyDmsVn.Bootstrap5WinFormUI
    `--> SourceGrid
```

Do not introduce a SourceGrid-to-Bootstrap dependency, a Bootstrap-to-SourceGrid dependency, or a wrapper that duplicates SourceGrid's public grid model without an explicitly approved architectural change.

## Clone and vendor initialization

After cloning:

```powershell
git submodule update --init --recursive
git submodule status
```

Expected baseline commits are documented in `docs/UPSTREAM.md`.

Never work against whatever happens to be the vendor default branch when implementing a planned MVP task. Use the pinned submodule state unless the task is explicitly a vendor-upgrade task.

## Branch and PR scope

Prefer one implementation stage or one coherent bug fix per branch/PR.

Keep changes focused:

- product behavior;
- tests for that behavior;
- demo changes needed to verify it;
- canonical documentation updates required by the behavior.

Avoid unrelated formatting changes in vendor/submodule content.

## TDD expectation

For behavior that can be automated:

1. add a focused failing test;
2. run it and confirm the intended failure;
3. implement the minimum change;
4. run focused tests;
5. run the relevant dual-target/full gate.

For Designer/DPI/visual behavior that cannot be asserted robustly in unit tests, add deterministic logic tests first and record the required manual verification in `docs/verification/`.

## WinForms test safety

Handle-based tests must run on STA and must never wait for a person.

Do not add:

```text
unbounded ShowDialog()
MessageBox-based failure handling
unbounded Application.DoEvents() loops
background UI waiting without a deadline
```

Use the repository's `WinFormsTestGuard` once Stage 0 has implemented it and run GUI-sensitive tests with bounded hang diagnostics.

## Build and test

Canonical full gate:

```powershell
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
```

Target-specific checks:

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --blame-hang --blame-hang-timeout 5m
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --blame-hang --blame-hang-timeout 5m
```

## Vendor cleanliness

Before completing any change:

```powershell
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected: no output unless the task is specifically updating a submodule pointer after an approved vendor change. Never leave an undocumented dirty vendor worktree.

## Public API review

New public API should be rare.

Before adding a public member, ask:

1. Is this a Bootstrap-specific semantic that a consumer genuinely needs?
2. Does SourceGrid already expose the concept?
3. Would this member shadow or duplicate a SourceGrid API?
4. Can the requirement stay internal while retaining normal SourceGrid usage?

Do not add `BootstrapRow`, `BootstrapColumn`, `BootstrapCell`, `BootstrapRange`, or similar wrapper abstractions solely for naming consistency.

## Consumer customization

Explicit consumer customization wins over integration defaults:

- consumer-assigned `Font` is consumer-owned;
- custom SourceGrid Views are not overwritten by theme changes;
- `EditorBase.UseCellViewProperties = false` opts an editor out of View-driven appearance;
- SourceGrid selection/style properties changed by the consumer should not be repeatedly reset by Bootstrap theme updates.

## Documentation

Update durable docs when changing:

```text
target frameworks
dependency direction
vendor baseline/API seam
public API
theme/font/DPI lifecycle
SourceGrid behavior intentionally overridden
testing/release procedure
package dependency strategy
```

Implementation plans are execution artifacts. Promote lasting behavior/rules into the canonical docs instead of leaving them only in a completed plan.

## Commit quality

Prefer small commits that describe one logical step, such as:

```text
feat: add Bootstrap SourceGrid theme adapter
test: preserve SourceGrid edit lifecycle
fix: harden repeated SourceGrid theme changes
docs: record DPI and designer verification
```

Do not claim a task/stage complete until its documented acceptance gate has actually passed.
