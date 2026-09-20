---
title: 'Story 1.1: Add and Review Expenses'
type: 'feature'
created: '2026-09-20'
status: 'in-progress'
route: 'dispatch'
review_loop_iteration: 0
baseline_commit: '3288013955fe027936da5627ecc664ff5b46a98c'
context:
  - '{project-root}/_bmad-output/implementation-artifacts/epic-1-context.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The repository has no working application, and users cannot record or inspect expenses. Story 1.1 must establish the minimal .NET solution and deliver only the `add` and `list` commands as a complete, testable in-memory experience.

**Approach:** Create the three-project ports-and-adapters skeleton, implement an immutable validated Expense and a single core state owner, then connect console input and output through a recoverable command loop. Use a core-owned local-date provider so omitted dates are deterministic in tests.

**Interaction decision:** Entering `add` starts a prompted field flow for amount, category, optional description, and optional date. Story 1.1 does not support inline `add` arguments.

## Boundaries & Constraints

**Always:** Pin SDK 10.0.400 with `latestPatch`, target `net10.0`, and use the existing `BmadTutorial.slnx`. Core references no other project and performs no console, filesystem, or JSON I/O. ConsoleApp references Core; Tests reference the projects required by the behavior under test. Expenses are immutable and use `decimal` and `DateOnly`; category and description are trimmed, blank descriptions become null, and invalid additions leave state unchanged. The core state owner returns detached snapshots ordered by date then insertion sequence. Amount parsing uses invariant culture, explicit dates accept only `yyyy-MM-dd`, omitted dates use the local-date port, and expected failures use the architecture's operation-result categories. The console renders each expected failure once and continues until EOF.

**Never:** Implement or advertise `total`, `categories`, `storage`, JSON persistence, editing, deletion, databases, network services, authentication, telemetry, or terminal-level end-to-end automation. Do not edit `_bmad/**`, `.agents/**`, planning/spec artifacts, or architecture review files.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Empty list | No expenses; `list` | Clear empty-state message; loop continues | None |
| Valid add | Positive invariant decimal, non-blank category, optional description, valid date | Adds exactly one normalized immutable expense | Success result |
| Omitted date | Valid fields without date | Adds using date-provider local date | Success result |
| Invalid add | Non-decimal/non-positive amount, blank category, or bad date | Adds nothing; existing snapshot is unchanged | One `InvalidInput` message; loop continues |
| Ordered list | Multiple expenses, including equal dates | Displays date, amount, category, description ordered by date then insertion | None |
| Unknown command | Anything except implemented `add` and `list` | No state change and no unfinished feature exposure | One `InvalidCommand` message; loop continues |
| End of input | Console EOF | Command loop exits normally | None |

</frozen-after-approval>

## Code Map

- `BmadTutorial.slnx` — existing empty solution; add the three Story 1.1 projects without replacing the file.
- `global.json` — new SDK pin for 10.0.400 with `latestPatch` roll-forward.
- `src/ExpenseTracker.Core/` — new dependency-free domain, results, date port, and single state-owner service.
- `src/ExpenseTracker.ConsoleApp/` — new console command parsing/rendering, system date adapter, composition, and EOF loop.
- `tests/ExpenseTracker.Tests/` — new xUnit.net v3 tests for validation, normalization, date defaulting, snapshot isolation, ordering, command parsing, and recoverable errors.
- `_bmad-output/implementation-artifacts/sprint-status.yaml` — workflow-owned status only; update Story 1.1 through the build sync, not product code.

## Tasks & Acceptance

**Execution:**
- [ ] `global.json`, `BmadTutorial.slnx`, and the three project files — scaffold the pinned .NET 10 solution, project references, and xUnit.net v3 test setup while preserving inward dependency direction.
- [ ] `src/ExpenseTracker.Core/Models/Expense.cs`, `Results/OperationResult.cs`, and `Ports/ILocalDateProvider.cs` — define immutable validated domain values, shared expected-error categories, and the deterministic date seam.
- [ ] `src/ExpenseTracker.Core/Services/ExpenseTracker.cs` — implement sole collection ownership, atomic add validation, detached snapshots, and date/sequence ordering.
- [ ] `src/ExpenseTracker.ConsoleApp/Console/` and `Program.cs` — implement only `add` and `list`, console-owned error wording, unknown-command recovery, and EOF termination.
- [ ] `tests/ExpenseTracker.Tests/Core/` and `Console/` — cover every matrix scenario without real terminal automation or filesystem access.

**Acceptance Criteria:**
- Given a clean checkout with SDK 10.0.400, when the solution builds and tests run, then all three net10.0 projects compile, Core has no outward project dependency, and the xUnit.net v3 suite passes.
- Given Story 1.1 is running, when the user interacts with it, then only `add` and `list` are available and every other command is recoverably rejected until later stories.

## Implementation Notes

## Spec Change Log

## Review Triage Log

## Design Notes

Keep command handling as a thin console adapter: syntactic parsing occurs outside Core, while all domain validation and state mutation occur inside the core service. A future story may register additional commands without changing the Expense creation rules or state owner.

## Verification

**Commands:**
- `dotnet --version` — expected: resolves SDK 10.0.400 or a later 10.0.4xx patch allowed by `latestPatch`.
- `dotnet build BmadTutorial.slnx` — expected: build succeeds with no errors.
- `dotnet test BmadTutorial.slnx` — expected: all Story 1.1 tests pass.
- `dotnet run --project src/ExpenseTracker.ConsoleApp` — expected: manual add/list/invalid-command/EOF smoke check succeeds.
