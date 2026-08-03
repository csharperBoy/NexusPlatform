USE [master];
GO

-- اگر خطایی پیش بیاد، کل تراکنش را برگردان
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

BEGIN TRY
    -- =============================================
    -- مرحله ۱: پاک کردن داده‌های جداول مقصد (به ترتیب از فرزند به والد)
    -- =============================================
    -- (چون این جداول ممکن است به JobLevel و JobTitle ارجاع داشته باشند، اول پاک می‌شوند)
 /*   DELETE FROM [AksteelDb].[hr].[IrisaSyncJobLevelMap];
    DELETE FROM [AksteelDb].[hr].[IrisaSyncJobTitleMap];
    DELETE FROM [AksteelDb].[hr].[IrisaSyncOrganizationUnitMap];
    
*/
    -- =============================================
    -- مرحله ۲: درج داده‌ها (به ترتیب از والد به فرزند)
    -- =============================================

    -- ۱. جدول JobLevel (والد)
    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[hr].[JobLevel]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].[hr].[JobLevel] ON;

    INSERT INTO [AksteelDb].[hr].[JobLevel]
    SELECT * FROM [PhoneBookDb].[hr].[JobLevel];  -- اگر همه ستون‌ها دقیقاً یکی هستند

    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[hr].[JobLevel]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].[hr].[JobLevel] OFF;
        
           -- 11. جدول Parties (فرزند JobTitle)
    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[people].[Parties]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].people.Parties ON;

    INSERT INTO [AksteelDb].people.Parties
    SELECT * FROM [PhoneBookDb].people.Parties;

    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[people].Parties'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].people.Parties OFF;
        -- 11. جدول PartyContacts (فرزند JobTitle)
    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[people].[PartyContacts]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].people.PartyContacts ON;

    INSERT INTO [AksteelDb].people.PartyContacts
    SELECT * FROM [PhoneBookDb].people.PartyContacts;

    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[people].PartyContacts'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].people.PartyContacts OFF;
-- 9. جدول naturalPersons (فرزند JobTitle)
    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[people].[naturalPersons]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].people.naturalPersons ON;

    INSERT INTO [AksteelDb].people.naturalPersons
    SELECT * FROM [PhoneBookDb].people.naturalPersons;

    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[people].naturalPersons'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].people.naturalPersons OFF;
-- 10. جدول NaturalPersonProfiles (فرزند JobTitle)
    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[people].[NaturalPersonProfiles]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].people.NaturalPersonProfiles ON;

    INSERT INTO [AksteelDb].people.NaturalPersonProfiles
    SELECT * FROM [PhoneBookDb].people.NaturalPersonProfiles;

    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[people].NaturalPersonProfiles'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].people.NaturalPersonProfiles OFF;

    -- ۲. جدول JobTitle (والد)
    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[hr].[JobTitle]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].[hr].[JobTitle] ON;

    INSERT INTO [AksteelDb].[hr].[JobTitle]
    SELECT * FROM [PhoneBookDb].[hr].[JobTitle];

    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[hr].[JobTitle]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].[hr].[JobTitle] OFF;
 

    -- ۳. جدول IrisaSyncOrganizationUnitMap (معمولاً مستقل است، ولی اگر به جدول دیگری ارجاع دارد، جایگاهش را تغییر دهید)
    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[hr].[IrisaSyncOrganizationUnitMap]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].[hr].[IrisaSyncOrganizationUnitMap] ON;

    INSERT INTO [AksteelDb].[hr].[IrisaSyncOrganizationUnitMap]
    SELECT * FROM [PhoneBookDb].[hr].[IrisaSyncOrganizationUnitMap];

    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[hr].[IrisaSyncOrganizationUnitMap]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].[hr].[IrisaSyncOrganizationUnitMap] OFF;


    -- ۴. جدول IrisaSyncJobLevelMap (فرزند JobLevel)
    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[hr].[IrisaSyncJobLevelMap]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].[hr].[IrisaSyncJobLevelMap] ON;

    INSERT INTO [AksteelDb].[hr].[IrisaSyncJobLevelMap]
    SELECT * FROM [PhoneBookDb].[hr].[IrisaSyncJobLevelMap];

    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[hr].[IrisaSyncJobLevelMap]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].[hr].[IrisaSyncJobLevelMap] OFF;


    -- ۵. جدول IrisaSyncJobTitleMap (فرزند JobTitle)
    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[hr].[IrisaSyncJobTitleMap]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].[hr].[IrisaSyncJobTitleMap] ON;

    INSERT INTO [AksteelDb].[hr].[IrisaSyncJobTitleMap]
    SELECT * FROM [PhoneBookDb].[hr].[IrisaSyncJobTitleMap];

    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[hr].[IrisaSyncJobTitleMap]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].[hr].[IrisaSyncJobTitleMap] OFF;


         -- 6. جدول OrganizationUnits (فرزند JobTitle)
    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[hr].[OrganizationUnits]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].[hr].[OrganizationUnits] ON;

    INSERT INTO [AksteelDb].[hr].OrganizationUnits
    SELECT * FROM [PhoneBookDb].[hr].OrganizationUnits;

    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[hr].[OrganizationUnits]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].[hr].[OrganizationUnits] OFF;

     -- 7. جدول Post (فرزند JobTitle)
    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[hr].[Post]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].[hr].[Post] ON;

    INSERT INTO [AksteelDb].[hr].Post
    SELECT * FROM [PhoneBookDb].[hr].Post;

    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[hr].[Post]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].[hr].Post OFF;

        
     -- 9. جدول Employment (فرزند JobTitle)
    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[hr].[ Employment]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].[hr].[ Employment] ON;

    INSERT INTO [AksteelDb].[hr].[ Employment]
    SELECT * FROM [PhoneBookDb].[hr].[ Employment];

    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[hr].[ Employment]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].[hr].[ Employment] OFF;
    
 -- 8. جدول Assignments (فرزند JobTitle)
    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[hr].[Assignments]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].[hr].Assignments ON;

    INSERT INTO [AksteelDb].[hr].Assignments
    SELECT * FROM [PhoneBookDb].[hr].Assignments;

    IF OBJECTPROPERTY(OBJECT_ID('[AksteelDb].[hr].[Assignments]'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [AksteelDb].[hr].Assignments OFF;
      /*  */
    -- =============================================
    -- اعمال تراکنش در صورت موفقیت
    -- =============================================
    COMMIT TRANSACTION;
    PRINT '✅ کلیه داده‌ها با موفقیت کپی شدند.';

END TRY
BEGIN CATCH
    -- =============================================
    -- برگشت همه تغییرات در صورت بروز خطا
    -- =============================================
    ROLLBACK TRANSACTION;
    PRINT '❌ خطایی رخ داد! تراکنش برگشت خورد.';
    THROW;  -- خطای اصلی را نمایش بده
END CATCH;
GO