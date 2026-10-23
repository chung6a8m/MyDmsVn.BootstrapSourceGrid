# UI-NEXT T05b decimal adapter and UI pin

On 2026-10-23, the integration advances Bootstrap5WinFormUI from the T04 pure-core pin
`59ec6b3a7c741f716c05c9f2f4166e48706f0273` to the T05a native-control commit
`d36febd3270cfba01bec545e7cb92fce2be1e0b5`. SourceGrid remains
`f4e457b43582bf01892f50bdc74aa480531e5944`; the integration parent was
`7aec7bdd2e5ec89ed86d80c1ebd1fc3df5a12dca` before T05b. No SourceGrid source changed.

The opt-in `BootstrapDecimalInputEditor` and registry factory use one grid-owned control per
configuration. `SetEditValue` starts an atomic session from the current cell model and selected
profile; SourceGrid's cancel path invokes it with the *current* model, not an adapter snapshot.
`GetEditedValue` parses only, returning typed decimal/null. An invalid candidate raises one
SourceGrid `EditException` and retains the pending text and model. Successful `ApplyEdit` lets
SourceGrid write once, then projects the accepted model without another pending-change event.
The editor rejects profile changes during an edit; a later cell starts with a cleared candidate
and undo history. External validation/error mapping belongs to the application, so the adapter
does not erase an application-owned `ValidationState` from a different row.
The internal native textbox preserves the application's prior right alignment and vertical
centering inside SourceGrid rows without changing row height.

## Verification

Interactive Windows NT 10.0.26300.0, .NET SDK 8.0.426, VSTest 17.11.1, Release. A stub
adapter first gave an observable net48 x86 RED (`3` failed / `3` passed) for accepted value,
current-model cancel and reuse reset in `TestResults/uin-t05b-red/uin-t05b-red.trx`. Its initial
dependency-restore attempts were setup failures and are not behavioral RED. The final candidate
restored and built the solution for both TFMs with zero warnings/errors. The full integration
suite executed sequentially with bounded hang collection and actual ignored TRX files:

| Target | TRX | Executed | Result |
| --- | --- | ---: | --- |
| net48 x86 | `TestResults/uin-t05b-final3-net48/uin-t05b-final3-net48.trx` | 256/256 | PASS, 0 failed/skipped |
| net8.0-windows x64 | `TestResults/uin-t05b-final3-net8/uin-t05b-final3-net8.trx` | 256/256 | PASS, 0 failed/skipped |

The selected adapter and legacy formatter, sizing, ownership, registry and edit-lifecycle
suites also passed (`82/82` per TFM) on the earlier UI T05a commit. The final full-suite runs
above supersede those preliminary results. New T05b rows include parser-only conversion,
far-tail precision rejection that SourceGrid's default decimal converter would accept,
retained external grouping, one model write, current-model cancel, first character, nullable
blank, profile/culture reuse, undo-history reset, disposal, one pending notification and native
right/vertical alignment without row-height mutation.

The tested Release binaries have these SHA-256 hashes:

| Binary | SHA-256 |
| --- | --- |
| BSG net48 | `ae02e0ae49884f44b1e003ed48d73a49928909e83fc727cf39dac759ed238854` |
| BSG net8.0-windows | `72c3b2dbc5aa911bb0330f07abe472a7cc1ea30d77fa4ea7561bb78283c26e6a` |
| BSG tests net48 | `b011464077172528d8188d1738be5e7c391d3741db9e99650252d73159a4de03` |
| BSG tests net8.0-windows | `11f7f5df51f66f49948f8552009405c4ca232ef80d7e51ba04d068ecce2463d8` |

Because this BSG checkout is nested under the application repository, the application's
`Directory.Packages.props` otherwise activates central package management for BSG's test
project, which declares inline package versions. For this local gate only, an untracked BSG-root
`Directory.Packages.props` containing `<Project />` isolated the test project; SourceGrid kept
its own nested central package file. It was removed after verification and is not part of the
candidate. The commands were ordinary `dotnet restore` / `dotnet build` and sequential
`dotnet test -f net48` (with `RunConfiguration.TargetPlatform=x86`) / `-f net8.0-windows`,
using `--no-build --no-restore --blame-hang --blame-hang-timeout 5m` for the final runs.

This is T05b adapter and pin evidence only. Application Sales/Goods adoption and UIN-A04/A06
native100/native125 acceptance are recorded by the containing application. No package was built
or accepted here; Owner, package and production release acceptance remain separate.
