SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (
SELECT  schema_name
FROM    information_schema.schemata
WHERE   schema_name = 'integrations' ) 
 
BEGIN
EXEC sp_executesql N'CREATE SCHEMA integrations'   
END

GO


CREATE OR ALTER PROCEDURE integrations.sp_ExecAndCommit @ProcName varchar(255), @Params nvarchar(max), @Result nvarchar(max) output, @RaiseError bit = 0, @TimeoutInSeconds int = 5
AS
BEGIN
    declare @RC int

    declare @MbTaskAttachments varbinary(max) = cast(@Params as varbinary(max))

    insert into integrations.MbTasks (MbTaskType, MbTaskCode, MbTaskParams, MbTaskAttachments, MbSynchronousFlag,CreatedBy)
    values('ONE-TIME-REQUEST', 'exec-proc', @ProcName, @MbTaskAttachments, 1,'sp_ExecAndCommit')

    declare @MbTaskId BIGINT = SCOPE_IDENTITY()

    DECLARE @wait_counter INT = 0, @time_out_counter INT = @TimeoutInSeconds * 10;


    WHILE (@wait_counter < @time_out_counter)
    BEGIN
      WAITFOR DELAY '00:00:00.100'
      DECLARE @MbTaskExecutionErrorCode int, @MbResponseValue nvarchar(max) 

      exec integrations.getTaskResponseAndMarkItAsProcessed @MbTaskId = @MbTaskId, @MbTaskExecutionErrorCode = @rc OUTPUT, @MbResponseValue = @Result OUTPUT

      if @rc is not null
      begin
          if @rc <> 0 and @RaiseError = 1 RAISERROR(@Result, 16, 1) WITH NOWAIT
          else break
          return @rc           
      end
    
      SET  @wait_counter = @wait_counter + 1;
    END 

    if (@wait_counter = @time_out_counter) 
    BEGIN 
      set @Result = 'Timeout'
      if @RaiseError = 1 RAISERROR(@Result, 16, 1) WITH NOWAIT
      return -1           
    END 

    return @rc
  
END

GO

