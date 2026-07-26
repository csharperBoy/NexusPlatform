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
    id: "employee",

    header: "متصدی",

    accessorKey: "firstName"

  },



  {
    id: "phone",

    header: "تلفن",

    accessorKey: "officePhone"

  }


];