// models/postCommand.ts

// معادل enum C# (PostAssignmentType)
export enum PostAssignmentType {
    Delegation = 0,//دائمی
    Permanent = 1,//هیئت
    Acting = 2,//نمایندگی
    Temporary =3,//موقت
}

export interface UpdatePostCommand {
  id: string; // Guid
  code?: string | null;
  organizationUnitId?: string | null;
  jobTitleId?: string | null;
  jobLevelId?: string | null;
  gradeId?: string | null;
  costCenterId?: string | null;
  reportsToPostId?: string | null; // همان ParentId
  isActive?: boolean | null;
  employmentId?: string | null;
  assignType?: PostAssignmentType | null;
  officePhone?: string | null;
  orgEmail?: string | null;
  orgMobile?: string | null;
}

export interface CreatePostCommand {
  code: string ;
  organizationUnitId: string ;
  jobTitleId: string ;
  jobLevelId?: string | null;
  gradeId?: string | null;
  costCenterId?: string | null;
  reportsToPostId?: string | null; // همان ParentId
  isActive?: boolean | null;
  employmentId?: string | null;
  assignType?: PostAssignmentType | null;
  officePhone?: string | null;
  orgEmail?: string | null;
  orgMobile?: string | null;
}

