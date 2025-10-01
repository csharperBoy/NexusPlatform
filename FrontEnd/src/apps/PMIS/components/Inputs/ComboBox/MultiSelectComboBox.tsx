// src/components/ComboBox/MultiSelectComboBox.tsx
import React from 'react';
import { Autocomplete, TextField } from '@mui/material';
import type { Option } from '../../../models/Option';



interface MultiSelectComboBoxProps {
  label: string;
  options: Option[];
  selectedValues: string[];
  onChange: (newValues: string[]) => void;
}

const MultiSelectComboBox: React.FC<MultiSelectComboBoxProps> = ({
  label,
  options,
  selectedValues,
  onChange,
}) => {
  const selectedOptions = options.filter(option => selectedValues.includes(option.value));

  return (
    <Autocomplete
      multiple
      options={options}
      getOptionLabel={(option) => option.label}
      value={selectedOptions}
      onChange={(event, newValue) => {
          console.info(event);
        onChange(newValue.map(option => option.value));
      }}
      isOptionEqualToValue={(option, value) => option.value === value.value}
      renderInput={(params) => (
        <TextField {...params} variant="outlined" label={label} size="small" />
      )}
      sx={{ minWidth: 200 }}
    />
  );
};

export default MultiSelectComboBox;
