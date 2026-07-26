import { useEffect, useState } from "react";

import { postApi } from "../../api/PostApi";
import { PostInfoView } from "../../models/postInfoView";

import {
  useDataTreeGrid,
} from "@/core/components/DataTreeGrid";

import {
  postTreeAdapter,
} from "../../adapters/postTreeAdapter";
import {
  DataTreeGrid
}
from "@/core/components/DataTreeGrid";
import { postColumns } from "./postColumns";




export default function PostManagementPage() {


  const [
    posts,
    setPosts
  ] = useState<PostInfoView[]>([]);



  /**
   * Node انتخاب شده برای عملیات
   */
  const [
    selectedId,
    setSelectedId
  ] = useState<string | null>(null);



  /**
   * Parent جدیدی که قرار است Node زیر آن منتقل شود
   */
  const [
    newParentId,
    setNewParentId
  ] = useState<string | null>(null);





  useEffect(() => {


    postApi
      .GetList()
      .then(result => {

        setPosts(result);

      });


  }, []);






  const tree =
    useDataTreeGrid({

      data: posts,

      adapter: postTreeAdapter,

      defaultExpandAll: false,

    });








  /**
   * انتقال Node انتخاب شده
   */
  const moveSelectedNode = () => {


    if (!selectedId) {
      return;
    }



    const validation =
      tree.validation.canMove(
        selectedId,
        newParentId
      );



    if (!validation.allowed) {

      alert(
        validation.reason ??
        "انتقال غیرمجاز است"
      );

      return;

    }





    const newData =
      tree.manipulation.moveNode(
        selectedId,
        newParentId
      );



    setPosts(newData);


  };





const selectedNode =
    selectedId
      ?
      tree.navigation.findNode(selectedId)
      :
      null;


console.log(
  "SELECTED NODE",
  selectedNode
);





  return (

    <div className="p-6">


      <h1 className="text-xl font-bold mb-5">
        مدیریت چارت سازمانی
      </h1>




      {/* Toolbar */}
      <div className="flex flex-wrap gap-3 items-center mb-5">


        <button
          className="btn btn-primary"
          onClick={
            tree.expansion.expandAll
          }
        >
          باز کردن همه
        </button>




        <button
          className="btn btn-secondary"
          onClick={
            tree.expansion.collapseAll
          }
        >
          بستن همه
        </button>






        <select

          className="select select-bordered"

          value={
            newParentId ?? ""
          }

          onChange={(e)=>{

            setNewParentId(
              e.target.value || null
            );

          }}

        >

          <option value="">
            Root
          </option>


          {
            tree.rows.map(row => (

              <option
                key={row.id}
                value={row.id}
              >

                {
                  row.item.jobTitleName
                }

                {" - "}

                {
                  row.item.firstName
                }

                {" "}

                {
                  row.item.lastName
                }

              </option>

            ))
          }


        </select>





        <button

          className="btn btn-warning"

          disabled={
            !selectedId
          }

          onClick={
            moveSelectedNode
          }

        >

          تغییر Parent

        </button>



      </div>








      {/* Selected Info */}
      <div className="mb-5 p-3 border rounded">


        <div>

          <b>
            Node انتخاب شده:
          </b>


          {
            selectedNode
              ?
              (
                <>
                  {" "}
                  {
                    selectedNode.jobTitleName
                  }

                  {" - "}

                  {
                    selectedNode.firstName
                  }

                  {" "}

                  {
                    selectedNode.lastName
                  }
                </>
              )
              :
              " هیچ موردی انتخاب نشده"
          }


        </div>


      </div>






<div className="border rounded p-4">


<DataTreeGrid

    data={posts}

    adapter={postTreeAdapter}

    columns={postColumns}

    defaultExpandAll={false}

/>


</div>









      {/* Debug */}
      <div className="mt-8">


        <h2 className="font-bold mb-3">
          Debug Information
        </h2>



        <pre className="bg-gray-100 p-4 rounded text-xs overflow-auto">

          {
            JSON.stringify(

              {

                totalRows:
                  tree.rows.length,


                rootCount:
                  tree.index.rootIds.length,


                selectedId,


                selectedNode,


                newParentId,


              },

              null,

              2

            )
          }


        </pre>


      </div>




    </div>

  );

}