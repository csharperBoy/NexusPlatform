CREATE VIEW [dbo].[Employment_Info_View]
AS
SELECT        people.naturalPersons.NationalCode, people.naturalPersons.FirstName, people.naturalPersons.LastName, hr.[ Employment].EmploymentCode, hr.[ Employment].EffectiveFrom AS Employment_EffectiveFrom, 
                         hr.[ Employment].EffectiveTo AS Employment_EffectiveTo, people.Parties.Id AS Party_Id, PartyContacts_Mobile.Value AS Party_Mobile, PartyContacts_Address.Value AS Party_Address, PartyContacts_Phone.Value AS Party_Phone,
                          PartyContacts_Email.Value AS Party_Email, hr.[ EmploymentStatus].Name AS Employment_Status_Name, hr.[ EmploymentType].Name AS Employment_Type_Name, hr.Assignments.AssigneeType AS Assignments_AssigneeType, 
                         hr.Assignments.EffectiveFrom AS Assignments_EffectiveFrom, hr.Assignments.EffectiveTo AS Assignments_EffectiveTo, hr.Post.Code AS Post_Code, hr.[ Grade].Title AS Grade_Title, hr.[ CostCenter].Name AS CostCenter_Name, 
                         hr.JobLevel.Title AS JobLevel_Title, hr.JobTitle.Name AS JobTitle_Name, hr.OrganizationUnits.Name AS OrganizationUnits_Name, PostContact_Phone.Value AS PostContact_Phone, 
                         PostContact_Mobile.Value AS PostContact_Mobile, PostContact_Email.Value AS PostContact_Email, PostContact_Fax.Value AS PostContact_Fax, EmploymentContact_Phone.Value AS EmploymentContact_Phone, 
                         EmploymentContact_Mobile.Value AS EmploymentContact_Mobile, EmploymentContact_Email.Value AS EmploymentContact_Email, EmploymentContact_Fax.Value AS EmploymentContact_Fax, 
                         hr.[ EmploymentLocations].EffectiveFrom AS EmploymentLocations_EffectiveFrom, hr.[ EmploymentLocations].EffectiveTo AS EmploymentLocations_EffectiveTo, hr.Location.Title AS Location_Title
FROM            hr.Location INNER JOIN
                         hr.[ EmploymentLocations] ON hr.Location.Id = hr.[ EmploymentLocations].FkLocationId AND hr.[ EmploymentLocations].IsCurrent = 1 RIGHT OUTER JOIN
                         hr.[ Employment] INNER JOIN
                         people.naturalPersons ON hr.[ Employment].FkNaturalPersonId = people.naturalPersons.Id INNER JOIN
                         people.Parties ON people.naturalPersons.FkPartyId = people.Parties.Id INNER JOIN
                         hr.Assignments ON hr.[ Employment].Id = hr.Assignments.FkEmploymentId AND hr.Assignments.IsCurrent = 1 INNER JOIN
                         hr.Post ON hr.Assignments.FkPostId = hr.Post.Id ON hr.[ EmploymentLocations].FkEmploymentId = hr.[ Employment].Id AND hr.[ EmploymentLocations].IsCurrent = 1 LEFT OUTER JOIN
                         hr.PostContacts AS PostContact_Phone ON hr.Post.Id = PostContact_Phone.FkPostId AND PostContact_Phone.ContactType = 0 AND PostContact_Phone.IsCurrent = 1 LEFT OUTER JOIN
                         hr.PostContacts AS PostContact_Mobile ON hr.Post.Id = PostContact_Mobile.FkPostId AND PostContact_Mobile.ContactType = 1 AND PostContact_Mobile.IsCurrent = 1 LEFT OUTER JOIN
                         hr.PostContacts AS PostContact_Fax ON hr.Post.Id = PostContact_Fax.FkPostId AND PostContact_Fax.ContactType = 2 AND PostContact_Fax.IsCurrent = 1 LEFT OUTER JOIN
                         hr.PostContacts AS PostContact_Email ON hr.Post.Id = PostContact_Email.FkPostId AND PostContact_Email.ContactType = 3 AND PostContact_Email.IsCurrent = 1 LEFT OUTER JOIN
                         hr.OrganizationUnits ON hr.Post.FkOrganizationUnitId = hr.OrganizationUnits.Id LEFT OUTER JOIN
                         hr.JobTitle ON hr.Post.FkJobTitleId = hr.JobTitle.Id LEFT OUTER JOIN
                         hr.JobLevel ON hr.Post.FkJobLevelId = hr.JobLevel.Id LEFT OUTER JOIN
                         hr.[ CostCenter] ON hr.Post.FkCostCenterId = hr.[ CostCenter].Id LEFT OUTER JOIN
                         hr.[ Grade] ON hr.Post.FkGradeId = hr.[ Grade].Id LEFT OUTER JOIN
                         hr.[ EmploymentType] ON hr.[ Employment].FkEmploymentTypeId = hr.[ EmploymentType].Id LEFT OUTER JOIN
                         hr.[ EmploymentStatus] ON hr.[ Employment].FkEmploymentStatusId = hr.[ EmploymentStatus].Id LEFT OUTER JOIN
                         people.PartyContacts AS PartyContacts_Mobile ON people.Parties.Id = PartyContacts_Mobile.FkPartyId AND PartyContacts_Mobile.ContactType = 1 AND PartyContacts_Mobile.IsCurrent = 1 LEFT OUTER JOIN
                         people.PartyContacts AS PartyContacts_Email ON people.Parties.Id = PartyContacts_Email.FkPartyId AND PartyContacts_Email.ContactType = 2 AND PartyContacts_Email.IsCurrent = 1 LEFT OUTER JOIN
                         people.PartyContacts AS PartyContacts_Phone ON people.Parties.Id = PartyContacts_Phone.FkPartyId AND PartyContacts_Phone.ContactType = 0 AND PartyContacts_Phone.IsCurrent = 1 LEFT OUTER JOIN
                         people.PartyContacts AS PartyContacts_Address ON people.Parties.Id = PartyContacts_Address.FkPartyId AND PartyContacts_Address.ContactType = 3 AND PartyContacts_Address.IsCurrent = 1 LEFT OUTER JOIN
                         hr.EmploymentContacts AS EmploymentContact_Phone ON hr.[ Employment].Id = EmploymentContact_Phone.FkEmploymentId AND EmploymentContact_Phone.ContactType = 0 AND 
                         EmploymentContact_Phone.IsCurrent = 1 LEFT OUTER JOIN
                         hr.EmploymentContacts AS EmploymentContact_Mobile ON hr.[ Employment].Id = EmploymentContact_Mobile.FkEmploymentId AND EmploymentContact_Mobile.ContactType = 1 AND 
                         EmploymentContact_Mobile.IsCurrent = 1 LEFT OUTER JOIN
                         hr.EmploymentContacts AS EmploymentContact_Fax ON hr.[ Employment].Id = EmploymentContact_Fax.FkEmploymentId AND EmploymentContact_Fax.ContactType = 2 AND 
                         EmploymentContact_Fax.IsCurrent = 1 LEFT OUTER JOIN
                         hr.EmploymentContacts AS EmploymentContact_Email ON hr.[ Employment].Id = EmploymentContact_Email.FkEmploymentId AND EmploymentContact_Email.ContactType = 3 AND EmploymentContact_Email.IsCurrent = 1