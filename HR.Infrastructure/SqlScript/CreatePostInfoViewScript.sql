CREATE VIEW [dbo].[Post_Info_View]
AS
SELECT        hr.Post.Id, hr.Post.Code AS Post_Code, hr.Post.ParentId, hr.Post.FkParentId, hr.Post.FkJobTitleId, hr.Post.FkOrganizationUnitId, hr.Post.FkJobLevelId, hr.Post.FkGradeId, hr.Post.FkCostCenterId, 
                         hr.[CostCenter].Name AS CostCenter_Name, hr.[Grade].Title AS Grade_Title, hr.JobLevel.Title AS JobLevel_Title, hr.JobTitle.Name AS JobTitle_Name, OfficePhone.Value AS OfficePhone, OrgMobile.Value AS OrgMobile, 
                         OrgEmail.Value AS OrgEmail, hr.[Employment].EmploymentCode, people.naturalPersons.FirstName, people.naturalPersons.LastName, people.naturalPersons.NationalCode, people.naturalPersons.Gender, 
                         hr.Assignments.AssigneeType AS Assignments_AssigneeType, hr.OrganizationUnits.Name AS OrganizationUnits_Name,
                          hr.[PostLocations].EffectiveFrom AS Locations_EffectiveFrom, hr.[PostLocations].EffectiveTo AS Locations_EffectiveTo, hr.Location.Title AS Location_Title, hr.Location.Id AS Location_Id
FROM            hr.Location INNER JOIN
                         hr.[PostLocations] ON hr.Location.Id = hr.[PostLocations].FkLocationId AND hr.[PostLocations].IsCurrent = 1 RIGHT OUTER JOIN
                         
                         hr.Post INNER JOIN
                         
                         hr.Grade ON hr.Post.FkGradeId = hr.Grade.Id LEFT OUTER JOIN
                         hr.OrganizationUnits ON hr.Post.FkOrganizationUnitId = hr.OrganizationUnits.Id LEFT OUTER JOIN
                         hr.[CostCenter] ON hr.Post.FkCostCenterId = hr.[CostCenter].Id  LEFT OUTER JOIN
                         hr.[Employment] INNER JOIN
                         hr.Assignments ON hr.[Employment].Id = hr.Assignments.FkEmploymentId INNER JOIN
                         people.naturalPersons ON hr.[Employment].FkNaturalPersonId = people.naturalPersons.Id ON hr.Post.Id = hr.Assignments.FkPostId LEFT OUTER JOIN
                         hr.JobLevel ON hr.Post.FkJobLevelId = hr.JobLevel.Id LEFT OUTER JOIN
                         hr.JobTitle ON hr.Post.FkJobTitleId = hr.JobTitle.Id LEFT OUTER JOIN
                         contact.PostContacts AS OfficePhone ON hr.Post.Id = OfficePhone.FkPostId AND OfficePhone.ContactType = 0 AND OfficePhone.IsCurrent = 1 LEFT OUTER JOIN
                         contact.PostContacts AS OrgMobile ON hr.Post.Id = OrgMobile.FkPostId AND OrgMobile.ContactType = 1 AND OrgMobile.IsCurrent = 1 LEFT OUTER JOIN
                         contact.PostContacts AS OrgEmail ON hr.Post.Id = OrgEmail.FkPostId AND OrgEmail.ContactType = 3 AND OrgEmail.IsCurrent = 1