# Architecture Decisions


## ADR-001

Decision:

Tree Engine is framework independent.


Reason:

Allow future support for:

- TanStack Table
- AG Grid
- MUI DataGrid
- Custom renderer


---

## ADR-002

Decision:

Adapters are responsible for mapping entities.

Reason:

Different modules have different parent structures.

Example:

fkParentId

parentId

reportsTo