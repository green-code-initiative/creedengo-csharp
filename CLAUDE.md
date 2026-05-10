# Notes for Claude (and other contributors)

This file collects context that isn't obvious from the code, plus a small backlog of follow-up work captured during the v3.0 refresh.

## Project shape

- **Creedengo.Core / Creedengo.Package** — the analyzer NuGet shipped to consumers. Targets `netstandard2.0`, references Roslyn 4.8.0 (the lowest version that compiles the source). Bumping Roslyn raises the minimum SDK consumers need: 4.8 → SDK 8, 5.x → SDK 10. Don't bump unless you've decided to drop a consumer SDK floor.
- **Creedengo.Tool** — the CLI tool published as a `dotnet tool`. Multi-targets `net8.0;net10.0` so users on either runtime work. `Microsoft.Build` is pinned to `17.11.48` because 18.x ships only `net472`+`net10.0` libs (no net8) and produces NU1701 on net8.
- **Creedengo.Tests** — MSTest + Microsoft.CodeAnalysis.Testing harness. Test class names should match the analyzer class name with a `Tests` suffix.

## Versioning

Release versioning uses the SDK's `VersionPrefix` / `VersionSuffix` split, set at the workflow level, **not** by rewriting `Directory.Build.props`. The dummy `<VersionPrefix>1.0.0</VersionPrefix>` is overridden by `02-build.yml` via `-p:VersionPrefix=… -p:VersionSuffix=…`. The SDK derives `Version`, `PackageVersion`, `AssemblyInformationalVersion` automatically. `AssemblyVersion`/`FileVersion` get the numeric stem only (CS7035 requires it).

If you ever need to verify the wiring locally:
```
dotnet pack ToolOnly.slnf -c Release -o /tmp/out -p:VersionPrefix=3.1.0 -p:VersionSuffix=beta1
unzip -l /tmp/out/Creedengo.Tool.3.1.0-beta1.nupkg | grep DotnetToolSettings
```
The `DotnetToolSettings.xml` line should be present for both `tools/net8.0/any/` and `tools/net10.0/any/`. Its absence is the symptom that bit a user pre-3.0 (caused by `--no-build` on `dotnet pack` of a multi-targeted tool).

## Analyzer authoring conventions

- **Always check semantic compatibility before reporting.** GCI93 had a bug where `async ValueTask` awaiting a `Task` was flagged but unfixable (different task wrappers). Pattern to follow: get the method's return type, get the awaited expression's type, compare `OriginalDefinition` with `SymbolEqualityComparer.Default`.
- **Cover all the syntactic shapes the rule applies to.** `async`/`await` analyzers should subscribe to both `MethodDeclaration` and `LocalFunctionStatement`. Lambda-context walks should pattern-match `LambdaExpressionSyntax` (covers both simple and parenthesized) plus `AnonymousMethodExpressionSyntax`.
- **Hoist `Compilation.GetTypeByMetadataName` lookups.** Use `RegisterCompilationStartAction`, resolve the well-known types once, capture them in the closure for `RegisterSyntaxNodeAction`. GCI87 and GCI96 follow this pattern.
- **In code fixers, prefer `root.FindNode(span, getInnermostNodeForTie: true)`** over plain `FindNode`. Without it, when the diagnostic span exactly matches a wrapping node (e.g. an `ArgumentSyntax`), the call returns the outer node instead of the one you reported on. This is how GCI96's fixer accumulated five duplicated branches before it got cleaned up.
- **When removing the `async` modifier, transfer its leading trivia to the next token if it's the first modifier.** Otherwise local functions and methods declared without an accessibility modifier lose their indentation. GCI93's fixer handles this for both `MethodDeclaration` and `LocalFunctionStatement`.
- **`async void` analyzers should never trigger on `OperationCanceledException`.** When a `try/catch (Exception)` swallows everything, cancellation propagation breaks; specific catches first, generic catches last.

## Filtering ideas for new rules

`Rules to work on.md` already filters out anything covered by the .NET SDK's built-in `CAxxxx` analyzers or `SYSLIB1xxx` compiler diagnostics. Before adding a new candidate, check the [Performance rules table](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/performance-warnings) — many "third-party perf rule" ideas are already shipped by Microsoft. The header note in that file lists the typical overlaps.

## Open follow-ups

These were noted during the 3.0 refresh but not done. None are blocking.

### Build / CI consistency
- [ ] **Drop `--no-build` from `dotnet pack` in the `build-nuget` job of [.github/workflows/02-build.yml](.github/workflows/02-build.yml).** The tool pack already does this; keeping the flag for one job and not the other is gratuitous inconsistency. Build time is negligible.
- [ ] **Add a post-pack assertion to the tool job** that `unzip -l ./tool/*.nupkg | grep DotnetToolSettings.xml` finds matches for every TFM. This would have caught the user issue that triggered the v3.0 refresh.
- [ ] **Verify nothing was lost from the original `Directory.Build.props` change** that was uncommitted at the start of the refresh. Run `git diff <release-base>` once more before the next push.

### Tool UX
- [ ] **Confirm `creedengo --help` lists the new `--version` option.** `Spectre.Console.Cli`'s `SetApplicationVersion` registers it, but I never visually verified the help-text rendering after install.
- [ ] **Pass diagnostic messages through `Markup.Escape` in `Program.WriteLine(string, string color)`.** Currently `[red]{line}[/]` will break if `line` contains a literal `[` or `]`. Diagnostic messages from Roslyn don't usually, but Windows file paths can.
- [ ] **Add a comment above `MSBuildLocator.RegisterDefaults()` in [Program.cs](src/Creedengo.Tool/Program.cs)** explaining that nothing must run before it. Anything that touches an MSBuild type before locator registration triggers FileNotFoundException at runtime, and the failure mode is opaque.

### Core analyzer cleanups (post-3.0)
- [ ] **Audit [TestRunner.cs](src/Creedengo.Tests/TestRunner.cs).** It was the only file in the test project not reviewed during the test-coverage pass.
- [ ] **GCI95 `UseIsOperatorInsteadOfAsOperator` could subscribe to `BinaryExpression` directly** instead of unwrapping conditions from 6 different statement kinds. The current shape is a relic of when only `if`/`while`/etc. were checked; a `==`/`!=` against null is the actual pattern, and it can be detected uniformly.
- [ ] **`Spectre.Console.Cli`'s `RunAsync` doesn't propagate the `CancellationToken` source.** If the tool ever needs cancellation from inside (timeout, watchdog), the current setup can't do it — the token comes from Spectre's internal Ctrl+C handler only. Not blocking, but worth knowing.

## Process notes

- **Verify agent findings before acting on them.** Across the v3.0 refresh, the Explore agent invented several findings — claimed GCI88 missed lambdas (it didn't), GCI72 wasn't hoisted (it was), GCI96 had inverted null-check logic (the operator just looks confusing). Two real bugs (GCI82 const-type validation, GCI96 fixer dead-arm misdiagnosis) only surfaced when negative tests failed unexpectedly during verification.
- **Coverage % is not test quality.** The GCI82 `const int? = null` bug existed despite the analyzer being at 96% line / 92% branch coverage. It surfaced only when a *negative* test exercised the `IsReferenceType` branch with a value-type-shaped null. Write tests that target specific behavioral claims, not lines.
