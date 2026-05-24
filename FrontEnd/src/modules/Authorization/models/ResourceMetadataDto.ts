
export interface fieldDto {
  name: string;
  baseType: number;
  clrType: string;
  displayName: string;
  
} 

public record JoinDto(
    string NavigationName,
    string TargetEntity,
    string TargetKey,      // PK property names on the target entity
    string CurrentKey     // FK property names on the current entity
);

public record ResourceMetadataDto(
    string ResourceKey,
    string EntityName,
    List<FieldDto> ScalarFields,
    List<JoinDto> Joins
);