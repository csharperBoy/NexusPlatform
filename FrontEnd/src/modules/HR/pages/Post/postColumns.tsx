//src/modules/HR/pages/Post/postColumns.tsx
import type {
  DataTreeGridColumn
}
from "@/core/components/DataTreeGrid";


import type {
  PostInfoView
}
from "../../models/postInfoView";




export const postColumns:
DataTreeGridColumn<PostInfoView>[] =
[


  {
    id: "title",

    header: "عنوان پست",

    accessorKey: "jobTitleName",

    treeColumn: true

  },



  {
    id: "employeeFirstName",

    header: "متصدی",

    accessorKey: "firstName"

  },

{
    id: "employeeLastName",

    header: "متصدی",

    accessorKey: "lastName"

  },

  {
    id: "phone",

    header: "تلفن",

    accessorKey: "officePhone"

  },
  {
  id:"gender1",

  header:"جنسیت1",

  accessorKey:"gender",

  cell:({value})=>{

    return (
      <span>
        {
          value === 1
          ? "♂ مرد"
          : "♀ زن"
        }
      </span>
    );

  }

},
{
  id: "gender",

  header: "جنسیت",

  accessorKey: "gender",

  formatter: ({ value }) => {

    switch (value) {

      case 1:
        return "مرد";

      case 2:
        return "زن";

      default:
        return "نامشخص";

    }

  }

}
];