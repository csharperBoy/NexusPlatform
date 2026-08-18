// src/core/components/Input/TagInput.tsx

import React, { useState, KeyboardEvent } from "react";

interface TagInputProps {
  value?: string[] | null;
  onChange: (values: string[]) => void;
  placeholder?: string;
  className?: string;
}

export const TagInput: React.FC<TagInputProps> = ({
  value,
  onChange,
  placeholder = "افزودن...",
  className = "",
}) => {
  const [inputValue, setInputValue] = useState("");
  const tags = value ?? [];

  const handleAddTag = () => {
    const trimmed = inputValue.trim();
    if (trimmed && !tags.includes(trimmed)) {
      onChange([...tags, trimmed]);
      setInputValue("");
    }
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter" || e.key === ",") {
      e.preventDefault();
      handleAddTag();
    } else if (e.key === "Backspace" && !inputValue && tags.length > 0) {
      onChange(tags.slice(0, -1));
    }
  };

  const removeTag = (indexToRemove: number) => {
    onChange(tags.filter((_, index) => index !== indexToRemove));
  };

  return (
    <div
      className={`flex flex-wrap items-center gap-1 p-1 border border-gray-300 rounded-lg bg-white focus-within:ring-1 focus-within:ring-blue-500 focus-within:border-blue-500 min-h-[34px] transition-colors ${className}`}
    >
      {tags.map((tag, index) => (
        <span
          key={index}
          className="inline-flex items-center gap-1 px-1.5 py-0.5 bg-blue-50 text-blue-700 border border-blue-200 rounded text-xs font-mono"
        >
          {tag}
          <button
            type="button"
            onClick={() => removeTag(index)}
            className="text-blue-400 hover:text-blue-700 text-xs font-bold leading-none cursor-pointer"
          >
            ×
          </button>
        </span>
      ))}
      <input
        type="text"
        value={inputValue}
        onChange={(e) => setInputValue(e.target.value)}
        onKeyDown={handleKeyDown}
        onBlur={handleAddTag}
        placeholder={tags.length === 0 ? placeholder : ""}
        className="flex-1 min-w-[70px] bg-transparent outline-none text-xs font-mono text-gray-700 dir-ltr text-center placeholder:font-sans placeholder:dir-rtl px-1 py-0.5"
      />
    </div>
  );
};