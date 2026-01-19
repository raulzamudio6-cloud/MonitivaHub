
IF NOT EXISTS (
SELECT  schema_name
FROM    information_schema.schemata
WHERE   schema_name = 'integrations' ) 
 
BEGIN
EXEC sp_executesql N'CREATE SCHEMA integrations'   
END
GO


IF OBJECT_ID('integrations.MbHookTasks_log', 'U') IS NOT NULL 
  DROP TABLE integrations.MbHookTasks_log; 

IF OBJECT_ID('integrations.MbHookTasksListeners', 'U') IS NOT NULL 
  DROP TABLE integrations.MbHookTasksListeners; 


CREATE TABLE integrations.MbHookTasks_log (
    id int identity,
    MbTaskId BIGINT,
    MbHookListenerCode nvarchar(1024) NOT NULL, 
    
    MbHookBusinessID nvarchar(1024) NOT NULL, 
    MbHookParams nvarchar(max) NOT NULL,

    MbHookAttachments varbinary(max) NULL,

    CreatedOn datetime not null default getdate(),
    CreatedBy nvarchar(128) not null
)

CREATE TABLE integrations.MbHookTasksListeners (
    MbHookListenerCode nvarchar(450) NOT NULL PRIMARY KEY,
    MbSQLProcedureName nvarchar(1024) NOT NULL -- Requirements: such procedure needs to have 2 parameters: (MbHookParams nvarchar(max) NOT NULL, MbHookListenerCode nvarchar(1024) NOT NULL)
)




IF EXISTS(SELECT 1 FROM sys.procedures  
          WHERE Name = 'processIntegratorHookTask')
BEGIN
    DROP PROCEDURE integrations.processIntegratorHookTask
END

GO



CREATE OR ALTER PROCEDURE integrations.processIntegratorHookTask
    @MbTaskId BIGINT, 
    @MbHookListenerCode nvarchar(1024), 
    @MbHookBusinessID nvarchar(1024), 
    @MbHookPArAms nvarchar(max) output,
    @MbHookAttachments varbinary(max) output,
    @CreatedOn datetime,
    @CreatedBy nvarchar(128), 
    @ErrorCode int OUTPUT
WITH EXECUTE AS CALLER
AS
begin

SET NOCOUNT ON;


INSERT INTO integrations.MbHookTasks_log (MbTaskId,MbHookListenerCode,MbHookBusinessID, MbHookParams, MbHookAttachments, CreatedOn,CreatedBy)
VALUES (@MbTaskId,@MbHookListenerCode,@MbHookBusinessID,@MbHookParams,@MbHookAttachments,@CreatedOn,@CreatedBy)

DECLARE @MbSQLProcedureName nvarchar(1024) = (SELECT MbSQLProcedureName FROM integrations.MbHookTasksListeners WHERE MbHookListenerCode = @MbHookListenerCode)

if (@MbSQLProcedureName IS NULL)
BEGIN
   print 'Fatal. No Hook listener registered for this MbHookListenerCode code.'
   SELECT @ErrorCode = 101
   RETURN
END


DECLARE @ParmDefinition NVARCHAR(2000) = N'@MbHookBusinessID nvarchar(1024), @MbHookParams nvarchar(max) OUTPUT, @MbHookAttachments varbinary(max) OUTPUT, @ErrorCode int OUTPUT'  
DECLARE @SQLStatement NVARCHAR(2000)= 'EXECUTE '+@MbSQLProcedureName+' @MbHookBusinessID = @MbHookBusinessID,  @MbHookParams=@MbHookParams output, @MbHookAttachments=@MbHookAttachments output,  @ErrorCode=@ErrorCode OUTPUT'
PRINT @SQLStatement 
  
  
begin try
  EXECUTE sp_executesql @SQLStatement,  @ParmDefinition, @MbHookBusinessID = @MbHookBusinessID, @MbHookParams=@MbHookParams output, @MbHookAttachments=@MbHookAttachments output, @ErrorCode=@ErrorCode OUTPUT
end try
begin catch
  declare @err nvarchar(max) = ERROR_MESSAGE()+ ' at '+@MbSQLProcedureName++'..'+ERROR_PROCEDURE() + ' line:'+cast(ERROR_LINE() as varchar)
  raiserror(@err,16,1)
end catch

end

GO


if not exists (select * from integrations.MbHookTasksListeners where MbHookListenerCode = 'integrations')
  insert into integrations.MbHookTasksListeners (MbHookListenerCode,MbSQLProcedureName)
  values ('integrations','spdf_Integrations_Process_Async_Event')

GO

