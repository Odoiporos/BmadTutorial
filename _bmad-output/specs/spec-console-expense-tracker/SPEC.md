---
id: SPEC-console-expense-tracker
companions:
  - command-contract.md
  - ../../planning-artifacts/architecture/architecture-BmadTutorial-2026-09-20/ARCHITECTURE-SPINE.md
sources:
  - ../../brainstorming/brainstorm-beginner-friendly-dotnet-project-2026-09-20/.memlog.md
---

> **Canonical contract.** This SPEC and the files in `companions:` are the complete, preservation-validated contract for what to build, test, and validate. Source documents are retained for traceability only.

# Console Expense Tracker

## Why

A beginner learning .NET needs a project small enough to finish while still exercising input handling, validation, domain modeling, collections, LINQ-style aggregation, JSON serialization, file I/O, error handling, and automated tests. A local console expense tracker provides those layers without web, authentication, database, or deployment overhead.

## Capabilities

- **CAP-1**
  - **intent:** The user can add one expense with an amount, category, optional description, and date.
  - **success:** Valid input adds exactly one expense; invalid input reports the problem and leaves the collection unchanged.
- **CAP-2**
  - **intent:** The user can list every recorded expense to inspect the current collection.
  - **success:** The command displays every expense with date, amount, category, and description, ordered by date and then entry order; an empty collection produces a clear empty-state message.
- **CAP-3**
  - **intent:** The user can view total spending across all recorded expenses.
  - **success:** The displayed total equals the sum of all expense amounts and is zero when no expenses exist.
- **CAP-4**
  - **intent:** The user can view spending totals grouped by category.
  - **success:** Each category appears once with the sum of its expenses, ordered by category; an empty collection produces a clear empty-state message.
- **CAP-5**
  - **intent:** The user can save the current collection to local JSON and load a collection from local JSON.
  - **success:** Saving produces JSON that can restore the same expense values; loading valid JSON replaces the in-memory collection, while missing, unreadable, or invalid data reports an error and preserves the current collection.

## Constraints

- V1 is a local .NET console application with exactly the five top-level commands defined in `command-contract.md`.
- The domain contains one entity, Expense, with: positive decimal Amount; required non-blank Category; optional Description; and Date, defaulting to the current local date when omitted.
- Persistence uses a user-selected local JSON file; no database or network dependency is allowed.
- Invalid command, field, or file input must not terminate the running application or partially change its in-memory collection.
- Monetary values are currency-agnostic decimals; V1 performs no conversion or mixed-currency handling.
- Core domain and aggregation behavior must be independently testable without interactive console input or real filesystem access.

## Non-goals

- Editing or deleting expenses.
- Budgets, accounts, income, recurring transactions, reports, charts, or export formats other than JSON.
- Authentication, multiple users, synchronization, cloud storage, database storage, web APIs, graphical interfaces, notifications, or deployment.
- Localization, currency conversion, and mixed-currency accounting.

## Success signal

In one console session, a user can add expenses in at least two categories, list them, see the correct overall and per-category totals, save them, restart with an empty in-memory collection, and load the file to recover identical values. Invalid input and invalid files produce useful errors without data loss or application termination.

## Assumptions

- The fifth top-level command is `storage`, with `save` and `load` actions, so both persistence operations fit the agreed five-command limit.
- Loading replaces the current collection rather than merging it.
