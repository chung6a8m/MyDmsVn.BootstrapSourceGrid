# Documentation map

Canonical and active documentation for `MyDmsVn.BootstrapSourceGrid`.

## Normal agent read order

Keep context narrow. Read only:

1. `../README.md`
2. `../AGENTS.md`
3. `../AI_CONTEXT.md`
4. the active scoped spec
5. the active plan under `plans/`
6. relevant sections of `UPSTREAM_API_SEAMS.md`
7. exact pinned/target vendor source/tests touched by the task

## Active initiative

Scoped spec:

[`BOOTSTRAP_BASELINE_UPGRADE_AND_DEMO_TYPOGRAPHY.md`](./BOOTSTRAP_BASELINE_UPGRADE_AND_DEMO_TYPOGRAPHY.md)

Master roadmap:

[`plans/20260921-001-bootstrap-baseline-upgrade-and-demo-typography-master-roadmap.md`](./plans/20260921-001-bootstrap-baseline-upgrade-and-demo-typography-master-roadmap.md)

The roadmap first upgrades and verifies the Bootstrap5WinFormUI vendor baseline, then adds PR #63-equivalent demo typography profiles, then closes with a full regression/manual/documentation gate.

Current implemented Bootstrap editor architecture remains documented in [`EDITOR_REPLACEMENT.md`](./EDITOR_REPLACEMENT.md). Its completed roadmap is archived under [`archive/20260909-bootstrap-editor-replacement/`](./archive/20260909-bootstrap-editor-replacement/).

Do not read archived plans during normal work unless historical reasoning is required.

## Canonical documents

- [`DECISIONS.md`](./DECISIONS.md) — approved architectural decisions.
- [`ARCHITECTURE.md`](./ARCHITECTURE.md) — stable ownership/integration architecture.
- [`EDITOR_REPLACEMENT.md`](./EDITOR_REPLACEMENT.md) — implemented Bootstrap editor architecture.
- [`UPSTREAM_API_SEAMS.md`](./UPSTREAM_API_SEAMS.md) — exact APIs/seams verified at accepted vendor commits.
- [`UPSTREAM.md`](./UPSTREAM.md) — vendor pins, dependency strategy, and upgrade policy.
- [`COMPATIBILITY.md`](./COMPATIBILITY.md) — frameworks, API, Designer, DPI, and behavior compatibility.
- [`TESTING.md`](./TESTING.md) — automated/manual validation rules.
- [`DEVELOPMENT_PLAN.md`](./DEVELOPMENT_PLAN.md) — compact active/completed development status.
- [`PRD.md`](./PRD.md) — completed MVP requirements; use scoped specs for post-MVP initiatives.
- [`RELEASE.md`](./RELEASE.md) — packaging/publication gates.
- [`PENDING_DECISIONS.md`](./PENDING_DECISIONS.md) — unresolved owner decisions/audit trail.
- [`KNOWN_LIMITATIONS.md`](./KNOWN_LIMITATIONS.md) — current documented limitations.
- [`PACKAGE_README.md`](./PACKAGE_README.md) — package-facing consumer guide.

## Plan policy

`plans/` is **active work only**.

Plan naming:

```text
YYYYMMDD-00#-plan-name.md
```

Each stage plan must be independently reviewable, testable, and end with a build/test/documentation gate.

When a roadmap is complete, move the entire dated plan set under `archive/` and remove it from the normal agent read order.

## Source-of-truth precedence

1. Explicit current user instruction.
2. `DECISIONS.md`.
3. `ARCHITECTURE.md`.
4. Active scoped spec.
5. `UPSTREAM_API_SEAMS.md` for accepted/target-vendor facts.
6. Relevant `UPSTREAM.md` / `COMPATIBILITY.md` / `TESTING.md` / release docs.
7. `DEVELOPMENT_PLAN.md`.
8. Active implementation plan.
9. Exact vendor source/tests.
10. Archived material.

The current active spec explicitly authorizes a Bootstrap vendor-baseline upgrade. Until Stage 0 completes, canonical docs that state the old accepted baseline remain factually correct; the active spec records the approved target transition.

## Documentation maintenance

Update canonical docs in the same implementation change when a durable public API, ownership rule, accepted vendor baseline/seam, target framework, theme/DPI lifecycle, test rule, or packaging requirement changes.

Do not use completed implementation plans as long-term product documentation; promote durable outcomes to canonical documents, then archive the plan set.
