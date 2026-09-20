# Command Contract

This companion defines the five-command V1 interface and its load-bearing behavior. Exact prompts and visual formatting may vary if these behaviors remain intact.

| Command | Inputs | Required behavior |
|---|---|---|
| `add` | Amount, category, optional description, optional date | Reject a non-decimal or non-positive amount, blank category, or invalid date. Omitted date becomes the current local date. Add only after all fields are valid. |
| `list` | None | Display every expense with date, amount, category, and description, ordered by date ascending and then original entry order. Report an empty collection clearly. |
| `total` | None | Display the decimal sum of all expense amounts; display zero for an empty collection. |
| `categories` | None | Group expenses by category and display one total per category, ordered by category. Category matching is case-insensitive while display preserves the first recorded spelling. Report an empty collection clearly. |
| `storage` | Action (`save` or `load`) and file path | `save` writes the full collection as JSON. `load` validates the entire file before replacing the collection. Any failure reports the cause and preserves the current collection. |

The application continues accepting commands until the user ends the process through the console environment. A separate `exit` command is excluded because V1 is capped at five top-level commands.

## Verification examples

- Expenses `12.50 / Food` and `7.50 / food` produce overall total `20.00` and one category total `Food = 20.00`.
- Rejecting an amount of `0`, a blank category, or an invalid date leaves the expense count unchanged.
- Loading malformed JSON after recording an expense leaves that expense available.
- Saving and loading expenses preserves amount, category, description, and date values.
