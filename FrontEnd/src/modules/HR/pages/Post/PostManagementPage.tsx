// pages/PostManagement.tsx

import React, { useEffect, useState, useCallback } from 'react';
import {
  DndContext,
  DragEndEvent,
  DragOverlay,
  DragStartEvent,
  useDraggable,
  useDroppable,
  closestCenter,
} from '@dnd-kit/core';
import { postApi } from '../../api/PostApi';
import { UpdatePostCommand , PostAssignmentType} from '../../models/postCommand';
import { PostInfoView } from '../../models/postInfoView';

// کامپوننت هر گره درختی
interface TreeNodeProps {
  node: PostInfoView;
  allNodes: PostInfoView[];
  onDrop: (draggedId: string, targetId: string) => void;
  isChanged: boolean;
}

const TreeNode: React.FC<TreeNodeProps> = ({ node, allNodes, onDrop, isChanged }) => {
  const children = allNodes.filter((n) => n.parentId === node.id);

  const { attributes, listeners, setNodeRef: setDragRef, isDragging } = useDraggable({
    id: node.id,
  });

  const { setNodeRef: setDropRef } = useDroppable({
    id: node.id,
  });

  // ترکیب refها
  const setRefs = (element: HTMLDivElement | null) => {
    setDragRef(element);
    setDropRef(element);
  };

  return (
    <div
      ref={setRefs}
      {...attributes}
      {...listeners}
      style={{
        paddingLeft: '20px',
        margin: '4px 0',
        border: isDragging ? '2px dashed #aaa' : '1px solid #ddd',
        background: isChanged ? '#fff3cd' : 'transparent',
        cursor: 'grab',
        borderRadius: '4px',
      }}
    >
      <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
        <span>{node.postCode || 'بدون کد'}</span>
        <span style={{ fontSize: '0.8rem', color: '#666' }}>
          {node.firstName} {node.lastName}
        </span>
        {isChanged && <span style={{ color: '#856404' }}>✏️</span>}
      </div>

      {children.length > 0 && (
        <div style={{ marginLeft: '20px' }}>
          {children.map((child) => (
            <TreeNode
              key={child.id}
              node={child}
              allNodes={allNodes}
              onDrop={onDrop}
              isChanged={isChanged}
            />
          ))}
        </div>
      )}
    </div>
  );
};

// کامپوننت اصلی
const PostManagement: React.FC = () => {
  const [posts, setPosts] = useState<PostInfoView[]>([]);
  const [changedMap, setChangedMap] = useState<Map<string, string | null>>(new Map()); // id -> newParentId (null means root)
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // بارگذاری اولیه
  useEffect(() => {
    loadPosts();
  }, []);

  const loadPosts = async () => {
    try {
      setLoading(true);
      const data = await postApi.getPosts();
      setPosts(data);
      setChangedMap(new Map());
      setError(null);
    } catch (err) {
      setError('خطا در بارگذاری لیست پست‌ها');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  // ریشه‌ها (گره‌هایی که parentId ندارند یا برابر null هستند)
  const rootNodes = posts.filter((p) => p.parentId === null || p.parentId === undefined);

  // بررسی اینکه آیا گره فرزند گره دیگر است یا خیر (برای جلوگیری از ایجاد چرخه)
  const isDescendant = useCallback(
    (ancestorId: string, descendantId: string): boolean => {
      let currentId = descendantId;
      while (currentId) {
        const node = posts.find((p) => p.id === currentId);
        if (!node) break;
        if (node.parentId === ancestorId) return true;
        currentId = node.parentId || '';
      }
      return false;
    },
    [posts]
  );

  // مدیریت دراپ
  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    if (!over) return;

    const draggedId = active.id as string;
    const targetId = over.id as string;

    // اگر روی خودش رها شده یا هدف در زیرمجموعه درگ‌شونده است، کاری نکن
    if (draggedId === targetId) return;
    if (isDescendant(draggedId, targetId)) {
      alert('نمی‌توانید یک گره را به زیرمجموعه خودش منتقل کنید.');
      return;
    }

    // تغییر parentId در state محلی
    setPosts((prevPosts) =>
      prevPosts.map((p) =>
        p.id === draggedId ? { ...p, parentId: targetId } : p
      )
    );

    // ثبت تغییر در نقشه
    setChangedMap((prev) => {
      const newMap = new Map(prev);
      // اگر parentId جدید برابر با parentId قبلی است، می‌توانیم حذف کنیم (اختیاری)
      // اما برای سادگی، هر تغییر را ذخیره می‌کنیم
      newMap.set(draggedId, targetId);
      return newMap;
    });
  };

  // ذخیره تغییرات
  const handleSave = async () => {
    if (changedMap.size === 0) {
      alert('هیچ تغییری برای ذخیره وجود ندارد.');
      return;
    }

    setSaving(true);
    setError(null);

    try {
      // برای هر آیتم تغییر کرده، یک درخواست PUT ارسال کن
      const updatePromises: Promise<void>[] = [];
      for (const [postId, newParentId] of changedMap.entries()) {
        const post = posts.find((p) => p.id === postId);
        if (!post) continue;

        // ساخت command با استفاده از مقادیر فعلی
        const command: UpdatePostCommand = {
          id: postId,
          code: post.postCode,
          organizationUnitId: post.fkOrganizationUnitId,
          jobTitleId: post.fkJobTitleId,
          jobLevelId: post.fkJobLevelId,
          gradeId: post.fkGradeId,
          costCenterId: post.fkCostCenterId,
          reportsToPostId: newParentId, // مقدار جدید
          isActive: true, // یا مقدار پیش‌فرض
          employeeId: null, // در صورت نیاز
          assignType: null,
          officePhone: post.officePhone,
          orgEmail: post.orgEmail,
          orgMobile: post.orgMobile,
        };

        updatePromises.push(postApi.updatePost(command));
      }

      await Promise.all(updatePromises);

      // پس از موفقیت، تغییرات را پاک کن و دوباره بارگذاری کن
      setChangedMap(new Map());
      await loadPosts(); // یا به‌روزرسانی local
      alert('تغییرات با موفقیت ذخیره شد.');
    } catch (err) {
      setError('خطا در ذخیره تغییرات');
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <div>در حال بارگذاری...</div>;
  if (error) return <div style={{ color: 'red' }}>{error}</div>;

  return (
    <div style={{ padding: '20px' }}>
      <h2>مدیریت پست‌ها (ساختار سازمانی)</h2>
      <div style={{ marginBottom: '10px' }}>
        <button onClick={handleSave} disabled={saving || changedMap.size === 0}>
          {saving ? 'در حال ذخیره...' : 'ذخیره تغییرات'}
        </button>
        <span style={{ marginLeft: '15px', color: '#666' }}>
          تعداد تغییرات: {changedMap.size}
        </span>
      </div>

      <DndContext collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
        <div style={{ border: '1px solid #ccc', padding: '10px', borderRadius: '8px' }}>
          {rootNodes.length === 0 ? (
            <p>هیچ پستی یافت نشد.</p>
          ) : (
            rootNodes.map((root) => (
              <TreeNode
                key={root.id}
                node={root}
                allNodes={posts}
                onDrop={() => {}} // از DndContext استفاده می‌کنیم
                isChanged={changedMap.has(root.id)}
              />
            ))
          )}
        </div>
      </DndContext>

      {/* نمایش لیست تغییرات (اختیاری) */}
      {changedMap.size > 0 && (
        <div style={{ marginTop: '20px', borderTop: '1px solid #ddd', paddingTop: '10px' }}>
          <h4>تغییرات اعمال شده:</h4>
          <ul>
            {Array.from(changedMap.entries()).map(([id, newParent]) => {
              const post = posts.find((p) => p.id === id);
              return (
                <li key={id}>
                  {post?.postCode || id} → والد جدید: {newParent || '(بدون والد)'}
                </li>
              );
            })}
          </ul>
        </div>
      )}
    </div>
  );
};

export default PostManagement;