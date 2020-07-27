IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE  TABLE_NAME = 'include_table_list'))
BEGIN
	DROP TABLE include_table_list
END