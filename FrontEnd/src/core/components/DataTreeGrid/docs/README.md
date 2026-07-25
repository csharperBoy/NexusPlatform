# DataTreeGrid

A reusable Tree Grid component for React + TypeScript.

## Goal

Create a reusable, free and extensible Tree Grid component that supports:

- Tree visualization
- Tabular data display
- Inline editing
- Drag & Drop hierarchy management
- Generic entity support through Adapter pattern


## Main Technologies

- React
- TypeScript
- TanStack Table
- dnd-kit
- Tailwind CSS


## Main Use Cases

Examples:

- Organization Chart
- Permission Tree
- Product Categories
- Project Hierarchy
- File Explorer


## Core Philosophy

DataTreeGrid should:

- Be independent from business modules
- Not know domain entities
- Work with any hierarchical data
- Keep original data structure unchanged
- Use adapters for mapping


## Current Status

Implemented:

[x] Tree Builder
[x] Tree Index
[x] Flattening
[x] Expand / Collapse
[x] Navigation
[x] Node Move
[x] Move Validation


In Progress:

- TanStack Table integration
- Column system
- Cell editors
- Drag & Drop


Future:

- Server synchronization
- Undo / Redo
- Virtualization
- Keyboard navigation