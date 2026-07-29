// src/modules/HR/pages/Post/PostManagementPage.tsx

import {
  useEffect,
  useState
} from "react";


import {
  postApi
} from "../../api/PostApi";


import {
  PostInfoView
} from "../../models/postInfoView";


import {
  useDataTreeGrid
} from "@/core/components/DataTreeGrid";


import {
  postTreeAdapter
} from "../../adapters/postTreeAdapter";





export default function PostManagementPage(){


  const [
    posts,
    setPosts
  ] = useState<PostInfoView[]>([]);



  const [
    selectedId,
    setSelectedId
  ] = useState<string|null>(null);





  useEffect(()=>{


    postApi
      .GetList()
      .then(result=>{

        setPosts(result);

      });


  },[]);







  const tree =
    useDataTreeGrid({

      data:posts,

      adapter:
        postTreeAdapter,

      defaultExpandAll:false

    });









  const selectedNode =
    selectedId
      ?
      tree.navigation.findNode(selectedId)
      :
      null;










  return (

    <div className="p-6">


      <h1 className="text-xl font-bold mb-5">

        مدیریت چارت سازمانی

      </h1>





      <div className="flex gap-3 mb-5">


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



      </div>









      <div className="border rounded">


        <table className="w-full">


          <thead>


            <tr>



              <th className="border p-2">
                عنوان پست
              </th>


              <th className="border p-2">
                متصدی
              </th>



              <th className="border p-2">
                تلفن
              </th>


            </tr>


          </thead>






          <tbody>


          {
            tree.rows.map(row=>{


              const expanded =
                tree.expansion.isExpanded(
                  row.id
                );



              const hasChildren =
                !tree.validation.isLeaf(
                  row.id
                );




              return (

                <tr

                  key={row.id}


                  className={

                    selectedId === row.id

                    ?

                    "bg-blue-100"

                    :

                    ""

                  }


                  onClick={()=>{

                    setSelectedId(
                      row.id
                    );

                  }}


                >



                  {/* Drag Handle */}

                  <td className="border p-2">

  <div

    className="flex items-center gap-2"

    style={{
      paddingRight:
        row.depth * 24
    }}

  >

    {/* Drag */}

    <button
      className="cursor-grab"
      onClick={(e)=>{
        e.stopPropagation();

        console.log(
          "DRAG",
          row.id
        );

      }}
    >
      ☰
    </button>



    {/* Expand */}

    {
      hasChildren
      &&
      <button
        onClick={(e)=>{

          e.stopPropagation();

          tree.expansion.toggle(
            row.id
          );

        }}
      >

        {
          expanded
          ?
          "▼"
          :
          "▶"
        }

      </button>
    }



    <span>

      {
        row.item.jobTitleName
      }

    </span>


  </div>


</td>




                  <td className="border p-2">

                    {
                      row.item.firstName + " " + row.item.lastName
                    }

                  </td>









                  <td className="border p-2">

                    {
                      row.item.officePhone
                    }

                  </td>



                </tr>

              );


            })
          }


          </tbody>


        </table>


      </div>









      <div className="mt-5 border p-3 rounded">


        <b>
          Node انتخاب شده:
        </b>


        {
          selectedNode

          ?

          <>
            {" "}
            {selectedNode.jobTitleName}
            {" - "}
            {selectedNode.firstName}
            {" "}
            {selectedNode.lastName}
          </>

          :

          " هیچ موردی انتخاب نشده"

        }


      </div>






      <pre className="mt-5 bg-gray-100 p-3 text-xs">


        {
          JSON.stringify(

            {

              total:
                tree.rows.length,

              selectedId

            },

            null,

            2

          )
        }


      </pre>



    </div>

  );

}