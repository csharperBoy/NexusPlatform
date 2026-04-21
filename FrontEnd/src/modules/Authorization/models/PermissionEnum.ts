export const assignTypeMap: Record<number,string> = {
         0:'Person',
         1:'Position',
         2:'Role',
        3:'User',  
  
      };
  export const actionMap: Record<number,string> = {
         0:'View',
         1:'Create' ,
         2:'Edit',
          3:'Delete',
         4:'Export',
         99:'Full'
      };

      export const effectMap: Record< number,string> = {
         0:'allow',    
         1  : 'Deny'
      };

      export const scopeMap: Record<number,string> = {
        0: 'None',
        1: 'Account' ,
       2:'Self',
        3:'Unit',
        4: 'UnitAndBelow',
        5: 'SpecificProperty',
        99: 'All' 
      };