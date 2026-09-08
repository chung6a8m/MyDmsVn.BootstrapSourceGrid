# Release process

## Release status

Development and source validation use pinned vendor submodules plus `ProjectReference` (D-017 temporary strategy 2B). A locally generated package is an inspection artifact only until every public-dependency gate below passes. CI and local validation must not describe or publish that artifact as a public release candidate while the gate is blocked.

The current integration inspection package version is `0.1.0-preview.1`. It belongs to this repository's own version line and is not derived from the Bootstrap vendor's `1.0.0-rc.1` metadata. A later release version must still pass every gate in this document.

As verified on 2026-09-09, public publication is **BLOCKED**: no exact Bootstrap package is available, NuGet.org exposes only legacy SourceGrid `4.4.0`, the configured internal feed contains neither required package, source/package correspondence cannot be established, and the Bootstrap vendor license/notice obligation is unresolved. See [UPSTREAM.md](UPSTREAM.md#11-public-package-verification-status-2026-09-09) for evidence and unblock requirements.

## Pre-release checklist

1. Start from a clean release branch and confirm `git status --short` is empty.
2. Initialize submodules and compare `git submodule status` with the exact commits in [UPSTREAM.md](UPSTREAM.md).
3. Confirm neither vendor worktree is dirty and neither vendor imports the integration or the other vendor.
4. Review the public integration API for accidental wrappers or newly exposed helpers.
5. Run the full dual-TFM automated gate.
6. Run the demo, editor/interaction, DPI, theme, and Designer manual matrices.
7. Pack to a clean artifact directory and inspect both `.nupkg` and `.snupkg`.
8. Validate a clean consumer for both TFMs with no vendor source checkout.

## Automated validation

```powershell
dotnet clean MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release --no-restore
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
```

Automated WinForms tests must remain STA, bounded, and free of modal failure UI.

## Manual validation

Record results for both target workflows:

- light, dark, and repeated runtime theme switching with data and selection retained;
- text, numeric, date, bool, enum, and DateTimePicker editing, including commit/cancel and theme switching during edit;
- Tab, Shift+Tab, arrows, Home/End, PageUp/PageDown, F2, typing, focus transfer, selection, spans, sorting, and scrolling;
- 100%, 125%, 150%, and 200% DPI where practical, including a per-monitor transition;
- Visual Studio Designer construction, resize/dock/anchor, serialization, reopen, run, and repeated create/dispose.

Environment-limited manual cells must be recorded as not run, not silently marked passed.

## Public NuGet dependency gate

For each vendor package, record all of the following in [UPSTREAM.md](UPSTREAM.md):

```text
PackageId
exact version
feed and resolvability
supported TFMs
verified source commit or approved upgraded baseline
public API and behavior equivalence
transitive dependency review
license and notice obligations
```

The release build must use only those exact `PackageReference` dependencies. It must not simultaneously reference project and package copies of the same assembly. If either package cannot be tied confidently to an approved baseline, public publication stops and source-development mode remains on submodules.

Never copy vendor DLLs or source into the integration package to bypass this gate.

### Current project-reference pack evidence

The pinned Bootstrap project declares `PackageId` `MyDmsVn.Bootstrap5WinFormUI` and version `1.0.0-rc.1`. The pinned SourceGrid project declares version `5.0.0`; its default SDK package identity is `SourceGrid`, but public feed availability and correspondence to fork commit `f4e457b43582bf01892f50bdc74aa480531e5944` still require independent verification.

Packing the development graph as `0.1.0-preview.1` produces dependency groups for both TFMs with bare version values `MyDmsVn.Bootstrap5WinFormUI` `1.0.0-rc.1` and `SourceGrid` `5.0.0`. Those entries are not exact NuGet bracket pins, and the package contains no vendor binaries. This is useful inspection evidence, but it does not satisfy D-017's exact-package/source-equivalence gate and must not be published as the public integration package.

## Package inspection

```powershell
dotnet pack src/MyDmsVn.BootstrapSourceGrid/MyDmsVn.BootstrapSourceGrid.csproj -c Release -o artifacts/packages
```

Inspect the archive and generated `.nuspec` for:

- `lib/net48` and `lib/net8.0-windows` assets;
- exact dependency groups and no duplicate vendor binaries;
- root `README.md` sourced from `docs/PACKAGE_README.md`;
- symbols package;
- repository URL/type and package identity;
- `license type="expression">MIT</license>`;
- required vendor notices, based on the completed license review.

## Versioning, tagging, and publication

Choose this package's version from its own release history; never copy a vendor version. Update release notes, package version metadata, and any API baseline together. Create an annotated tag matching the approved version only after all gates pass. Publication requires an explicit release workflow and secrets policy; ordinary CI never publishes.

After publication, restore the exact published version from the public feed into clean `net48` and `net8.0-windows` consumers, compile the minimal sample, run the runtime smoke, verify package metadata on the feed, and compare package hashes with the approved artifacts.
