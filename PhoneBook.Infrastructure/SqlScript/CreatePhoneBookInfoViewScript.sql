CREATE VIEW phonebook.PhoneBook_Info_View
AS
SELECT        np.NationalCode, np.FirstName, np.LastName, emp.EmployeeCode, ou.Name AS OrganizationUnitsName, jt.Name AS JobTitleName, jl.Title AS JobLevelTitle, loc.Title AS LocationTitle, pc_mobile.Value AS Party_Mobile, 
                         pc_phone.Value AS Party_Phone, pc_email.Value AS Party_Email, pc_address.Value AS Party_Address, post_phone.Value AS PostContact_Phone, post_mobile.Value AS PostContact_Mobile, 
                         post_email.Value AS PostContact_Email, post_fax.Value AS PostContact_Fax, employment_phone.Value AS EmploymentContact_Phone, employment_mobile.Value AS EmploymentContact_Mobile, 
                         employment_email.Value AS EmploymentContact_Email, employment_fax.Value AS EmploymentContact_Fax
FROM            hr.[ Employment] AS emp INNER JOIN
                         people.naturalPersons AS np ON emp.FkNaturalPersonId = np.Id INNER JOIN
                         people.Parties AS p ON np.FkPartyId = p.Id INNER JOIN
                         hr.Assignments AS ass ON emp.Id = ass.FkEmploymentId AND ass.IsCurrent = 1 INNER JOIN
                         hr.Post AS post ON ass.FkPostId = post.Id LEFT OUTER JOIN
                         hr.OrganizationUnits AS ou ON post.FkOrganizationUnitId = ou.Id LEFT OUTER JOIN
                         hr.JobTitle AS jt ON post.FkJobTitleId = jt.Id LEFT OUTER JOIN
                         hr.JobLevel AS jl ON post.FkJobLevelId = jl.Id LEFT OUTER JOIN
                         hr.[ EmploymentLocations] AS el ON emp.Id = el.FkEmployeeId AND el.IsCurrent = 1 LEFT OUTER JOIN
                         hr.Location AS loc ON el.FkLocationId = loc.Id LEFT OUTER JOIN
                         people.PartyContacts AS pc_mobile ON p.Id = pc_mobile.FkPartyId AND pc_mobile.ContactType = 1 AND pc_mobile.IsCurrent = 1 LEFT OUTER JOIN
                         people.PartyContacts AS pc_phone ON p.Id = pc_phone.FkPartyId AND pc_phone.ContactType = 0 AND pc_phone.IsCurrent = 1 LEFT OUTER JOIN
                         people.PartyContacts AS pc_email ON p.Id = pc_email.FkPartyId AND pc_email.ContactType = 2 AND pc_email.IsCurrent = 1 LEFT OUTER JOIN
                         people.PartyContacts AS pc_address ON p.Id = pc_address.FkPartyId AND pc_address.ContactType = 3 AND pc_address.IsCurrent = 1 LEFT OUTER JOIN
                         hr.PostContacts AS post_phone ON post.Id = post_phone.FkPostId AND post_phone.ContactType = 0 AND post_phone.IsCurrent = 1 LEFT OUTER JOIN
                         hr.PostContacts AS post_mobile ON post.Id = post_mobile.FkPostId AND post_mobile.ContactType = 1 AND post_mobile.IsCurrent = 1 LEFT OUTER JOIN
                         hr.PostContacts AS post_fax ON post.Id = post_fax.FkPostId AND post_fax.ContactType = 2 AND post_fax.IsCurrent = 1 LEFT OUTER JOIN
                         hr.PostContacts AS post_email ON post.Id = post_email.FkPostId AND post_email.ContactType = 3 AND post_email.IsCurrent = 1 LEFT OUTER JOIN
                         hr.EmploymentContacts AS employment_phone ON emp.Id = employment_phone.FkEmploymentId AND employment_phone.ContactType = 0 AND employment_phone.IsCurrent = 1 LEFT OUTER JOIN
                         hr.EmploymentContacts AS employment_mobile ON emp.Id = employment_mobile.FkEmploymentId AND employment_mobile.ContactType = 1 AND employment_mobile.IsCurrent = 1 LEFT OUTER JOIN
                         hr.EmploymentContacts AS employment_fax ON emp.Id = employment_fax.FkEmploymentId AND employment_fax.ContactType = 2 AND employment_fax.IsCurrent = 1 LEFT OUTER JOIN
                         hr.EmploymentContacts AS employment_email ON emp.Id = employment_email.FkEmploymentId AND employment_email.ContactType = 3 AND employment_email.IsCurrent = 1