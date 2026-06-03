
export interface fieldDto {
  name: string;
  baseType: number;
  clrType: string;
  displayName: string;
  
} 
export interface joinDto {
  navigationName: string;
  targetEntity: number;
  targetKey: string;
  currentKey: string;
  targetScalarFields: fieldDto[];
}


export interface resourceMetadataDto {
     resourceKey: string;
     entityName: string;
     scalarFields: fieldDto[];
     joins: joinDto[];
} 