

--foregin keys-unique-constraints-and-foreign-keys-in-a-sql-server-database-using-tsql/
--Ref. link: https://www.mssqltips.com/sqlservertip/3443/script-all-primary-keys-unique-constraints-and-foreign-keys-in-a-sql-server-database-using-tsql/
--Export all foreign keys
declare @ForeignKeyID int
declare @ForeignKeyName varchar(4000)
declare @ParentTableName varchar(4000)
declare @ParentColumn varchar(4000)
declare @ReferencedTable varchar(4000)
declare @ReferencedColumn varchar(4000)
declare @StrParentColumn varchar(max)
declare @StrReferencedColumn varchar(max)
declare @ParentTableSchema varchar(4000)
declare @ReferencedTableSchema varchar(4000)
declare @TSQLCreationFK varchar(max)


declare @constraints table
(
command varchar(max)
)

select * from include_table_list
--Written by Percy Reyes www.percyreyes.com
declare CursorFK cursor for select object_id--, name, object_name( parent_object_id) 
from sys.foreign_keys
where
( schema_name(schema_id)+'.'+ object_name( parent_object_id) in(select tablename from @TableToInclude)
or (select count(1) from @TableToInclude)=0)  
open CursorFK
fetch next from CursorFK into @ForeignKeyID
while (@@FETCH_STATUS=0)
begin
 set @StrParentColumn=''
 set @StrReferencedColumn=''
 declare CursorFKDetails cursor for
  select  fk.name ForeignKeyName, schema_name(t1.schema_id) ParentTableSchema,
  object_name(fkc.parent_object_id) ParentTable, c1.name ParentColumn,schema_name(t2.schema_id) ReferencedTableSchema,
   object_name(fkc.referenced_object_id) ReferencedTable,c2.name ReferencedColumn
  from --sys.tables t inner join 
  sys.foreign_keys fk 
  inner join sys.foreign_key_columns fkc on fk.object_id=fkc.constraint_object_id
  inner join sys.columns c1 on c1.object_id=fkc.parent_object_id and c1.column_id=fkc.parent_column_id 
  inner join sys.columns c2 on c2.object_id=fkc.referenced_object_id and c2.column_id=fkc.referenced_column_id 
  inner join sys.tables t1 on t1.object_id=fkc.parent_object_id 
  inner join sys.tables t2 on t2.object_id=fkc.referenced_object_id 
  where fk.object_id=@ForeignKeyID
 
 open CursorFKDetails
 fetch next from CursorFKDetails into  @ForeignKeyName, @ParentTableSchema, @ParentTableName, @ParentColumn, @ReferencedTableSchema, @ReferencedTable, @ReferencedColumn
 while (@@FETCH_STATUS=0)
 begin    
  set @StrParentColumn=@StrParentColumn + ', ' + @ParentColumn
  set @StrReferencedColumn=@StrReferencedColumn + ', ' + @ReferencedColumn
  
     fetch next from CursorFKDetails into  @ForeignKeyName, @ParentTableSchema, @ParentTableName, @ParentColumn, @ReferencedTableSchema, @ReferencedTable, @ReferencedColumn
 end
 close CursorFKDetails
 deallocate CursorFKDetails
 --print @StrParentColumn
 --print @StrReferencedColumn
 set @StrParentColumn=substring(@StrParentColumn,2,len(@StrParentColumn))
 set @StrReferencedColumn=substring(@StrReferencedColumn,2,len(@StrReferencedColumn))
 set @TSQLCreationFK='ALTER TABLE '+ dbo.udf_SpacesForCases (case @ParentTableSchema when 'dbo' then 'public' else @ParentTableSchema end) +'.'+ dbo.udf_SpacesForCases (@ParentTableName) +'  ADD CONSTRAINT '+  replace(replace(replace(replace(replace(replace(dbo.udf_SpacesForCases (@ForeignKeyName),'Registration','reg'),'Patient','pat'),'Additional','add'),'Organisation','org') ,'f_k__','fk_'),'__','_') 
 + ' FOREIGN KEY('+ lower(dbo.udf_SpacesForCases(rtrim(ltrim(@StrParentColumn))))+') '+ char(13) +'REFERENCES '+ dbo.udf_SpacesForCases (case @ReferencedTableSchema when 'dbo' then 'public' else @ReferencedTableSchema end) +'.'+dbo.udf_SpacesForCases ( @ReferencedTable)+' ('+ lower(ltrim(dbo.udf_SpacesForCases (rtrim(ltrim(@StrReferencedColumn)))))+');' + char(13)
 --dmd.route
 --if not exists
 --(select * from @TableToInclude where tablename = @ParentTableSchema + '.' + @ParentTableName )
 --or 
 --not exists
 --(select * from @TableToInclude where tablename = @ReferencedTableSchema + '.' + @ReferencedTable )
	--print lower( @TSQLCreationFK)

	if ltrim(@TSQLCreationFK)<>''
	begin
	insert into @constraints
	select @TSQLCreationFK
	end


fetch next from CursorFK into @ForeignKeyID 
end
close CursorFK
deallocate CursorFK

select * from @constraints

