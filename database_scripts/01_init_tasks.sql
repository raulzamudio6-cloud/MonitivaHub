
IF NOT EXISTS (
SELECT  schema_name
FROM    information_schema.schemata
WHERE   schema_name = 'integrations' ) 
 
BEGIN
EXEC sp_executesql N'CREATE SCHEMA integrations'   
END
GO


IF OBJECT_ID('integrations.MbTasks', 'U') IS NOT NULL 
  DROP TABLE integrations.MbTasks; 


IF OBJECT_ID('integrations.MbTasks_in_process', 'U') IS NOT NULL 
  DROP TABLE integrations.MbTasks_in_process; 

  
IF OBJECT_ID('integrations.MbTasks_archive', 'U') IS NOT NULL 
  DROP TABLE integrations.MbTasks_archive;

IF OBJECT_ID('integrations.MbTasks_in_process_archive', 'U') IS NOT NULL 
  DROP TABLE integrations.MbTasks_in_process_archive;

  
IF OBJECT_ID('integrations.MbTasksResponses', 'U') IS NOT NULL 
  DROP TABLE integrations.MbTasksResponses; 

IF OBJECT_ID('integrations.MbTasksResponses_archive', 'U') IS NOT NULL 
  DROP TABLE integrations.MbTasksResponses_archive; 


CREATE TABLE integrations.MbTasks (

    MbTaskID BIGINT IDENTITY(1,1) PRIMARY KEY,
    MbTaskType nvarchar(32) NOT NULL CHECK (MbTaskType IN('ONE-TIME-REQUEST', 'IN-TRANSACTION-REQUEST')),
    MbTaskCode nvarchar(128) NOT NULL,
    MbTaskParams nvarchar(max),
    MbTaskAttachments varbinary(max),
    MbAttachmentsIsString bit NOT NULL default 1,

    MbSynchronousFlag bit NOT NULL DEFAULT 0, --Task is asynchrorous by default

    ExecuteOn datetime not null default getdate(),
    ExecuteTries int,
    
    CreatedOn datetime not null default getdate(),
    CreatedBy nvarchar(128) not null,

    ValidUntil datetime not null default getdate()+30,
    RetryIntervalSec int default 60,

    Tag varchar(5)
)


CREATE TABLE integrations.MbTasks_in_process (
    MbTaskID BIGINT PRIMARY KEY,
    TaskHandlerInvocationId nvarchar(64),
    CreatedOn datetime not null default getdate(),
    Status int default 0
)


CREATE TABLE integrations.MbTasks_archive (

    MbTaskID BIGINT NOT NULL,
    MbTaskType nvarchar(32) NOT NULL,
    MbTaskCode nvarchar(128) NOT NULL,
    MbTaskParams nvarchar(max),
    MbTaskAttachments varbinary(max),
    MbAttachmentsIsString bit,

    MbSynchronousFlag bit NOT NULL DEFAULT 0, --Task is asynchrorous by default

    CreatedOn datetime not null default getdate(),
    CreatedBy nvarchar(128) not null
)


CREATE TABLE integrations.MbTasks_in_process_archive (
    MbTaskID BIGINT NOT NULL,
    TaskHandlerInvocationId nvarchar(64),
    CreatedOn datetime not null default getdate(),
    Status int 
)


CREATE TABLE integrations.MbTasksResponses (
    MbTaskID BIGINT,
    MbTaskExecutionErrorCode int,
    MbResponseValue nvarchar(max), 
    MbResponseAttachments varbinary(max),

    MbSynchronousFlag bit NOT NULL DEFAULT 0, 

    CreatedOn datetime not null default getdate(),
    CreatedBy nvarchar(128) not null,
    ResponseWriterInvocationId nvarchar(64) default NULL
)


CREATE TABLE integrations.MbTasksResponses_archive (
    MbTaskID BIGINT,
    MbTaskExecutionErrorCode int,
    MbResponseValue nvarchar(max), 
    MbResponseAttachments varbinary(max),

    MbSynchronousFlag bit NOT NULL DEFAULT 0, 

    CreatedOn datetime not null default getdate(),
    CreatedBy nvarchar(128) not null,
    UpdatedOn datetime not null default getdate(),
    UpdatedBy nvarchar(128),
    ResponseWriterInvocationId nvarchar(64) default NULL
)


GO


IF EXISTS(SELECT 1 FROM sys.procedures  
          WHERE Name = 'getNextTaskToBeProcessed')
BEGIN
    DROP PROCEDURE integrations.getNextTaskToBeProcessed
END

GO



CREATE PROCEDURE integrations.getNextTaskToBeProcessed
    @MbTaskID BIGINT OUTPUT,
    @TaskHandlerInvocationId nvarchar(64)
WITH EXECUTE AS CALLER
AS
BEGIN

SET LOCK_TIMEOUT 0;

/*
Gets one-time task and marks it as 'in process'
*/

DECLARE @TASK AS TABLE (
    MbTaskID BIGINT
);


with TTBP as (
        SELECT T2.MbTaskID, T2.ExecuteOn
        FROM integrations.MbTasks T2 WITH (NOLOCK) 
        LEFT JOIN integrations.MbTasks_in_process P WITH (NOLOCK) ON P.MbTaskID = T2.MbTaskID
        WHERE
            T2.MbTaskType = 'ONE-TIME-REQUEST' AND
            P.MbTaskID IS NULL AND
            T2.ExecuteOn  <= getdate() AND
            T2.ValidUntil >= getdate()
        UNION ALL    
        SELECT T2.MbTaskID, T2.ExecuteOn
        FROM integrations.MbTasks T2 WITH (READCOMMITTED) 
        LEFT JOIN integrations.MbTasks_in_process P WITH (NOLOCK) ON P.MbTaskID = T2.MbTaskID
        WHERE
            T2.MbTaskType = 'IN-TRANSACTION-REQUEST' AND 
            P.MbTaskID IS NULL AND
            T2.ExecuteOn  <= getdate() and
            T2.ValidUntil >= getdate()
),
TASK_TO_BE_PROCESSED AS (
    SELECT TOP 1 MbTaskID, ExecuteOn
    FROM TTBP
    ORDER BY ExecuteOn, MbTaskID   
) 
MERGE INTO integrations.MbTasks_in_process AS dest
USING TASK_TO_BE_PROCESSED AS src ON 1=0   -- always false
WHEN NOT MATCHED BY TARGET          -- happens for every row, because 1 is never 0
    THEN INSERT (MbTaskID,TaskHandlerInvocationId)
         VALUES (src.MbTaskID, @TaskHandlerInvocationId)
OUTPUT src.MbTaskID
INTO @TASK;

SELECT @MbTaskID=MbTaskID
FROM  @TASK


END

GO




CREATE OR ALTER PROCEDURE integrations.selectNextTaskToBeProcessed
    @TaskHandlerInvocationId nvarchar(64),
    @Tag varchar(5) = null
WITH EXECUTE AS CALLER
AS
BEGIN

begin transaction

declare @cnt int
select @cnt = count(*) from integrations.MbTasks_in_process (tablockx)

DECLARE @TASK AS TABLE (
    MbTaskID BIGINT,
    MbTaskType nvarchar(32) NOT NULL,
    MbTaskCode nvarchar(128) NOT NULL,
    MbTaskParams nvarchar(max),
    MbTaskAttachments varbinary(max),
    MbAttachmentsIsString bit,
	TTL bigint
)


set LOCK_TIMEOUT -1


;with TTBP as (
        SELECT T2.MbTaskID, T2.MbTaskType, T2.MbTaskCode, T2.MbTaskParams, T2.MbTaskAttachments, T2.MbAttachmentsIsString, T2.ExecuteOn, DATEDIFF_BIG(MILLISECOND,T2.Createdon, T2.ValidUntil) TTL
        FROM integrations.MbTasks T2 WITH (NOLOCK) 
        LEFT JOIN integrations.MbTasks_in_process P WITH (NOLOCK) ON P.MbTaskID = T2.MbTaskID
        WHERE 
            T2.MbTaskType = 'ONE-TIME-REQUEST' AND
            P.MbTaskID IS NULL AND
            T2.ExecuteOn  <= getdate() AND
            T2.ValidUntil >= getdate() AND
            isnull(Tag,'') = isnull(@Tag,'')
        UNION ALL    
        SELECT T2.MbTaskID, T2.MbTaskType, T2.MbTaskCode, T2.MbTaskParams, T2.MbTaskAttachments, T2.MbAttachmentsIsString, T2.ExecuteOn, DATEDIFF_BIG(MILLISECOND,T2.Createdon, T2.ValidUntil) TTL
        FROM integrations.MbTasks T2 WITH (READCOMMITTED) 
        LEFT JOIN integrations.MbTasks_in_process P WITH (NOLOCK) ON P.MbTaskID = T2.MbTaskID
        WHERE
            T2.MbTaskType = 'IN-TRANSACTION-REQUEST' AND 
            P.MbTaskID IS NULL AND
            T2.ExecuteOn  <= getdate() and
            T2.ValidUntil >= getdate() AND
            isnull(Tag,'') = isnull(@Tag,'')
),
TASK_TO_BE_PROCESSED AS (
    SELECT TOP 200 MbTaskID, MbTaskType, MbTaskCode, MbTaskParams, MbTaskAttachments, MbAttachmentsIsString, TTL
    FROM TTBP
    ORDER BY ExecuteOn, MbTaskID   
) 
MERGE INTO integrations.MbTasks_in_process AS dest
USING TASK_TO_BE_PROCESSED AS src ON 1=0   -- always false
WHEN NOT MATCHED BY TARGET          -- happens for every row, because 1 is never 0
    THEN INSERT (MbTaskID,TaskHandlerInvocationId,Status)
         VALUES (src.MbTaskID, @TaskHandlerInvocationId,0)
OUTPUT src.MbTaskID, src.MbTaskType, src.MbTaskCode, src.MbTaskParams, src.MbTaskAttachments, src.MbAttachmentsIsString, src.TTL
INTO @TASK;

SELECT * FROM  @TASK

commit

END

GO

    

CREATE OR ALTER PROCEDURE integrations.UpdateTasksInProcess
    @IDs varchar(max),
    @Status int
WITH EXECUTE AS CALLER
AS
BEGIN
  if @IDs = '' return

  begin transaction
    declare @SQL nvarchar(max)
    if @Status = -1000
      set @SQL = 'delete from integrations.MbTasks_in_process where MbTaskId in ('+@IDs+')'
    else
      set @SQL = 'update integrations.MbTasks_in_process set Status = '+cast(@Status as varchar)+' where MbTaskId in ('+@IDs+')'
    exec sp_executesql @SQL
  commit
END

GO


CREATE OR ALTER PROCEDURE integrations.saveTaskResults
    @MbTaskID BIGINT,
    @MbHookListenerCode nvarchar(max), 
    @MbTaskExecutionErrorCode int,
    @MbResponseValue nvarchar(max), 
    @MbResponseAttachments varbinary(max),
    @CreatedBy nvarchar(128), -- not null,
    @ResponseWriterInvocationId nvarchar(64)-- default NULL

WITH EXECUTE AS CALLER
AS
BEGIN
    declare @ErrorCode int = 0

    declare @MbSynchronousFlag bit
    select @MbSynchronousFlag = MbSynchronousFlag from integrations.MbTasks (nolock) where MbTaskID = @MbTaskID


    if @MbHookListenerCode > '' and  isnull(@MbSynchronousFlag,0) = 0
    begin
	    declare @dt datetime = getdate()
        exec integrations.processIntegratorHookTask
           @MbTaskID = @MbTaskID,
           @MbHookListenerCode = @MbHookListenerCode, 
           @MbHookBusinessID = @MbTaskExecutionErrorCode, 
           @MbHookParams = @MbResponseValue OUTPUT,
           @MbHookAttachments  = @MbResponseAttachments OUTPUT,
           @CreatedOn = @dt,
           @CreatedBy = @CreatedBy, 
           @ErrorCode = @ErrorCode output
    end

    if @MbTaskID > 0
    begin
        if @MbSynchronousFlag = 1 
        begin
          INSERT INTO integrations.MbTasksResponses (MbTaskID, MbTaskExecutionErrorCode, MbResponseValue, MbResponseAttachments, MbSynchronousFlag, CreatedBy, ResponseWriterInvocationId)
          VALUES (@MbTaskID, @MbTaskExecutionErrorCode, @MbResponseValue, @MbResponseAttachments, @MbSynchronousFlag, @CreatedBy, @ResponseWriterInvocationId)
        end
        else
        begin
          INSERT INTO integrations.MbTasksResponses_archive (MbTaskID, MbTaskExecutionErrorCode, MbResponseValue, MbResponseAttachments, MbSynchronousFlag, CreatedBy, ResponseWriterInvocationId)
          VALUES (@MbTaskID, @MbTaskExecutionErrorCode, @MbResponseValue, @MbResponseAttachments, @MbSynchronousFlag, @CreatedBy, @ResponseWriterInvocationId)
        end
    end

	return @ErrorCode
END

GO



IF EXISTS(SELECT 1 FROM sys.procedures  
          WHERE Name = 'getTaskResponseAndMarkItAsProcessed')
BEGIN
    DROP PROCEDURE integrations.getTaskResponseAndMarkItAsProcessed
END

GO

create PROCEDURE integrations.getTaskResponseAndMarkItAsProcessed
    @MbTaskID BIGINT,
    @MbTaskExecutionErrorCode int OUTPUT,
    @MbResponseValue nvarchar(max) OUTPUT,
    @MbResponseAttachments varbinary(max) = NULL OUTPUT
WITH EXECUTE AS CALLER
AS
begin

SET LOCK_TIMEOUT 0;
SET NOCOUNT ON;

    DECLARE @RESPONSE AS TABLE (
        MbTaskID BIGINT ,
        MbTaskExecutionErrorCode int ,
        MbResponseValue nvarchar(max),
        MbResponseAttachments varbinary(max),

        MbSynchronousFlag bit, 

        CreatedOn datetime ,
        CreatedBy nvarchar(128), 
        ResponseWriterInvocationId nvarchar(64)
        )


    DELETE FROM integrations.MbTasksResponses WITH (READPAST)
    OUTPUT [deleted].MbTaskID,[deleted].MbTaskExecutionErrorCode,[deleted].MbResponseValue, [deleted].MbResponseAttachments, [deleted].MbSynchronousFlag, [deleted].CreatedOn,[deleted].CreatedBy, [deleted].ResponseWriterInvocationId
    INTO @RESPONSE
    WHERE MbTaskID = @MbTaskID

    if exists(select 1 from @RESPONSE)
    BEGIN 
        INSERT INTO integrations.MbTasksResponses_archive (MbTaskID,MbTaskExecutionErrorCode,MbResponseValue,MbResponseAttachments,MbSynchronousFlag,CreatedOn,CreatedBy, ResponseWriterInvocationId)
        SELECT MbTaskID,MbTaskExecutionErrorCode,MbResponseValue,MbResponseAttachments,MbSynchronousFlag,CreatedOn,CreatedBy, ResponseWriterInvocationId
        from @RESPONSE

        SELECT @MbTaskExecutionErrorCode=MbTaskExecutionErrorCode,@MbResponseValue=MbResponseValue, @MbResponseAttachments=MbResponseAttachments
        FROM @RESPONSE
    end


end

GO

IF EXISTS(SELECT 1 FROM sys.procedures  
          WHERE Name = 'dataMaintanance_archiveProcessedTasks')
BEGIN
    DROP PROCEDURE integrations.dataMaintanance_archiveProcessedTasks
END

GO

CREATE PROCEDURE integrations.dataMaintanance_archiveProcessedTasks
WITH EXECUTE AS CALLER
AS
begin

SET LOCK_TIMEOUT 0;

    
    -- deleting processed tasks (those that are taken for processing 1+ hour ago).
    -- putting those rows to archive.
    
    DELETE FROM integrations.MbTasks 
    OUTPUT                             [deleted].MbTaskID,[deleted].MbTaskType, [deleted].MbTaskCode,[deleted].MbTaskParams,[deleted].MbTaskAttachments,[deleted].MbSynchronousFlag,[deleted].CreatedOn,[deleted].CreatedBy
    INTO integrations.MbTasks_archive (MbTaskID,           MbTaskType,          MbTaskCode,           MbTaskParams,          MbTaskAttachments,          MbSynchronousFlag,         CreatedOn,           CreatedBy)
    WHERE MbTaskID IN (
         -- Scheduling for deletion tasks that are:
         -- (a) have relevant entry in 'in process' folder.
         -- (b) relevant 'in process' entry is not 'within past 1 hour'  (this constraint is to allow retries for recent tasks).
         SELECT T2.MbTaskID 
         FROM integrations.MbTasks T2 WITH (READPAST)
         INNER JOIN integrations.MbTasks_in_process P ON T2.MbTaskID = P.MbTaskID
         WHERE T2.MbTaskType in ('ONE-TIME-REQUEST', 'IN-TRANSACTION-REQUEST') AND P.CreatedOn < dateadd(hour, -1, getdate()) 
         UNION
         SELECT T2.MbTaskID 
         FROM integrations.MbTasks T2 WITH (READPAST)
         WHERE T2.MbTaskType in ('ONE-TIME-REQUEST', 'IN-TRANSACTION-REQUEST') AND T2.ValidUntil < getdate()

     )
     
    DELETE FROM integrations.MbTasks_in_process
    OUTPUT                                       [deleted].MbTaskID, [deleted].TaskHandlerInvocationId, [deleted].CreatedOn, [deleted].Status
    INTO integrations.MbTasks_in_process_archive(MbTaskID,           TaskHandlerInvocationId,           CreatedOn,           Status)    
    WHERE MbTaskID NOT IN (SELECT MbTaskID FROM integrations.MbTasks T2 WITH (NOLOCK))

  
    
end

GO

IF OBJECT_ID('integrations.ReqRespLog', 'U') IS NULL 
begin

CREATE TABLE integrations.ReqRespLog (

    id int identity,
    sys nvarchar(32),
    thedate datetime,
    req_id nvarchar(32),
    doc int,
    req_type nvarchar(32),
    req nvarchar(max),
    resp nvarchar(max)
)

create index Log_doc on integrations.ReqRespLog(doc)
create index Log_thedate on integrations.ReqRespLog(thedate)
create index Log_reqid on integrations.ReqRespLog(req_id)
create index Log_reqtype on integrations.ReqRespLog(req_type)

end

GO

create or alter procedure integrations.sp_SaveLog 
    @sys nvarchar(32),
    @thedate datetime,
    @req_id nvarchar(32),
    @doc int,
    @req_type nvarchar(32),
    @req nvarchar(max),
    @resp nvarchar(max)
WITH EXECUTE AS CALLER
AS
begin

   insert into integrations.ReqRespLog (sys,thedate,doc,req_id,req_type,req,resp)
   values (@sys,@thedate,@doc,@req_id,@req_type,@req,@resp)

end

GO

  