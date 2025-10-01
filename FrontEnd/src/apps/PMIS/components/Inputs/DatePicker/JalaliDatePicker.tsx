// src/components/inputs/datePicker/JalaliDatePicker.tsx
import //React,
 { forwardRef } from "react";
import { HiOutlineCalendar } from 'react-icons/hi';
import DatePicker from "react-multi-date-picker";
import persian from "react-date-object/calendars/persian";
import persian_fa from "react-date-object/locales/persian_fa";
import type { DateObject } from "react-multi-date-picker";

interface JalaliDatePickerProps {
  label?: string;
  value: Date | null;
  onChange: (date: Date | null) => void;
  className?: string;
  disabled?: boolean;
  required?: boolean;
}

const JalaliDatePicker = forwardRef<typeof DatePicker, JalaliDatePickerProps>(
  ({ label, value, onChange, className = "", disabled = false, required = false }, ref) => {


    const handleChange = (dateObject: DateObject | null) => {
      if (dateObject) {
        const date = new Date(dateObject.toDate());
        onChange(date);
      } else {
        onChange(null);
      }
    };

    const convertToDateObject = (date: Date | null) => {
      if (!date) return undefined;
      return new Date(date);
    };

    return (
      <div className={`flex flex-col gap-1 ${className}`}>
        {label && (
          <label className="text-sm font-medium text-gray-700 dark:text-gray-300 text-right">
            {label}
            {required && <span className="text-red-500"> *</span>}
          </label>
        )}

        <div className="relative w-full">
          <DatePicker
            portal
            containerClassName="w-full"
            className="w-full"
            inputClass="input input-bordered w-full pr-10 text-right"
            calendarClassName="rounded-lg shadow-lg bg-base-100 p-2"
            calendar={persian}
            locale={persian_fa}
            calendarPosition="bottom-right"
            value={convertToDateObject(value)}
            onChange={handleChange}
            disabled={disabled}
            format="YYYY/MM/DD"
            editable={!disabled}
            hideOnScroll
            // @ts-ignore
            ref={ref}
          />

          <div className="absolute inset-y-0 left-3 flex items-center pointer-events-none text-gray-400">
            <HiOutlineCalendar className="text-lg" />
          </div>
        </div>
      </div>
    );
  }
);

JalaliDatePicker.displayName = "JalaliDatePicker";

export default JalaliDatePicker;