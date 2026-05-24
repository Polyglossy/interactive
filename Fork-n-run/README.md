# Fork-n-run Trademark Rename Plan

This folder contains a non-invasive plan for renaming trademark-bearing components in this fork without changing any existing project files.

Contents:

- `rename-surface-inventory.md`: Concrete rename surfaces discovered in the repository.
- `phased-rename-plan.md`: Step-by-step execution plan with verification after each phase.
- `rename-dictionary.md`: Frozen old-to-new identifier mapping template and candidate rows.
- `phase-gate-checklist.md`: Command-driven validation checklist and per-phase go/no-go record.

Planning assumptions:

- The fork should stop shipping Microsoft-owned product names, extension IDs, package IDs, tool commands, and branding strings unless they are required only as historical references during migration.
- Renames should be staged so that client and server protocol changes are validated before the next wave begins.
- Existing automated tests and current manual notebook scenarios should be reused as phase gates wherever possible.

Recommended working order inside this folder:

1. Fill in `rename-dictionary.md` until every shipped identity has an approved replacement.
2. Use `phase-gate-checklist.md` to record the baseline and each rename wave's commands and pass/fail outcome.
3. Keep `phased-rename-plan.md` as the sequencing and policy source of truth.
4. Use `rename-surface-inventory.md` when a phase uncovers an untracked rename seam.

This folder is intentionally non-invasive. It is for planning and tracking only; it does not require changes to the existing product files until the rename work actually begins.

## Suggested Deliverable Order

- Freeze the rename dictionary.
- Capture baseline command results.
- Execute Phase 1 and record the phase gate.
- Repeat for each subsequent phase.
