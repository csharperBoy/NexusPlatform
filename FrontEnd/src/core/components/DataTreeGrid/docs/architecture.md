# DataTreeGrid Architecture


## High Level Design


Backend Flat Data

        |
        |
        v

Adapter Layer

        |
        |
        v

Tree Engine

        |
        |
        v

useDataTreeGrid Hook

        |
        |
        v

DataTreeGrid UI Component




# Design Principles


## 1. Flat Data First

The source data remains flat.

Example:

[
 {
   id:"A",
   parentId:null
 },
 {
   id:"B",
   parentId:"A"
 }
]


Tree structure is generated only for rendering.



## 2. Adapter Pattern


Core must not know business fields.


Example:


HR:

PostInfoView

fields:

- id
- fkParentId
- jobTitleName


Core only knows:


getId()

getParentId()

setParentId()



## 3. Immutable Operations


Operations must return new data.


Example:


moveNode()

does not mutate original array.



# Current Layers


## treeBuilder

Responsibility:

- Create tree index
- Build relationships
- Calculate depth


## treeEngine

Responsibility:

- Navigation
- Move
- Validation


## useDataTreeGrid

Responsibility:

React integration layer.


## DataTreeGrid

Responsibility:

UI rendering.