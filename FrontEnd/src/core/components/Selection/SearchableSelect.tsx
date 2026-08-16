import React, { useState, useRef, useEffect } from 'react';
import { Search, ChevronDown, Check, X } from 'lucide-react';
import { SelectionListDto } from '@/core/models/SelectionListDto';
//src/Core/Component/Selection/SearchableSelect.tsx
interface SearchableSelectProps<T extends SelectionListDto = SelectionListDto> {
  options: T[];
  value: string | null | undefined;
  onChange: (selectedOption: T | null) => void;
  placeholder?: string;
  searchPlaceholder?: string;
  emptyMessage?: string;
  disabled?: boolean;
  className?: string;
}

export const SearchableSelect = <T extends SelectionListDto = SelectionListDto>({
  options = [],
  value,
  onChange,
  placeholder = "انتخاب کنید...",
  searchPlaceholder = "تایپ کنید تا فیلتر شود...",
  emptyMessage = "موردی یافت نشد.",
  disabled = false,
  className = "",
}: SearchableSelectProps<T>) => {
  const [isOpen, setIsOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const containerRef = useRef<HTMLDivElement>(null);

  // یافتن آیتم انتخاب‌شده بر اساس value
  const selectedOption = options.find((opt) => opt.value === value);

  // فیلتر هوشمند روی هر دو فیلد label و display
  const filteredOptions = options.filter((option) => {
    const term = searchTerm.trim().toLowerCase();
    if (!term) return true;

    const matchLabel = option.label?.toLowerCase().includes(term) ?? false;
    const matchDisplay = option.display?.toLowerCase().includes(term) ?? false;
    const matchValue = option.value?.toLowerCase().includes(term) ?? false;

    return matchLabel || matchDisplay || matchValue;
  });

  // بستن منو با کلیک بیرون از کامپوننت
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const handleSelect = (option: T) => {
    onChange(option);
    setIsOpen(false);
    setSearchTerm("");
  };

  const handleClear = (e: React.MouseEvent) => {
    e.stopPropagation();
    onChange(null);
    setSearchTerm("");
  };

  return (
    <div ref={containerRef} className={`relative w-full dir-rtl ${className}`}>
      {/* Trigger Button */}
      <div
        onClick={() => !disabled && setIsOpen(!isOpen)}
        className={`flex items-center justify-between w-full px-3 py-2 text-sm bg-white border rounded-lg shadow-sm cursor-pointer transition-all ${
          disabled
            ? "bg-gray-100 cursor-not-allowed border-gray-200 text-gray-400"
            : isOpen
            ? "border-blue-500 ring-2 ring-blue-100"
            : "border-gray-300 hover:border-gray-400"
        }`}
      >
        <span className={`truncate ${selectedOption ? "text-gray-900 font-medium" : "text-gray-400"}`}>
          {selectedOption ? (selectedOption.display || selectedOption.label) : placeholder}
        </span>

        <div className="flex items-center gap-1 shrink-0">
          {selectedOption && !disabled && (
            <button
              type="button"
              onClick={handleClear}
              className="p-1 text-gray-400 rounded-full hover:bg-gray-100 hover:text-gray-600 transition-colors"
              title="پاک‌کردن"
            >
              <X className="w-4 h-4" />
            </button>
          )}
          <ChevronDown
            className={`w-4 h-4 text-gray-400 transition-transform duration-200 ${
              isOpen ? "rotate-180" : ""
            }`}
          />
        </div>
      </div>

      {/* Dropdown Menu */}
      {isOpen && (
        <div className="absolute z-50 w-full mt-1 bg-white border border-gray-200 rounded-lg shadow-lg max-h-64 flex flex-col overflow-hidden animate-in fade-in zoom-in-95 duration-100">
          {/* Search Box */}
          <div className="p-2 border-b border-gray-100 bg-gray-50 flex items-center gap-2">
            <Search className="w-4 h-4 text-gray-400 shrink-0" />
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder={searchPlaceholder}
              autoFocus
              className="w-full text-sm bg-transparent border-none outline-none text-gray-800 placeholder-gray-400 focus:ring-0"
            />
            {searchTerm && (
              <button
                type="button"
                onClick={() => setSearchTerm("")}
                className="text-xs text-gray-400 hover:text-gray-600 shrink-0"
              >
                پاک‌کردن
              </button>
            )}
          </div>

          {/* Options List */}
          <div className="overflow-y-auto max-h-48 divide-y divide-gray-50">
            {filteredOptions.length > 0 ? (
              filteredOptions.map((option) => {
                const isSelected = option.value === value;
                return (
                  <div
                    key={option.value}
                    onClick={() => handleSelect(option)}
                    className={`flex items-center justify-between px-3 py-2.5 text-sm cursor-pointer transition-colors ${
                      isSelected
                        ? "bg-blue-50 text-blue-700 font-medium"
                        : "hover:bg-gray-50 text-gray-700"
                    }`}
                  >
                    <div className="flex flex-col gap-0.5 truncate">
                      <span className="truncate">{option.label}</span>
                      {option.display && option.display !== option.label && (
                        <span className="text-xs text-gray-400 truncate">{option.display}</span>
                      )}
                    </div>
                    {isSelected && <Check className="w-4 h-4 text-blue-600 shrink-0 mr-2" />}
                  </div>
                );
              })
            ) : (
              <div className="px-3 py-4 text-sm text-center text-gray-400">
                {emptyMessage}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};