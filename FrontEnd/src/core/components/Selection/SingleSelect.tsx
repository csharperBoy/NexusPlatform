// src/core/components/Selection/SingleSelect.tsx
import React from 'react';
import { SelectionListDto } from '@/core/models/SelectionListDto';

interface EnumSelectProps {
  options: SelectionListDto[];
  value?: string | number | null;
  onChange: (value: string | number | undefined) => void;
  label: string;
  disabled?: boolean;
  placeholder?: string;
  required?: boolean;          // اضافه شد
}

export const SingleSelect: React.FC<EnumSelectProps> = ({
  options,
  value,
  onChange,
  label,
  disabled,
  placeholder = 'انتخاب کنید...',
  required = false,            // پیش‌فرض false
}) => {
  const handleChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const selectedValue = e.target.value;
    if (selectedValue === '') {
      onChange(undefined);
      return;
    }
    const num = Number(selectedValue);
    if (!isNaN(num) && selectedValue.trim() !== '') {
      onChange(num);
    } else {
      onChange(selectedValue);
    }
  };

  const selectValue = value !== undefined && value !== null ? String(value) : '';

  return (
    <div className="mb-4">
      <label className="block mb-1">
        {label}
        {required && <span className="text-red-500 mr-1">*</span>}
      </label>
      <select
        value={selectValue}
        onChange={handleChange}
        className="w-full p-2 border rounded"
        disabled={disabled}
        required={required}
      >
        <option value="" disabled={required}>
          {placeholder}
        </option>
        {options.map(opt => (
          <option key={opt.value} value={opt.value}>
            {opt.display}
          </option>
        ))}
      </select>
    </div>
  );
};