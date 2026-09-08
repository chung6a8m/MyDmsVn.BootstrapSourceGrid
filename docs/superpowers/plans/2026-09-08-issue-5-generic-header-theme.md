# Issue 5 Generic Header Theme Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking. The user explicitly prohibited subagents for this work.

**Goal:** Ensure every consumer-created `SourceGrid.Cells.Header` adopts and retains Bootstrap semantic header styling in light and dark themes.

**Architecture:** Extend the existing identity-based lazy View substitution with a dedicated `BootstrapSourceGridHeaderView` for `SourceGrid.Cells.Views.Header.Default`. Keep SourceGrid behavior intact, preserve consumer custom Views, and centralize common header palette, border, padding, and font application so generic, row, and column headers stay synchronized.

**Tech Stack:** C# 12, Windows Forms, SourceGrid Views/DevAge visual elements, NUnit, `net48`, `net8.0-windows`.

**Spec:** GitHub Issue #5; `docs/PRD.md` FR-03/FR-05; `docs/ARCHITECTURE.md` sections 6 and 8; `docs/UPSTREAM_API_SEAMS.md` sections 4-6.

## Global Constraints

- Public control remains `MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid : SourceGrid.Grid`.
- Target frameworks remain `net48;net8.0-windows`.
- Replace only exact SourceGrid default View singleton identities; preserve custom consumer Views.
- Do not patch either vendor or special-case demo cell `[0,0]`.
- Native scrollbar styling remains out of scope.

---

### Task 1: Theme the generic SourceGrid header

**Files:**
- Create: `src/MyDmsVn.BootstrapSourceGrid/Views/BootstrapSourceGridHeaderView.cs`
- Create: `src/MyDmsVn.BootstrapSourceGrid/Internal/BootstrapSourceGridHeaderStyle.cs`
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Views/BootstrapSourceGridColumnHeaderView.cs`
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Views/BootstrapSourceGridRowHeaderView.cs`
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Internal/BootstrapSourceGridStyleApplicator.cs`
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridHeaderViewTests.cs`
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridRuntimeViewThemeTests.cs`
- Modify: `docs/UPSTREAM_API_SEAMS.md`

**Interfaces:**
- Consume `SourceGrid.Cells.Views.Header.Default`, `BootstrapSourceGridThemeSnapshot`, and `BootstrapSourceGridDpiMetrics`.
- Produce one shared integration-owned generic header View per grid, updated in place on theme/DPI changes.

- [x] **Step 1: Write focused failing tests**

Add tests using an actual `new SourceGrid.Cells.Header(...)` that assert default-view substitution, solid programmable generic-header background, semantic light/dark colors, custom generic Header View preservation, and runtime Light → Dark → Light synchronization while keeping the same integration View instance.

- [x] **Step 2: Verify RED on a representative target framework**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --filter "BootstrapSourceGridHeaderViewTests|BootstrapSourceGridRuntimeViewThemeTests" --blame-hang --blame-hang-timeout 5m
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter "BootstrapSourceGridHeaderViewTests|BootstrapSourceGridRuntimeViewThemeTests" --blame-hang --blame-hang-timeout 5m
```

Expected: generic Header assertions fail because its View remains `SourceGrid.Cells.Views.Header.Default` / `HeaderThemed`.

- [x] **Step 3: Implement the minimum production fix**

Create `BootstrapSourceGridHeaderView : SourceGrid.Cells.Views.Header`, replace its themed background with a solid `DevAge.Drawing.VisualElements.Header`, and apply semantic header styling through a shared internal helper. Add the generic default identity check before the ordinary-cell fallback; update the shared View on theme/DPI changes.

- [x] **Step 4: Verify GREEN and regression behavior**

Run the focused commands from Step 2, then run the complete dual-target gate:

```powershell
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
```

- [x] **Step 5: Update verified seam documentation and inspect repository hygiene**

Document `Views.Header.Default` and the programmable generic `VisualElements.Header` seam. Confirm both vendor submodules remain pinned and clean, then commit the coherent fix.
