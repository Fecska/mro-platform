# ADR-003: Clean-Room Design Policy

**Date:** 2026-03-14
**Status:** Accepted

## Context

This platform operates in a space with established commercial MRO software (e.g. AMOS, OASES, TRAX). We must ensure the product is independently designed to avoid IP infringement claims.

## Decision

This project follows **clean-room design principles**.

### What we derive requirements FROM

- EASA Part-145 regulation text (AMC, GM, IR)
- FAA Part 145 regulation text (CFR 14)
- Direct interviews with MRO professionals and certifying staff
- Own UI wireframes created from scratch
- Own domain vocabulary derived from regulatory text

### What is PROHIBITED

- Copying or recreating screens, layouts, or UI flows from any commercial MRO software
- Using field names, status labels, or workflow terminology copied from competitor products
- Replicating document templates, task card formats, or report layouts from competitor software
- Using any database schema obtained from a competitor product
- Importing marketing copy, help text, or product documentation from competitors

### How decisions are documented

Every significant design decision is recorded as an ADR in `docs/adr/`.
UI designs are created in `docs/ui/` from original wireframes.
Compliance mapping is derived from official regulatory sources in `docs/compliance/`.

## Consequences

- Slower initial design (all screens must be independently created)
- Strong IP protection for the product
- Easier to differentiate from existing products
- All team members must sign acknowledgement of this policy
