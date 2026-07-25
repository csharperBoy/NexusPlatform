# DataTreeGrid Current State

Last Updated:
2026-07-25


# Project Context

DataTreeGrid is a reusable React + TypeScript Tree Grid component.

Current target module:
HR Organization Chart


Frontend Stack:

- React
- TypeScript
- Vite
- Tailwind CSS
- TanStack Table (planned)
- dnd-kit (planned)



# Business Scenario

The HR module manages organization posts.

Backend returns a flat list.

Each item has:

- id
- fkParentId

Hierarchy is created from parent relation.



Example:

[
 {
   id:"A",
   fkParentId:null
 },
 {
   id:"B",
   fkParentId:"A"
 }
]



# Current Architecture



Backend Data

        |
        |
        v

PostInfoView

        |
        |
        v

postTreeAdapter

        |
        |
        v

DataTreeGrid Core

        |
        |
        v

PostManagementPage



# Implemented Files


## Core


src/core/components/DataTreeGrid/


Current files:

- treeBuilder.ts
- treeEngine.ts
- useDataTreeGrid.ts


Responsibilities:


treeBuilder.ts

- Build tree index
- Create node relationships
- Generate flattened rows


treeEngine.ts

- Navigation
- Node movement
- Validation


useDataTreeGrid.ts

- React hook wrapper
- Exposes tree capabilities



# Current Hook API


useDataTreeGrid({

 data,

 adapter,

 defaultExpandAll

})



Returns:


{
 rows,

 index,

 expansion,

 navigation,

 manipulation,

 validation
}



# Current Features


Implemented:


[x] Build tree from flat list

[x] Root detection

[x] Child relationship

[x] Flatten rows

[x] Depth calculation

[x] Expand / Collapse

[x] Find item by id

[x] Move node

[x] Validate move



# Current Validation Rules


Implemented:


Cannot:

- Move node under itself

- Move node under its descendants


Allowed:

- Move node to root

- Move node under another branch



# Current HR Integration


Module:

src/modules/HR



Entity:

PostInfoView



Adapter:

postTreeAdapter.ts



Mapping:


id

<=>

item.id



parent

<=>

item.fkParentId



# Current Test Page


File:


src/modules/HR/pages/Post/PostManagementPage.tsx



Current purpose:

Testing Tree Engine.



Current capabilities:


- Load organization posts from API

- Render tree

- Expand/Collapse

- Select node

- Change parent manually



# Backend APIs


Current API:


GET

/api/HR/OrgChart/GetList



Returns:

PostInfoView[]



Update API:


PUT

/api/hr/OrgChart/batch



Consumes:

UpdatePostCommand[]



# Current Known Limitations


Not implemented yet:


- TanStack Table integration

- Column definitions

- Editable cells

- Enum editors

- Date editors

- Checkbox editors

- Drag and Drop

- Drop position detection

- Server synchronization



# Next Step


Priority:


1. Integrate TanStack Table

2. Create DataTreeGrid component

3. Implement column system

4. Add cell editors

5. Integrate dnd-kit

6. Connect move operation to batchUpdatePosts API



# Important Notes


Core must remain independent from HR models.


Do not put HR-specific logic inside DataTreeGrid core.


All business mapping must happen through adapters.

## 2026-07-25

Completed:

- Tree engine validation
- Move node testing

Discovered:

- findNode returns entity not wrapper

Decision:

- Keep current behavior temporarily
- Rename API before public release