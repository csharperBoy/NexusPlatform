
export interface fieldDto {
  name: string;
  baseType: number;
  clrType: string;
  displayName: string;
  
} 
export interface joinDto {
  navigationName: string;
  targetEntity: string;
  targetKey: string;
  currentKey: string;
  targetScalarFields: fieldDto[];
}


export interface resourceMetadataDto {
     resourceKey: string;
     entityName: string;
      useDynamicFilter:boolean;
      useNavigate:boolean;
      useScope:boolean;
     scalarFields: fieldDto[];
     joins: joinDto[];
} 