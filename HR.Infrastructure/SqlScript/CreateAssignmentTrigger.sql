CREATE TRIGGER trg_Assignments_CheckOverlap
ON hr.Assignments
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (
        SELECT 1
        FROM hr.Assignments AS a
        INNER JOIN inserted AS i
            ON a.FkPostId = i.FkPostId
            AND a.AssigneeType = i.AssigneeType
            AND a.Id <> i.Id
        WHERE
            (i.EffectiveFrom <= a.EffectiveTo OR a.EffectiveTo IS NULL)
            AND (a.EffectiveFrom <= i.EffectiveTo OR i.EffectiveTo IS NULL)
    )
    BEGIN
        RAISERROR('تداخل زمانی: این پست با نوع انتصاب مشخص، در بازه مورد نظر توسط فرد دیگری اشغال شده است.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END