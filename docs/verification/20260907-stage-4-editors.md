# Stage 4 editor verification

Verified against SourceGrid `f4e457b43582bf01892f50bdc74aa480531e5944` on both `net48` and `net8.0-windows`.

| Editor type | View color propagation | Font | Border | Commit | Cancel | Theme switch active | Result / limitation |
|---|---|---|---|---|---|---|---|
| `string` factory / `Editors.TextBox` | Back/foreground propagate | Inherits grid font | SourceGrid `DevAgeTextBox` remains `BorderStyle.None` | Pass | Pass | Pass | Supported; no replacement editor. |
| `int` factory / `Editors.TextBox` | Back/foreground propagate | Inherits grid font | SourceGrid `DevAgeTextBox` remains `BorderStyle.None` | Pass | Pass | Pass | Supported through the factory converter. |
| `DateTime` factory / `Editors.TextBoxUITypeEditor` | Outer editor control propagates | Inherits grid font | Native SourceGrid button/drop-down visuals remain unchanged | Pass | Pass | Pass | Supported; OS/native inner visuals are not repainted by the integration. |
| `bool` factory / `Editors.ComboBox` | Editor control propagates | Inherits grid font | Native combo border/drop-down remains unchanged | Pass | Pass | Pass | Supported as the factory-provided boolean editor. |
| enum factory / `Editors.ComboBox` | Editor control propagates | Inherits grid font | Native combo border/drop-down remains unchanged | Pass | Pass | Pass | Supported representative list/drop-down editor. |
| Explicit `Editors.DateTimePicker` | Editor control propagates | Inherits grid font | OS-native picker border/calendar remains unchanged | Pass | Pass | Pass | Supported when explicitly assigned; native subparts are intentionally not replaced. |

## Boundaries

- BootstrapSourceGrid does not replace SourceGrid editor classes or their commit/cancel/focus lifecycle.
- `EditorBase.UseCellViewProperties` is the ownership switch. When false, runtime theme changes leave consumer editor colors and font untouched.
- SourceGrid checkbox cells use their existing cell View/controller and do not create an active WinForms editor control. They remain native and are outside the active-editor refresh bridge.
- SourceGrid does not expose Home/End in `GridSpecialKeys` at the pinned baseline. Stage 4 does not add a Bootstrap keyboard controller for those keys.
