import { CategoryModel } from "../../models/Category/CategoryModel";

export async function GetCategoryList(): Promise<CategoryModel[]> {
  console.warn('Mock: GetCategoryList');

  return Promise.resolve([
    { 
        id: 'cat-001',
        FklkpTypeId: 1,
        FkParentId: null,
        Code: 'a',
        Title: 'test1',
        OrderNum: 1,
        Description: 'test des',
    },
    { 
        id: 'cat-002',
        FklkpTypeId: 1,
        FkParentId: null,
        Code: 'b',
        Title: 'test2',
        OrderNum: 2,
        Description: 'test des',
    }
    ,
    { 
        id: 'cat-003',
        FklkpTypeId: 1,
        FkParentId: 'cat-001',
        Code: 'c',
        Title: 'test1-1',
        OrderNum: 1,
        Description: 'test des',
    },
    { 
        id: 'cat-005',
        FklkpTypeId: 1,
        FkParentId: 'cat-003',
        Code: 'c',
        Title: 'test1-1-1',
        OrderNum: 1,
        Description: 'test des',
    }
    ,
    { 
        id: 'cat-004',
        FklkpTypeId: 1,
        FkParentId: 'cat-001',
        Code: 'c',
        Title: 'test1-2',
        OrderNum: 2,
        Description: 'test des',
    },
  ]);
}