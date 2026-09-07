# Demo, Packaging, and Release Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Complete the MVP with an integrated demo, consumer documentation, reproducible release validation, package metadata, and an explicit dependency/licensing decision before public NuGet publication.

**Architecture:** The release continues to build against exact vendor submodule commits. The integration assembly remains separate. Do not silently embed/copy vendor source or binaries into the package to bypass dependency packaging. Public package publication is gated on an approved license and a resolvable vendor dependency strategy.

**Tech Stack:** WinForms demo, SDK-style NuGet pack, Windows CI, Markdown docs, dual-target test matrix.

**Spec:** PRD FR-13 and MVP acceptance gates; UPSTREAM; COMPATIBILITY; TESTING; decisions D-005/D-006/D-010/D-011.

## Release decision gates

Before publishing a public package, obtain explicit project-owner decisions for:

```text
R1. Repository/package license.
R2. NuGet dependency distribution strategy for the exact SourceGrid and Bootstrap vendor baselines.
```

Development artifacts, demo, tests, local pack validation, and documentation may proceed before those decisions. Public package publishing may not.

---

### Task 1: Build the integrated demo experience

**Files:**
- Modify: `samples/MyDmsVn.BootstrapSourceGrid.Demo/MainForm.cs`
- Add focused demo helper files under: `samples/MyDmsVn.BootstrapSourceGrid.Demo/`

**Interfaces:**
- Demonstrates only public consumer APIs plus Bootstrap theme-switch APIs.

- [ ] **Step 1: Create a compact desktop-first demo layout**

Use a top command strip plus a fill-docked `BootstrapSourceGrid`. Include:

```text
Light theme button
Dark theme button
Reset/repopulate button
current DPI/theme diagnostic label
```

Do not introduce a separate demo UI framework.

- [ ] **Step 2: Populate representative SourceGrid content**

The grid must include:

```text
column headers with sortable-header model/controller intact
row headers
ordinary editable string cells
numeric/date/bool or supported representative editors from Stage 4
read-only/disabled example where SourceGrid supports it
alternating rows
multi-selection
one row/column span
custom consumer View cell proving opt-out
explicit consumer Font example or toggle
sufficient rows/columns to scroll
```

Use normal SourceGrid APIs; do not call internal integration helpers.

- [ ] **Step 3: Add theme-switch demonstration**

Set:

```csharp
BootstrapThemeManager.CurrentTheme = BootstrapTheme.CreateDefault(BootstrapThemeMode.Light);
```

and Dark equivalent. Do not recreate the grid when switching themes.

- [ ] **Step 4: Add interaction instructions inside demo**

A small read-only help area/status text should tell reviewers to test:

```text
Tab/Shift+Tab
arrow/PageUp/PageDown
start/commit/cancel edit
sort header
select range
scroll
switch theme while selected/editing
```

- [ ] **Step 5: Build demo both TFMs**

```powershell
dotnet build samples/MyDmsVn.BootstrapSourceGrid.Demo/MyDmsVn.BootstrapSourceGrid.Demo.csproj -c Release -f net48
dotnet build samples/MyDmsVn.BootstrapSourceGrid.Demo/MyDmsVn.BootstrapSourceGrid.Demo.csproj -c Release -f net8.0-windows
```

- [ ] **Step 6: Commit demo**

```powershell
git add samples/MyDmsVn.BootstrapSourceGrid.Demo
git commit -m "demo: showcase BootstrapSourceGrid MVP"
```

---

### Task 2: Complete consumer README/package documentation

**Files:**
- Modify: `README.md`
- Create: `docs/PACKAGE_README.md`
- Create: `docs/KNOWN_LIMITATIONS.md`
- Create: `docs/RELEASE.md`

- [ ] **Step 1: Update root README from planning status to usable product documentation**

Add:

```text
installation/dependency prerequisites
supported TFMs
quick-start source sample
theme switching sample
custom SourceGrid View opt-out example
consumer Font override behavior
links to demo and docs
build-from-source/submodule commands
```

Retain exact vendor baseline information.

- [ ] **Step 2: Create package README**

`docs/PACKAGE_README.md` must be concise and consumer-oriented:

```text
what the package is
required Windows/TFMs
minimal code sample
SourceGrid API compatibility promise
Bootstrap theme behavior
known MVP boundaries
project/documentation link
```

- [ ] **Step 3: Create known limitations**

At minimum document:

```text
SourceGrid/native scrollbars are intentionally retained
editor architecture remains SourceGrid-native
some editor borders/native subcontrols may retain OS rendering
only default SourceGrid Views are automatically substituted; explicit custom Views are respected
this product currently targets concrete SourceGrid.Grid, not a separate Bootstrap GridVirtual type
Windows-only
```

Do not call intentional MVP boundaries bugs.

- [ ] **Step 4: Create release-process document**

`docs/RELEASE.md` must define:

```text
pre-release branch cleanliness
vendor submodule SHA verification
dual-TFM build/test
demo/manual matrices
pack validation
public API review
license/dependency gate
versioning/tagging
post-release verification
```

- [ ] **Step 5: Commit consumer docs**

```powershell
git add README.md docs/PACKAGE_README.md docs/KNOWN_LIMITATIONS.md docs/RELEASE.md
git commit -m "docs: add BootstrapSourceGrid consumer and release guides"
```

---

### Task 3: Add package metadata without hiding unresolved dependency issues

**Files:**
- Modify: `src/MyDmsVn.BootstrapSourceGrid/MyDmsVn.BootstrapSourceGrid.csproj`
- Use: `docs/PACKAGE_README.md`

- [ ] **Step 1: Add stable package identity**

Set:

```xml
<PackageId>MyDmsVn.BootstrapSourceGrid</PackageId>
<Title>MyDmsVn.BootstrapSourceGrid</Title>
<Description>Bootstrap-themed SourceGrid control for native Windows Forms applications targeting .NET Framework 4.8 and .NET 8 on Windows.</Description>
<Authors>chung6a8m</Authors>
<PackageProjectUrl>https://github.com/chung6a8m/MyDmsVn.BootstrapSourceGrid</PackageProjectUrl>
<RepositoryUrl>https://github.com/chung6a8m/MyDmsVn.BootstrapSourceGrid</RepositoryUrl>
<RepositoryType>git</RepositoryType>
<PackageTags>winforms;sourcegrid;bootstrap;grid;desktop;net48;net8</PackageTags>
<PackageReadmeFile>README.md</PackageReadmeFile>
<IsPackable>true</IsPackable>
<IncludeSymbols>true</IncludeSymbols>
<SymbolPackageFormat>snupkg</SymbolPackageFormat>
```

Use the repository's chosen versioning policy; do not copy Bootstrap vendor's `1.0.0-rc.1` version as this package's version merely because it is a vendor version.

- [ ] **Step 2: Include package README**

```xml
<ItemGroup>
  <None Include="..\..\docs\PACKAGE_README.md" Pack="true" PackagePath="\" Link="README.md" />
</ItemGroup>
```

Verify the resulting `.nupkg` contains `README.md` at package root.

- [ ] **Step 3: Do not set `PackageLicenseExpression` until R1 is approved**

After project owner approves a license, add the correct `PackageLicenseExpression` or packaged license file and create root `LICENSE` in the same change.

- [ ] **Step 4: Inspect project-reference pack output**

Run local pack while vendors remain project references:

```powershell
dotnet pack src/MyDmsVn.BootstrapSourceGrid/MyDmsVn.BootstrapSourceGrid.csproj -c Release -o artifacts/packages
```

Inspect generated `.nuspec` inside `.nupkg` and record how project references are represented.

The expected concern is that exact source commit pins are not necessarily resolvable as public NuGet dependencies. Do not solve this by manually embedding vendor DLLs without approval.

- [ ] **Step 5: Record dependency evidence for R2**

Create/update `docs/RELEASE.md` with:

```text
Bootstrap baseline has PackageId MyDmsVn.Bootstrap5WinFormUI and version metadata at the pinned commit.
SourceGrid baseline has Version 5.0.0 but its exact public package identity/availability must be verified before publication.
Local ProjectReference builds remain canonical until dependency packages are approved and verified.
```

- [ ] **Step 6: Commit package metadata/local-pack capability**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/MyDmsVn.BootstrapSourceGrid.csproj docs/RELEASE.md
git commit -m "build: add BootstrapSourceGrid package metadata"
```

---

### Task 4: Decide and implement public NuGet dependency strategy

**Files:**
- Modify depending on approved R2: product csproj, `docs/UPSTREAM.md`, `docs/RELEASE.md`, possibly version props.

- [ ] **Step 1: Verify public/internal package availability for exact-equivalent vendor builds**

For each vendor record:

```text
PackageId
package version
target TFMs
commit/source correspondence
feed
license
```

Do not assume SourceGrid package version `5.0.0` on a feed corresponds to the pinned fork commit without verification.

- [ ] **Step 2: Present R2 choices to project owner if not already approved**

Allowed strategies:

```text
A. Publish/consume matching vendor NuGet packages; integration package depends on them. Preferred for public distribution.
B. Keep BootstrapSourceGrid source/submodule-only for MVP and postpone public NuGet publication.
C. Publish coordinated packages from controlled feeds for both vendor baselines, then depend on exact versions.
```

Forbidden default:

```text
silently bundle vendor assemblies/source into MyDmsVn.BootstrapSourceGrid.nupkg
```

- [ ] **Step 3: Implement approved strategy and rebuild from a clean package restore**

If strategy A/C switches to `PackageReference`, remove the corresponding `ProjectReference` from release build graph and verify both TFMs against the package. Do not reference project + package copies of the same assembly simultaneously.

If strategy B is chosen, mark package publication disabled/not-release-ready while retaining local project-reference pack only for inspection; do not publish a broken dependency package.

- [ ] **Step 4: Update `docs/UPSTREAM.md`**

Record exact release dependency versions/feeds and the source commit equivalence that was verified.

- [ ] **Step 5: Commit dependency strategy**

Commit message should state the actual chosen strategy, e.g.:

```text
build: use pinned vendor packages for release
```

or:

```text
docs: defer NuGet publication pending vendor packages
```

---

### Task 5: Add Windows CI validation

**Files:**
- Create: `.github/workflows/ci.yml`
- Optionally create: `scripts/Test-All.ps1`

- [ ] **Step 1: Add Windows workflow checkout with submodules**

Use `actions/checkout` with submodules enabled. Pin actions to maintained major versions approved by repository policy.

- [ ] **Step 2: Restore/build/test**

Workflow must execute equivalent of:

```powershell
git submodule status
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release --no-restore
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
```

- [ ] **Step 3: Fail if vendor submodules are dirty**

After build/test:

```powershell
if (git -C vendor/Bootstrap5WinFormUI status --porcelain) { throw "Bootstrap vendor became dirty" }
if (git -C vendor/sourcegrid status --porcelain) { throw "SourceGrid vendor became dirty" }
```

- [ ] **Step 4: Pack as a non-publishing CI check once package dependency strategy permits**

Run `dotnet pack` and upload the `.nupkg`/`.snupkg` as CI artifacts. CI must not publish to NuGet without an explicit release workflow and secrets policy.

- [ ] **Step 5: Commit CI**

```powershell
git add .github/workflows/ci.yml scripts
git commit -m "ci: validate dual-target BootstrapSourceGrid"
```

---

### Task 6: Final public API and compatibility review

**Files:**
- Create: `docs/verification/20260907-mvp-api-review.md`
- Modify canonical docs if findings require corrections.

- [ ] **Step 1: Inventory public integration API**

List all public types/members added by this repository. Verify helpers in `Theming`, `Views`, `Editors`, and `Internal` remain internal unless a consumer requirement proves otherwise.

- [ ] **Step 2: Search for accidental SourceGrid wrappers**

Reject unnecessary public types/members matching concepts such as:

```text
BootstrapRow
BootstrapColumn
BootstrapCell
BootstrapRange
BootstrapSelection
```

unless separately approved.

- [ ] **Step 3: Search for forbidden dependency leakage**

Verify neither vendor submodule has changes/imports referencing the integration or the other vendor.

- [ ] **Step 4: Re-run compatibility scenarios**

At minimum:

```text
Redim/cell indexer
rows/columns
spans
selection/active position
custom View
editing commit/cancel
keyboard
scrolling
theme switching
consumer font override
DPI/manual Designer records
```

- [ ] **Step 5: Record review**

`docs/verification/20260907-mvp-api-review.md` must list reviewed API, any findings/fixes, known limitations, and final verdict.

- [ ] **Step 6: Commit review fixes/evidence**

```powershell
git add src tests docs README.md
git commit -m "docs: complete BootstrapSourceGrid MVP API review"
```

---

### Task 7: Final release gate

- [ ] **Step 1: Verify exact vendor source pins or approved package equivalents**

```powershell
git submodule status
```

If release build uses packages, compare versions to `docs/UPSTREAM.md` instead.

- [ ] **Step 2: Full clean validation**

```powershell
dotnet clean MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release --no-restore
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
```

- [ ] **Step 3: Run demo/manual checklist**

Execute the recorded Stage 3 DPI/Designer and Stage 4 editor/interaction matrices plus final light/dark demo smoke.

- [ ] **Step 4: Pack and inspect**

```powershell
dotnet pack src/MyDmsVn.BootstrapSourceGrid/MyDmsVn.BootstrapSourceGrid.csproj -c Release --no-build -o artifacts/packages
```

Inspect package assets for both TFMs, README, symbols, dependency declarations, repository metadata, and approved license metadata.

- [ ] **Step 5: Confirm R1/R2 are resolved before publication**

Public publish is BLOCKED if license or vendor dependency strategy is unresolved.

- [ ] **Step 6: Verify repository cleanliness**

```powershell
git status --short
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected: no uncommitted release changes and no vendor dirt.

## Stage completion criteria

- [ ] Demo exercises all PRD MVP scenarios.
- [ ] Consumer/package/release docs are complete.
- [ ] Package metadata is correct for `MyDmsVn.BootstrapSourceGrid`.
- [ ] Public API remains thin and SourceGrid-compatible.
- [ ] CI validates dual-target build/tests without modal hangs.
- [ ] Exact vendor dependency strategy is documented.
- [ ] License is explicitly approved and encoded before public publication.
- [ ] Known limitations document native scrollbar and conservative editor boundaries.
- [ ] Full release validation passes.
