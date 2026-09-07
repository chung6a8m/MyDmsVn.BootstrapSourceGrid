# Demo, Packaging, and Release Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Complete the MVP with an integrated demo, consumer documentation, reproducible release validation, package metadata, MIT licensing, and the approved two-phase vendor dependency strategy.

**Architecture:** Development/pre-release continues to build against exact vendor submodule commits. The integration assembly remains separate. The repository/package license is MIT (D-016). Public NuGet publication uses exact matching/resolvable vendor NuGet packages (D-017 / strategy 2A); pinned submodules plus `ProjectReference` remain the temporary development model (strategy 2B). Do not silently embed/copy vendor source or binaries into the package to bypass dependency packaging.

**Tech Stack:** WinForms demo, SDK-style NuGet pack, Windows CI, Markdown docs, dual-target test matrix.

**Spec:** PRD FR-13 and MVP acceptance gates; UPSTREAM; COMPATIBILITY; TESTING; decisions D-005/D-006/D-010/D-011/D-016/D-017.

## Release decision status

Project-owner decisions were resolved on 2026-09-07:

```text
R1. Repository/package license: MIT (1A / D-016).
R2. Public NuGet dependency strategy: 2A, with 2B temporary during development (D-017).
```

These policy choices no longer require another approval prompt. Public publication is still blocked until their implementation/verification requirements pass: vendor package identity/equivalence, vendor license/notice review, clean consumer restore, dual-TFM package validation, and final package inspection.

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
MIT license link
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
MIT license
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
MIT package metadata
vendor package-equivalence/dependency gate
vendor license/notice verification
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
- Use: `LICENSE`

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
<PackageLicenseExpression>MIT</PackageLicenseExpression>
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

- [ ] **Step 3: Encode the approved MIT license**

D-016 is already approved and root `LICENSE` already exists. Keep `PackageLicenseExpression` set to `MIT` and verify generated package metadata reports MIT.

Do not duplicate the MIT text inside the package unless NuGet/package policy later requires a license file instead of the expression.

- [ ] **Step 4: Inspect project-reference pack output**

Run local pack while vendors remain project references:

```powershell
dotnet pack src/MyDmsVn.BootstrapSourceGrid/MyDmsVn.BootstrapSourceGrid.csproj -c Release -o artifacts/packages
```

Inspect generated `.nuspec` inside `.nupkg` and record how project references are represented.

The expected concern is that exact source commit pins are not necessarily resolvable as public NuGet dependencies. Do not solve this by manually embedding vendor DLLs.

- [ ] **Step 5: Record dependency evidence for approved D-017**

Create/update `docs/RELEASE.md` with:

```text
Bootstrap baseline has PackageId MyDmsVn.Bootstrap5WinFormUI and version metadata at the pinned commit.
SourceGrid baseline has Version 5.0.0 but its exact public package identity/availability and source correspondence must be verified before publication.
Local ProjectReference builds remain canonical during development (temporary 2B).
Public NuGet publication requires verified exact matching/resolvable vendor PackageReference dependencies (2A).
```

- [ ] **Step 6: Commit package metadata/local-pack capability**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/MyDmsVn.BootstrapSourceGrid.csproj docs/RELEASE.md
git commit -m "build: add BootstrapSourceGrid package metadata"
```

---

### Task 4: Implement the approved public NuGet dependency strategy

**Files:**
- Modify: product csproj, `docs/UPSTREAM.md`, `docs/RELEASE.md`, possibly version props.

**Approved strategy:** D-017 / 2A for public release, with 2B temporary during development.

- [ ] **Step 1: Verify public/internal package availability for exact-equivalent vendor builds**

For each vendor record:

```text
PackageId
package version
target TFMs
commit/source correspondence
feed
license/notices
transitive dependencies
```

Do not assume SourceGrid package version `5.0.0` on a feed corresponds to the pinned fork commit without verification.

- [ ] **Step 2: Apply D-017 without requesting another strategy decision**

The required behavior is:

```text
Development: pinned submodules + ProjectReference (temporary 2B).
Public release: exact verified vendor NuGet PackageReference dependencies (2A).
```

If matching vendor packages are not yet available or cannot be tied confidently to an approved source baseline, stop the public-publication path and keep development/source distribution on 2B. This is an implementation/verification blocker, not a reason to silently choose a different policy.

Forbidden default:

```text
silently bundle vendor assemblies/source into MyDmsVn.BootstrapSourceGrid.nupkg
```

- [ ] **Step 3: Switch the release build graph to approved PackageReference dependencies**

Remove the corresponding release `ProjectReference` dependencies and verify both TFMs against the vendor packages. Do not reference project + package copies of the same assembly simultaneously.

The implementation may use an explicit build property/configuration to retain project references for source-development while using package references for clean release validation, provided the graph is unambiguous and documented.

- [ ] **Step 4: Validate clean package consumption**

From a clean consumer project with no vendor source checkout:

```text
restore MyDmsVn.BootstrapSourceGrid plus its dependencies
compile a minimal BootstrapSourceGrid sample for net48
compile a minimal BootstrapSourceGrid sample for net8.0-windows
run representative runtime smoke tests
```

- [ ] **Step 5: Update `docs/UPSTREAM.md` and `docs/RELEASE.md`**

Record exact release dependency versions/feeds and the source commit equivalence that was verified, plus vendor license/notice findings.

- [ ] **Step 6: Commit dependency strategy implementation**

Once verified:

```text
build: use verified vendor packages for release
```

If vendor packages are still unavailable, do not mark this task complete and do not publish the integration package.

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

- [ ] **Step 4: Pack as a non-publishing CI check once approved package dependencies are available**

Run `dotnet pack` and upload the `.nupkg`/`.snupkg` as CI artifacts. CI must not publish to NuGet without an explicit release workflow and secrets policy.

Until D-017 package-equivalence requirements are met, source/submodule CI may perform local pack inspection but must not represent that artifact as publicly publishable.

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

- [ ] **Step 1: Verify exact vendor source pins and approved package equivalents**

For development/source verification:

```powershell
git submodule status
```

For public release, compare exact PackageReference versions/feeds and verified source correspondence to `docs/UPSTREAM.md`.

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

Inspect package assets for both TFMs, README, symbols, dependency declarations, repository metadata, `PackageLicenseExpression=MIT`, and required vendor notices.

- [ ] **Step 5: Verify D-016/D-017 implementation before publication**

The policy decisions are already resolved. Public publish is BLOCKED unless all of the following are true:

```text
MIT package metadata is encoded correctly.
Vendor license/notice obligations are verified.
Exact vendor package versions/feeds/source equivalence are documented.
Release build uses those approved resolvable PackageReference dependencies.
Clean consumer restore succeeds without vendor source checkout.
Both TFMs pass build/test/demo/Designer validation against package dependencies.
```

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
- [ ] Package metadata is correct for `MyDmsVn.BootstrapSourceGrid` and declares MIT.
- [ ] Public API remains thin and SourceGrid-compatible.
- [ ] CI validates dual-target build/tests without modal hangs.
- [ ] Development source mode remains pinned/reproducible through submodules + ProjectReference.
- [ ] Public NuGet dependency graph uses verified exact vendor packages per D-017.
- [ ] Vendor license/notice obligations are verified for the public dependency graph.
- [ ] Known limitations document native scrollbar and conservative editor boundaries.
- [ ] Full release validation passes.
