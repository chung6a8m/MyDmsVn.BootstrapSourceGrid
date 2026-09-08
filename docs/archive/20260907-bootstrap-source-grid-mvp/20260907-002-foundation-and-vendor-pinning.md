# Foundation and Vendor Pinning Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a reproducible dual-target solution with pinned vendor submodules, product/test/demo projects, and an automation-safe WinForms test baseline.

**Architecture:** The integration project references vendor projects from exact Git submodule commits. The solution contains no copied vendor source and no vendor modifications. Tests target both supported TFMs and establish non-interactive WinForms execution before UI behavior is added.

**Tech Stack:** Git submodules, C#, SDK-style projects, Windows Forms, `net48`, `net8.0-windows`, NUnit.

**Spec:** `docs/PRD.md`; `docs/UPSTREAM.md`; `docs/COMPATIBILITY.md`; `docs/TESTING.md`.

## Global Constraints

- Bootstrap commit: `95077df0c8bad8593143c2190606d2f444bfc653`.
- SourceGrid commit: `f4e457b43582bf01892f50bdc74aa480531e5944`.
- Product namespace: `MyDmsVn.Bootstrap5WinFormUI.Controls`.
- TFMs: `net48;net8.0-windows`.
- Do not modify vendor source.
- GUI tests must be bounded and non-interactive.

---

### Task 1: Add and pin vendor submodules

**Files:**
- Create: `.gitmodules`
- Add gitlink: `vendor/Bootstrap5WinFormUI`
- Add gitlink: `vendor/sourcegrid`
- Verify: `docs/UPSTREAM.md`

**Interfaces:**
- Consumes: approved vendor repositories/commit SHAs.
- Produces: deterministic source locations for later `ProjectReference` items.

- [ ] **Step 1: Confirm clean superproject state**

Run:

```powershell
git status --short
```

Expected: no unrelated local changes before submodule operations.

- [ ] **Step 2: Add Bootstrap framework submodule**

```powershell
git submodule add https://github.com/chung6a8m/MyDmsVn.Bootstrap5WinFormUI.git vendor/Bootstrap5WinFormUI
git -C vendor/Bootstrap5WinFormUI checkout 95077df0c8bad8593143c2190606d2f444bfc653
```

- [ ] **Step 3: Add SourceGrid submodule**

```powershell
git submodule add https://github.com/chung6a8m/sourcegrid.git vendor/sourcegrid
git -C vendor/sourcegrid checkout f4e457b43582bf01892f50bdc74aa480531e5944
```

- [ ] **Step 4: Verify exact pins**

```powershell
git submodule status
```

Expected output contains exactly:

```text
95077df0c8bad8593143c2190606d2f444bfc653 vendor/Bootstrap5WinFormUI
f4e457b43582bf01892f50bdc74aa480531e5944 vendor/sourcegrid
```

Leading status characters may vary with Git state, but the SHAs must match.

- [ ] **Step 5: Verify vendor worktrees are clean**

```powershell
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected: no output.

- [ ] **Step 6: Commit vendor pins**

```powershell
git add .gitmodules vendor/Bootstrap5WinFormUI vendor/sourcegrid
git commit -m "build: pin Bootstrap and SourceGrid vendors"
```

---

### Task 2: Create the dual-target product project and solution

**Files:**
- Create: `MyDmsVn.BootstrapSourceGrid.sln`
- Create: `src/MyDmsVn.BootstrapSourceGrid/MyDmsVn.BootstrapSourceGrid.csproj`
- Create: `src/MyDmsVn.BootstrapSourceGrid/Properties/AssemblyInfo.cs`

**Interfaces:**
- Consumes: vendor project locations from Task 1.
- Produces: assembly `MyDmsVn.BootstrapSourceGrid` referencing both vendors.

- [ ] **Step 1: Create project directories**

```powershell
New-Item -ItemType Directory -Force src/MyDmsVn.BootstrapSourceGrid/Properties | Out-Null
```

- [ ] **Step 2: Create the product project**

Write `src/MyDmsVn.BootstrapSourceGrid/MyDmsVn.BootstrapSourceGrid.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net48;net8.0-windows</TargetFrameworks>
    <UseWindowsForms>true</UseWindowsForms>
    <AssemblyName>MyDmsVn.BootstrapSourceGrid</AssemblyName>
    <RootNamespace>MyDmsVn.Bootstrap5WinFormUI</RootNamespace>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <Nullable>enable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\vendor\Bootstrap5WinFormUI\src\MyDmsVn.Bootstrap5WinFormUI\MyDmsVn.Bootstrap5WinFormUI.csproj" />
    <ProjectReference Include="..\..\vendor\sourcegrid\SourceGrid\SourceGrid.csproj" />
  </ItemGroup>
</Project>
```

If `net48` compilation proves the pinned vendor language settings require a repository-wide language-version property, add the narrowest compatible `LangVersion` in this project only and document it in `docs/COMPATIBILITY.md`; do not change vendor project files.

- [ ] **Step 3: Create platform annotation for modern target if required by warnings**

Create `src/MyDmsVn.BootstrapSourceGrid/Properties/AssemblyInfo.cs` only with attributes proven necessary by the build. Start with:

```csharp
#if NET8_0_OR_GREATER
using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("windows")]
#endif
```

- [ ] **Step 4: Create solution and add product project**

```powershell
dotnet new sln --format sln -n MyDmsVn.BootstrapSourceGrid
dotnet sln MyDmsVn.BootstrapSourceGrid.sln add src/MyDmsVn.BootstrapSourceGrid/MyDmsVn.BootstrapSourceGrid.csproj
```

- [ ] **Step 5: Restore and build product project**

```powershell
dotnet restore src/MyDmsVn.BootstrapSourceGrid/MyDmsVn.BootstrapSourceGrid.csproj
dotnet build src/MyDmsVn.BootstrapSourceGrid/MyDmsVn.BootstrapSourceGrid.csproj -c Release -f net48
dotnet build src/MyDmsVn.BootstrapSourceGrid/MyDmsVn.BootstrapSourceGrid.csproj -c Release -f net8.0-windows
```

Expected: both builds exit `0` without modifying vendor source.

- [ ] **Step 6: Commit product foundation**

```powershell
git add MyDmsVn.BootstrapSourceGrid.sln src/MyDmsVn.BootstrapSourceGrid
git commit -m "build: add dual-target integration project"
```

---

### Task 3: Create the NUnit test project with WinForms safety baseline

**Files:**
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/WinFormsTestGuard.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/FoundationTests.cs`
- Modify: `MyDmsVn.BootstrapSourceGrid.sln`

**Interfaces:**
- Consumes: product project from Task 2.
- Produces: dual-target NUnit harness and reusable `WinFormsTestGuard`.

- [ ] **Step 1: Create NUnit test project**

```powershell
dotnet new nunit -n MyDmsVn.BootstrapSourceGrid.Tests -o tests/MyDmsVn.BootstrapSourceGrid.Tests
dotnet sln MyDmsVn.BootstrapSourceGrid.sln add tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj
```

- [ ] **Step 2: Replace generated project framework settings**

Ensure the project contains:

```xml
<PropertyGroup>
  <TargetFrameworks>net48;net8.0-windows</TargetFrameworks>
  <UseWindowsForms>true</UseWindowsForms>
  <IsPackable>false</IsPackable>
  <Nullable>enable</Nullable>
  <ImplicitUsings>disable</ImplicitUsings>
</PropertyGroup>

<ItemGroup>
  <ProjectReference Include="..\..\src\MyDmsVn.BootstrapSourceGrid\MyDmsVn.BootstrapSourceGrid.csproj" />
</ItemGroup>
```

Keep package versions generated by `dotnet new nunit` only if they restore for both TFMs. If the template selects a package incompatible with `net48`, align with the NUnit/NUnit3TestAdapter versions already proven by the pinned vendor test projects.

- [ ] **Step 3: Add WinForms exception guard**

Create `WinFormsTestGuard.cs`:

```csharp
using System;
using System.Threading;
using System.Windows.Forms;

namespace MyDmsVn.BootstrapSourceGrid.Tests;

internal static class WinFormsTestGuard
{
    private static int _configured;

    public static void Configure()
    {
        if (Interlocked.Exchange(ref _configured, 1) != 0)
        {
            return;
        }

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
    }
}
```

- [ ] **Step 4: Add a baseline STA test**

Create `FoundationTests.cs`:

```csharp
using System.Threading;
using NUnit.Framework;

namespace MyDmsVn.BootstrapSourceGrid.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class FoundationTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        WinFormsTestGuard.Configure();
    }

    [Test]
    public void VendorTypesAreLoadable()
    {
        Assert.That(typeof(SourceGrid.Grid), Is.Not.Null);
        Assert.That(typeof(MyDmsVn.Bootstrap5WinFormUI.Theme.BootstrapThemeManager), Is.Not.Null);
    }
}
```

- [ ] **Step 5: Run both target test frameworks with bounded hang detection**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --blame-hang --blame-hang-timeout 5m
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --blame-hang --blame-hang-timeout 5m
```

Expected: PASS, no modal window.

- [ ] **Step 6: Commit test foundation**

```powershell
git add tests/MyDmsVn.BootstrapSourceGrid.Tests MyDmsVn.BootstrapSourceGrid.sln
git commit -m "test: add dual-target WinForms-safe harness"
```

---

### Task 4: Create the demo project shell

**Files:**
- Create: `samples/MyDmsVn.BootstrapSourceGrid.Demo/MyDmsVn.BootstrapSourceGrid.Demo.csproj`
- Create: `samples/MyDmsVn.BootstrapSourceGrid.Demo/Program.cs`
- Create: `samples/MyDmsVn.BootstrapSourceGrid.Demo/MainForm.cs`
- Modify: `MyDmsVn.BootstrapSourceGrid.sln`

**Interfaces:**
- Consumes: product project.
- Produces: executable WinForms host for later visual/manual scenarios.

- [ ] **Step 1: Create WinForms project**

```powershell
dotnet new winforms -n MyDmsVn.BootstrapSourceGrid.Demo -o samples/MyDmsVn.BootstrapSourceGrid.Demo
dotnet sln MyDmsVn.BootstrapSourceGrid.sln add samples/MyDmsVn.BootstrapSourceGrid.Demo/MyDmsVn.BootstrapSourceGrid.Demo.csproj
```

- [ ] **Step 2: Configure both TFMs and project reference**

Set:

```xml
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <TargetFrameworks>net48;net8.0-windows</TargetFrameworks>
  <UseWindowsForms>true</UseWindowsForms>
  <Nullable>enable</Nullable>
  <ImplicitUsings>disable</ImplicitUsings>
</PropertyGroup>

<ItemGroup>
  <ProjectReference Include="..\..\src\MyDmsVn.BootstrapSourceGrid\MyDmsVn.BootstrapSourceGrid.csproj" />
</ItemGroup>
```

- [ ] **Step 3: Keep the initial form intentionally minimal**

`MainForm.cs` should contain only a title and a placeholder panel/label indicating later stages populate the demo. Do not prematurely implement grid theming in Stage 0.

- [ ] **Step 4: Build demo on both TFMs**

```powershell
dotnet build samples/MyDmsVn.BootstrapSourceGrid.Demo/MyDmsVn.BootstrapSourceGrid.Demo.csproj -c Release -f net48
dotnet build samples/MyDmsVn.BootstrapSourceGrid.Demo/MyDmsVn.BootstrapSourceGrid.Demo.csproj -c Release -f net8.0-windows
```

Expected: both builds exit `0`.

- [ ] **Step 5: Commit demo shell**

```powershell
git add samples/MyDmsVn.BootstrapSourceGrid.Demo MyDmsVn.BootstrapSourceGrid.sln
git commit -m "demo: add dual-target WinForms host"
```

---

### Task 5: Run the clean-clone reproducibility gate

**Files:**
- Verify: `.gitmodules`
- Verify: solution/projects
- Modify docs only if commands differ from canonical instructions.

**Interfaces:**
- Consumes: Tasks 1-4.
- Produces: evidence that a fresh checkout can reproduce the build.

- [ ] **Step 1: Verify submodule pointers and no vendor dirt**

```powershell
git submodule status
git status --short
```

Expected: exact pinned SHAs, no dirty vendor marker.

- [ ] **Step 2: Run full restore/build/test**

```powershell
git submodule update --init --recursive
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
```

Expected: all commands exit `0`.

- [ ] **Step 3: Confirm vendor projects were not modified by tooling**

```powershell
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected: no output.

- [ ] **Step 4: Record any command correction in canonical docs before final commit**

If actual generated project/package details require a command change, update `README.md`, `AGENTS.md`, or `docs/TESTING.md` in the same stage. Do not leave the docs describing commands that were not executed successfully.

- [ ] **Step 5: Final stage commit if docs changed**

```powershell
git add README.md AGENTS.md docs
git commit -m "docs: align foundation validation instructions"
```

Skip this commit only if no files changed.
