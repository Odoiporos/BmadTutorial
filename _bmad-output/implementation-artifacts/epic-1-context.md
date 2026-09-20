# Epic 1 Context: Track and Preserve Personal Expenses

<!-- Compiled from planning artifacts. Edit freely. Regenerate with compile-epic-context if planning docs change. -->

## Goal

Deliver a complete local console expense tracker in which a user can record validated expenses, inspect entries and spending summaries, save the current collection to a user-selected JSON file, and restore it later. The application must remain usable after invalid commands, input, paths, or stored data, and failures must never partially change or erase the current in-memory collection.

## Stories

- Story 1.1: Add and Review Expenses
- Story 1.2: Understand Spending Totals
- Story 1.3: Save and Restore Expenses Safely

## Requirements & Constraints

- Support exactly five top-level commands: `add`, `list`, `total`, `categories`, and `storage`; keep accepting commands until console EOF. `storage` supports only `save` and `load`.
- An expense has a positive decimal amount, a required non-blank category, an optional description, and a date. Parse amounts invariantly and interactive dates as `yyyy-MM-dd`. When an add omits its date, use the current local date supplied through a testable date provider.
- Trim category and description, reject a blank category, and normalize a blank description to null. Validate every field before adding; invalid input leaves state unchanged.
- List every expense with date, amount, category, and description, ordered by date ascending and then original sequence position. Show a clear empty state when there are no expenses.
- Compute the exact decimal total of all expenses, returning zero for an empty collection. Group category totals using ordinal case-insensitive identity, preserve the first recorded spelling for display, and order categories ordinally without regard to case.
- Save the full collection to a selected local file and load by replacing, never merging. Preserve amount, canonical category, description, date, and sequence order across a round trip.
- Treat expected command, validation, path, filesystem, and JSON failures as one user-safe error, then continue the loop. A failed operation must preserve live state; unexpected programming faults may remain exceptions.
- Keep V1 local and beginner-sized: one process, one domain entity, local JSON, and no server, network, database, authentication, cloud service, background worker, telemetry, concurrent writer, editing, deletion, budgets, accounts, income, recurring transactions, reports, charts, or other export formats.
- Core domain and aggregation behavior must be testable without interactive console input or real filesystem access. Test JSON behavior with isolated temporary files; terminal-level end-to-end automation is outside V1.

## Technical Decisions

- Use .NET SDK `10.0.400` with `latestPatch` roll-forward, target `net10.0`, and organize exactly three projects: `ExpenseTracker.Core`, `ExpenseTracker.ConsoleApp`, and `ExpenseTracker.Tests`. Use xUnit.net v3 `4.0.1` from the matching `xunit3` template.
- Follow ports and adapters with dependencies pointing inward. Core owns domain policy, use cases, state, operation results, the local-date port, and the synchronous storage port. ConsoleApp owns console I/O, JSON/filesystem adaptation, and composition. Core must contain no console, filesystem, or JSON concerns and reference no other project in the solution.
- One core `ExpenseTracker` service exclusively owns collection mutation. Expenses are immutable, use `DateOnly`, and are created only through core validation. Queries and save operations expose detached point-in-time immutable snapshots rather than live views.
- Use a single operation-result shape categorized as `InvalidCommand`, `InvalidInput`, `StorageUnavailable`, or `StorageDataInvalid`. Keep technical context internal; only the console maps categories to display wording and renders each failure once.
- The core orchestrates persistence through its storage port. Save passes one detached snapshot. Load receives core-owned unvalidated inputs, materializes and validates the entire candidate through the same rules as add, and replaces live state exactly once only after all items succeed. The adapter neither receives the state owner nor enforces domain validation.
- Keep persistence DTOs inside the JSON adapter. Accept only a non-null root array containing non-null items with required `amount`, `category`, and `date` and optional `description`; ignore unknown properties. Missing stored dates are invalid and must not receive the current-date default.
- Write indented UTF-8 JSON with camel-case properties, invariant decimal numbers, and `yyyy-MM-dd` dates. JSON array order is the expense sequence order.

## UX & Interaction Patterns

Command behavior is fixed, but exact prompts, colors, wording, and table formatting are implementation choices. Every expected failure should be displayed once in user-safe language and return control to the command loop; empty lists and category summaries need clear empty-state messages.

## Cross-Story Dependencies

Story 1.2 builds its totals on the immutable collection, deterministic ordering, and command loop established by Story 1.1. Story 1.3 depends on the same domain validation and state owner so loaded data behaves identically to interactively added data; it must preserve all listing and aggregation behavior from Stories 1.1 and 1.2. Until its owning story is complete, a later command must remain unavailable and produce a recoverable `InvalidCommand` result rather than being advertised or partially implemented.
