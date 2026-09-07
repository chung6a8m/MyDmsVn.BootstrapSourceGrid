# Pending Project-Owner Decisions

This document contains decisions that should not be silently made by coding agents. They do not block normal MVP implementation unless the relevant stage says otherwise.

When the project owner decides an item, move the durable outcome into `DECISIONS.md`/`UPSTREAM.md`/release metadata and mark the item resolved here.

## 1. Repository and NuGet package license

**Status:** Pending — blocks public package publication, does not block implementation.

The repository currently integrates two independent vendor repositories without copying their source into the integration assembly. A license for this repository/package still needs explicit owner approval, and vendor redistribution/dependency obligations must be verified before public release.

### Options

**1A — MIT**

Simple permissive license, commonly suitable for a reusable .NET UI library.

**1B — Apache-2.0**

Permissive license with an explicit patent grant and somewhat more notice requirements.

**1C — Other / private-only**

Choose another license or keep the repository/package non-publicly distributed.

### Recommendation

**Recommend 1A (MIT)** for the integration's own source **if** vendor-license verification confirms the planned dependency/distribution model is compatible. Do not infer vendor license compatibility from this recommendation.

### Required follow-up after approval

```text
[ ] add root LICENSE
[ ] add package license metadata
[ ] verify both vendor licenses/notices
[ ] update docs/RELEASE.md and PACKAGE_README if needed
```

---

## 2. Public NuGet dependency strategy

**Status:** Pending — blocks public package publication, does not block implementation/local builds.

Development is already standardized on exact Git submodules + `ProjectReference`:

```text
vendor/Bootstrap5WinFormUI @ 95077df...
vendor/sourcegrid          @ f4e457b...
```

That is reproducible for source builds, but a public `MyDmsVn.BootstrapSourceGrid` NuGet package also needs dependencies that consumers can resolve from an approved feed.

### Options

**2A — Publish/consume matching vendor NuGet packages, then make BootstrapSourceGrid depend on exact versions**

Preferred long-term public distribution model. Each package should correspond to the verified pinned source baseline or an explicitly upgraded equivalent.

**2B — MVP remains source/submodule distribution only; postpone public NuGet**

Safest short-term choice if either vendor package is unavailable or cannot be tied confidently to the pinned commit.

**2C — Publish coordinated vendor packages to a controlled/internal feed, then publish BootstrapSourceGrid against those exact versions**

Appropriate for controlled enterprise/internal distribution.

**Not recommended:** silently embedding/copying vendor assemblies or source into `MyDmsVn.BootstrapSourceGrid.nupkg` merely to make dependencies disappear.

### Recommendation

Use **2B during implementation**, which is already the repository bootstrap model. Move to **2A for public NuGet release** once exact-equivalent vendor package versions are verified/published. This avoids coupling MVP implementation progress to release-feed work while retaining a clean eventual package dependency graph.

### Required follow-up after approval

```text
[ ] verify exact vendor PackageId/version/feed/source correspondence
[ ] verify both TFMs from package references
[ ] update UPSTREAM.md
[ ] test a clean consumer restore
[ ] inspect generated nupkg dependency groups
```

---

## How to answer

The project owner can answer compactly, for example:

```text
1A
2A (2B until matching packages are available)
```

or provide a different choice/rationale for either numbered item.
