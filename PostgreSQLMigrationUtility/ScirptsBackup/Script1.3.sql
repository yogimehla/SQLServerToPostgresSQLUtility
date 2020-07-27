IF EXISTS (
    SELECT * FROM sysobjects WHERE id = object_id(N'udf_SpacesForCases') 
    AND xtype IN (N'FN', N'IF', N'TF')
)
begin  
  DROP FUNCTION udf_SpacesForCases
end