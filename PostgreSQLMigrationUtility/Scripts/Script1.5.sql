IF EXISTS (
    SELECT * FROM sysobjects WHERE id = object_id(N'getuqpkData') 
    AND xtype IN (N'FN', N'IF', N'TF')
) begin
    DROP FUNCTION getuqpkData
	end