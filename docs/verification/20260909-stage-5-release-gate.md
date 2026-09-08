# Stage 5 release-gate verification

Date: 2026-09-09

## Source graph and automated gate

Verified vendor pins:

```text
95077df0c8bad8593143c2190606d2f444bfc653 vendor/Bootstrap5WinFormUI
f4e457b43582bf01892f50bdc74aa480531e5944 vendor/sourcegrid
```

The required clean validation sequence completed successfully:

```powershell
dotnet clean MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release --no-restore
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
```

Results:

- build: 0 warnings, 0 errors;
- `net48`: 95 passed, 0 failed, 0 skipped;
- `net8.0-windows`: 95 passed, 0 failed, 0 skipped;
- both vendor submodules remained clean.

## Demo and manual evidence

- The Release demo executables for both TFMs completed a bounded launch smoke without early process exit.
- `BootstrapSourceGridDemoTests` shows the real form and verifies theme switching, reset without grid replacement, custom View preservation, consumer Font preservation, sortable headers, read-only content, multi-selection, span, help text, and scrolling-sized data on both TFMs.
- The existing 96-DPI runtime and Visual Studio Designer checks remain recorded in [Stage 3 DPI and Designer verification](20260907-stage-3-dpi-designer.md).
- Editor, keyboard, focus, span activation, commit/cancel, active theme switching, and native-editor limitations remain recorded in [Stage 4 editor verification](20260907-stage-4-editors.md).
- Physical 125%, 150%, 200%, and mixed-monitor testing was not rerun because the recorded environment has only one 96-DPI display. These cells remain explicitly environment-limited rather than passed.

## Inspection package

`dotnet pack --no-build` produced:

```text
MyDmsVn.BootstrapSourceGrid.0.1.0-preview.1.nupkg
MyDmsVn.BootstrapSourceGrid.0.1.0-preview.1.snupkg
```

Archive inspection verified:

- integration assets and XML documentation under `lib/net48` and `lib/net8.0-windows7.0`;
- root `README.md`;
- symbols for both TFMs;
- package ID/version/title/description/authors/tags/repository metadata;
- MIT license expression;
- no embedded SourceGrid or Bootstrap5WinFormUI binaries;
- project-reference-derived dependency entries for Bootstrap5WinFormUI `1.0.0-rc.1` and SourceGrid `5.0.0`.

The dependency values are bare/minimum version entries produced from the development project references, not the exact verified bracket pins required by D-017. The archive is therefore an inspection artifact, not a publicly publishable package.

## Blocking release findings

Public NuGet publication remains blocked because:

1. `MyDmsVn.Bootstrap5WinFormUI` has no package candidate on NuGet.org or the configured internal feed.
2. NuGet.org exposes only SourceGrid `4.4.0`; no SourceGrid `5.0.0` candidate tied to the pinned fork exists on the configured feeds.
3. Package/source correspondence, package-level transitive dependency review, and clean consumer restore cannot be completed without those candidates.
4. The pinned Bootstrap vendor has no license file or package license metadata, so its license/notice obligation is unresolved.
5. Physical multi-scale/mixed-monitor manual cells remain environment-limited.

Detailed package evidence and unblock requirements are in [UPSTREAM.md](../UPSTREAM.md#11-public-package-verification-status-2026-09-09).

## Verdict

Demo, documentation, package metadata, Windows CI, API review, and the complete source/submodule validation gate are implemented. The source-distributed MVP is ready for review under temporary strategy 2B. Stage 5 cannot be declared fully complete and no public NuGet publication is authorized until the D-017 dependency/license/clean-consumer gate and remaining required manual environment checks pass.
