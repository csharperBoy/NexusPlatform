CREATE VIEW [dbo].[Post_Info_View]
AS
SELECT        hr.Post.Id, hr.Post.Code, hr.Post.ParentId, hr.Post.FkParentId, hr.Post.FkJobTitleId, hr.Post.FkOrganizationUnitId, hr.Post.FkJobLevelId, hr.Post.FkGradeId, hr.Post.FkCostCenterId, hr.[ CostCenter].Name AS CostCenter_Name, 
                         hr.[ Grade].Title AS Grade_Title, hr.JobLevel.Title AS JobLevel_Title, hr.JobTitle.Name AS JobTitle_Name, OfficePhone.Value AS OfficePhone, OrgMobile.Value AS OrgMobile, OrgEmail.Value AS OrgEmail, 
                         hr.[ Employment].EmployeeCode, people.naturalPersons.FirstName, people.naturalPersons.LastName, people.naturalPersons.NationalCode, people.naturalPersons.Gender, hr.Assignments.AssigneeType
FROM            hr.[ Employment] INNER JOIN
                         hr.Assignments ON hr.[ Employment].Id = hr.Assignments.FkEmploymentId INNER JOIN
                         people.naturalPersons ON hr.[ Employment].FkNaturalPersonId = people.naturalPersons.Id RIGHT OUTER JOIN
                         hr.Post LEFT OUTER JOIN
                         hr.[ CostCenter] ON hr.Post.FkCostCenterId = hr.[ CostCenter].Id LEFT OUTER JOIN
                         hr.[ Grade] ON hr.Post.FkGradeId = hr.[ Grade].Id ON hr.Assignments.FkPostId = hr.Post.Id LEFT OUTER JOIN
                         hr.JobLevel ON hr.Post.FkJobLevelId = hr.JobLevel.Id LEFT OUTER JOIN
                         hr.JobTitle ON hr.Post.FkJobTitleId = hr.JobTitle.Id LEFT OUTER JOIN
                         hr.PostContacts AS OfficePhone ON hr.Post.Id = OfficePhone.FkPostId AND OfficePhone.ContactType = 0 LEFT OUTER JOIN
                         hr.PostContacts AS OrgMobile ON hr.Post.Id = OrgMobile.FkPostId AND OrgMobile.ContactType = 1 LEFT OUTER JOIN
                         hr.PostContacts AS OrgEmail ON hr.Post.Id = OrgEmail.FkPostId AND OrgEmail.ContactType = 3