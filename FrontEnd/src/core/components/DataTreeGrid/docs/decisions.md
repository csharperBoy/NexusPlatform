# Architecture Decisions


## ADR-001

Date:
2026-07-25


Decision:

Use flat data internally.


Reason:

Backend APIs naturally provide parentId relation.

Benefits:

- Easier updates
- Easier synchronization
- Less duplication


---


## ADR-002


Decision:

Use Adapter pattern.


Reason:

Core must remain independent from business models.


---


## ADR-003


Decision:

Use dnd-kit for drag and drop.


Reason:

- Free
- Flexible
- React friendly


---


## ADR-004


Decision:

Use TanStack Table for grid functionality.


Reason:

- Headless architecture
- Free
- Highly customizable