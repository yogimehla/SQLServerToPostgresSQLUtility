create function [dbo].[GetMigrationString] (@value as nvarchar(max))
returns nvarchar(max)
as
begin
	return case 
			when @value is null
				then 'null'
			when @value = ''
				then '""'
			else '"' + replace(replace(replace(@value, '"', '""'),'“','""'),'”','""') + '"'
			end
end;