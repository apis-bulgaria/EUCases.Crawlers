USE [master]
GO

IF EXISTS(SELECT * FROM sys.databases WHERE name='CrawlerFramework')
DROP DATABASE CrawlerFramework
GO

CREATE DATABASE CrawlerFramework
GO
 ALTER DATABASE CrawlerFramework MODIFY FILE 
( NAME = N'CrawlerFramework', SIZE = 8GB , MAXSIZE = UNLIMITED, FILEGROWTH = 20%)
GO
 ALTER DATABASE CrawlerFramework MODIFY FILE 
( NAME = N'CrawlerFramework_log', SIZE = 100MB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO

USE CrawlerFramework
GO

/* Crawlers */
IF OBJECT_ID('Crawlers') IS NOT NULL
BEGIN
  EXEC p_ak_drop_all_foreign_keys 'Crawlers';
  DROP TABLE Crawlers
END
GO

CREATE TABLE Crawlers
(
  CrawlerId INT IDENTITY(1,1) NOT NULL
  ,CrawlerName NVARCHAR(MAX)
  ,CONSTRAINT pk_CrawlerId PRIMARY KEY (CrawlerId)
)
GO

/* DocumentGroups */
IF OBJECT_ID('DocumentGroups') IS NOT NULL
BEGIN
  EXEC p_ak_drop_all_foreign_keys 'DocumentGroups';
  DROP TABLE DocumentGroups
END
GO

CREATE TABLE DocumentGroups
(
  DocumentGroupId INT IDENTITY(1,1) NOT NULL
  ,DocumentGroupDate NVARCHAR(100)
  ,Lang NVARCHAR(50) 
  ,GroupType INT
  ,DocumentGroupFormat NVARCHAR(100)
  ,DocumentGroupName NVARCHAR(250) 
  ,Identifier NVARCHAR(50)
  ,Operation INT
  ,DataContent VARBINARY(MAX)
  ,CrawlerId INT
  ,CONSTRAINT pk_DocumentGroupId PRIMARY KEY (DocumentGroupId)
  ,CONSTRAINT fk_DocumentGroups_Crawlers FOREIGN KEY (CrawlerId)
  REFERENCES Crawlers(CrawlerId)
)
GO

CREATE INDEX idx_DocumentGroups_CrawlerId ON DocumentGroups (CrawlerId)
CREATE UNIQUE INDEX uk_Identifier ON DocumentGroups (Identifier)

/* Documents */
IF OBJECT_ID('Documents') IS NOT NULL
BEGIN
  EXEC p_ak_drop_all_foreign_keys 'Documents';
  DROP TABLE Documents
END
GO

CREATE TABLE Documents
(
  DocumentId INT IDENTITY(1,1) NOT NULL
  ,DocumentFormat NVARCHAR(100) 
  ,DocumentName NVARCHAR(250) 
  ,Identifier NVARCHAR(50) 
  ,Operation INT 
  ,Url NVARCHAR(MAX) 
  ,Md5 NVARCHAR(50) 
  ,DataContent VARBINARY(MAX)
  ,DocumentOrder INT
  ,DocumentGroupId INT
  ,CONSTRAINT pk_DocumentId PRIMARY KEY (DocumentId)
  ,CONSTRAINT fk_Documents_DocumentGroups FOREIGN KEY (DocumentGroupId)
  REFERENCES DocumentGroups(DocumentGroupId) ON DELETE CASCADE
)
GO

CREATE INDEX idx_Documents_DocumentGroupId ON Documents (DocumentGroupId)

/* CrawlerLogs */
IF OBJECT_ID('CrawlerLogs') IS NOT NULL
BEGIN
  EXEC p_ak_drop_all_foreign_keys 'CrawlerLogs';
  DROP TABLE CrawlerLogs
END
GO

CREATE TABLE CrawlerLogs
(
  CrawlerLogId INT IDENTITY(1,1) NOT NULL
  ,LogDate DATETIME2
  ,IpAddress NVARCHAR(50)
  ,MetaXml NVARCHAR(MAX) 
  ,ZipLength INT
  ,Operation INT
  ,IsSuccess bit
  ,Error NVARCHAR(MAX)
  ,CrawlerName NVARCHAR(300)
  ,Identifier NVARCHAR(50)
  ,CONSTRAINT pk_CrawlerLogId PRIMARY KEY (CrawlerLogId)
)
GO

CREATE INDEX idx_CrawlerLogs_LogDate ON CrawlerLogs (LogDate DESC) 

IF OBJECT_ID('p_DeleteDocumentGroup') is not null 
  DROP PROCEDURE p_DeleteDocumentGroup 
GO
CREATE PROCEDURE p_DeleteDocumentGroup @identifier NVARCHAR (50) AS 
BEGIN

DELETE  FROM DocumentGroups 
WHERE Identifier = @identifier
END
GO

if OBJECT_ID('p_ak_drop_all_foreign_keys') is not null 
  DROP PROCEDURE p_ak_drop_all_foreign_keys 
GO

CREATE PROCEDURE p_ak_drop_all_foreign_keys @tableName NVARCHAR (MAX) as 
BEGIN
  CREATE TABLE #statements 
  ( 
    STATEMENT NVARCHAR (MAX) 
  ) 

  ;WITH ForeignKeyInfo as 
  ( 
    SELECT 
      OBJECT_NAME(f.parent_object_id) AS TableName, 
      f.name as fkname, 
      OBJECT_NAME (f.referenced_object_id) AS ReferenceTableName 
    FROM sys.foreign_keys AS f 
    WHERE OBJECT_NAME (f.referenced_object_id) = @tableName 
  ) 

  INSERT INTO #statements (STATEMENT) 
    SELECT 
      'alter TABLE '+TableName+' DROP constraint '+fkname 
    FROM ForeignKeyInfo 

  DECLARE @q NVARCHAR (MAX) 
  DECLARE crsStatements CURSOR FOR SELECT STATEMENT FROM #statements 
  OPEN crsStatements 
  FETCH NEXT FROM crsStatements INTO @q 
  WHILE @@FETCH_STATUS=0 
  BEGIN 
    EXECUTE (@q) 
    FETCH next FROM crsStatements INTO @q 
  END 
  CLOSE crsStatements 
  DEALLOCATE crsStatements 
  DROP TABLE #statements 
END
GO

IF OBJECT_ID('p_ak_create_fk_indeces') IS NOT NULL
  DROP PROCEDURE p_ak_create_fk_indeces
GO

-- _p_create_fk_indexes 'COURTS'
CREATE PROCEDURE p_ak_create_fk_indeces @tableName NVARCHAR (MAX) AS
BEGIN
  CREATE TABLE #statements 

  ( 
    STATEMENT NVARCHAR (MAX) 
  ) 

  ;WITH ForeignKeyInfo AS 

  ( 
    SELECT 
      OBJECT_NAME(f.parent_object_id) AS TableName, 
      COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName, 
      OBJECT_NAME (f.referenced_object_id) AS ReferenceTableName, 
      COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferenceColumnName, 
      'idx_'+OBJECT_NAME(f.parent_object_id)+'_'+COL_NAME(fc.parent_object_id, fc.parent_column_id) IndexName 
    FROM sys.foreign_keys AS f 
    INNER JOIN sys.foreign_key_columns AS fc ON f.OBJECT_ID = fc.constraint_object_id 
    WHERE OBJECT_NAME (f.parent_object_id) = @tableName 
  ) 

  INSERT INTO #statements (statement) 
    SELECT 
    'if exists (SELECT * FROM sysindexes WHERE id=object_id('''+TableName+''') and name='''+Indexname+''') '+ 
    'DROP index '+indexname+' on '+tablename+'; '+ 
    'CREATE index '+indexname+' on '+tablename+'('+columnname+'); ' FROM ForeignKeyInfo 

  DECLARE @q NVARCHAR (max) 
  DECLARE crsStatements cursor for SELECT statement FROM #statements 
  OPEN crsStatements 
  FETCH next FROM crsStatements INTO @q 
  WHILE @@FETCH_STATUS=0 
  BEGIN 
  --print @q
    EXECUTE (@q) 
    FETCH next FROM crsStatements INTO @q 
  END 
  CLOSE crsStatements 
  DEALLOCATE crsStatements 
  DROP TABLE #statements 
END
GO
