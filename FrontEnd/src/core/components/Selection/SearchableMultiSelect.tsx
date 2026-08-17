// src/core/components/Selection/SearchableMultiSelect.tsx
import React, { useState, useRef, useEffect } from "react";
import { SelectionListDto } from "@/core/models/SelectionListDto";

interface SearchableMultiSelectProps {
  options: SelectionListDto[];
  value: string[];
  onChange: (selectedValues: string[]) => void;
  placeholder?: string;
  disabled?: boolean;
}

export const SearchableMultiSelect: React.FC<SearchableMultiSelectProps> = ({
  options = [],
  value = [],
  onChange,
  placeholder = "انتخاب موارد...",
  disabled = false,
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const filteredOptions = options.filter(
    (opt) =>
      opt.label?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      opt.display?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const toggleOption = (val: string, e: React.MouseEvent) => {
    e.stopPropagation();
    if (value.includes(val)) {
      onChange(value.filter((v) => v !== val));
    } else {
      onChange([...value, val]);
    }
  };

  const removeValue = (val: string, e: React.MouseEvent) => {
    e.stopPropagation();
    onChange(value.filter((v) => v !== val));
  };

  const selectedOptions = options.filter((opt) => value.includes(opt.value));

  return (
    <div className="relative w-full text-right font-sans" ref={containerRef}>
      <div
        onClick={() => !disabled && setIsOpen((prev) => !prev)}
        className={`min-h-[34px] p-1.5 border rounded-lg bg-white flex flex-wrap items-center gap-1 cursor-pointer transition-colors ${
          disabled ? "bg-gray-100 cursor-not-allowed opacity-60" : "hover:border-gray-400"
        } ${isOpen ? "border-blue-500 ring-1 ring-blue-500" : "border-gray-300"}`}
      >
        {selectedOptions.length === 0 && (
          <span className="text-xs text-gray-400 px-1">{placeholder}</span>
        )}

        {selectedOptions.map((opt) => (
          <span
            key={opt.value}
            className="inline-flex items-center gap-1 bg-blue-50 text-blue-700 border border-blue-200 text-xs px-2 py-0.5 rounded-md font-medium"
          >
            {opt.label || opt.display}
            <button
              type="button"
              onClick={(e) => removeValue(opt.value, e)}
              className="hover:text-red-600 font-bold text-xs"
            >
              ×
            </button>
          </span>
        ))}
      </div>

      {isOpen && (
        <div className="absolute z-50 mt-1 w-full bg-white border border-gray-200 rounded-lg shadow-lg max-h-60 overflow-hidden flex flex-col">
          <div className="p-2 border-b border-gray-100 bg-gray-50">
            <input
              type="text"
              className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:outline-none focus:border-blue-500"
              placeholder="جستجو..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              onClick={(e) => e.stopPropagation()}
            />
          </div>

          <div className="overflow-y-auto max-h-48 p-1">
            {filteredOptions.length === 0 ? (
              <div className="text-xs text-gray-400 p-2 text-center">موردی یافت نشد</div>
            ) : (
              filteredOptions.map((opt) => {
                const isSelected = value.includes(opt.value);
                return (
                  <div
                    key={opt.value}
                    onClick={(e) => toggleOption(opt.value, e)}
                    className={`flex items-center gap-2 p-1.5 text-xs rounded cursor-pointer transition-colors ${
                      isSelected ? "bg-blue-50 text-blue-700 font-medium" : "hover:bg-gray-100 text-gray-700"
                    }`}
                  >
                    <input
                      type="checkbox"
                      checked={isSelected}
                      readOnly
                      className="rounded border-gray-300 text-blue-600 focus:ring-0 pointer-events-none"
                    />
                    <span>{opt.display || opt.label}</span>
                  </div>
                );
              })
            )}
          </div>
        </div>
      )}
    </div>
  );
};