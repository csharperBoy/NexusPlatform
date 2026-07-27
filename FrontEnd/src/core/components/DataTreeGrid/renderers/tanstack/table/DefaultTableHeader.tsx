import type {
  Table
}
from "@tanstack/react-table";

import {
  flexRender
}
from "@tanstack/react-table";


import type {
  TreeRow
}
from "../../../types";

import type {
  TreeNodeBase
}
from "../../../contracts";



interface DefaultTableHeaderProps<
  T extends TreeNodeBase
>{

  table:
    Table<TreeRow<T>>;

}



export default function DefaultTableHeader<
  T extends TreeNodeBase
>(
  props:
    DefaultTableHeaderProps<T>
){

  return (

    <thead>

      {
        props.table
          .getHeaderGroups()
          .map(
            headerGroup => (

              <tr
                key={
                  headerGroup.id
                }
              >

                {
                  headerGroup.headers.map(
                    header => (

                      <th

                        key={
                          header.id
                        }

                        className="
                          border
                          p-2
                        "

                      >

                        {
                          flexRender(
                            header.column.columnDef.header,
                            header.getContext()
                          )
                        }

                      </th>

                    )
                  )
                }

              </tr>

            )
          )
      }


    </thead>

  );

}