---
stepsCompleted:
  - step-01-validate-prerequisites
  - step-02-design-epics
  - step-03-create-stories
  - step-04-final-validation
inputDocuments:
  - ../specs/spec-console-expense-tracker/SPEC.md
  - ../specs/spec-console-expense-tracker/command-contract.md
  - architecture/architecture-BmadTutorial-2026-09-20/ARCHITECTURE-SPINE.md
---

# BmadTutorial - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for BmadTutorial, decomposing the requirements from the specification, command contract, and architecture spine into implementable stories.

## Requirements Inventory

### Functional Requirements

FR1: The application accepts exactly five top-level commands—`add`, `list`, `total`, `categories`, and `storage`—and continues accepting commands until console EOF.

FR2: The user can add one expense with a positive decimal amount, required non-blank category, optional description, and optional date that defaults to the current local date.

FR3: Adding an expense trims category and description, normalizes a blank description to null, rejects invalid fields, and changes state only when every field is valid.

FR4: The user can list every expense with date, amount, category, and description, ordered by date and then original sequence position; an empty collection produces a clear empty-state message.

FR5: The user can display the sum of all expense amounts; an empty collection produces a total of zero.

FR6: The user can display one total per category, using case-insensitive category identity, preserving the first canonical spelling, ordering categories consistently, and reporting an empty collection clearly.

FR7: Through the `storage save` action, the user can write one coherent snapshot of all expenses to a selected local JSON file.

FR8: Through the `storage load` action, the user can load and validate an entire JSON collection before atomically replacing current state; any failure preserves current state.

FR9: Saving and loading preserves every expense's amount, category, description, date, and sequence order.

FR10: Invalid commands, fields, paths, unreadable files, and invalid JSON produce one user-safe error and return control to the command loop without partial state changes.

### NonFunctional Requirements

NFR1: V1 runs as one local .NET console process and requires no server, network, database, authentication, cloud service, deployment environment, background worker, telemetry, or concurrent writer.

NFR2: The core domain and aggregation behavior is testable without interactive console input or real filesystem access.

NFR3: Console amount parsing is culture-independent, dates use `yyyy-MM-dd`, and category matching and ordering use ordinal case-insensitive comparison so behavior is deterministic across machines.

NFR4: Expenses are immutable after validated construction, queries return detached point-in-time snapshots, and one core service exclusively owns collection mutation.

NFR5: Expected input and storage failures do not terminate the command loop; unexpected programming faults remain exceptions.

NFR6: The scope remains beginner-friendly: one domain entity, five commands, local JSON persistence, and no features listed in the specification's non-goals.

### Additional Requirements

- Start from the .NET 10 console and class-library templates targeting `net10.0`; pin SDK `10.0.401` in `global.json` with `latestPatch` roll-forward.
- Create exactly three projects: `ExpenseTracker.Core`, `ExpenseTracker.ConsoleApp`, and `ExpenseTracker.Tests`, organized under the architecture spine's `src/` and `tests/` seed.
- Apply ports and adapters: Core owns policy, use cases, state, result types, and port contracts; ConsoleApp owns console and JSON filesystem adapters plus composition; dependencies point inward.
- Keep `ExpenseTracker.Core` free of project references, console I/O, filesystem I/O, and JSON types.
- Use one core `ExpenseTracker` service as the sole state owner; load materializes and validates a complete candidate before one atomic replacement.
- Use one operation-result shape with the categories `InvalidCommand`, `InvalidInput`, `StorageUnavailable`, and `StorageDataInvalid`; the console alone maps categories to display text.
- Keep JSON DTOs in the storage adapter. The root is a non-null array of non-null items with required `amount`, `category`, and `date` plus optional `description`; ignore unknown properties.
- Use a core-owned synchronous storage port: save receives one detached immutable snapshot; load returns core-owned unvalidated expense inputs; the adapter never receives the state owner or validates domain rules.
- Use a core-owned local-date provider returning `DateOnly`; production supplies system local date and tests supply a fixed date. Stored data must always contain a date.
- Serialize UTF-8 camel-case indented JSON with invariant decimal numbers and `yyyy-MM-dd` dates; preserve JSON array order as expense sequence order.
- Scaffold tests with `xunit.v3` `4.0.1` using the `xunit.v3.templates` `4.0.1` `xunit3` template.
- Test core behavior directly in memory and JSON adapter behavior with isolated temporary files; terminal-level end-to-end automation is outside V1.

### UX Design Requirements

None. V1 is a console application with behavior defined by the command contract; exact prompts, colors, and table formatting are intentionally left to implementation.

### FR Coverage Map

FR1: Epic 1 — Run the five-command application loop.

FR2: Epic 1 — Add an expense with validated fields and a date default.

FR3: Epic 1 — Normalize input and prevent invalid additions from changing state.

FR4: Epic 1 — List expenses deterministically with an empty state.

FR5: Epic 1 — Display total spending.

FR6: Epic 1 — Display deterministic totals by category.

FR7: Epic 1 — Save a coherent expense snapshot to JSON.

FR8: Epic 1 — Validate and atomically load an expense collection.

FR9: Epic 1 — Preserve values and sequence order across save and load.

FR10: Epic 1 — Recover from invalid commands, input, paths, and files without data loss or termination.

## Epic List

### Epic 1: Track and Preserve Personal Expenses

The user can record expenses, inspect individual and summarized spending, save the collection locally, and restore it later. Invalid commands, input, and files are handled without losing data or terminating the application.

**FRs covered:** FR1–FR10

## Epic 1: Track and Preserve Personal Expenses

The user can record expenses, inspect individual and summarized spending, save the collection locally, and restore it later. Invalid commands, input, and files are handled without losing data or terminating the application.

### Story 1.1: Add and Review Expenses

As a personal expense tracker user,
I want to add valid expenses and list what I have recorded,
So that I can maintain and inspect an accurate in-memory spending record.

**Requirements addressed:** FR1, FR2, FR3, FR4, FR10

**Acceptance Criteria:**

**Given** a new checkout with .NET SDK 10.0.401 available
**When** the solution is initialized
**Then** it contains `ExpenseTracker.Core`, `ExpenseTracker.ConsoleApp`, and `ExpenseTracker.Tests` targeting `net10.0`
**And** dependencies point inward as defined by AD-1 and the xUnit v3 test suite runs successfully.

**Given** the application has no expenses
**When** the user runs `list`
**Then** the application displays a clear empty-state message
**And** returns to the command loop.

**Given** the user supplies a positive invariant-culture decimal amount, non-blank category, optional description, and valid `yyyy-MM-dd` date
**When** the user runs `add`
**Then** one immutable expense is added through the core state owner
**And** category and description are trimmed, with a blank description normalized to null.

**Given** the user omits the date for a valid expense
**When** the expense is added
**Then** the core uses the local date supplied by its date-provider port
**And** the behavior can be tested with a fixed date provider.

**Given** the user supplies an invalid amount, blank category, or invalid date
**When** the user runs `add`
**Then** the application displays one user-safe `InvalidInput` error
**And** the expense collection remains unchanged
**And** the command loop continues.

**Given** multiple expenses, including expenses sharing the same date
**When** the user runs `list`
**Then** every expense is displayed with date, amount, category, and description
**And** expenses are ordered by date and then original sequence position.

**Given** Story 1.1 is the current implementation state
**When** the user enters anything other than `add` or `list`
**Then** the application displays one user-safe `InvalidCommand` error
**And** no unfinished command is advertised or executed
**And** the command loop continues until console EOF.

### Story 1.2: Understand Spending Totals

As a personal expense tracker user,
I want to see my overall spending and spending grouped by category,
So that I can understand where my money is going.

**Requirements addressed:** FR5, FR6, FR10

**Acceptance Criteria:**

**Given** the expense collection is empty
**When** the user runs `total`
**Then** the application displays a total of zero
**And** returns to the command loop.

**Given** multiple recorded expenses
**When** the user runs `total`
**Then** the application displays the exact decimal sum of every expense amount
**And** does not mutate the collection.

**Given** the expense collection is empty
**When** the user runs `categories`
**Then** the application displays a clear empty-state message
**And** returns to the command loop.

**Given** expenses whose categories differ only by case, such as `Food` and `food`
**When** the user runs `categories`
**Then** they appear as one category using ordinal case-insensitive identity
**And** the first canonical spelling is displayed
**And** the displayed amount equals the exact sum of those expenses.

**Given** expenses in multiple categories
**When** the user runs `categories`
**Then** each category appears once
**And** categories use the architecture's deterministic ordinal case-insensitive ordering.

**Given** Story 1.2 is complete
**When** the application runs
**Then** `add`, `list`, `total`, and `categories` are available and functional
**And** `storage` remains unavailable and produces one recoverable `InvalidCommand` result
**And** all prior Story 1.1 behavior remains passing.

### Story 1.3: Save and Restore Expenses Safely

As a personal expense tracker user,
I want to save my expenses to a local file and restore them later,
So that my spending record survives between application sessions.

**Requirements addressed:** FR1, FR7, FR8, FR9, FR10

**Acceptance Criteria:**

**Given** the user has recorded expenses
**When** the user runs `storage save` with a writable path
**Then** one detached snapshot is written as an indented UTF-8 JSON array
**And** properties use camel case, amounts remain JSON numbers, dates use `yyyy-MM-dd`, and array order matches expense sequence order
**And** the in-memory collection remains unchanged.

**Given** a valid saved expense file
**When** the user runs `storage load`
**Then** the adapter maps every DTO to a core-owned unvalidated input
**And** the core validates and materializes the complete candidate collection
**And** the current collection is replaced once, only after the complete candidate is valid.

**Given** expenses are saved and loaded into a new state owner
**When** the user runs `list`, `total`, and `categories`
**Then** amount, canonical category, description, date, sequence order, overall total, and category totals match the saved collection.

**Given** the JSON contains unknown properties but all required properties are valid
**When** the user runs `storage load`
**Then** unknown properties are ignored
**And** the valid collection is loaded.

**Given** the JSON root or an item is null, a required property is missing, the JSON is malformed, or any expense violates domain validation
**When** the user runs `storage load`
**Then** the application returns one user-safe `StorageDataInvalid` result
**And** the existing collection remains unchanged
**And** no missing stored date is replaced with the current date.

**Given** a path is missing, unreadable, or unwritable
**When** the user runs the relevant `storage` action
**Then** the application returns one user-safe `StorageUnavailable` result
**And** the in-memory collection remains unchanged
**And** the command loop continues.

**Given** Story 1.3 is complete
**When** the application runs
**Then** exactly `add`, `list`, `total`, `categories`, and `storage` are available
**And** `storage` accepts only `save` and `load` actions
**And** the application continues until console EOF
**And** core tests run in memory while JSON adapter tests use isolated temporary files
**And** every test from Stories 1.1 and 1.2 remains passing.
