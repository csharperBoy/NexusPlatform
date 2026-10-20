export interface StrategyConfigFieldView {
  name: string;
  typeName: string;
  isRequired: boolean;
  defaultValue?: string | null;
}

export interface StrategyInfoView {
  key: string;
  displayName: string;
  configTypeName: string;
  configFields: StrategyConfigFieldView[];
}