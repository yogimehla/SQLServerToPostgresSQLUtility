create function udf_SpacesForCases (@string nvarchar(MAX))
returns nvarchar(MAX)
as
begin
	declare @len int = LEN(@string)
		,@iterator int = 2 --Don't put space to left of first even if it's a capital
		;

	while @iterator <= LEN(@string)
	begin
		if PATINDEX('[ABCDEFGHIJKLMNOPQRSTUVWXYZ]', SUBSTRING(@string, @iterator, 1) COLLATE Latin1_General_CS_AI) <> 0
		begin
			set @string = STUFF(@string, @iterator, 0, '_');
			set @iterator += 1;
		end;

		set @iterator += 1;
	end

	return @string;
end;