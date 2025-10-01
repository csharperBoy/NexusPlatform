import { useEffect, useState, useImperativeHandle, forwardRef } from 'react';
import {
  Box,
  Typography,
  TextField,
  MenuItem,
  CircularProgress,
  Button,
} from '@mui/material';
import {
  GetIndicatorCategories,
  GetIndicatorList,
  SaveIndicatorCategories,
} from '../api/IndicatorCollection';
import { GetIndicatorListResponse } from '../models/Indicator';
import { GetCategoryList } from '../api/CategoryCollection';
import { CategoryModel } from '../models/Category/CategoryModel';
import { SaveIndicatorCategoriesRequest } from '../models/Indicator/SaveIndicatorCategories';
import { RichTreeView } from '@mui/x-tree-view/RichTreeView';
import { TreeViewBaseItem } from '@mui/x-tree-view/models';

export interface IndicatorCategoryAssignmentRef {
  getRequest: () => SaveIndicatorCategoriesRequest | null;
}

interface Props {
  indicatorId?: string;
  onNext?: () => void;
  onBack?: () => void;
  onSave?: (req: SaveIndicatorCategoriesRequest) => Promise<void>;
}

const IndicatorCategoryAssignment = forwardRef<IndicatorCategoryAssignmentRef, Props>(
  ({ indicatorId, onNext, onBack, onSave }, ref) => {
    const [categories, setCategories] = useState<TreeViewBaseItem[]>([]);
    const [assignedCategoryIds, setAssignedCategoryIds] = useState<string[]>([]);
    const [loading, setLoading] = useState(true);

    // indicators list and currently selected indicator id (local state so user can choose when prop not provided)
    const [indicators, setIndicators] = useState<GetIndicatorListResponse[]>([]);
    const [selectedIndicatorId, setSelectedIndicatorId] = useState<string>(indicatorId ?? '');

    useEffect(() => {
      (async () => {
        try {
          const [categoryList, indicatorList] = await Promise.all([GetCategoryList(), GetIndicatorList()]);
          setCategories(buildTree(categoryList));
          setIndicators(indicatorList);

          // if we have an initial indicatorId (prop) or selectedIndicatorId after fetching, load assigned categories
          const idToLoad = indicatorId ?? selectedIndicatorId ?? '';
          if (idToLoad) {
            const assigned = await GetIndicatorCategories(idToLoad);
            setAssignedCategoryIds(assigned.map(c => c.id!).filter(Boolean));
            setSelectedIndicatorId(idToLoad);
          }
        } finally {
          setLoading(false);
        }
      })();
    }, [indicatorId]);

    // when user picks another indicator (only possible when prop indicatorId not provided), reload assigned categories
    useEffect(() => {
      if (!selectedIndicatorId) return;
      // if prop was provided and equals selectedIndicatorId we already loaded in initial effect
      if (indicatorId && indicatorId === selectedIndicatorId) return;

      let cancelled = false;
      (async () => {
        setLoading(true);
        try {
          const assigned = await GetIndicatorCategories(selectedIndicatorId);
          if (!cancelled) setAssignedCategoryIds(assigned.map(c => c.id!).filter(Boolean));
        } finally {
          if (!cancelled) setLoading(false);
        }
      })();

      return () => {
        cancelled = true;
      };
    }, [selectedIndicatorId, indicatorId]);

    useImperativeHandle(ref, () => ({
      getRequest: () => ({
        indicatorId: selectedIndicatorId,
        categoryIds: assignedCategoryIds,
      }),
    }));

    const [isSaving, setIsSaving] = useState(false);

    const handleSave = async () => {
      const req: SaveIndicatorCategoriesRequest = {
        indicatorId: selectedIndicatorId,
        categoryIds: assignedCategoryIds,
      };
      try {
        setIsSaving(true);
        if (onSave) {
          await onSave(req);
        } else {
          await SaveIndicatorCategories(req);
        }
      } finally {
        setIsSaving(false);
      }
    };

    const handleSaveAndNext = async () => {
      await handleSave();
      if (onNext) onNext();
    };

    if (loading) return <CircularProgress />;

    return (
      <Box>
        <Typography variant="h6" mb={2}>
          تخصیص شاخص به دسته‌بندی
        </Typography>

        {/* Indicator selector: editable only when prop indicatorId was not provided */}
        <TextField
          select
          fullWidth
          label="انتخاب شاخص"
          value={selectedIndicatorId}
          onChange={e => setSelectedIndicatorId(e.target.value)}
          disabled={Boolean(indicatorId)}
          sx={{ mb: 2 }}
        >
          <MenuItem value="">انتخاب کنید</MenuItem>
          {indicators.map(ind => (
            <MenuItem key={ind.id} value={ind.id}>
              {ind.title}
            </MenuItem>
          ))}
        </TextField>

        <Typography variant="subtitle1" mb={1}>
          دسته‌بندی‌ها:
        </Typography>

        <Box
          sx={{
            border: '1px dashed gray',
            padding: 2,
            maxHeight: 400,
            overflow: 'auto',
            minWidth: 300,
          }}
        >
          <RichTreeView
            checkboxSelection
            multiSelect
            items={categories}
            selectedItems={assignedCategoryIds}
            onSelectedItemsChange={(_, selected) =>
              setAssignedCategoryIds(Array.isArray(selected) ? selected : [])
            }
            sx={{
              direction: 'rtl',
              '& [role="group"]': {
                pr: 2,
              },
            }}
          />
        </Box>

        <Box sx={{ mt: 3, display: 'flex', justifyContent: 'space-between' }}>
          {/* Wizard mode: when onNext is provided show optional Back + single Continue (ادامه) button.
              Standalone mode: only show the Save (ثبت) button. */}
          {onNext ? (
            <>
              <Box>
                {onBack && (
                  <Button variant="outlined" onClick={onBack} disabled={isSaving}>
                    بازگشت
                  </Button>
                )}
              </Box>

              <Box>
                <Button variant="contained" color="primary" onClick={handleSaveAndNext} disabled={!selectedIndicatorId || isSaving}>
                  ادامه
                </Button>
              </Box>
            </>
          ) : (
            <Box sx={{ marginLeft: 'auto' }}>
              <Button variant="contained" onClick={handleSave} disabled={!selectedIndicatorId || isSaving}>
                ثبت
              </Button>
            </Box>
          )}
        </Box>
      </Box>
    );
  }
);

export default IndicatorCategoryAssignment;

// 🧩 Utility: ساخت درخت RichTreeView از دسته‌بندی‌های فلت
function buildTree(items: CategoryModel[]): TreeViewBaseItem[] {
  const itemMap = new Map<string, TreeViewBaseItem>();
  const roots: TreeViewBaseItem[] = [];

  for (const item of items) {
    itemMap.set(item.id!, {
      id: item.id!,
      label: item.Title ?? '',
      children: [],
    });
  }

  for (const item of items) {
    const node = itemMap.get(item.id!)!;
    if (item.FkParentId) {
      const parent = itemMap.get(item.FkParentId);
      if (parent) {
        parent.children!.push(node);
      }
    } else {
      roots.push(node);
    }
  }

  return roots;
}
