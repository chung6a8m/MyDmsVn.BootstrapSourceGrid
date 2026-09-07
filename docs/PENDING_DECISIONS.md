# Pending Project-Owner Decisions

This document tracks project-owner decisions that coding agents must not make silently. As of 2026-09-07, the initial license and NuGet dependency-strategy decisions are resolved.

Durable decisions live in `DECISIONS.md` and `UPSTREAM.md`. This file remains as an audit trail and a place for future owner-only decisions.

## 1. Repository and NuGet package license

**Status:** Resolved — selected **1A (MIT)** on 2026-09-07.

### Approved outcome

- The repository's own integration source uses the MIT license.
- Root `LICENSE` is present.
- Future NuGet metadata must set `PackageLicenseExpression` to `MIT`.
- Vendor redistribution/dependency license obligations must still be verified before public release.

### Remaining release verification

```text
[x] add root LICENSE
[ ] add PackageLicenseExpression=MIT when product package metadata is implemented
[ ] verify Bootstrap5WinFormUI vendor license/notices
[ ] verify SourceGrid vendor license/notices
[ ] reflect verified notices/obligations in release/package docs if required
```

Canonical decision: `DECISIONS.md` D-016.

---

## 2. Public NuGet dependency strategy

**Status:** Resolved — selected **2A with 2B temporary during development** on 2026-09-07.

### Approved outcome

Development and pre-release continue to use exact Git submodules plus `ProjectReference`:

```text
vendor/Bootstrap5WinFormUI @ 95077df...
vendor/sourcegrid          @ f4e457b...
```

Public NuGet publication must use strategy **2A**: the `MyDmsVn.BootstrapSourceGrid` package depends on resolvable exact-version vendor NuGet packages that are verified as equivalent to the tested source baselines, or to explicitly approved upgraded baselines.

Temporary strategy **2B** remains valid only for source builds/development while matching vendor packages are unavailable or unverified.

### Public-release requirements

For each vendor package verify and record:

```text
PackageId
package version
feed
supported TFMs
source/commit correspondence
public API/behavior equivalence
transitive dependencies
license/notice obligations
```

Then:

```text
[ ] switch release validation from ProjectReference to approved PackageReference dependencies
[ ] verify both net48 and net8.0-windows
[ ] update UPSTREAM.md with exact package versions/feeds/source equivalence
[ ] test a clean consumer restore with no vendor source checkout
[ ] inspect generated nupkg dependency groups
[ ] verify project and package copies of the same vendor assembly are never referenced together
```

Forbidden workaround:

```text
silently embedding/copying vendor assemblies or source into MyDmsVn.BootstrapSourceGrid.nupkg
```

Canonical decision: `DECISIONS.md` D-017.

---

## Current owner-decision status

No unresolved owner decision currently blocks MVP implementation.

Public NuGet publication is still gated by **verification and implementation work** for the approved D-016/D-017 decisions, but not by an undecided policy choice.

Add future owner-only decisions below using the next numbered item.
