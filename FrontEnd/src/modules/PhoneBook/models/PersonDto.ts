
// modules/HR/models/PersonDto.ts
export interface PersonDto {
    id: string ;
    NationalCode?: string | null;
  FullName?: string | null;
  Gender?: string | null;
  Age?: number | 0;
}

