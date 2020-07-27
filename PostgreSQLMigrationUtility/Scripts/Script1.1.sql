IF EXISTS (
    SELECT * FROM sysobjects WHERE id = object_id(N'GetMigrationString') 
    AND xtype IN (N'FN', N'IF', N'TF')
)begin
    DROP FUNCTION GetMigrationString end
