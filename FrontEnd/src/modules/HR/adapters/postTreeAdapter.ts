import { TreeAdapter } from "@/core/components/DataTreeGrid";
import { PostInfoView } from "../models/postInfoView";


export const postTreeAdapter:
TreeAdapter<PostInfoView> = {


  getId(item) {

    return item.id;

  },


  getParentId(item) {

    return item.fkParentId ?? null;

  },


  setParentId(
    item,
    parentId
  ) {

    return {
      ...item,
      fkParentId: parentId
    };

  },

  /**
   * عنوان نمایشی Node در Tree
   */
  getLabel(item) {

    return item.jobTitleName
      ?? "";

  },



  /**
   * فعلاً همه پست‌ها قابلیت انتقال دارند.
   *
   * بعداً می‌توانیم قوانین HR را اینجا اعمال کنیم.
   *
   * مثال:
   * - مدیرعامل قابل انتقال نباشد
   * - پست غیرفعال منتقل نشود
   */
  canMove(item) {

    return true;

  },



  /**
   * فعلاً همه پست‌ها قابل ویرایش هستند.
   *
   * بعداً می‌تواند بر اساس Permission
   * یا وضعیت پست تغییر کند.
   */
  canEdit(item) {

    return true;

  }
};

