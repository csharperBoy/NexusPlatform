// pages/PostManagementPage.tsx

import React, { useEffect, useState } from 'react';
import SortableTree from 'react-sortable-tree';
import 'react-sortable-tree/style.css'; // استایل پیش‌فرض
import { postApi } from '../../api/PostApi';
import { UpdatePostCommand } from '../../models/postCommand';
import { PostInfoView } from '../../models/postInfoView';
import { TreeItem } from 'react-sortable-tree';

// تبدیل داده‌های مسطح به ساختار درختی
const buildTree = (items: PostInfoView[]): TreeItem[] => {
  const itemMap: { [key: string]: TreeItem } = {};
  const tree: TreeItem[] = [];

  // ایجاد map
  items.forEach((item) => {
    itemMap[item.id] = {
      id: item.id,
      title: item.postCode || 'بدون کد',
      nodeData: item,
      children: [],
    };
  });

  // ساخت درخت
  items.forEach((item) => {
    const node = itemMap[item.id];
    if (item.fkParentId && itemMap[item.fkParentId]) {
      // اگر والد وجود دارد، به فرزندان اضافه کن
      if (!itemMap[item.fkParentId].children) {
        itemMap[item.fkParentId].children = [];
      }
      itemMap[item.fkParentId].children!.push(node);
    } else {
      // گره ریشه
      tree.push(node);
    }
  });

  return tree;
};

// تبدیل درخت به داده‌های مسطح (برای ذخیره‌سازی)
const flattenTree = (treeData: TreeItem[]): PostInfoView[] => {
  const result: PostInfoView[] = [];
  const traverse = (node: TreeItem, parentId: string | null = null) => {
    // به‌روزرسانی nodeData با parentId جدید
    const updatedNode: PostInfoView = {
      ...node.nodeData,
      fkParentId: parentId,
    };
    result.push(updatedNode);

    if (node.children) {
      node.children.forEach((child) => traverse(child, node.id));
    }
  };
  treeData.forEach((root) => traverse(root, null));
  return result;
};

// کامپوننت اصلی
const PostManagementPage: React.FC = () => {
  const [treeData, setTreeData] = useState<TreeItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // بارگذاری داده‌ها
  useEffect(() => {
    loadPosts();
  }, []);

  const loadPosts = async () => {
    try {
      setLoading(true);
      const data = await postApi.GetList();
      const tree = buildTree(data);
      setTreeData(tree);
      setError(null);
    } catch (err) {
      setError('خطا در بارگذاری لیست پست‌ها');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  // ذخیره تغییرات (با استفاده از flattenTree)
  const handleSave = async () => {
    setSaving(true);
    setError(null);

    try {
      // تبدیل درخت به لیست مسطح با parentIdهای به‌روز
      const flatPosts = flattenTree(treeData);

      // ساخت لیست دستورات به‌روزرسانی برای همه آیتم‌ها
      const commands: UpdatePostCommand[] = flatPosts.map((post) => ({
        id: post.id,
        code: post.postCode,
        organizationUnitId: post.fkOrganizationUnitId,
        jobTitleId: post.fkJobTitleId,
        jobLevelId: post.fkJobLevelId,
        gradeId: post.fkGradeId,
        costCenterId: post.fkCostCenterId,
        reportsToPostId: post.fkParentId,
        isActive: true,
        employeeId: null,
        assignType: null,
        officePhone: post.officePhone,
        orgEmail: post.orgEmail,
        orgMobile: post.orgMobile,
      }));

      await postApi.batchUpdatePosts(commands);
      alert('تغییرات با موفقیت ذخیره شد.');
    } catch (err) {
      setError('خطا در ذخیره تغییرات');
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  // رندر سفارشی برای هر گره (نمایش اطلاعات بیشتر و فیلدهای ویرایش)
  const renderNode = ({ node }: { node: TreeItem }) => {
    const data = node.nodeData as PostInfoView;
    return (
      <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
        <span style={{ fontWeight: 'bold' }}>{data.postCode}</span>
        <span>
          {data.firstName} {data.lastName}
        </span>
        {data.jobTitleName && (
          <span style={{ background: '#e6f0ff', padding: '2px 8px', borderRadius: '12px', fontSize: '0.8rem' }}>
            {data.jobTitleName}
          </span>
        )}
        <span style={{ fontSize: '0.8rem', color: '#666' }}>
          تلفن: {data.officePhone || '-'}
        </span>
        <span style={{ fontSize: '0.8rem', color: '#666' }}>
          موبایل: {data.orgMobile || '-'}
        </span>
      </div>
    );
  };

  if (loading) return <div>در حال بارگذاری...</div>;
  if (error) return <div style={{ color: 'red' }}>{error}</div>;

  return (
    <div style={{ padding: '20px', height: '80vh' }}>
      <h2>مدیریت پست‌ها (ساختار سازمانی)</h2>
      <button onClick={handleSave} disabled={saving} style={{ marginBottom: '10px' }}>
        {saving ? 'در حال ذخیره...' : 'ذخیره تغییرات'}
      </button>

      <div style={{ height: '100%', border: '1px solid #ccc' }}>
        <SortableTree
          treeData={treeData}
          onChange={(newTree) => setTreeData(newTree)}
          generateNodeProps={({ node }) => ({
            title: renderNode({ node }),
          })}
          canDrag={(props) => true}
          canDrop={(props) => {
            // جلوگیری از انتقال گره به زیرمجموعه خودش (در صورت نیاز)
            return true;
          }}
        />
      </div>
    </div>
  );
};

export default PostManagementPage;