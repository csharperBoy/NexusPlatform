// src/modules/HR/pages/Post/PostManagementPage.tsx

import {
  useEffect,
  useState
} from "react";

import {
  DndContext,
  DragEndEvent,
  PointerSensor,
  useSensor,
  useSensors,
  useDraggable,
  useDroppable
} from "@dnd-kit/core";

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
import { UpdatePostCommand } from "../../models/postCommand";

function DroppableRow({
  id,
  children,
  className,
  onClick
}: {
  id: string;
  children: React.ReactNode;
  className?: string;
  onClick?: () => void;
}) {


  const {
    setNodeRef,
    isOver
  } =
    useDroppable({
      id
    });



  return (

    <tr

      ref={setNodeRef}

      onClick={onClick}

      className={

        isOver

          ?

          "bg-green-100"

          :

          className

      }

    >

      {children}

    </tr>

  );

}
function DragHandle({
  id
}: {
  id: string;
}) {


  const {
    attributes,
    listeners,
    setNodeRef,
    transform
  } =
    useDraggable({
      id
    });



  const style = transform
    ? {
      transform:
        `translate3d(${transform.x}px, ${transform.y}px, 0)`
    }
    :
    undefined;



  return (

    <button

      ref={setNodeRef}

      style={style}

      {...listeners}

      {...attributes}

      className="cursor-grab"

    >

      ☰

    </button>

  );

}


function TreeRowDropTarget({

  id,

  children

}: {

  id: string;

  children: React.ReactNode;

}) {


  const {
    setNodeRef
  } =
    useDroppable({
      id
    });



  return (

    <tr ref={setNodeRef}>

      {children}

    </tr>

  );

}

export default function PostManagementPage() {


  const [
    posts,
    setPosts
  ] = useState<PostInfoView[]>([]);


  const [
    originalPosts,
    setOriginalPosts
  ] = useState<PostInfoView[]>([]);


  const [
    hasChanges,
    setHasChanges
  ] = useState(false);


  const [
    pendingChanges,
    setPendingChanges
  ] = useState<
    Map<string, Partial<UpdatePostCommand>>
  >(
    () => new Map()
  );


  const [
    selectedId,
    setSelectedId
  ] = useState<string | null>(null);


  const [
    draggingId,
    setDraggingId
  ] = useState<string | null>(null);


  useEffect(() => {


    postApi
      .GetList()
      .then(result => {

        setPosts(result);

        setOriginalPosts(result);

      });


  }, []);







  const tree =
    useDataTreeGrid({

      data: posts,

      adapter:
        postTreeAdapter,

      defaultExpandAll: false

    });









  const selectedNode =
    selectedId
      ?
      tree.navigation.findNode(selectedId)
      :
      null;





  const sensors =
    useSensors(

      useSensor(
        PointerSensor,
        {
          activationConstraint: {
            distance: 5
          }
        }

      )

    );


  function handleDragEnd(
    event: DragEndEvent
  ) {
    console.log(
      "DRAG END",
      {
        active: event.active.id,
        over: event.over?.id
      }
    );
    const {
      active,
      over
    } = event;


    setDraggingId(null);


    if (!over) {
      return;
    }


    const sourceId =
      String(active.id);



    const targetId =
      String(over.id);



    if (sourceId === targetId) {
      return;
    }



    const validation =
      tree.validation.canMove(
        sourceId,
        targetId
      );



    if (!validation.allowed) {

      alert(
        validation.reason ??
        "انتقال مجاز نیست"
      );

      return;

    }



    setPosts(previous => {

      return previous.map(item => {


        if (item.id === sourceId) {

          return {

            ...item,

            fkParentId:
              targetId

          };

        }


        return item;

      });

    });



    registerChange(

      sourceId,

      {
        id: sourceId,

        reportsToPostId:
          targetId

      }

    );

  }


  function handleDragStart(event: any) {

    console.log(
      "DRAG START",
      event.active.id
    );


    setDraggingId(
      String(event.active.id)
    );

  }



  function registerChange(

    id: string,

    change: Partial<UpdatePostCommand>

  ) {

    setPendingChanges(previous => {

      const next = new Map(previous);

      const current =
        next.get(id)
        ?? {};

      next.set(

        id,

        {
          ...current,
          ...change
        }

      );

      return next;

    });

    setHasChanges(true);

  }

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

        <button

          className="btn btn-success"

          disabled={!hasChanges}

          onClick={() => {

            console.log(

              Array.from(
                pendingChanges.values()
              )

            );

          }}

        >

          ذخیره تغییرات

        </button>
        <button

          className="btn"

          disabled={!hasChanges}

          onClick={() => {

            setPosts(originalPosts);

            setPendingChanges(
              new Map()
            );

            setHasChanges(false);

          }}

        >

          لغو تغییرات

        </button>

      </div>








      <DndContext

        sensors={sensors}

        onDragStart={handleDragStart}

        onDragEnd={handleDragEnd}

      >

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
                tree.rows.map(row => {


                  const expanded =
                    tree.expansion.isExpanded(
                      row.id
                    );



                  const hasChildren =
                    !tree.validation.isLeaf(
                      row.id
                    );




                  return (

                    <DroppableRow

                      key={row.id}

                      id={row.id}

                      className={
                        selectedId === row.id
                          ?
                          "bg-blue-100"
                          :
                          ""
                      }

                      onClick={() => {

                        setSelectedId(row.id);

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

                          <DragHandle
                            id={row.id}
                          />



                          {/* Expand */}

                          {
                            hasChildren
                            &&
                            <button
                              onClick={(e) => {

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


                    </DroppableRow>

                  );


                })
              }


            </tbody>


          </table>


        </div>

      </DndContext>







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