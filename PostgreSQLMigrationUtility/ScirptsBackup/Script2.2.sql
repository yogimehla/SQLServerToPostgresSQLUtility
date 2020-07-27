declare @databaseName varchar(100)

set @databaseName = {DBName}

--Table script
select cast('create table ' + (
			lower(case TABLE_SCHEMA
						when 'dbo'
							then 'public'
						else  dbo.udf_SpacesForCases(TABLE_SCHEMA)
						end + '.' + dbo.udf_SpacesForCases(table_name))
			) + char(13) + '('  + STUFF((
				select ', ' + char(13) + lower( char(9) + dbo.udf_SpacesForCases (
							case column_name
								when 'order'
									then '"order"'
								when 'default'
									then '"default"'
								when 'offset'
									then '"offset"'
								else column_name
								end
							) + (
							case 
								when DATA_TYPE in (
										'nvarchar'
										,'varchar'
										,'char'
										,'nchar'
										)
									and CHARACTER_MAXIMUM_LENGTH <> - 1
									and CHARACTER_MAXIMUM_LENGTH < 8000
									then ' varchar(' + cast(isnull(CHARACTER_MAXIMUM_LENGTH + 100, 8000) as varchar(10)) + ')'
								when DATA_TYPE in (
										'nvarchar'
										,'text'
										,'varchar'
										,'char'
										,'nchar'
										,'ntext'
										)
									or CHARACTER_MAXIMUM_LENGTH = - 1
									then ' text'
								when DATA_TYPE = 'decimal'
									then ' Numeric(' + cast(NUMERIC_PRECISION as varchar(2)) + ',' + cast(numeric_scale as varchar(2)) + ')'
								when DATA_TYPE = 'bit'
									then ' Boolean' + iif(COLUMN_default = '((0))', ' default false', iif(COLUMN_default = '((1))', ' default true', ''))
								when DATA_TYPE = 'tinyint'
									then ' smallint' + iif(COLUMN_default is not null, ' default ' + replace(replace(COLUMN_default, '((', ''), '))', ''), '')
								when DATA_TYPE = 'date'
									then ' date'
								when DATA_TYPE = 'datetime'
									or DATA_TYPE = 'datetime2'
									or DATA_TYPE = 'datetimeoffset'
									then ' timestamptz'
								when DATA_TYPE = 'timestamp'
									then ' bytea'
								when DATA_TYPE = 'uniqueidentifier'
									then ' UUID'
								when DATA_TYPE in (
										'money'
										,'smallmoney'
										)
									then ' Numeric(8,2)'
								when DATA_TYPE in (
										'binary'
										,'varbinary'
										,'image'
										)
									then ' bytea'
								else ' ' + case COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'IsIdentity')
										when 1
											then 'serial'
										else DATA_TYPE
										end
								end + iif(IS_Nullable = 'NO', ' not null', ' null') + iif(COLUMN_default = '(newid())', ' default uuid_generate_v1()', '')
							))
				from INFORMATION_SCHEMA.COLUMNS
				where (
						table_name = Results.table_name
						and TABLE_SCHEMA = Results.TABLE_SCHEMA
						)
				for xml PATH('')
					,TYPE
				).value('(./text())[1]', 'VARCHAR(MAX)'), 1, 2, '') + dbo.getuqpkData(TABLE_SCHEMA + '.' + table_name)+ char(13) + ');' + char(13) + char(13) as xml)
from INFORMATION_SCHEMA.COLUMNS Results
where OBJECT_ID(TABLE_SCHEMA + '.' + table_name) not in (
		select object_id
		from sys.views
		)
	and TABLE_SCHEMA + '.' + table_name in (
		select tablename
		from include_table_list
		)
	or (
		select count(1)
		from include_table_list
		) = 0
group by Results.TABLE_SCHEMA
	,table_name
for xml PATH('')