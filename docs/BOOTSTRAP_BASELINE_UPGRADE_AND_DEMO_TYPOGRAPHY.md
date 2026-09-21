# Bootstrap baseline upgrade and demo typography profiles

## Status

Active scoped specification. Stage 0 accepted the Bootstrap pin and Stage 1 implemented the demo profiles on 2026-09-21. Stage 2 automated regression and documentation synchronization are underway; manual 150%/200% DPI and Designer cells remain open. See `verification/20260921-regression-validation-and-doc-sync.md`. This specification stays active until the manual gate passes.

Owner intent:

1. move the `vendor/Bootstrap5WinFormUI` submodule from the current pinned baseline to the latest `main` HEAD;
2. prove that the integration remains compatible on both supported TFMs and across the existing SourceGrid behavior/theme/editor contracts;
3. add to `samples/MyDmsVn.BootstrapSourceGrid.Demo` a demo-only typography-profile capability equivalent in behavior to Bootstrap5WinFormUI PR #63: `Default`, `Base 14px`, and `Base 16px`, while preserving the framework core default.

This specification is an explicit owner-approved exception to the old Bootstrap baseline pin. SourceGrid's baseline is not part of this initiative.

## Verified planning snapshot — 2026-09-21

Current BootstrapSourceGrid pin:

```text
Bootstrap5WinFormUI  cceba3c969e28726935793a1c6ca3772bed60a35
SourceGrid           f4e457b43582bf01892f50bdc74aa480531e5944
```

Bootstrap5WinFormUI `main` HEAD verified while this roadmap was created:

```text
aba102e33c48937fd92468791c287afda0a59e77
Merge pull request #71 — Fix BootstrapToast auto-hide with WinForms timer
2026-09-21T05:00:12Z
```

The current Bootstrap pin is an ancestor of that HEAD. The compare contains 106 commits and 197 changed files.

Preliminary compatibility scan:

- no changed file under `src/.../Theme/`;
- no change to `Rendering/DpiScaler.cs`;
- no change to `BootstrapTextBox.cs`;
- no change to `BootstrapFormattedTextBox.cs`;
- no change to the `BootstrapLookupBox*.cs` implementation files;
- no change to `MyDmsVn.Bootstrap5WinFormUI.csproj`;
- the framework still targets `net48;net8.0-windows`.

This lowers direct API-break risk for BootstrapSourceGrid but is not compatibility proof. The baseline-upgrade stage must still compile and exercise the integration against the actual target HEAD.

Because the user requested the latest HEAD, implementation must fetch `origin/main` immediately before moving the submodule. If it has advanced beyond the planning snapshot, the newer commit becomes the target and the compare/evidence in this initiative must be refreshed before the pointer is committed.

## PR #63 behavior to carry into the SourceGrid demo

Reference: `chung6a8m/MyDmsVn.Bootstrap5WinFormUI#63`, merged 2026-09-10.

The capability is demo-owned. It must not change `BootstrapThemeTypography.Default`, `BootstrapTheme.CreateDefault(...)`, or the default typography of the framework package.

### Profiles

| Profile | Body | BodySmall | Label | HeadingSmall | HeadingMedium |
| --- | ---: | ---: | ---: | ---: | ---: |
| Default | exact `BootstrapThemeTypography.Default` | exact default | exact default | exact default | exact default |
| Base 14px | 10.5pt | 9.1875pt | 10.5pt Bold | 13.125pt Bold | 15.75pt Bold |
| Base 16px | 12pt | 10.5pt | 12pt Bold | 15pt Bold | 18pt Bold |

`Default` must reuse the exact framework typography object by reference. Do not copy its current numeric values into a second "default" object.

### State-composition contract

Theme mode, typography profile, and reduced motion are independent inputs that compose one immutable `BootstrapTheme` and are published only through `BootstrapThemeManager.CurrentTheme`.

Required behavior:

- changing Light/Dark preserves the active known typography profile and reduced-motion value;
- changing typography preserves mode and reduced-motion value;
- changing reduced motion preserves mode and typography;
- when the current theme contains an unknown/custom `BootstrapThemeTypography`, the typography selector shows no known preset;
- Light/Dark or reduced-motion changes in that custom state preserve the exact custom typography object by reference;
- choosing a known typography profile is the explicit action that replaces custom typography;
- each user settings action publishes exactly one replacement theme / `ThemeChanged` notification;
- selector synchronization caused by `ThemeChanged` must not republish.

Do not create a second global typography manager/event channel.

## SourceGrid-demo-specific decisions

### Startup behavior

Do not copy the vendor Integrated Demo's historical Base 16px startup policy.

BootstrapSourceGrid Demo currently starts from the framework/application `CurrentTheme`. Preserve that behavior. With the framework default Light theme, the typography selector therefore starts at `Default`.

If an application/test installs another theme before `MainForm` is constructed, `MainForm` must reflect that existing theme rather than overwrite it.

### Existing demo controls

The existing Light/Dark buttons may remain. Add:

- a native `ComboBox` labelled `Base font`;
- items in this exact order: `Default`, `Base 14px`, `Base 16px`;
- a native `CheckBox` labelled `Reduced motion`.

The existing Reset and Consumer font actions remain.

### Consumer grid font contract

The existing `Use consumer font` demo scenario is a BootstrapSourceGrid compatibility contract, not a demo typography preset.

After the grid has opted into a consumer-owned font, later profile/theme changes must not replace or dispose that grid font. Bootstrap-native editors may continue to follow the active theme typography according to their own framework behavior.

### Demo-owned native fonts

The demo shell should inherit `theme.Typography.Body` and update live when the typography token changes.

Any demo-owned `Font` must be cached/owned/disposed deterministically:

1. create replacement;
2. assign replacement to all consumers;
3. update the ownership field;
4. dispose the previous owned font.

Do not allocate a replacement when the effective font token has not changed.

Because this repository has one demo form, do not copy the vendor's multi-page `DemoFormBase` architecture only for parity. Keep the minimum helper surface required by this demo.

### SourceGrid dimensions and clipping

Typography profiles must not silently redefine SourceGrid/application-owned row heights or column widths in the product control.

The demo must nevertheless be usable at all three profiles. If Base 14px/Base 16px exposes clipping in demo-owned rows/header/toolbars, fix only demo-owned layout/metrics with explicit, tested demo behavior. Do not introduce automatic product-level row/column resizing.

Profile changes must not rebuild data, replace the grid instance, replace SourceGrid Views, recreate editor adapters, or change selection/edit semantics.

## Baseline compatibility contract

The Bootstrap upgrade must preserve:

- `BootstrapSourceGrid : SourceGrid.Grid`;
- `net48;net8.0-windows`;
- SourceGrid cell/range/span/selection/navigation behavior;
- runtime Light/Dark theming;
- consumer View precedence;
- consumer font precedence;
- Bootstrap editor registry ownership/sharing;
- Text / RawValue / SelectedValue logical bridges;
- editor start/commit/cancel/focus behavior;
- DPI metrics and Designer construction;
- non-modal bounded GUI tests;
- a clean `vendor/sourcegrid` submodule.

Do not modify either vendor while performing the integration upgrade. If the new Bootstrap baseline reveals a real vendor defect, first document a focused failing integration test and handle the vendor fix in its owning repository.

## Required validation

Automated gate:

```powershell
git submodule update --init --recursive
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Focused runs must cover both `-f net48` and `-f net8.0-windows` when isolating failures.

Manual/demo matrix after UI work:

- Default / Base 14px / Base 16px;
- Light / Dark;
- Reduced motion off/on;
- consumer grid font mode;
- editing with text, formatted, and lookup Bootstrap editors;
- popup open during theme/profile change;
- selection, span, sort, keyboard navigation, scrolling;
- 100%, 150%, 200% DPI where available;
- Designer construction/open-reopen smoke for both target workflows where available.

## Documentation synchronization after implementation

When the new baseline is actually accepted, update the baseline facts in the same change set:

- `README.md`;
- `AGENTS.md`;
- `AI_CONTEXT.md`;
- `docs/DECISIONS.md` D-011;
- `docs/UPSTREAM.md`;
- `docs/UPSTREAM_API_SEAMS.md`;
- relevant compatibility/testing/release notes if evidence changes them.

Do not rewrite those canonical baseline hashes before the submodule pointer and validation evidence exist.

## Definition of done

The initiative is complete only when:

- Bootstrap submodule points at the then-latest approved `main` HEAD;
- both TFMs restore/build/test cleanly;
- no existing BootstrapSourceGrid compatibility contract regresses;
- the SourceGrid demo offers the three typography profiles with PR #63-equivalent state composition;
- framework defaults remain unchanged;
- custom typography and consumer grid font precedence are preserved;
- demo UI has no proven clipping/containment regression at the required profile/DPI matrix;
- canonical baseline/seam docs match the accepted commit;
- `docs/README.md` no longer advertises this initiative as active once the plans are archived;
- this scoped spec is explicitly transitioned from `Active` to a completed/historical status before it leaves the normal agent read order;
- both vendor worktrees are clean;
- active plans are ready to archive after completion.
