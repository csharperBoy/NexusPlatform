# DataTreeGrid Architecture


## Principle


DataTreeGrid should provide tree capabilities
without coupling business entities.


The architecture should be:

Data Source
      |
      v
Adapter
      |
      v
Tree Engine
      |
      v
Tree Controller
      |
      v
Renderer


## Important Rule


UI frameworks are adapters.

The engine must not depend on:

- React
- TanStack
- MUI
- AG Grid