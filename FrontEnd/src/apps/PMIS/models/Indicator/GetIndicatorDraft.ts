export interface GetIndicatorDraftRequest {
  fromDateTime?: Date; 
  toDateTime?: Date;
  fkIndicatorId?: string[];
  fkLkpShiftId?: number[];
  fkLkpValueTypeId?: number[];
  fkLkpPeriodId?: number[];         // شناسه دوره
  fkFormId?: number[];              // شناسه فرم
  previousList: GetIndicatorDraftResponse[];

}


export interface GetIndicatorDraftResponse {
  fkIndicatorId?: string;
  fkLkpValueTypeId?: number;
  fkLkpShiftId?: number;
  dateTime?: Date; 
  value?: number;
  valueCumulative?: number;
  description?: string | null;
}
