# Workflow: Material Issue

**Version:** 1.0
**Date:** 2026-03-14

---

## Overview

A mechanic or stores person issues a part from stock to a work order task. The system ensures traceability, shelf-life validity, and release documentation before allowing the issue.

---

## Flow

```
1. Stores request created
   - Work order reference
   - Part number (and acceptable alternates)
   - Required quantity
   - Task reference
       │
       ▼
2. System locates matching stock item(s)
   - Matches part number or approved alternate
   - Checks bin location
   - Checks available quantity
       │
       ▼
3. Pre-issue checks [ALL must pass]
   │
   ├── [HARD STOP HS-008] Is item in quarantine? → BLOCK
   ├── [HARD STOP HS-009] Is shelf-life expired? → BLOCK
   ├── [HARD STOP HS-010] Missing trace document (Form 1 / 8130 / cert)? → BLOCK
   ├── Condition code acceptable? → warn or block per config
   └── Quantity available? → warn if partial
       │
       ▼
4. Issue confirmed by stores person
   - Stock balance decremented
   - `material_issue` record created
   - Serial/batch trace updated
   - Reservation converted to issue
       │
       ▼
5. Part handed to mechanic
   - Mechanic records part on task sign-off
   - Part number + serial/batch + quantity + trace doc ref stored on task
       │
       ▼
6. Post-task outcomes

   ├── Part installed on aircraft
   │     └── `installed_components` record updated
   │
   └── Surplus returned to stores
         └── `material_return` record created
               - Condition assessed
               - If unserviceable → quarantine
               - If serviceable → back to stock
```

---

## Stock Item States

| State | Description |
|-------|-------------|
| `serviceable` | Available for issue |
| `reserved` | Reserved to a work order, not yet issued |
| `issued` | Currently issued to a work order |
| `quarantine` | Hold — cannot be issued [HARD STOP HS-008] |
| `unserviceable` | Scrapped or pending repair |
| `in_repair` | Sent for repair (rotable) |

---

## Traceability Requirements

Every issued part must have a traceable record that can answer:

| Question | Where stored |
|---------|-------------|
| What is the part number? | `parts.part_no` |
| What is the serial/batch? | `stock_items.serial_no` / `batch_no` |
| Where did it come from? | `stock_items.supplier`, `stock_items.received_at` |
| What is the release document? | `stock_items.trace_document_id` → attachment |
| Which aircraft/task was it installed on? | `material_issues.work_order_id` + `task_id` |
| Who issued it? | `material_issues.issued_by` |
| When was it issued? | `material_issues.issued_at` |

---

## Shelf-Life Rules

- All shelf-life expiry dates stored on `stock_items.shelf_life_expires_at`
- System checks on every issue attempt
- Items within 30 days of expiry: yellow warning to stores
- Items expired: hard block [HARD STOP HS-009]
- Items with no expiry date: treated as no shelf-life control (unless part master says otherwise)

---

## Actors

| Actor | Role |
|-------|------|
| Mechanic / Planner | Raises stores request |
| Stores person | Locates, checks, issues parts |
| System | Enforces all hard stops automatically |
