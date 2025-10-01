import React, { useRef, useEffect } from 'react';
import { GridRenderEditCellParams } from '@mui/x-data-grid';

const NumericInputCell: React.FC<GridRenderEditCellParams> = (params) => {
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    inputRef.current?.focus();
  }, []);

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Tab') {
      e.preventDefault();
      inputRef.current?.blur(); // خروج از حالت ویرایش
    }

    if (e.key === 'Enter') {
      e.preventDefault();
      inputRef.current?.blur(); // خروج از حالت ویرایش
    }

    if (e.key === 'Escape') {
      e.preventDefault();
      inputRef.current?.blur(); // خروج از حالت ویرایش بدون تغییر
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const input = e.target.value;
    const isNumeric = /^-?\d*\.?\d*$/.test(input);
    if (isNumeric || input === '') {
      params.api.setEditCellValue({
        id: params.id,
        field: params.field,
        value: input,
      });
    }
  };

  return (
    <input
      ref={inputRef}
      type="text"
      value={params.value ?? ''}
      onChange={handleChange}
      onKeyDown={handleKeyDown}
      className="w-full h-full px-2 border-none bg-transparent focus:outline-none"
    />
  );
};

export default NumericInputCell;
