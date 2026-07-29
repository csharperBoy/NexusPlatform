CREATE VIEW phonebook.PhoneBook_Info_View
AS
SELECT 
    np.NationalCode, 
    np.FirstName, 
    np.LastName, 
    emp.EmployeeCode,
    
    -- اطلاعات سازمانی
    ou.Name AS OrganizationUnitsName,
    jt.Name AS JobTitleName,
    jl.Title AS JobLevelTitle,
    loc.Title AS LocationTitle,
    
    -- تماس‌های شخصی (PartyContacts)
    pc_mobile.Value  AS Party_Mobile,
    pc_phone.Value   AS Party_Phone,
    pc_email.Value   AS Party_Email,
    pc_address.Value AS Party_Address,
    
    -- تماس‌های سازمانی (PostContacts)
    post_phone.Value  AS PostContact_Phone,
    post_mobile.Value AS PostContact_Mobile,
    post_email.Value  AS PostContact_Email,
    post_fax.Value    AS PostContact_Fax

FROM hr.[ Employment] emp
INNER JOIN people.naturalPersons np ON emp.FkNaturalPersonId = np.Id
INNER JOIN people.Parties p ON np.FkPartyId = p.Id

-- انتصاب جاری کارمند (Assignments)
INNER JOIN hr.Assignments ass ON emp.Id = ass.FkEmploymentId AND ass.IsCurrent = 1
INNER JOIN hr.Post post ON ass.FkPostId = post.Id

-- اطلاعات پست و ساختار سازمانی
LEFT OUTER JOIN hr.OrganizationUnits ou ON post.FkOrganizationUnitId = ou.Id
LEFT OUTER JOIN hr.JobTitle jt ON post.FkJobTitleId = jt.Id
LEFT OUTER JOIN hr.JobLevel jl ON post.FkJobLevelId = jl.Id

-- محل خدمت جاری
LEFT OUTER JOIN hr.[ EmploymentLocations] el ON emp.Id = el.FkEmployeeId AND el.IsCurrent = 1
LEFT OUTER JOIN hr.Location loc ON el.FkLocationId = loc.Id

-- تماس‌های شخصی (مستقیم با FkPartyId جوین شده‌اند)
LEFT OUTER JOIN people.PartyContacts pc_mobile  ON p.Id = pc_mobile.FkPartyId  AND pc_mobile.ContactType = 1
LEFT OUTER JOIN people.PartyContacts pc_phone   ON p.Id = pc_phone.FkPartyId   AND pc_phone.ContactType = 0
LEFT OUTER JOIN people.PartyContacts pc_email   ON p.Id = pc_email.FkPartyId   AND pc_email.ContactType = 2
LEFT OUTER JOIN people.PartyContacts pc_address ON p.Id = pc_address.FkPartyId AND pc_address.ContactType = 3

-- تماس‌های سازمانی (مستقیم با post.Id جوین شده‌اند و باگ شرط ON برطرف شد)
LEFT OUTER JOIN hr.PostContacts post_phone  ON post.Id = post_phone.FkPostId  AND post_phone.ContactType = 0
LEFT OUTER JOIN hr.PostContacts post_mobile ON post.Id = post_mobile.FkPostId AND post_mobile.ContactType = 1
LEFT OUTER JOIN hr.PostContacts post_fax    ON post.Id = post_fax.FkPostId    AND post_fax.ContactType = 2
LEFT OUTER JOIN hr.PostContacts post_email  ON post.Id = post_email.FkPostId  AND post_email.ContactType = 3