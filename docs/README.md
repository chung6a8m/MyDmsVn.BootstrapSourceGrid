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
7. exact pinned vendor source/tests touched by the task

Current active spec: [`EDITOR_REPLACEMENT.md`](./EDITOR_REPLACEMENT.md).

Current master roadmap: [`plans/20260909-001-bootstrap-editor-replacement-master-roadmap.md`](./plans/20260909-001-bootstrap-editor-replacement-master-roadmap.md).

Do not read archived plans during normal work. For historical context, see [Archive](./archive/).

## Canonical documents

- [`DECISIONS.md`](./DECISIONS.md) — approved architectural decisions.
- [`ARCHITECTURE.md`](./ARCHITECTURE.md) — stable ownership/integration architecture.
- [`EDITOR_REPLACEMENT.md`](./EDITOR_REPLACEMENT.md) — active post-MVP editor architecture.
- [`UPSTREAM_API_SEAMS.md`](./UPSTREAM_API_SEAMS.md) — exact APIs/seams verified at pinned vendor commits.
- [`UPSTREAM.md`](./UPSTREAM.md) — vendor pins, dependency strategy, and upgrade policy.
- [`COMPATIBILITY.md`](./COMPATIBILITY.md) — frameworks, API, Designer, DPI, and behavior compatibility.
- [`TESTING.md`](./TESTING.md) — automated/manual validation rules.
- [`DEVELOPMENT_PLAN.md`](./DEVELOPMENT_PLAN.md) — compact active stage map.
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
5. `UPSTREAM_API_SEAMS.md` for pinned-vendor facts.
6. Relevant `UPSTREAM.md` / `COMPATIBILITY.md` / `TESTING.md` / release docs.
7. `DEVELOPMENT_PLAN.md`.
8. Active implementation plan.
9. Pinned vendor source/tests.
10. Archived material.

Do not let a lower-precedence or archived document silently reverse an approved current decision.

## Documentation maintenance

Update canonical docs in the same change when a durable public API, ownership rule, vendor baseline/seam, target framework, theme/DPI lifecycle, test rule, or packaging requirement changes.

Do not use completed implementation plans as long-term product documentation; promote durable outcomes to canonical documents, then archive the plan set.
