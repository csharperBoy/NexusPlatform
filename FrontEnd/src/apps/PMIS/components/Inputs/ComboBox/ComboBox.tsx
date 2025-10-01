// src/components/ComboBox/ComboBox.tsx
import React from 'react';
import { Autocomplete, TextField } from '@mui/material';

export interface Option {
  label: string;
  value: number;
}

interface ComboBoxProps {
  label: string;
  options: Option[];
  selectedValue: number | null;
  onChange: (newValue: number | null) => void;
}

const ComboBox: React.FC<ComboBoxProps> = ({
  label,
  options,
  selectedValue,
  onChange,
}) => {
  const selectedOption = options.find(option => option.value === selectedValue) || null;

  return (
    <Autocomplete
      options={options}
      getOptionLabel={(option) => option.label}
      value={selectedOption}
      onChange={(event, newValue) => {
        console.info(event);
        onChange(newValue ? newValue.value : null);
      }}
      isOptionEqualToValue={(option, value) => option.value === value.value}
      renderInput={(params) => (
        <TextField {...params} variant="outlined" label={label} size="small" />
      )}
      sx={{ minWidth: 200 }}
    />
  );
};

export default ComboBox;
