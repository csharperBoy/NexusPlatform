CREATE VIEW [dbo].[Employment_Info_View]
AS
SELECT 
    hr.Employment.Id,
    people.naturalPersons.NationalCode,
    people.naturalPersons.FirstName,
    people.naturalPersons.LastName,
    hr.Employment.EmploymentCode,
    hr.Employment.EffectiveFrom AS Employment_EffectiveFrom,
    hr.Employment.EffectiveTo AS Employment_EffectiveTo,
    people.Parties.Id AS Party_Id,
    hr.EmploymentStatus.Name AS Employment_Status_Name,
    hr.EmploymentType.Name AS Employment_Type_Name,
    Assign.AssigneeType AS Assignments_AssigneeType,
    Assign.EffectiveFrom AS Assignments_EffectiveFrom,
    Assign.EffectiveTo AS Assignments_EffectiveTo,
    hr.Post.Code AS Post_Code,
    hr.Grade.Title AS Grade_Title,
    hr.CostCenter.Name AS CostCenter_Name,
    hr.JobLevel.Title AS JobLevel_Title,
    hr.JobTitle.Name AS JobTitle_Name,
    hr.OrganizationUnits.Name AS OrganizationUnits_Name,
    hr.Employment.FkContactProfileId,
    people.Parties.FkContactProfileId AS FkPartyContactProfileId,
    people.naturalPersons.Gender
FROM hr.Employment
LEFT JOIN people.naturalPersons 
    ON hr.Employment.FkNaturalPersonId = people.naturalPersons.Id
LEFT JOIN people.Parties 
    ON people.naturalPersons.FkPartyId = people.Parties.Id
OUTER APPLY (
    SELECT TOP 1 *
    FROM hr.Assignments a
    WHERE a.FkEmploymentId = hr.Employment.Id AND a.IsCurrent = 1
    ORDER BY a.EffectiveFrom DESC
) Assign
LEFT JOIN hr.Post ON Assign.FkPostId = hr.Post.Id
LEFT JOIN hr.OrganizationUnits ON hr.Post.FkOrganizationUnitId = hr.OrganizationUnits.Id
LEFT JOIN hr.JobTitle ON hr.Post.FkJobTitleId = hr.JobTitle.Id
LEFT JOIN hr.JobLevel ON hr.Post.FkJobLevelId = hr.JobLevel.Id
LEFT JOIN hr.CostCenter ON hr.Post.FkCostCenterId = hr.CostCenter.Id
LEFT JOIN hr.Grade ON hr.Post.FkGradeId = hr.Grade.Id
LEFT JOIN hr.EmploymentType ON hr.Employment.FkEmploymentTypeId = hr.EmploymentType.Id
LEFT JOIN hr.EmploymentStatus ON hr.Employment.FkEmploymentStatusId = hr.EmploymentStatus.Id
WHERE hr.Employment.IsRemove <> 1;
