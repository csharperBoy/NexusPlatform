CREATE VIEW [dbo].[Post_Info_View]
AS
SELECT
    hr.Post.Id,
    hr.Post.Code AS Post_Code,
    hr.Post.ParentId,
    hr.Post.FkParentId,
    hr.Post.FkJobTitleId,
    hr.Post.FkOrganizationUnitId,
    hr.Post.FkJobLevelId,
    hr.Post.FkGradeId,
    hr.Post.FkCostCenterId,
    hr.CostCenter.Name AS CostCenter_Name,
    hr.Grade.Title AS Grade_Title,
    hr.JobLevel.Title AS JobLevel_Title,
    hr.JobTitle.Name AS JobTitle_Name,
    Assign.EmploymentId,
    Assign.EmploymentCode,
    Assign.FirstName,
    Assign.LastName,
    Assign.NationalCode,
    Assign.Gender,
    Assign.AssigneeType AS Assignments_AssigneeType,
    hr.OrganizationUnits.Name AS OrganizationUnits_Name,
    hr.Post.FkContactProfileId
FROM hr.Post
LEFT JOIN hr.JobTitle ON hr.Post.FkJobTitleId = hr.JobTitle.Id
LEFT JOIN hr.Grade ON hr.Post.FkGradeId = hr.Grade.Id
LEFT JOIN hr.OrganizationUnits ON hr.Post.FkOrganizationUnitId = hr.OrganizationUnits.Id
LEFT JOIN hr.CostCenter ON hr.Post.FkCostCenterId = hr.CostCenter.Id
LEFT JOIN hr.JobLevel ON hr.Post.FkJobLevelId = hr.JobLevel.Id

OUTER APPLY (
    SELECT TOP 1
        e.Id AS EmploymentId,
        e.EmploymentCode,
        np.FirstName,
        np.LastName,
        np.NationalCode,
        np.Gender,
        a.AssigneeType
    FROM hr.Assignments a
    INNER JOIN hr.Employment e ON a.FkEmploymentId = e.Id
    INNER JOIN people.naturalPersons np ON e.FkNaturalPersonId = np.Id
    WHERE a.FkPostId = hr.Post.Id AND a.IsCurrent = 1
    ORDER BY a.EffectiveFrom DESC   -- یا هر معیار مناسب
) Assign

WHERE hr.Post.IsRemove <> 1;
