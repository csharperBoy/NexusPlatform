import {
  TreeAdapter,
  TreeNodeBase,
} from "../contracts";
import { TreeBuildResult, TreeDiagnostics, TreeIndex } from "../types";

/**
 * ایجاد ایندکس داخلی Tree
 *
 * Complexity:
 * O(n)
 */
export function buildTree<T extends TreeNodeBase>(
  items: readonly T[],
  adapter: TreeAdapter<T>
): TreeBuildResult<T> {
  const nodeMap = new Map<string, T>();
  const parentMap = new Map<string, string | null>();
  const childrenMap = new Map<string | null, string[]>();

  const rootIds: string[] = [];

  const duplicateIds: string[] = [];
  const missingParents: {
    nodeId: string;
    parentId: string;
  }[] = [];

  // -----------------------------
  // مرحله اول:
  // ساخت nodeMap و تشخیص Duplicate
  // -----------------------------

  for (const item of items) {
    const id = adapter.getId(item);

    if (nodeMap.has(id)) {
      duplicateIds.push(id);
      continue;
    }

    nodeMap.set(id, item);
  }

  // -----------------------------
  // مرحله دوم:
  // ساخت parentMap
  // -----------------------------

  for (const item of items) {
    const id = adapter.getId(item);
    const parentId = adapter.getParentId(item);

    parentMap.set(id, parentId);
  }

  // -----------------------------
  // مرحله سوم:
  // ساخت childrenMap و Rootها
  // -----------------------------

  for (const item of items) {
    const id = adapter.getId(item);
    const parentId = adapter.getParentId(item);

    if (parentId === null) {
      rootIds.push(id);
    } else {
      if (!nodeMap.has(parentId)) {
        missingParents.push({
          nodeId: id,
          parentId,
        });

        rootIds.push(id);

        continue;
      }
    }

    const children = childrenMap.get(parentId);

    if (children) {
      children.push(id);
    } else {
      childrenMap.set(parentId, [id]);
    }
  }

  // همیشه کلید null وجود داشته باشد
  if (!childrenMap.has(null)) {
    childrenMap.set(null, []);
  }

  const index: TreeIndex<T> = {
    items,

    nodeMap,

    parentMap,

    childrenMap,

    rootIds,
  };

  const diagnostics: TreeDiagnostics = {
    duplicateIds,
    missingParents,
  };

  return {
    index,
    diagnostics,
  };
}