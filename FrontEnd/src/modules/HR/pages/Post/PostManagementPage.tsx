import { useEffect, useState } from "react";

import { postApi } from "../../api/PostApi";
import { PostInfoView } from "../../models/postInfoView";

import {
  useDataTreeGrid,
} from "@/core/components/DataTreeGrid";

import {
  postTreeAdapter,
} from "../../adapters/postTreeAdapter";



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








      {/* Tree */}
      <div className="border rounded p-4">


        {
          tree.rows.map(row => (


            <div

              key={row.id}


              onClick={() =>
                setSelectedId(row.id)
              }


              className={`
                flex
                items-center
                gap-2
                p-2
                mb-1
                rounded
                cursor-pointer
                border

                ${
                  selectedId === row.id
                    ?
                    "bg-blue-200 border-blue-500"
                    :
                    "hover:bg-gray-100"
                }

              `}


              style={{
                paddingRight:
                  row.depth * 24
              }}

            >



              {
                row.hasChildren && (

                  <button

                    className="w-6 h-6"

                    onClick={(event)=>{


                      event.stopPropagation();



                      tree.expansion.toggle(
                        row.id
                      );


                    }}

                  >

                    {
                      tree.expansion.isExpanded(
                        row.id
                      )
                        ?
                        "-"
                        :
                        "+"
                    }


                  </button>

                )
              }






              <span>


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


              </span>



            </div>


          ))

        }


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