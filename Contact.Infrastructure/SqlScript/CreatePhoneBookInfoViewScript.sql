CREATE VIEW contact.PhoneBook_Info_View
AS
SELECT 
    np.NationalCode, 
    np.FirstName, 
    np.LastName, 
    emp.EmploymentCode, 
    ou.Name AS OrganizationUnitsName, 
    jt.Name AS JobTitleName, 
    jl.Title AS JobLevelTitle, 
    emp_loc.Title AS LocationTitle, 
    pc_mobile.Value AS Party_Mobile, 
    pc_phone.Value AS Party_Phone, 
    pc_email.Value AS Party_Email, 
    pc_address.Value AS Party_Address, 
    post_phone.Value AS PostContact_Phone, 
    post_mobile.Value AS PostContact_Mobile, 
    post_email.Value AS PostContact_Email, 
    post_fax.Value AS PostContact_Fax, 
    employment_phone.Value AS EmploymentContact_Phone, 
    employment_mobile.Value AS EmploymentContact_Mobile, 
    employment_email.Value AS EmploymentContact_Email, 
    employment_fax.Value AS EmploymentContact_Fax,

    -- اطلاعات تماس موقعیت مستقیم کارمند
    emp_loc_phone.Value AS EmpLocationContact_Phone,
    emp_loc_mobile.Value AS EmpLocationContact_Mobile,
    emp_loc_fax.Value AS EmpLocationContact_Fax,
    emp_loc_email.Value AS EmpLocationContact_Email,

    -- اطلاعات عنوان و تماس موقعیت پست کارمند
    post_loc.Title AS PostLocationTitle,
    post_loc_phone.Value AS PostLocationContact_Phone,
    post_loc_mobile.Value AS PostLocationContact_Mobile,
    post_loc_fax.Value AS PostLocationContact_Fax,
    post_loc_email.Value AS PostLocationContact_Email

FROM hr.[Employment] AS emp 
INNER JOIN people.naturalPersons AS np ON emp.FkNaturalPersonId = np.Id 
INNER JOIN people.Parties AS p ON np.FkPartyId = p.Id 
INNER JOIN hr.Assignments AS ass ON emp.Id = ass.FkEmploymentId AND ass.IsCurrent = 1 
INNER JOIN hr.Post AS post ON ass.FkPostId = post.Id 
LEFT OUTER JOIN hr.OrganizationUnits AS ou ON post.FkOrganizationUnitId = ou.Id 
LEFT OUTER JOIN hr.JobTitle AS jt ON post.FkJobTitleId = jt.Id 
LEFT OUTER JOIN hr.JobLevel AS jl ON post.FkJobLevelId = jl.Id 

-- موقعیت مستقیم کارمند و اطلاعات تماس آن
LEFT OUTER JOIN hr.EmploymentLocations AS el ON emp.Id = el.FkEmploymentId AND el.IsCurrent = 1 
LEFT OUTER JOIN hr.Location AS emp_loc ON el.FkLocationId = emp_loc.Id 
LEFT OUTER JOIN contact.LocationContacts AS emp_loc_phone ON emp_loc.Id = emp_loc_phone.FkLocationId AND emp_loc_phone.ContactType = 0 AND emp_loc_phone.IsCurrent = 1 
LEFT OUTER JOIN contact.LocationContacts AS emp_loc_mobile ON emp_loc.Id = emp_loc_mobile.FkLocationId AND emp_loc_mobile.ContactType = 1 AND emp_loc_mobile.IsCurrent = 1 
LEFT OUTER JOIN contact.LocationContacts AS emp_loc_fax ON emp_loc.Id = emp_loc_fax.FkLocationId AND emp_loc_fax.ContactType = 2 AND emp_loc_fax.IsCurrent = 1 
LEFT OUTER JOIN contact.LocationContacts AS emp_loc_email ON emp_loc.Id = emp_loc_email.FkLocationId AND emp_loc_email.ContactType = 3 AND emp_loc_email.IsCurrent = 1 

-- موقعیت پست کارمند و اطلاعات تماس آن
LEFT OUTER JOIN hr.PostLocations AS pl ON post.Id = pl.FkPostId AND pl.IsCurrent = 1 
LEFT OUTER JOIN hr.Location AS post_loc ON pl.FkLocationId = post_loc.Id 
LEFT OUTER JOIN contact.LocationContacts AS post_loc_phone ON post_loc.Id = post_loc_phone.FkLocationId AND post_loc_phone.ContactType = 0 AND post_loc_phone.IsCurrent = 1 
LEFT OUTER JOIN contact.LocationContacts AS post_loc_mobile ON post_loc.Id = post_loc_mobile.FkLocationId AND post_loc_mobile.ContactType = 1 AND post_loc_mobile.IsCurrent = 1 
LEFT OUTER JOIN contact.LocationContacts AS post_loc_fax ON post_loc.Id = post_loc_fax.FkLocationId AND post_loc_fax.ContactType = 2 AND post_loc_fax.IsCurrent = 1 
LEFT OUTER JOIN contact.LocationContacts AS post_loc_email ON post_loc.Id = post_loc_email.FkLocationId AND post_loc_email.ContactType = 3 AND post_loc_email.IsCurrent = 1 

-- اطلاعات تماس فرد
LEFT OUTER JOIN contact.PartyContacts AS pc_mobile ON p.Id = pc_mobile.FkPartyId AND pc_mobile.ContactType = 1 AND pc_mobile.IsCurrent = 1 
LEFT OUTER JOIN contact.PartyContacts AS pc_phone ON p.Id = pc_phone.FkPartyId AND pc_phone.ContactType = 0 AND pc_phone.IsCurrent = 1 
LEFT OUTER JOIN contact.PartyContacts AS pc_email ON p.Id = pc_email.FkPartyId AND pc_email.ContactType = 2 AND pc_email.IsCurrent = 1 
LEFT OUTER JOIN contact.PartyContacts AS pc_address ON p.Id = pc_address.FkPartyId AND pc_address.ContactType = 3 AND pc_address.IsCurrent = 1 

-- اطلاعات تماس پست
LEFT OUTER JOIN contact.PostContacts AS post_phone ON post.Id = post_phone.FkPostId AND post_phone.ContactType = 0 AND post_phone.IsCurrent = 1 
LEFT OUTER JOIN contact.PostContacts AS post_mobile ON post.Id = post_mobile.FkPostId AND post_mobile.ContactType = 1 AND post_mobile.IsCurrent = 1 
LEFT OUTER JOIN contact.PostContacts AS post_fax ON post.Id = post_fax.FkPostId AND post_fax.ContactType = 2 AND post_fax.IsCurrent = 1 
LEFT OUTER JOIN contact.PostContacts AS post_email ON post.Id = post_email.FkPostId AND post_email.ContactType = 3 AND post_email.IsCurrent = 1 

-- اطلاعات تماس اشتغال
LEFT OUTER JOIN contact.EmploymentContacts AS employment_phone ON emp.Id = employment_phone.FkEmploymentId AND employment_phone.ContactType = 0 AND employment_phone.IsCurrent = 1 
LEFT OUTER JOIN contact.EmploymentContacts AS employment_mobile ON emp.Id = employment_mobile.FkEmploymentId AND employment_mobile.ContactType = 1 AND employment_mobile.IsCurrent = 1 
LEFT OUTER JOIN contact.EmploymentContacts AS employment_fax ON emp.Id = employment_fax.FkEmploymentId AND employment_fax.ContactType = 2 AND employment_fax.IsCurrent = 1 
LEFT OUTER JOIN contact.EmploymentContacts AS employment_email ON emp.Id = employment_email.FkEmploymentId AND employment_email.ContactType = 3 AND employment_email.IsCurrent = 1 

UNION ALL

-- موقعیت‌هایی که به هیچ کارمند یا پستی متصل نیستند
SELECT 
    NULL AS NationalCode, 
    NULL AS FirstName, 
    NULL AS LastName, 
    NULL AS EmploymentCode, 
     N'محل استقرار'  AS OrganizationUnitsName, 
     N'محل استقرار'  AS JobTitleName, 
    NULL AS JobLevelTitle, 
    standalone_loc.Title AS LocationTitle, 
    NULL AS Party_Mobile, 
    NULL AS Party_Phone, 
    NULL AS Party_Email, 
    NULL AS Party_Address, 
    NULL AS PostContact_Phone, 
    NULL AS PostContact_Mobile, 
    NULL AS PostContact_Email, 
    NULL AS PostContact_Fax, 
    NULL AS EmploymentContact_Phone, 
    NULL AS EmploymentContact_Mobile, 
    NULL AS EmploymentContact_Email, 
    NULL AS EmploymentContact_Fax,

    -- اطلاعات تماس موقعیت مستقل
    loc_phone.Value AS EmpLocationContact_Phone,
    loc_mobile.Value AS EmpLocationContact_Mobile,
    loc_fax.Value AS EmpLocationContact_Fax,
    loc_email.Value AS EmpLocationContact_Email,

    NULL AS PostLocationTitle,
    NULL AS PostLocationContact_Phone,
    NULL AS PostLocationContact_Mobile,
    NULL AS PostLocationContact_Fax,
    NULL AS PostLocationContact_Email

FROM hr.Location AS standalone_loc
LEFT OUTER JOIN contact.LocationContacts AS loc_phone ON standalone_loc.Id = loc_phone.FkLocationId AND loc_phone.ContactType = 0 AND loc_phone.IsCurrent = 1 
LEFT OUTER JOIN contact.LocationContacts AS loc_mobile ON standalone_loc.Id = loc_mobile.FkLocationId AND loc_mobile.ContactType = 1 AND loc_mobile.IsCurrent = 1 
LEFT OUTER JOIN contact.LocationContacts AS loc_fax ON standalone_loc.Id = loc_fax.FkLocationId AND loc_fax.ContactType = 2 AND loc_fax.IsCurrent = 1 
LEFT OUTER JOIN contact.LocationContacts AS loc_email ON standalone_loc.Id = loc_email.FkLocationId AND loc_email.ContactType = 3 AND loc_email.IsCurrent = 1 

WHERE NOT EXISTS (
    SELECT 1 FROM hr.EmploymentLocations AS el WHERE el.FkLocationId = standalone_loc.Id
)
AND NOT EXISTS (
    SELECT 1 FROM hr.PostLocations AS pl WHERE pl.FkLocationId = standalone_loc.Id
)