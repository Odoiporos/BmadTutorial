# Technology Currency Review — Architecture Spine

Reviewed: 2026-09-20  
Artifact: `_bmad-output/planning-artifacts/architecture/architecture-BmadTutorial-2026-09-20/ARCHITECTURE-SPINE.md`  
Scope: every named technology/version plus claims that depend on SDK starter or default behavior.

## Verdict

**PASS WITH CHANGES.** All four named stack selections are current and mutually compatible as of 2026-09-20, and `.slnx` is the .NET 10 solution-template default. The architecture is nevertheless not implementation-reproducible as written: it names an exact SDK without specifying `global.json`, names xUnit.net v3 without identifying the `xunit.v3` package or its separately installed `xunit3` starter, and contains no primary technology sources in its source metadata.

## Findings

### TC-1 — Medium: the exact SDK selection is not enforced

The stack says `.NET SDK / target framework | 10.0.401 / net10.0`, but the structural seed contains no `global.json`. Microsoft documents that, when no `global.json` selects an SDK, the CLI uses the highest installed SDK (effectively `latestMajor`), so a machine with .NET 11 installed would not necessarily build with 10.0.401. `net10.0` remains the project target independently of the selected SDK.

**Required change:** add `global.json` to the structural seed and state the intended roll-forward policy. Use `"version": "10.0.401"` with `latestPatch` if later security patches in the 10.0.4xx feature band are acceptable, or `disable` only if byte-for-byte SDK selection is genuinely required.

Primary source: [Microsoft Learn — global.json overview](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json).

### TC-2 — Medium: xUnit.net v3 is not reproducibly scaffolded by the stated seed

`xUnit.net v3 | 4.0.1` is a valid, current selection, but the document does not give the package ID (`xunit.v3`) or starter command. This matters because xUnit’s official v3 instructions require installing `xunit.v3.templates` and then using the `xunit3` short name. The SDK’s built-in starter is `dotnet new xunit`; the .NET SDK project was still tracking migration of that built-in template from xUnit v2 in 2026. Therefore an implementer following ordinary SDK defaults can create the wrong generation of test project.

**Required change:** specify either `dotnet new install xunit.v3.templates --version 4.0.1` followed by `dotnet new xunit3`, or explicitly create the test project with a pinned `<PackageReference Include="xunit.v3" Version="4.0.1" />`. Also state the selected test runner mode if the starter exposes a choice.

Primary sources: [xUnit.net v3 getting started](https://xunit.net/docs/getting-started/v3/getting-started), [xUnit.net 4.0.1 release notes](https://xunit.net/releases/v3/4.0.1), [NuGet — xunit.v3 4.0.1](https://www.nuget.org/packages/xunit.v3/4.0.1), and [dotnet/sdk template migration issue #54499](https://github.com/dotnet/sdk/issues/54499).

### TC-3 — Low: the artifact has no technology-source traceability

The front matter lists only the local product specification. It does not cite the vendor pages establishing the SDK patch, language default, shared-framework status, solution format default, or xUnit version. The claims happen to check out today, but a future currency audit cannot distinguish an intentional pin from a copied default.

**Required change:** add the primary references listed in the verification table to architecture decision evidence or a dedicated technology-baseline section. Record an `as of 2026-09-20` date beside the stack.

### TC-4 — Low: `System.Text.Json` is described accurately, but not versioned

`System.Text.Json | .NET 10 shared framework` correctly communicates that a separate NuGet reference is unnecessary for `net10.0`. It is not, however, a concrete version in a table headed `Version`; the effective implementation patch follows the selected .NET runtime/SDK servicing level. This becomes especially ambiguous while the SDK row is not enforced.

**Required change:** label the second column `Version / source`, or say `in-box with Microsoft.NETCore.App for net10.0 (no PackageReference)` and let the SDK/runtime baseline carry the servicing version.

Primary sources: [Microsoft Learn — package compatibility for assemblies overlapping shared frameworks](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/nuget-package-compatibility-rules) and [System.Text.Json API for .NET 10](https://learn.microsoft.com/en-us/dotnet/api/system.text.json?view=net-10.0).

## Verification Table

| Architecture claim | Verification as of 2026-09-20 | Assessment |
| --- | --- | --- |
| .NET SDK `10.0.401` | Microsoft’s .NET 10 download page lists SDK 10.0.401, released 2026-09-08, as the current .NET 10 SDK. | Current and supported. |
| Target framework `net10.0` | .NET 10 is the current LTS line; Microsoft states a three-year support term. SDK 10 templates default to `net10.0`. | Current and compatible. |
| C# `14` | Microsoft’s language-version table maps .NET 10.x to C# 14 by default. | Correct; explicit `<LangVersion>` is unnecessary unless the choice is meant to be pinned independently. |
| `System.Text.Json` from the .NET 10 shared framework | Microsoft documents `System.Text.Json` as one of the assemblies whose APIs are available from the shared framework without a package reference; the .NET 10 API surface is published. | Correct, but it is a source designation rather than a precise version. |
| xUnit.net v3 `4.0.1` | xUnit published Core Framework v3 4.0.1 on 2026-09-12; NuGet identifies package `xunit.v3` 4.0.1 and reports compatibility with `net10.0`. | Current and compatible. |
| `BmadTutorial.slnx` as solution seed | Microsoft documents that `dotnet new sln` defaults to SLNX starting in .NET 10. | Correct SDK-default claim. |
| Console, class-library, and xUnit project starters | The .NET SDK ships console, class-library, and xUnit starters, but xUnit’s v3 starter is separately distributed as `xunit.v3.templates` with short name `xunit3`. | Production starters are supported; test starter must be made explicit. |

Primary sources for the table: [Download .NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0), [What’s new in .NET 10](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview), [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning), [.NET default templates](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new-sdk-templates), and [SLNX default change](https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/10.0/dotnet-new-sln-slnx-default).

## Currency Summary

No named version is stale on the review date. The highest-impact correction is not an upgrade: it is making the intended SDK and xUnit v3 setup executable and unambiguous. The architecture spine itself was not modified.

## Final Addendum — Revised Spine Re-review

Re-reviewed: 2026-09-20

**Final verdict: PASS. No unresolved blocker, high, or medium technology-reproducibility findings.**

- **TC-1 resolved:** the structural seed now includes `global.json` and specifies SDK `10.0.401` with `rollForward: latestPatch`. This constrains SDK selection to the intended 10.0.4xx feature band while allowing servicing patches.
- **TC-2 resolved:** the stack identifies package `xunit.v3` version `4.0.1`; the verified baseline specifies `xunit.v3.templates` version `4.0.1`, template short name `xunit3`, and explicitly warns against the ambiguous SDK `xunit` template.
- **TC-4 resolved:** the stack column is now `Version / source`, and `System.Text.Json` is accurately described as in-box with `Microsoft.NETCore.App` for `net10.0`.
- **TC-3 substantially resolved:** the new Verified Baseline records the verification date and identifies the authoritative Microsoft and xUnit documentation families. Direct URLs remain in this review rather than the spine; that is a low-severity traceability preference, not an implementation blocker.

The revised instructions are sufficient to generate the intended solution and test-project technology baseline without silently selecting a different SDK major or xUnit generation.
