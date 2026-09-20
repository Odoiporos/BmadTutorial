# Adversarial Boundary Review — Architecture Spine

## Verdict

The spine establishes the right coarse boundaries, but it is not yet a sufficient build contract for independently implemented units. Every rule below can be obeyed while pairs of implementations still disagree at integration time. The highest-risk gaps are the undefined core port protocol, incomplete persisted shape and ordering identity, mutable/read-only state semantics, and an unowned command-to-use-case contract. Tighten the existing ADs and add explicit clock, command, and persistence-commit decisions before parallel implementation.

## Inputs reviewed

- `_bmad-output/planning-artifacts/architecture/architecture-BmadTutorial-2026-09-20/ARCHITECTURE-SPINE.md`
- `_bmad-output/specs/spec-console-expense-tracker/SPEC.md`
- `_bmad-output/specs/spec-console-expense-tracker/command-contract.md`

The spine's front-matter paths (`../../../specs/...`) resolve beneath `_bmad-output/planning-artifacts`, but the canonical spec actually lives beneath `_bmad-output/specs`. This is itself a finding below.

## Adversarial findings

### 1. The persistence port has no callable protocol

- **Location:** AD-1, AD-3, AD-4, AD-6; Structural Seed (`Core/Ports`, `ConsoleApp/Storage`)
- **Independently built units:** A core developer defines `IExpenseStore.LoadAsync(): Result<IReadOnlyList<Expense>>` and makes the service perform replacement; a storage developer defines `LoadAsync(ExpenseTracker tracker): Result` and invokes replacement from the adapter. Both keep the interface in Core, dependency direction inward, validate through Core, and replace atomically.
- **Trigger condition:** The two units meet and disagree over whether the port transfers data or orchestrates mutation, as well as sync/async shape and cancellation.
- **Guard snippet:** Tighten AD-1/AD-3 with a minimal port signature and ownership rule: storage returns a complete candidate value to a core-owned load use case; adapters never receive or mutate `ExpenseTracker`; specify sync versus async and cancellation explicitly.
- **Potential consequence:** Compile-time incompatibility or an adapter that becomes the transaction coordinator and weakens the stated state-owner boundary.

### 2. “One small operation-result shape” is not defined enough to share

- **Location:** AD-4; Consistency Conventions / Expected errors
- **Independently built units:** Core implements `Result<T>` with typed error codes; console and storage implement a non-generic `OperationResult` carrying display text. Each uses one small explicit result shape and avoids expected exceptions.
- **Trigger condition:** Commands must consume core validation results and storage outcomes through the same integration seam.
- **Guard snippet:** Extend AD-4 with the canonical result algebra: success payload rules, stable error code/category, optional user-safe message, composition of multiple validation failures, and which layer maps codes to console text.
- **Potential consequence:** Adapter-specific branching, duplicated result types, loss of error detail, or technical messages leaking to users.

### 3. Error translation ownership is contradictory

- **Location:** AD-4
- **Independently built units:** The JSON adapter catches `IOException` and returns a final user-facing sentence; the console adapter expects storage error codes so it can own wording. Both satisfy “adapters catch expected technical failures and translate them” and “messages crossing to the console are safe to display.”
- **Trigger condition:** Storage failure output is wired to the command renderer.
- **Guard snippet:** Amend AD-4 to assign exactly one translation boundary: storage maps exceptions to stable, non-display error codes plus safe context; the console renderer owns final prose (or explicitly choose final prose at storage and forbid retranslation).
- **Potential consequence:** Double-prefixed messages, inconsistent wording, and brittle tests coupled to the wrong layer.

### 4. Expense shape and mutability are left open

- **Location:** AD-3; Consistency Conventions / State; Structural Seed / Models
- **Independently built units:** The model developer creates a mutable `Expense` class with validated construction; the state owner returns `IReadOnlyList<Expense>`. No caller can mutate the collection, yet callers can mutate each expense. Another unit assumes snapshot elements are immutable values.
- **Trigger condition:** A list result is retained and an expense property is changed outside `ExpenseTracker`.
- **Guard snippet:** Tighten AD-3: `Expense` is immutable after validated construction; snapshots are detached immutable values (or explicitly state the accepted immutable collection/value types).
- **Potential consequence:** Mutation bypasses the sole state owner, historical snapshots change underneath renderers, and totals no longer match prior validation.

### 5. “Read-only snapshot” permits incompatible snapshot semantics

- **Location:** AD-3; Consistency Conventions / State
- **Independently built units:** The service returns a copied array on every read; a totals service receives a live `ReadOnlyCollection` view. Both expose read-only snapshots in the everyday sense, but one is point-in-time and one reflects later adds or loads.
- **Trigger condition:** A command obtains the collection and another state operation occurs before enumeration, or tests retain a returned value.
- **Guard snippet:** Define snapshot as a point-in-time detached immutable sequence, including whether expense values themselves are immutable; make all query use cases consume that same semantic.
- **Potential consequence:** Non-repeatable reads and command output assembled from two different states.

### 6. Original entry order has no durable identity

- **Location:** Consistency Conventions / Ordering; AD-6; CAP-2 and storage round-trip contract
- **Independently built units:** The core assigns an internal insertion index not present in DTOs; storage preserves JSON array order and reconstructs new indices. Both order same-date expenses by insertion order and restore all four required expense values.
- **Trigger condition:** A file is produced or edited with a different array order, or a DTO mapper enumerates through an intermediate collection without stable order.
- **Guard snippet:** Add an AD defining “entry order” as sequence position in the persisted JSON array and require mapping to preserve it end-to-end, or add a persisted stable sequence field and its validation rules.
- **Potential consequence:** `list` order changes after save/load even though all required expense values round-trip.

### 7. Date representation and omitted-date ownership can diverge

- **Location:** SPEC / Constraints; AD-6; Consistency Conventions / JSON
- **Independently built units:** Console parses an omitted date and sends `DateOnly.FromDateTime(DateTime.Now)`; Core accepts nullable `DateTime` and defaults it using `DateTime.Today`. Storage uses a string DTO. Each meets the visible default and `yyyy-MM-dd` output rule.
- **Trigger condition:** Type signatures are joined, tests cross midnight, or a date-bearing DTO is mapped into the model.
- **Guard snippet:** Add an AD selecting `DateOnly` as the domain/port value (if intended), assigning defaulting to one core use case, and forbidding persistence mapping from treating a missing stored date as “today.”
- **Potential consequence:** Compile-time shape mismatch, nondeterministic tests, timezone/midnight discrepancies, or corrupt files silently acquiring today’s date.

### 8. Current local date is an undeclared external dependency

- **Location:** AD-1, AD-5; SPEC / Constraints
- **Independently built units:** Core directly reads `DateTime.Today`; tests and console composition expect an injected clock because core behavior must be independently testable. Neither approach violates the current ADs.
- **Trigger condition:** The add use case with omitted date is tested deterministically or executed around local midnight.
- **Guard snippet:** Add a clock AD: Core owns an `IClock`/date-provider port returning local `DateOnly`, composition supplies the system implementation, and tests supply a fixed date.
- **Potential consequence:** Flaky tests and inconsistent default dates between parsing and domain creation.

### 9. Category equality and ordering use different unspecified comparers

- **Location:** Consistency Conventions / Category comparison and Ordering; command contract / `categories`
- **Independently built units:** Aggregation groups with `StringComparer.OrdinalIgnoreCase`; rendering orders with current-culture comparison. Another unit uses invariant-culture ignore-case for both. All are plausibly “case-insensitive” and “ordered by category.”
- **Trigger condition:** Categories include non-ASCII characters or the process culture changes.
- **Guard snippet:** Tighten the convention to one named comparer for equality and one named comparer for ordering—preferably specify `StringComparer.OrdinalIgnoreCase` for both—and state whether whitespace is trimmed before identity/display spelling is captured.
- **Potential consequence:** Categories group differently from how they sort, results vary by machine, and the “first spelling” rule becomes unstable.

### 10. Persistence schema validity is under-specified

- **Location:** AD-6; Consistency Conventions / JSON
- **Independently built units:** One DTO makes `description` optional and rejects unknown properties; another accepts missing `description` as null, ignores unknown properties, and accepts duplicate JSON properties with last-value wins. Both use a UTF-8 camel-case array and route mapped data through core validation.
- **Trigger condition:** Loading structurally ambiguous JSON, `null` roots/items, missing fields, duplicate properties, unknown fields, or numeric overflow.
- **Guard snippet:** Expand AD-6 with a V1 schema table: root and item nullability, required properties, types/ranges, missing versus null behavior, unknown and duplicate property policy, and exact deserialization options.
- **Potential consequence:** Files accepted by one build are rejected or interpreted differently by another, despite identical architecture conformance.

### 11. Atomic in-memory replacement lacks a commit API contract

- **Location:** AD-3 and AD-6
- **Independently built units:** The mapper validates items one-by-one into domain values and calls `ReplaceAll`; the service revalidates the whole sequence. Another service exposes `TryReplace(IEnumerable<Expense>)`, enumerates lazily, and clears before the enumerable later throws. Both claim to validate a complete candidate before replacement.
- **Trigger condition:** Candidate enumeration is lazy, throws midway, aliases a mutable list, or is enumerated twice with different results.
- **Guard snippet:** Tighten AD-3: materialize and validate into a detached immutable candidate first; the commit method accepts that validated candidate and swaps state in one non-throwing step; no clear-then-add implementation is allowed.
- **Potential consequence:** Partial state loss, time-of-check/time-of-use bugs, or a failed load changing live state.

### 12. Save ownership does not guarantee one coherent snapshot

- **Location:** AD-3; CAP-5 map; Consistency Conventions / State
- **Independently built units:** The console adapter enumerates the service snapshot into DTOs; the core persistence use case asks the port to save a domain snapshot. Both preserve inward dependencies and keep mutation in Core.
- **Trigger condition:** The independently authored save command and port disagree on whether the adapter pulls state or the core pushes a snapshot.
- **Guard snippet:** Add to the persistence AD that the core save use case captures exactly one detached snapshot and passes it to the port; specify the port payload and forbid the adapter from querying the state owner.
- **Potential consequence:** Incompatible APIs and an accidental outward dependency from storage to a concrete service.

### 13. Command parsing and dispatch have no shared integration contract

- **Location:** Deferred / prompts and formatting; Capability → Architecture Map; command contract
- **Independently built units:** A parser emits `{ Name = "storage", Args = ["save", path] }`; a dispatcher exposes separate `Save(path)`/`Load(path)` handlers or expects an interactive submenu. All preserve exactly five top-level commands and the storage action requirement.
- **Trigger condition:** Parser, command loop, and handlers are independently implemented and composed.
- **Guard snippet:** Add a command-boundary AD or companion section defining the parsed command discriminated union, tokenization/quoting and whitespace rules, case sensitivity, how paths with spaces are acquired, EOF behavior, and the single dispatch owner.
- **Potential consequence:** Units compile only after redesign, valid user paths cannot be entered, or a hidden sixth command/action model appears.

### 14. Validation ownership permits duplicate and inconsistent rules

- **Location:** AD-3, AD-4, AD-6; CAP-1 map
- **Independently built units:** Console validates amount/category/date and then calls an `AddValidatedExpense` method; Core validates only storage DTO mappings. Another implementation validates all construction in a shared Core factory. Both can claim invalid command input leaves state unchanged and loaded DTOs pass core validation.
- **Trigger condition:** Interactive add and JSON load receive boundary values such as whitespace categories or decimal extremes.
- **Guard snippet:** Tighten AD-3/AD-6: every creation path calls the same Core factory/value constructors; adapters may perform syntactic parsing only and cannot be the authority for domain validity.
- **Potential consequence:** An expense rejected interactively can be accepted from JSON, or vice versa, and validation rules drift.

### 15. Description and category normalization are undefined data mutations

- **Location:** SPEC / domain fields; category convention; AD-6
- **Independently built units:** Core trims category and converts blank descriptions to null; storage preserves exact strings and expects round-trip identity. Another core preserves leading/trailing whitespace while grouping case-insensitively. Each can satisfy required/non-blank and optional constraints.
- **Trigger condition:** Input or JSON contains surrounding whitespace or an empty description.
- **Guard snippet:** Add a value-normalization AD stating trim/preserve rules, empty-to-null behavior, maximum lengths if any, and whether “same expense values” means canonical domain values or byte-equivalent input strings.
- **Potential consequence:** Save/load changes displayed values, category identity differs across commands, and round-trip tests disagree.

### 16. File commit behavior is not defined at the storage boundary

- **Location:** CAP-5; AD-4; AD-6
- **Independently built units:** One adapter writes directly with `File.Create`; another writes a temporary sibling and replaces the destination. Both report exceptions and do not mutate the in-memory collection.
- **Trigger condition:** Save fails after truncating or partially writing an existing file.
- **Guard snippet:** Add a storage-commit AD: serialize fully first, write to a temporary file in the destination directory, flush/close, then replace or move atomically where supported; define overwrite and cleanup behavior.
- **Potential consequence:** A failed save preserves memory but destroys the user’s previously valid persisted collection.

### 17. Test boundaries omit the integration contracts most likely to clash

- **Location:** AD-5; Capability → Architecture Map
- **Independently built units:** Core tests use fake ports and storage tests call DTO mapping directly; no test composes the real command dispatcher, core use case, and JSON port. This obeys direct core testing and isolated adapter testing.
- **Trigger condition:** The projects compile separately but are connected in `Program.cs`.
- **Guard snippet:** Tighten AD-5 with non-interactive contract/integration tests for each port and one composition-level test covering add → save → new state owner → load → list/query, using fake console streams and a temporary file while remaining short of full terminal E2E.
- **Potential consequence:** Signature, result, ordering, and mapping mismatches survive until manual execution.

### 18. The architecture’s declared source links are broken

- **Location:** Front matter / `sources` and `companions`
- **Independently built units:** A documentation validator resolves the relative links literally and sees no canonical contract; a developer searches the repository and uses `_bmad-output/specs/...`. Neither behavior is constrained by an AD.
- **Trigger condition:** A downstream agent or tool follows the spine metadata without repository-wide discovery.
- **Guard snippet:** Correct both paths from the spine directory to `../../../specs/...` only if the files are moved there; with the current layout, use `../../../../specs/spec-console-expense-tracker/...`, and add a validation check that every declared source/companion resolves.
- **Potential consequence:** Builders miss the command contract and independently invent behavior that the architecture was meant to bind.

## AD changes implied

The findings can be closed compactly by revising AD-3, AD-4, AD-5, and AD-6 and adding three decisions:

1. **Core use-case and port protocol AD:** canonical command/use-case/result types; storage port signatures; Core owns orchestration and state commits.
2. **Immutable state and ordering AD:** immutable `Expense`, detached snapshots, durable definition of insertion order, materialized atomic replacement.
3. **Clock and date AD:** `DateOnly`, one Core-owned defaulting point, injected local-date provider.
4. **Command integration AD:** parsed command shape, storage action dispatch, token/path and EOF rules.
5. **Persistence schema and commit AD:** exact DTO schema/deserializer policy, normalization mapping, coherent save snapshot, and crash-safe file replacement.
6. **Comparer/normalization amendment:** named comparer and string canonicalization rules shared by add, load, grouping, and sorting.
7. **Boundary integration-test amendment:** port contract tests and a composed non-terminal workflow test.

---

## Final addendum after architecture revision

### Proportional verdict

No architecture blocker remains for this beginner V1. The revision closes the state, date, ordering, normalization, error-display, and persistence-orchestration gaps that could previously let adjacent units obey the spine yet still disagree materially. Two contract seams remain worth tightening before Core and the JSON/console adapters are assigned independently; both are small additions, not reasons to expand the design into parser grammar, crash-safe filesystem transactions, or exhaustive JSON policy.

The original finding 18 is withdrawn: from the architecture directory, `../../../specs/spec-console-expense-tracker/SPEC.md` and its companion both resolve correctly under `_bmad-output/specs`.

### Remaining material findings

#### A. The load candidate crosses the port before and after validation at the same time

- **Location:** AD-6 and AD-7
- **Independently built units:** A Core implementer makes the storage port return unvalidated core-owned input records so Core can perform the stated load validation; a JSON adapter implementer maps DTOs through `Expense.Create` and returns `Expense` values because AD-6 says every DTO maps through the same core creation validation. Both readings obey an AD, but their port payload types cannot integrate.
- **Trigger condition:** The Core persistence interface and JSON implementation are built separately from the two rules.
- **Guard snippet:** Choose one sequence explicitly. For example: “The storage adapter deserializes DTOs into core-owned `ExpenseInput` values without domain validation; the Core load use case validates every input through the same factory as `add`, materializes the complete `Expense` candidate, then commits it.” Alternatively, state that the adapter invokes the Core factory and AD-7 receives an already validated `IReadOnlyList<Expense>` without revalidation.
- **Potential consequence:** A compile-time port mismatch or duplicated validation with conflicting ownership.

#### B. Stable error categories are required but not enumerated for the console/storage seam

- **Location:** AD-4; Consistency Conventions / Expected errors
- **Independently built units:** The storage adapter returns categories such as `NotFound`, `AccessDenied`, `InvalidJson`, and `InvalidExpense`; the console renderer implements messages for a smaller Core result enum such as `InvalidInput` and `StorageFailure`. Both produce one explicit result with stable categories and leave final wording to the console, but the independently authored category sets do not join.
- **Trigger condition:** The console maps storage/core failures to its single display point.
- **Guard snippet:** Add the minimal shared Core-owned category set needed by V1, or require the console to render a category-independent safe message supplied as result context. Keep the set coarse; exact prose remains local to the console.
- **Potential consequence:** Compile-time enum mismatch, an unhandled category, or adapter-specific display branching that defeats AD-4’s single rendering boundary.

### Ten adversarial pair checks and disposition

1. **State owner ↔ console queries:** closed by immutable `Expense` plus detached snapshots.
2. **Add path ↔ load path:** one residual conflict remains—the pre/post-validation load payload in findings A.
3. **Core save use case ↔ JSON store:** closed by the synchronous Core-owned port and detached snapshot rule.
4. **Core load use case ↔ JSON store:** orchestration and commit ownership are closed; only the candidate type/validation phase remains.
5. **Clock adapter ↔ add use case:** closed by the Core-owned local-date provider and `DateOnly`.
6. **Category aggregation ↔ renderer ordering:** closed by the named `StringComparer.OrdinalIgnoreCase` convention and canonical spelling rule.
7. **List ordering ↔ JSON round-trip:** closed by sequence position and JSON array-order preservation.
8. **Parser/renderer ↔ operation results:** final display ownership and EOF behavior are closed; only the shared category vocabulary remains in finding B.
9. **DTO mapper ↔ domain values:** required shape and normalization are sufficient for V1 once finding A chooses the validation handoff; exhaustive duplicate/unknown-token rules are intentionally unnecessary.
10. **Documentation consumer ↔ canonical spec:** closed; the declared relative paths resolve, correcting the earlier review.

### Deliberately non-blocking for this V1

- Exact parser tokenization, quoting, and prompt flow can remain a console-adapter choice because there is one console project and the command contract fixes the five behaviors.
- Crash-safe replacement of an existing file is not required by the canonical success/failure contract; direct save with explicit failure reporting is proportionate.
- Duplicate-property handling and other exhaustive JSON strictness need not be standardized beyond AD-6’s required root/item/property rules.
- Exact method and type names remain implementation choices once the two semantic handoffs above are made unambiguous.
- A composed integration test would be useful implementation guidance, but its absence does not create an architecture contract conflict.
