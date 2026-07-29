# Personal Cognition Ledger documentation

This directory contains the maintained product and engineering documentation for Personal Cognition Ledger (PCL). Documents describe either the implemented system or explicitly labelled future work. Source code and executable tests remain the final authority when documentation and behavior disagree.

## Start here

| Need | Read |
| :--- | :--- |
| Understand the product and current scope | [Product](product.md) |
| Understand the system structure and runtime | [Architecture](architecture.md) |
| Learn domain terms, lifecycles, and invariants | [Domain model](domain-model.md) |
| Change module dependencies or communication | [Module boundaries](module-boundaries.md) |
| Set up, build, test, or contribute | [Development](development.md) |
| Use or change the HTTP API | [API reference](api/reference.md) |
| See current priorities and deferred work | [Roadmap](roadmap.md) |
| Work on an approved design that is not implemented | [Active plans](plans/) |

New contributors should read the repository `README.md`, then Product, Architecture, and Development. Read the Domain model or API reference when the task touches those surfaces.

## Documentation authority

Each maintained fact should have one primary home:

| Question | Authoritative document |
| :--- | :--- |
| Why does the product exist, and what is in scope? | `product.md` |
| What is implemented and how does it run? | `architecture.md` |
| What do domain terms mean and which rules apply? | `domain-model.md` |
| How may modules interact? | `module-boundaries.md` |
| How do contributors build, test, and document changes? | `development.md` |
| What HTTP contract exists today? | `api/reference.md` and executable request files |
| What should be built next? | `roadmap.md` |
| How will an approved but unfinished feature be implemented? | A status-labelled document under `plans/` |

Avoid copying facts between documents. Link to the authoritative section instead. API contracts must reflect mapped endpoints and integration tests; future behavior must not be presented as current behavior.

## Status conventions

- Documents outside `plans/` are maintained reference material.
- Every document in `plans/` must state its status, last review date, scope, and completion condition.
- Completed plans are removed after durable decisions and operational guidance are migrated to their authoritative documents. Git history retains the original plan.
- Speculative ideas belong in the Roadmap's deferred section, not in detailed domain reference material.

## Updating documentation

Update documentation in the same change when behavior, architecture, domain language, development workflow, or API contracts change. The [Development guide](development.md#documentation-maintenance) contains the review checklist.
