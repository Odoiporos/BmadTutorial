# Good-Spine Rubric Review

## Gate verdict

**Revise before finalizing.** The spine is compact, internally coherent, covers all five spec capabilities, and explicitly closes the operational/environmental envelope. Its project, ownership, persistence, and test boundaries prevent the main implementation splits. Two user-input seams remain under-specified enough that independently built console and core units can make incompatible choices.

## Scope reviewed

- `_bmad-output/planning-artifacts/architecture/architecture-BmadTutorial-2026-09-20/ARCHITECTURE-SPINE.md`
- Declared source: `_bmad-output/specs/spec-console-expense-tracker/SPEC.md`
- Declared companion: `_bmad-output/specs/spec-console-expense-tracker/command-contract.md`
- Checklist: `.agents/skills/bmad-architecture/references/reviewer-gate.md`

The frontmatter paths resolve correctly relative to the spine directory. No parent spine is declared. No implementation tree is present to ratify, so the brownfield-consistency criterion is not applicable.

## High findings

### H1 — The owner and test seam for the omitted-date default are not fixed

**Evidence:** CAP-1 and the command contract require an omitted date to become the current local date. The spine assigns validated value creation to Core and command parsing to the console adapter, but no AD or convention says which side supplies the default or how current time is obtained. AD-5 also requires core behavior to be tested directly in memory.

**Divergence:** A console implementation can substitute `DateTime.Today` before calling Core, while a core implementation can default internally from the system clock. Those choices produce different port signatures, different validation responsibilities, and nondeterministic versus deterministic core tests. Independently built units can therefore fail to compose cleanly.

**Disposition:** **Discuss/fix.** Add an enforceable rule naming the owner of defaulting and the clock seam. A suitable rule would make Core own the business default while receiving an injected date/clock capability (or an explicit current date from the use-case boundary), and prohibit direct ambient-clock reads in domain behavior.

### H2 — AD-4 does not cover all expected input failures required to keep the loop alive

**Evidence:** The spec requires invalid command, field, or file input not to terminate the application. The companion adds that the application continues accepting commands until the console environment ends it. AD-4 binds only CAP-1 and CAP-5 and explicitly governs validation and storage failures. It does not bind malformed/unknown commands or parsing failures for the other commands, and no other rule assigns the command-loop recovery boundary.

**Divergence:** One console unit may treat unknown commands, unsupported `storage` actions, or unexpected extra tokens as recoverable results; another may throw or exit the loop. Both could plausibly claim compliance with the current architecture text while violating the canonical contract in different ways.

**Disposition:** **Autofix.** Broaden AD-4 to bind the command interface across CAP-1 through CAP-5 and require every user-originated command/field/path parse failure to be translated to an expected result at the console boundary, rendered once, and followed by the next command read. Console-environment termination should remain the only normal loop exit.

## Medium finding

### M1 — Interactive decimal parsing culture is left ambiguous

**Evidence:** The spec requires decimal amounts and rejects non-decimal input. The spine specifies invariant decimal numbers for JSON but says nothing about console amount parsing. Localization is a non-goal, yet that does not select invariant culture or current culture.

**Divergence:** Different console implementations can accept `12.50`, `12,50`, or both depending on machine culture. This changes observable validity and makes tests/environment behavior inconsistent.

**Disposition:** **Discuss or defer explicitly.** Choose an invariant console syntax for reproducibility, or state that parsing follows the process culture and add that expectation to adapter tests. Do not leave the choice implicit.

## Checklist results

| Criterion | Result | Notes |
| --- | --- | --- |
| Fixes real divergence points for the level below | **Partial** | Strong boundaries and state ownership; time/defaulting and command recovery remain open. |
| Every AD Rule is enforceable and prevents its stated divergence | **Pass** | AD-1 through AD-6 are testable by dependency checks, unit tests, or adapter tests. AD-4 is enforceable but too narrowly scoped for the source contract. |
| Deferred contains no dangerous divergence | **Pass** | Prompt styling, lower-level naming, schema migration, telemetry, and packaging are safe at V1. |
| Named technology is verified current | **Pass** | On 2026-09-20, Microsoft lists .NET SDK 10.0.401 with C# 14 for .NET 10, and xUnit lists 4.0.1 as the current v3 release: https://dotnet.microsoft.com/en-us/download/dotnet/10.0 and https://xunit.net/releases/v3/4.0.1. |
| Ratifies brownfield codebase | **N/A** | No implementation code was found in scope. |
| Covers source-spec capabilities | **Pass with gaps** | CAP-1 through CAP-5 are mapped; the cross-cutting invalid-command loop behavior and omitted-date seam are not fully governed. |
| Preserves inherited spine | **N/A** | No parent spine is declared. |
| Every owned dimension is decided, deferred, or open | **Pass** | Paradigm, dependencies, state, persistence, errors, tests, stack, structure, and operations/environment are addressed. The operational paragraph explicitly excludes servers, providers, deployment environments, networks, workers, telemetry, and concurrent writers. |

## Mechanical observations

Manual inspection found no placeholders, duplicate AD identifiers, missing `Binds`/`Prevents`/`Rule` fields, or unpinned stack entries. The prescribed linter could not be executed because `uv` is unavailable in the environment; this is a review-tool limitation, not a spine defect.

## Strengths worth preserving

- AD-3 and AD-6 jointly make load replacement atomic and prevent persistence DTOs from bypassing domain validation.
- The capability-to-architecture map accounts for all five capabilities without bloating the spine.
- The explicit local-process operational envelope avoids the common omission of deployment, infrastructure, and operations.
- Deferred items are appropriately below feature altitude or triggered only by future requirements.

## Recommended gate action

Resolve H1 and H2 before changing `status` to `final`. M1 may either be decided now or moved into Deferred with an explicit implementation/test convention. No other rubric issue blocks finalization.

---

## Final addendum — revised spine

**Verdict: PASS; no blocker or high finding remains.**

The revised spine preserves every load-bearing claim from `SPEC.md` and `command-contract.md` either directly as an invariant/convention or by retaining the canonical companion as the behavior contract. The five commands and their required outcomes remain fixed by that companion; the spine now supplies the architectural decisions needed for independently implemented units to realize them consistently.

### Prior findings

- **H1 resolved:** AD-8 makes Core the owner of omitted-date defaulting, introduces a core-owned local-date provider port, fixes `DateOnly`, permits deterministic test time, and forbids applying a default during load.
- **H2 resolved:** AD-4 now binds CAP-1 through CAP-5, covers command, field, path, and storage failures, fixes a stable expected-result boundary, requires the console to render once and continue, and names console EOF as the only normal loop exit.
- **M1 resolved:** the Console parsing convention chooses invariant-culture amounts and `yyyy-MM-dd` interactive dates.

### Contract reconciliation

- Positive decimal, non-blank category, optional description, and date behavior are covered by AD-6/AD-8 plus the parsing and normalization conventions.
- Exact list ordering and persistence of insertion order are fixed by the Ordering convention.
- Totals and category behavior remain assigned to Core; category identity, display spelling, and ordering are now deterministic through `StringComparer.OrdinalIgnoreCase` and the category convention.
- Save/load round-tripping and failure atomicity are fixed by AD-3, AD-6, AD-7, AD-8, and the JSON/Ordering conventions.
- Independent core testing and filesystem-isolated adapter testing remain fixed by AD-5.
- Local-only scope and all major non-goals remain consistent with the explicit runtime/environment paragraph and Deferred section.

One non-blocking editorial cleanup remains: the capability map row for CAP-1 should cite AD-8 because AD-8 governs its omitted-date behavior. This does not weaken the AD or permit implementation divergence.
