# Clean-Room Design Policy

**Version:** 1.0
**Date:** 2026-03-14
**Status:** Active

---

## Purpose

This document defines the clean-room design policy for the MRO Platform project. It ensures that the product is independently designed, legally defensible, and clearly distinguishable from any existing commercial MRO software product.

---

## Scope

This policy applies to all contributors — developers, designers, business analysts, QA engineers, and documentation writers — who work on any part of the MRO Platform.

---

## What We Work From

All design decisions, data models, workflows, screen layouts, and business logic must derive exclusively from:

1. **Regulatory text** — EASA Part-145 (IR, AMC, GM), FAA 14 CFR Part 145, FAA Order 8900.1
2. **Interviews with domain experts** — MRO professionals, certifying staff, stores personnel, planners, quality managers
3. **Original wireframes** — created independently in this project, from scratch
4. **Original vocabulary** — derived from regulatory text, not from any competitor product
5. **General software engineering knowledge** — patterns, standards, open source

---

## What Is Prohibited

The following actions are strictly prohibited:

| Category | Prohibited Action |
|----------|-------------------|
| UI | Copying or recreating screen layouts from AMOS, OASES, TRAX, Quantum MX, or any other MRO software |
| Field names | Using field labels, column headers, or status names copied from a competitor product |
| Workflows | Recreating workflow steps or state transitions copied from competitor documentation or screenshots |
| Documents | Using task card templates, work order templates, release certificate formats copied from any MRO software |
| Reports | Recreating report layouts or output formats from any competitor product |
| Module names | Using branded module names (e.g. AMOSmobile, AMOSeTL, or equivalent) |
| Database | Importing, reverse-engineering, or copying any database schema from a competitor product |
| Help text | Copying user guide text, tooltip text, or in-app help from any competitor product |
| Marketing | Reusing product descriptions, feature lists, or comparison tables from competitor marketing material |

---

## How Decisions Are Documented

Every significant design decision must be recorded:

| Type | Where |
|------|-------|
| Architecture decisions | `docs/adr/` |
| Regulatory mapping | `docs/compliance/compliance-matrix.md` |
| UI wireframes | `docs/ui/` (original files only) |
| Workflow design | `docs/workflows/` |
| Domain vocabulary | `docs/architecture/domain-vocabulary.md` |

---

## Contributor Obligation

Every contributor working on this project must:

1. Read and understand this document before contributing
2. Raise a question to the project lead if they are unsure whether a design decision is clean
3. Never import screenshots, PDFs, or documentation from competitor MRO products into the project repository

---

## Legal Note

This policy does not constitute legal advice. Before any commercial release, the product should be reviewed by a qualified IP lawyer to confirm independent creation. The clean-room process provides the strongest available defence against IP infringement claims in most EU and US jurisdictions.

---

## Revision History

| Version | Date | Change |
|---------|------|--------|
| 1.0 | 2026-03-14 | Initial version |
