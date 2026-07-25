# DataTreeGrid API Design


## Hook


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



# Adapter Contract


interface TreeAdapter<T>{


 getId(item:T):string;


 getParentId(item:T):string|null;


 setParentId(
   item:T,
   parentId:string|null
 ):T;


}



# Move API


moveNode(

 sourceId,

 targetParentId

)



# Validation API


canMove(

 sourceId,

 targetParentId

)


Returns:


{
 allowed:boolean,

 reason?:string
}