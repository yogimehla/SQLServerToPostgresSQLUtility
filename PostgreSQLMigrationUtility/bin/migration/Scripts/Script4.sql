declare @query1 varchar(MAX)
declare @query2 varchar(MAX)
declare @table varchar(MAX)
declare @row int = 0
declare @bcptable table
(
bcp varchar(max)
)

declare @databaseName varchar(100)
declare @folderPath varchar(100)= {Path}
set @databaseName = {DBName}


while exists (
		select top 1 rownum
		from tempQueries
		)
begin
	select top 1 @query1 = col2
		,@query2 = col2
		,@row = rownum
		,@table = col3
	from tempQueries

	declare @sql varchar(8000) = ''

	select @sql = 'bcp "' + @query2 + '" queryout "' + @folderPath + replace( replace( dbo.udf_SpacesForCases( @table),'._','.'),'"','') + '.csv" -c -t~ -S "' + @@servername + '" -U ' + '"{User}"' + ' -P ' + '"{Pass}"' + ' -d ' + '"{DB}"'

	insert into @bcptable
	select @sql
	--print @sql

	--exec master..xp_cmdshell @sql

	delete
	from tempQueries
	where rownum = @row
end
select * from @bcptable
