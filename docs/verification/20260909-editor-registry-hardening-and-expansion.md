# Editor registry hardening and expansion verification

Date: 2026-09-09

Scope: completed Stage 4 of the Bootstrap-native editor roadmap, including the public grid-owned registry, ownership and allocation hardening, demo coverage, and documentation closure.

## Automated verification

The final repository gate completed successfully:

- recursive submodule initialization exited `0`;
- solution restore exited `0`;
- the Release solution build succeeded with 0 warnings and 0 errors for `net48` and `net8.0-windows`;
- 214 of 214 tests passed on `net48` and 214 of 214 tests passed on `net8.0-windows` with a five-minute hang timeout;
- Bootstrap5WinFormUI and SourceGrid vendor worktrees produced no status output;
- `git diff --check` exited `0`.

Focused tests additionally prove one shared editor control across 10,000 cells, exactly three explicitly configured editors across 3,000 cells, disposal of used and never-started registry controls, terminal registry disposal after either direct registry disposal or grid disposal, owner/type validation, closed-popup Enter/Escape from the focused native lookup editor, demo diagnostics after SourceGrid commit, and DisplayMember synchronization only after the logical lookup value commits.

## Manual WinForms smoke test

The Release `net8.0-windows` demo was exercised at the available 96 DPI (Windows 100%) setting in Light and Dark themes:

- text and formatted-value commits, including logical value/type diagnostics;
- Escape cancellation and Tab/Shift+Tab navigation;
- lookup Up/Down/PageUp/PageDown navigation;
- closed-popup lookup Enter committed and closed the editor after F2;
- closed-popup lookup Escape canceled and closed the editor after F2;
- lookup mouse selection, outside click, and logical `Int32` value diagnostics;
- the read-only lookup DisplayMember companion column displayed the matching name and did not enter edit mode on F2;
- runtime Light-to-Dark switching while the lookup popup was open.

Physical 150% and 200% Windows scaling were not available in this environment. DPI behavior remains covered by the existing automated metric and lifecycle tests; no claim of a physical high-DPI manual run is made here.
