SELECT 
	d.definition as sourceCode,
	p.name as name,
	('##DBNAME##' + '.' + s.name) as groupName,
	'##DBNAME##' as databaseName,
	s.name as schemaName
FROM 
	[##DBNAME##].sys.procedures p
	INNER JOIN
	[##DBNAME##].sys.schemas s on p.schema_id = s.schema_id
	INNER JOIN
	[##DBNAME##].sys.sql_modules d on p.object_id = d.object_id

--split

SELECT
	d2.definition as sourceCode,
	v.name as name,
	('##DBNAME##' + '.' + s2.name) as groupName,
	'##DBNAME##' as databaseName
        -- ,
	-- toto pada          s.name as schemaName
FROM
	[##DBNAME##].sys.views v
	INNER JOIN
		[##DBNAME##].sys.schemas s2 on v.schema_id = s2.schema_id
	INNER JOIN
		[##DBNAME##].sys.sql_modules d2 on v.object_id = d2.object_id

