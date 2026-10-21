# BCF lookup accessory candidate pin

Integration baseline `09eab7ad3d6ef0a7fcd27c2b198f7ca573dc4b0f`; Bootstrap candidate advances only from `aba102e33c48937fd92468791c287afda0a59e77` to `2be9a4d778fda7016207b75470020be6126f2f3f` ([upstream PR72](https://github.com/chung6a8m/MyDmsVn.Bootstrap5WinFormUI/pull/72)). SourceGrid stays `f4e457b43582bf01892f50bdc74aa480531e5944`.

The consuming application's BCF-T03 instructions admit a proven narrow vendor seam with independent evidence and explicit provenance. The private lookup accessory disables UserPaint despite drawing a chevron in OnPaint, and its owner omits the accessory when applying surface tokens. Public/protected adapter hooks cannot repair that private control; no private-child mutation or grid replacement is introduced. The patch exists in the owning upstream repository before this candidate pin. It enables the existing chevron paint path, keeps it nonselectable, and propagates the existing textbox palette/invalidation. No public API or adapter product code changes.

`ActiveLookupAccessoryPaintsChevronAcrossThemeWithoutEndingCellEdit` starts the real shared SourceGrid lookup editor and checks the actual child print surface/chevron in Light/Dark, theme switching, stable native EDIT handle/focus, unchanged cell value/change count, active edit and explicit cancel. It uses the existing STA/WinForms guards. Both rows fail against the old Bootstrap pin, before the candidate pin is restored.

| Run | Actual selected/executed | Result |
|---|---|---|
| New adapter rows / old pin / net48 x86 | 2 | 2 intended FAIL; zero skipped |
| Complete integration project / candidate / net48 x86 | 243 | 243 PASS; zero skipped |
| Complete integration project / candidate / net8.0-windows x64 | 243 | 243 PASS; zero skipped |

Ordinary `dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f <target> -p:PlatformTarget=<architecture> --blame-hang --blame-hang-timeout 2m --logger trx --results-directory <context> -- RunConfiguration.TargetPlatform=<architecture>` builds both graphs and executes the actual tests sequentially on interactive Windows, SDK8.0.425/VSTest17.11.1. The focused RED adds a FullyQualifiedName filter for the named new method. Raw TRX stay ignored under `TestResults/bcf-affordance-adapter-red`, `bcf-affordance-integration-net48` and `bcf-affordance-integration-net8`.

The upstream's separate Lookup/TextBox selector passes178/178 per target, including six new render rows. No manual125/Designer, consumer package, NuGet publication or production acceptance is granted here. Earlier accepted241-row/pin evidence remains historical and is not rewritten as this candidate's result. The consuming application must separately rebuild and verify its affected scope before using this pin.
