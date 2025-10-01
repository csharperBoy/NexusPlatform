import React, { useEffect, useState, useMemo, useCallback } from 'react';
import {
  getShifts,
  getForms,
  getPeriods,
  getValueTypes,
} from '../api/LookUpCollection';

import type { Option as ComboOption } from '../models/Option';


import NumericInputCell from '../components/Inputs/Grid/NumericInputCell';
import type { GridColDef, GridRenderEditCellParams, GridValueFormatterParams } from '@mui/x-data-grid';
import DateObject from "react-date-object";
import persian from "react-date-object/calendars/persian";
import persian_fa from "react-date-object/locales/persian_fa";
import {
  GetIndicatorList,
  GetIndicatorValue,
  GetIndicatorDraft,
  DeleteIndicatorValue,
} from '../api/IndicatorCollection';

import MultiSelectComboBox, {
} from '../components/Inputs/ComboBox/MultiSelectComboBox';
import JalaliDatePicker from '../components/Inputs/DatePicker/JalaliDatePicker';
import DataTableWithInlineAdd from '../components/DataTables/DataTableWithInlineAdd';

import type {
  GetIndicatorValueResponse,
  GetIndicatorDraftResponse,
} from '../models/Indicator';
// import type { GetLookupListResponse } from '../models/Lookup';

import { GridRowModel } from '@mui/x-data-grid';
// import dayjs from 'dayjs';
import toast from 'react-hot-toast';
import { SaveIndicatorValueBulkChangeRequest, SaveIndicatorValueChangeRequest } from '../models/Indicator/SaveIndicatorValueChange';
import { SaveIndicatorValueBulkChange } from '../api/mock/IndicatorCollectionMock';

const IndicatorValueInput: React.FC = () => {
  // --- فیلترها ---
  const [fromDate, setFromDate] = useState<Date | null>(null);
  const [toDate, setToDate] = useState<Date | null>(null);

  // --- داده‌های لیست‌های انتخابی (کامبوباکس‌ها) ---
  const [shiftOptions, setShiftOptions] = useState<ComboOption[]>([]);
  const [formOptions, setFormOptions] = useState<ComboOption[]>([]);
  const [periodOptions, setPeriodOptions] = useState<ComboOption[]>([]);
  const [valueTypeOptions, setValueTypeOptions] = useState<ComboOption[]>([]);
  const [indicatorOptions, setIndicatorOptions] = useState<ComboOption[]>([]);

  // --- مقادیر انتخاب شده در فیلترها ---
  const [selectedShifts, setSelectedShifts] = useState<string[]>([]);
  const [selectedForms, setSelectedForms] = useState<string[]>([]);
  const [selectedPeriods, setSelectedPeriods] = useState<string[]>([]);
  const [selectedValueTypes, setSelectedValueTypes] = useState<string[]>([]);
  const [selectedIndicators, setSelectedIndicators] = useState<string[]>([]);

  // --- داده‌های جدول ---
  const [tableRows, setTableRows] = useState<GetIndicatorValueResponse[]>([]);

  // --- مدیریت حالت ویرایش سطرها (برای DataGrid) ---
  const [rowModesModel, setRowModesModel] = useState({});

  // --- بارگذاری لیست‌های فیلتر از سرور (فقط یکبار) ---
  useEffect(() => {
    async function fetchLookups() {
      try {
        const [shifts, forms, periods, valueTypes, indicators] = await Promise.all([
          getShifts(),
          getForms(),
          getPeriods(),
          getValueTypes(),
          GetIndicatorList(),
        ]);

        // مپ کردن ساختار API به ساختار کامپوننت (label/value)
        setShiftOptions(shifts.map((item) => ({ label: item.display, value: String(item.id) })));
        setFormOptions(forms.map((item) => ({ label: item.display, value: String(item.id) })));
        setPeriodOptions(periods.map((item) => ({ label: item.display, value: String(item.id) })));
        setValueTypeOptions(valueTypes.map((item) => ({ label: item.display, value: String(item.id) })));
        setIndicatorOptions(indicators.map((item) => ({ label: String(item.title), value: String(item.id) })));
        console.info('Indicator',indicators);

        console.info('Shift',shifts);
      } catch (error) {
        toast.error('خطا در بارگذاری داده‌های فیلترها');
      }
    }
    fetchLookups();
  }, []);

  // --- تبدیل داده جدول (GetIndicatorValueResponse[]) به GridRowModel[] برای DataTable ---
  const rowsForTable: GridRowModel[] = useMemo(() => {
    return tableRows.map((row) => ({
      ...row,
      id: row.id,
      fkLkpShiftId: row.fkLkpShiftId.toString(),
      fkLkpValueTypeId: row.fkLkpValueTypeId.toString(),
      fkIndicatorId: row.fkIndicatorId.toString(),
    }));
  }, [tableRows]);

  const handleDeleteRow = useCallback(async (id: string | number) => {
    try {
      const isValidGuid = typeof id === 'string' && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(id);

      // فقط اگر رکورد ذخیره‌شده است، API حذف را بزن
      if (isValidGuid) {
        const success = await DeleteIndicatorValue(id);
        if (!success) {
          toast.error('حذف با خطا مواجه شد');
          return;
        }
      }

      // حذف از جدول در هر حالت (چه آیدیش معتبر بود چه نه)
      const newRows = tableRows.filter(row => row.id !== id);
      setTableRows(newRows);

      toast.success('رکورد حذف شد');
    } catch {
      toast.error('خطا در فرآیند حذف');
    }
  }, [tableRows]);

  // --- تبدیل داده برگشتی از جدول (GridRowModel[]) به مدل GetIndicatorValueResponse[] ---
  const handleRowsChange = useCallback(
    (newRows: GridRowModel[]) => {
      const converted: GetIndicatorValueResponse[] = newRows.map((r) => ({
        id: String(r.id),
        fkIndicatorId: String(r.fkIndicatorId),
        fkLkpShiftId: Number(r.fkLkpShiftId),
        fkLkpValueTypeId: Number(r.fkLkpValueTypeId),
        dateTime: r.dateTime instanceof Date ? r.dateTime : new Date(r.dateTime),
        value: Number(r.value),
        valueCumulative: Number(r.valueCumulative),
        description: r.description ?? '',
        fkLkpPeriodId: r.fkLkpPeriodId ? Number(r.fkLkpPeriodId) : undefined,
        fkFormId: r.fkFormId ? Number(r.fkFormId) : undefined,
        hasChanges: r.hasChanges ?? false,
      }));

      setTableRows(converted);
    },
    [setTableRows]
  );

  // --- جستجو و دریافت داده‌ها با فیلترها ---
  const handleSearch = useCallback(async () => {
    try {
      //  console.info('IndicatorOption',indicatorOptions);

        // console.info('ShiftOption',shiftOptions);
       const fromDateTime =
       fromDate != null
         ? new Date(
            fromDate.getFullYear(),
             fromDate.getMonth(),
             fromDate.getDate(),
             0, 0, 0, 0
           )
         : undefined;

      // if (fromDateTime) {
      //   fromDateTime.setMinutes(fromDateTime.getMinutes() - fromDateTime.getTimezoneOffset());
      // }

     const toDateTime =
       toDate != null
         ? new Date(
             toDate.getFullYear(),
             toDate.getMonth(),
             toDate.getDate(),
             23, 59, 59, 999
           )
         : undefined;

      // if (toDateTime) {
      //  toDateTime.setMinutes(toDateTime.getMinutes() - toDateTime.getTimezoneOffset());
      // }
      const result = await GetIndicatorValue({
        fromDateTime: fromDateTime,
        toDateTime: toDateTime,
        fkIndicatorId: selectedIndicators.length ? selectedIndicators : undefined,
        fkLkpShiftId: selectedShifts.length ? selectedShifts.map(Number) : undefined,
        fkLkpValueTypeId: selectedValueTypes.length ? selectedValueTypes.map(Number) : undefined,
        fkLkpPeriodId: selectedPeriods.length ? selectedPeriods.map(Number) : undefined,
        fkFormId: selectedForms.length ? selectedForms.map(Number) : undefined,
      });
      setTableRows(result);
    } catch {
      toast.error('خطا در دریافت داده‌ها');
    }
  }, [
    fromDate,
    toDate,
    selectedIndicators,
    selectedShifts,
    selectedValueTypes,
    selectedPeriods,
    selectedForms,
  ]);
function dayStart(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate(), 0, 0, 0, 0);
}

function dayEnd(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate(), 23, 59, 59, 999);
}

  // --- دریافت ردیف‌های درفت جدید ---
  const handleFetchDraft = useCallback(async (): Promise<GridRowModel[]> => {
    try {
      const fromDateTime =
      fromDate != null
        ? new Date(
            fromDate.getFullYear(),
            fromDate.getMonth(),
            fromDate.getDate(),
            0, 0, 0, 0
          )
        : undefined;

    // if (fromDateTime) {
    //   fromDateTime.setMinutes(fromDateTime.getMinutes() - fromDateTime.getTimezoneOffset());
    // }

    const toDateTime =
      toDate != null
        ? new Date(
            toDate.getFullYear(),
            toDate.getMonth(),
            toDate.getDate(),
            23, 59, 59, 999
          )
        : undefined;

    // if (toDateTime) {
    //   toDateTime.setMinutes(toDateTime.getMinutes() - toDateTime.getTimezoneOffset());
    // }


      const draftResult = await GetIndicatorDraft({
        fromDateTime: fromDateTime,
        toDateTime: toDateTime,
        fkLkpShiftId: selectedShifts.length ? selectedShifts.map(Number) : undefined,
        fkLkpValueTypeId: selectedValueTypes.length ? selectedValueTypes.map(Number) : undefined,
        fkIndicatorId: selectedIndicators.length ? selectedIndicators : undefined,
        fkLkpPeriodId: selectedPeriods.length ? selectedPeriods.map(Number) : undefined,
        fkFormId: selectedForms.length ? selectedForms.map(Number) : undefined,      
        previousList: tableRows.map(({ id, ...rest }) => rest) as GetIndicatorDraftResponse[],
      });
      console.info('fromDate',fromDate ? dayStart(fromDate) : undefined);
      console.info('toDate',toDate ? dayEnd(toDate) : undefined);
      // نگاشت به GridRowModel و اضافه کردن id موقت
      const draftRows: GridRowModel[] = draftResult.map((d, idx) => ({
        id: `draft-${Date.now()}-${idx}`,
        fkIndicatorId: d.fkIndicatorId ?? '',
        fkLkpValueTypeId: d.fkLkpValueTypeId?.toString() ?? '',
        fkLkpShiftId: d.fkLkpShiftId?.toString() ?? '',
        dateTime: d.dateTime ?? new Date(),
        value: d.value ?? 0,
        valueCumulative: d.valueCumulative ?? 0,
        description: d.description ?? '',
      }));

      return draftRows;
    } catch {
      toast.error('خطا در دریافت داده‌های درفت');
      return [];
    }
  }, [fromDate, toDate, selectedIndicators, tableRows]);

  // --- ذخیره نهایی همه ردیف‌ها ---
  const handleSaveAll = useCallback(async () => {
  try {
    const changedRows = tableRows.filter(row => row.hasChanges === true);

    if (changedRows.length === 0) {
      toast('موردی برای ثبت وجود ندارد');
      return;
    }

    const isValidGuid = (id: string): boolean => {
      return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(id);
    };
    // function normalizeUtc(date: Date ): Date  {
     
    //   return new Date(date.getTime() - date.getTimezoneOffset() * 60000);
    // }

    const saveList: SaveIndicatorValueChangeRequest[] = changedRows.map((row) => {
      const guidValid = isValidGuid(row.id);

      console.log('row.id', row.id);
      console.log('row.dateTime', row.dateTime);

      return {
        id: guidValid ? row.id : undefined, // فقط GUIDهای واقعی
        fkIndicatorId: row.fkIndicatorId,
        fkLkpShiftId: row.fkLkpShiftId,
        fkLkpValueTypeId: row.fkLkpValueTypeId,
        dateTime: row.dateTime,
        value: row.value,
        valueCumulative: row.valueCumulative,
        description: row.description ?? null,
        
      };
    });


    const payload: SaveIndicatorValueBulkChangeRequest = {
      saveList,
      deleteList: [],
    };

    const response = await SaveIndicatorValueBulkChange(payload);

    if (response?.isSuccess) {
      toast.success('ثبت با موفقیت انجام شد');

      const resetRows = tableRows.map(row => ({
        ...row,
        hasChanges: false,
      }));
      setTableRows(resetRows);

      await handleSearch();
    } else {
      toast.error('ثبت با خطا مواجه شد');
    }
  } catch {
    toast.error('خطا در ثبت نهایی');
  }
}, [tableRows, handleSearch]);


  // --- تعریف ستون‌ها با استفاده از useMemo برای بهینه‌سازی ---
  const columns: GridColDef[] = useMemo<GridColDef[]>(
  () => [
    {
      field: 'fkIndicatorId',
      headerName: 'شاخص',
      minWidth: 180,
      flex: 1,
      editable: true,
      type: 'singleSelect',
      valueOptions: indicatorOptions.map(i => ({ value: i.value, label: i.label })),
      valueFormatter: (params: GridValueFormatterParams) =>
        indicatorOptions.find(i => i.value === params.value)?.label ?? '' ,
      renderEditCell: (params: GridRenderEditCellParams) => {
        const label = indicatorOptions.find(i => i.value === params.value)?.label ?? '';
        return <div className="input input-bordered w-full">{label}</div>;
      },
    },
    {
      field: 'fkLkpShiftId',
      headerName: 'شیفت',
      minWidth: 130,
      flex: 1,
      editable: true,
      type: 'singleSelect',
      valueOptions: shiftOptions.map(i => ({ value: i.value, label: i.label })),
      valueFormatter: (params: GridValueFormatterParams) =>
        shiftOptions.find(i => i.value === params.value)?.label ?? '',
      renderEditCell: (params: GridRenderEditCellParams) => {
        const label = shiftOptions.find(i => i.value === params.value)?.label ?? '';
        return <div className="input input-bordered w-full">{label}</div>;
      },
    },
    {
      field: 'fkLkpValueTypeId',
      headerName: 'نوع مقدار',
      minWidth: 130,
      flex: 1,
      editable: true,
      type: 'singleSelect',
      valueOptions: valueTypeOptions.map(i => ({ value: i.value, label: i.label })),
      valueFormatter: (params: GridValueFormatterParams) =>
        valueTypeOptions.find(i => i.value === params.value)?.label ?? '',
      renderEditCell: (params: GridRenderEditCellParams) => {
        const label = valueTypeOptions.find(i => i.value === params.value)?.label ?? '';
        return <div className="input input-bordered w-full">{label}</div>;
      },
    },
    {
      field: 'dateTime',
      headerName: 'تاریخ',
      minWidth: 140,
      flex: 1,
      editable: false,
      type: 'date',
      valueFormatter: (params: GridValueFormatterParams) => {
      try {
        const date = params.value instanceof Date ? params.value : new Date(params.value);
        
        const jalaliDate = new DateObject({
          date: date,
          calendar: persian,
          locale: persian_fa,
        });

        return jalaliDate.format('YYYY/MM/DD');
      } catch {
        return '';
      }
    },
      renderEditCell: (params: GridRenderEditCellParams) => (
        <JalaliDatePicker
          value={params.value as Date}
          onChange={(val) =>
            params.api.setEditCellValue({
              id: params.id,
              field: params.field,
              value: val,
            }, event) // اگه خواستی از event هم استفاده کنی
          }
        />
      ),
    },
  {
    field: 'value',
    headerName: 'مقدار',
    minWidth: 110,
    flex: 1,
    editable: true,
    type: 'string', // مهم: نوعش رشته باشه
    renderEditCell: (params) => <NumericInputCell {...params} />,
  },
  {
    field: 'valueCumulative',
    headerName: 'مقدار تجمعی',
    minWidth: 130,
    flex: 1,
    editable: true,
    type: 'string',
    renderEditCell: (params) => <NumericInputCell  {...params} />,
  },
    {
      field: 'description',
      headerName: 'توضیحات',
      minWidth: 200,
      flex: 2,
      editable: true,
      type: 'string',
    },
  ],
  [indicatorOptions, shiftOptions, valueTypeOptions]
);


  return (
    <div className="p-4 space-y-6 max-w-full">
      {/* فیلترها */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <JalaliDatePicker label="از تاریخ" value={fromDate} onChange={setFromDate} />
        <JalaliDatePicker label="تا تاریخ" value={toDate} onChange={setToDate} />
        <MultiSelectComboBox
          label="شیفت"
          options={shiftOptions}
          selectedValues={selectedShifts}
          onChange={setSelectedShifts}
        />
        <MultiSelectComboBox
          label="نوع مقدار"
          options={valueTypeOptions}
          selectedValues={selectedValueTypes}
          onChange={setSelectedValueTypes}
        />
        <MultiSelectComboBox
          label="دوره"
          options={periodOptions}
          selectedValues={selectedPeriods}
          onChange={setSelectedPeriods}
        />
        <MultiSelectComboBox
          label="فرم"
          options={formOptions}
          selectedValues={selectedForms}
          onChange={setSelectedForms}
        />
        <MultiSelectComboBox
          label="شاخص"
          options={indicatorOptions}
          selectedValues={selectedIndicators}
          onChange={setSelectedIndicators}
        />
      </div>

      {/* دکمه جستجو */}
      <div className="flex gap-3">
        <button className="btn btn-primary" onClick={handleSearch}>
          جستجو
        </button>
      </div>

      {/* جدول */}
      <DataTableWithInlineAdd
        slug="indicator-value"
        columns={columns}
        rows={rowsForTable}
        rowModesModel={rowModesModel}
        includeActionColumn
        onRowsChange={handleRowsChange}
        onRowModesModelChange={setRowModesModel}
        fetchNewRowTemplate={handleFetchDraft}
        enableExcel
        onDeleteRow={handleDeleteRow}
        onSaveAll={async (rows: GridRowModel[]) => {
          // تبدیل GridRowModel[] به GetIndicatorValueResponse[]
          const dataToSave: GetIndicatorValueResponse[] = rows.map((r) => ({
            id: String(r.id),
            fkIndicatorId: String(r.fkIndicatorId),
            fkLkpShiftId: Number(r.fkLkpShiftId),
            fkLkpValueTypeId: Number(r.fkLkpValueTypeId),
            dateTime: r.dateTime instanceof Date ? r.dateTime : new Date(r.dateTime),
            value: Number(r.value),
            valueCumulative: Number(r.valueCumulative),
            description: r.description ?? '',
          }));

          setTableRows(dataToSave);
          await handleSaveAll();
        }}
      />
    </div>
  );
};

export default IndicatorValueInput;
