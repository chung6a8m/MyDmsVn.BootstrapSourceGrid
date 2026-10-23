# UIN-T04 decimal core pin

On 2026-10-23, the Bootstrap UI candidate moves from `2be9a4d778fda7016207b75470020be6126f2f3f` to `59ec6b3a7c741f716c05c9f2f4166e48706f0273`. Its T04a parser commit and T04b projection commit add pure, transport-neutral decimal APIs without changing the existing grid editor, formatting or native control path. SourceGrid remains `f4e457b43582bf01892f50bdc74aa480531e5944`. This integration repository changes only the Bootstrap UI gitlink and pin documentation.

On interactive Windows with SDK 8.0.425 and VSTest 17.11.1, the integration solution restored and built in Release with zero warnings/errors. The net48 x86 integration test project executed 243/243 passing rows, zero skipped; net8.0-windows x64 executed 243/243 passing rows, zero skipped. Both runs used ordinary `dotnet test`, bounded hang collection and actual TRX under ignored `TestResults/uin-t04-bsg-net48` and `TestResults/uin-t04-bsg-net8`.

The containing application provides its own receipt numeric oracle and T04 acceptance evidence. These integration tests establish compatibility of the new pin with existing BSG behavior; they do not establish T05 native edit, paste, IME, undo or commit behavior, package acceptance or release approval.
