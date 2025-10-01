import { useEffect, useState, useImperativeHandle, forwardRef } from 'react';
import { useParams } from 'react-router-dom';

import JalaliDatePicker from '../components/Inputs/DatePicker/JalaliDatePicker';

import { getForms, getPeriods, getValueTypes } from '../api/LookUpCollection';
import { GetIndicatorById } from '../api/IndicatorCollection';
import { SaveIndicatorChange } from '../api/IndicatorCollection';

import type { Option as ComboOption } from '../models/Option';
import type { IndicatorModel } from '../models/Indicator/IndicatorModel';
import type { SaveIndicatorChangeRequest } from '../models/Indicator/SaveIndicatorChange';

export interface IndicatorFormRef {
  getRequest: () => SaveIndicatorChangeRequest;
}

interface IndicatorFormProps {
  onSave?: (req: SaveIndicatorChangeRequest) => Promise<void>;
  onNext?: () => Promise<void> | void;
  onBack?: () => void;
}

const defaultForm: IndicatorModel = {
  code: '',
  title: '',
  dateTimeFrom: new Date(),
  dateTimeTo: null,
  fkLkpTypeId: 0,
  fkLkpManualityId: 0,
  fkLkpPeriodId: 0,
  fkLkpMeasureId: 0,
  fkLkpDesirabilityId: 0,
  fkLkpFormId: 0,
  formula: '',
  description: '',
};

const IndicatorForm = forwardRef<IndicatorFormRef, IndicatorFormProps>(({ onSave, onNext, onBack }, ref) => {
  const { id } = useParams<{ id?: string }>();
  const isEditMode = Boolean(id);

  const [formData, setFormData] = useState<IndicatorModel>(defaultForm);
  const [loading, setLoading] = useState(true);

  const [formOptions, setFormOptions] = useState<ComboOption[]>([]);
  const [periodOptions, setPeriodOptions] = useState<ComboOption[]>([]);
  const [valueTypeOptions, setValueTypeOptions] = useState<ComboOption[]>([]);
  const [manualityOptions, setManualityOptions] = useState<ComboOption[]>([]);
  const [measureOptions, setMeasureOptions] = useState<ComboOption[]>([]);
  const [desirabilityOptions, setDesirabilityOptions] = useState<ComboOption[]>([]);

  useEffect(() => {
    (async () => {
      try {
        const [forms, periods, types, manuality, measure, desirability] =
          await Promise.all([
            getForms(),
            getPeriods(),
            getValueTypes(),
            getValueTypes(),
            getValueTypes(),
            getValueTypes(),
          ]);

        setFormOptions(forms.map(item => ({ label: item.display, value: String(item.id) })));
        setPeriodOptions(periods.map(item => ({ label: item.display, value: String(item.id) })));
        setValueTypeOptions(types.map(item => ({ label: item.display, value: String(item.id) })));
        setManualityOptions(manuality.map(item => ({ label: item.display, value: String(item.id) })));
        setMeasureOptions(measure.map(item => ({ label: item.display, value: String(item.id) })));
        setDesirabilityOptions(desirability.map(item => ({ label: item.display, value: String(item.id) })));

        if (isEditMode && id) {
          const indicator = await GetIndicatorById(id);
          setFormData(indicator);
        }
      } catch (err) {
        console.error('خطا در دریافت داده‌های اولیه:', err);
      } finally {
        setLoading(false);
      }
    })();
  }, [id]);

  useImperativeHandle(ref, () => ({
    getRequest: () => ({
      id: isEditMode && id ? id : null,
      fkLkpValueTypeId: formData.fkLkpTypeId,
      fkLkpShiftId: 0,
      fkLkpPeriodId: formData.fkLkpPeriodId,
      fkLkpMeasureId: formData.fkLkpMeasureId,
      fkLkpDesirabilityId: formData.fkLkpDesirabilityId,
      fkLkpFormId: formData.fkLkpFormId,
      fkLkpManualityId: formData.fkLkpManualityId,
      code: formData.code,
      title: formData.title,
      formula: formData.formula ?? '',
      description: formData.description ?? '',
      dateTimeFrom: formData.dateTimeFrom,
      dateTimeTo: formData.dateTimeTo,
    }),
  }));

  const handleChange = <K extends keyof IndicatorModel>(field: K, value: IndicatorModel[K]) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const [isSaving, setIsSaving] = useState(false);

  const buildRequest = (): SaveIndicatorChangeRequest => ({
    id: isEditMode && id ? id : null,
    fkLkpValueTypeId: formData.fkLkpTypeId,
    fkLkpShiftId: 0,
    fkLkpPeriodId: formData.fkLkpPeriodId,
    fkLkpMeasureId: formData.fkLkpMeasureId,
    fkLkpDesirabilityId: formData.fkLkpDesirabilityId,
    fkLkpFormId: formData.fkLkpFormId,
    fkLkpManualityId: formData.fkLkpManualityId,
    code: formData.code,
    title: formData.title,
    formula: formData.formula ?? '',
    description: formData.description ?? '',
    dateTimeFrom: formData.dateTimeFrom,
    dateTimeTo: formData.dateTimeTo,
  });

  const handleSave = async () => {
    const req = buildRequest();
    try {
      setIsSaving(true);
      if (onSave) {
        await onSave(req);
      } else {
        // default standalone behavior
        await SaveIndicatorChange(req);
      }
    } finally {
      setIsSaving(false);
    }
  };

  const handleSaveAndNext = async () => {
    await handleSave();
    if (onNext) await onNext();
  };

  if (loading) {
    return (
      <div className="w-full flex items-center justify-center py-12">
        <div className="loader ease-linear rounded-full border-8 border-t-8 border-gray-200 h-16 w-16"></div>
      </div>
    );
  }

  const fieldMap = [
    { key: 'fkLkpTypeId', label: 'نوع شاخص', options: valueTypeOptions },
    { key: 'fkLkpManualityId', label: 'دستی / سیستمی', options: manualityOptions },
    { key: 'fkLkpPeriodId', label: 'دوره', options: periodOptions },
    { key: 'fkLkpMeasureId', label: 'واحد سنجش', options: measureOptions },
    { key: 'fkLkpDesirabilityId', label: 'مطلوبیت', options: desirabilityOptions },
    { key: 'fkLkpFormId', label: 'فرم منبع', options: formOptions },
  ] as const;

  return (
    <div className="max-w-4xl mx-auto bg-base-100 p-6 rounded-lg shadow-md">
      <div className="flex items-center justify-between mb-6">
        <h3 className="text-xl font-bold">{isEditMode ? 'ویرایش شناسنامه شاخص' : 'افزودن شناسنامه شاخص'}</h3>
        <div className="text-sm text-gray-500">{isEditMode ? `شناسه: ${id}` : ''}</div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="label">
            <span className="label-text">کد شاخص</span>
          </label>
          <input
            className="input input-bordered w-full"
            value={formData.code}
            onChange={e => handleChange('code', e.target.value)}
            placeholder="مثال: IND-001"
          />
        </div>

        <div>
          <label className="label">
            <span className="label-text">عنوان شاخص</span>
          </label>
          <input
            className="input input-bordered w-full"
            value={formData.title}
            onChange={e => handleChange('title', e.target.value)}
            placeholder="عنوان شاخص"
          />
        </div>

        <div>
          <label className="label">
            <span className="label-text">از تاریخ</span>
          </label>
          <JalaliDatePicker  value={formData.dateTimeFrom} onChange={date => handleChange('dateTimeFrom', date ?? new Date())} />
        </div>

        <div>
          <label className="label">
            <span className="label-text">تا تاریخ</span>
          </label>
          <JalaliDatePicker value={formData.dateTimeTo} onChange={date => handleChange('dateTimeTo', date)} />
        </div>

        {fieldMap.map(({ key, label, options }) => (
          <div key={key}>
            <label className="label">
              <span className="label-text">{label}</span>
            </label>
            <select
              className="select select-bordered w-full"
              value={String(formData[key])}
              onChange={e => handleChange(key, parseInt(e.target.value))}
            >
              <option value={0}>انتخاب کنید</option>
              {options.map(opt => (
                <option key={opt.value} value={opt.value}>{opt.label}</option>
              ))}
            </select>
          </div>
        ))}

        <div className="md:col-span-2">
          <label className="label">
            <span className="label-text">فرمول محاسبه</span>
          </label>
          <input className="input input-bordered w-full" value={formData.formula ?? ''} onChange={e => handleChange('formula', e.target.value)} placeholder="مثال: (a+b)/c" />
        </div>

        <div className="md:col-span-2">
          <label className="label">
            <span className="label-text">توضیحات</span>
          </label>
          <textarea className="textarea textarea-bordered w-full" rows={4} value={formData.description ?? ''} onChange={e => handleChange('description', e.target.value)} />
        </div>
      </div>
      <div className="mt-4 flex justify-between items-center">
        <div />
        <div className="flex items-center">
          {/* When used inside the wizard (onNext provided) show a single "ادامه" button and optional "بازگشت" */}
          {onNext ? (
            <>
              {onBack && (
                <button className="btn btn-ghost mr-2" onClick={onBack} type="button">بازگشت</button>
              )}
              <button className="btn btn-primary" onClick={handleSaveAndNext} disabled={isSaving} type="button">ادامه</button>
            </>
          ) : (
            // Standalone mode: only show the save button
            <button className="btn btn-primary" onClick={handleSave} disabled={isSaving} type="button">ثبت</button>
          )}
        </div>
      </div>
    </div>
  );
});

export default IndicatorForm;
