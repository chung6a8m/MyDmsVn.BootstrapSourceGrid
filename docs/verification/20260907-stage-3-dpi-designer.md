# Stage 3 DPI and Designer Verification

Date: 2026-09-08

## Environment

- Machine: Acer Nitro AN515-58 (`LAPTOP-DLPF4MIR`)
- OS: Windows 11 Home Single Language 10.0.26200 (build 26200)
- Visual Studio: Community 2022 17.14.39 (17.14.37614.0)
- Display: one 1920 x 1080 monitor at 96 DPI (100%)
- Target workflows: `net48` and `net8.0-windows`

## Automated DPI mapping evidence

The dual-target `BootstrapSourceGridDpiLifecycleTests` suite verifies the fixed
integration-owned scale mappings and proves that explicit SourceGrid row heights
and column widths are not rescaled.

| DPI | Scale | Padding | Cell border | Focus border | Result |
| ---: | ---: | ---: | ---: | ---: | --- |
| 96 | 100% | 4 | 1 | 2 | Pass on both TFMs |
| 120 | 125% | 5 | 1 | 3 | Pass on both TFMs (pure mapping) |
| 144 | 150% | 6 | 2 | 3 | Pass on both TFMs (pure mapping) |
| 192 | 200% | 8 | 2 | 4 | Pass on both TFMs (pure mapping) |

## Manual runtime DPI matrix

The demo exposes the runtime/TFM, `DeviceDpi`, current theme, computed metrics,
and theme-versus-consumer font ownership without a modal dialog.

| Scale | `net48` | `net8.0-windows` | Evidence |
| --- | --- | --- | --- |
| 100% / 96 DPI | Pass | Pass | Light/dark switching, cell and header text, span alignment, borders, alternating rows, selection/focus, editable-cell activation, and native scrolling remained usable. Diagnostics reported padding 4, border 1, and focus 2. |
| 125% / 120 DPI | Not exercised: environment limitation | Not exercised: environment limitation | The only connected display was fixed at 96 DPI. The pure metric mapping is covered automatically on both TFMs. |
| 150% / 144 DPI | Not exercised: environment limitation | Not exercised: environment limitation | The only connected display was fixed at 96 DPI. The pure metric mapping is covered automatically on both TFMs. |
| 200% / 192 DPI | Not exercised: environment limitation | Not exercised: environment limitation | The only connected display was fixed at 96 DPI. The pure metric mapping is covered automatically on both TFMs. |
| Mixed-monitor transition | Not exercised: environment limitation | Not exercised: environment limitation | No second or mixed-DPI monitor was available; the requirement remains open for a suitable environment. |

## WinForms Designer matrix

| Check | Result | Evidence |
| --- | --- | --- |
| Open form designer | Pass | `MainForm.cs [Design]` opened in Visual Studio 2022 17.14.39 with no Designer error. |
| Instantiate control | Pass | The public `BootstrapSourceGrid` appeared in the project Toolbox group and the demo form instantiated/rendered it. |
| Resize/dock/anchor | Pass | The grid and its containing table layout rendered with `DockStyle.Fill`; the design surface showed the grid tracking the available form area. |
| Save/close/reopen | Pass | The Designer was closed and reopened three times; each load reproduced the toolbar, diagnostic row, and grid surface without an error page. |
| Build/run `net48` | Pass | Release build completed with zero warnings/errors; the 96-DPI demo ran and was exercised. |
| Build/run `net8.0-windows` | Pass | Release build completed with zero warnings/errors; the 96-DPI demo ran and was exercised. |

## Findings and fixes

- The original code-only demo constructor produced a blank default design
  surface. The form was split into the conventional partial `MainForm.cs` and
  `MainForm.Designer.cs`, with layout initialization in `InitializeComponent()`.
- An initial internal diagnostic control subclass could not be resolved by the
  Designer host. The demo now instantiates the public `BootstrapSourceGrid`
  directly and keeps diagnostics in the form, avoiding a sample-only derived
  design-time type.
- No product-level Designer suppression or runtime service dependency was
  required. Representative theme initialization remains active in the Designer.

## Verdict

Stage 3 automated lifecycle coverage, both target builds, the available 96-DPI
runtime matrix, and the Visual Studio Designer matrix pass. Physical 125%, 150%,
200%, and mixed-monitor transitions were not exercised because this machine has
only one 96-DPI display; they remain explicitly recorded as environment-limited,
not removed or treated as manually passed.
