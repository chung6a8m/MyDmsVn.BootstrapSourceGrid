# Documentation map

This folder contains the canonical product, architecture, compatibility, testing, upstream, and implementation-planning documentation for `MyDmsVn.BootstrapSourceGrid`.

## Read order

1. `../README.md` — product overview.
2. `../AGENTS.md` — mandatory contributor/agent rules.
3. `../AI_CONTEXT.md` — compact system model.
4. `DECISIONS.md` — approved architectural decisions and invariants.
5. `PRD.md` — product requirements and MVP boundary.
6. `ARCHITECTURE.md` — integration architecture and ownership boundaries.
7. `UPSTREAM.md` — vendor commits, dependency consumption, and upgrade process.
8. `COMPATIBILITY.md` — framework, API, Designer, DPI, and behavioral compatibility.
9. `TESTING.md` — automated and manual validation strategy.
10. `DEVELOPMENT_PLAN.md` — staged roadmap and stage gates.
11. `plans/` — task-level implementation plans.

## Source-of-truth precedence

When documents disagree, use this order:

1. Explicit current user instruction.
2. `DECISIONS.md`.
3. `PRD.md`.
4. `ARCHITECTURE.md`.
5. `UPSTREAM.md`.
6. `COMPATIBILITY.md`.
7. `TESTING.md`.
8. `DEVELOPMENT_PLAN.md`.
9. Active implementation plan under `plans/`.
10. Pinned upstream source and tests.
11. Historical discussions and feasibility notes.

Do not silently change an approved architectural decision in a lower-precedence document.

## Plan naming

Implementation plans use:

```text
YYYYMMDD-00#-plan-name.md
```

For 2026-09-07 the initial plan set begins at `20260907-001-...`.

Each stage plan must be independently reviewable and end with a test/build/documentation gate.

## Documentation maintenance

Update canonical docs in the same change when any of these change:

- target frameworks;
- dependency direction;
- vendor baseline commits or package versions;
- public BootstrapSourceGrid API;
- SourceGrid behavior intentionally overridden by the integration;
- runtime theme or DPI lifecycle;
- test execution rules;
- release/packaging expectations.

Do not use implementation plans as long-term product documentation after behavior has shipped; promote enduring rules into the canonical documents above.
