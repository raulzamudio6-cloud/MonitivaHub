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


IF EXISTS(SELECT 1 FROM sys.procedures  
          WHERE Name = 'integrations_httpAnyRequest')
BEGIN
    DROP PROCEDURE integrations.integrations_httpAnyRequest
END

IF EXISTS(SELECT 1 FROM sys.procedures  
          WHERE Name = 'integrations_httpAnyRequest_binary')
BEGIN
    DROP PROCEDURE integrations.integrations_httpAnyRequest_binary
END


GO

create PROCEDURE integrations.integrations_httpAnyRequest_binary
    @optionsXML [nvarchar](max),
    @headersXML [nvarchar](max),
    @bodyData [nvarchar](max),
    @responseCode [nvarchar](max) OUTPUT,
    @responseData [varbinary](max) OUTPUT,
    @TimeoutInSeconds int = 30
WITH EXECUTE AS CALLER
AS
begin

SET NOCOUNT ON

declare @MbTaskParams [nvarchar](max) =
(SELECT 'httpAnyRequest' as HttpUtilsRequestType, @optionsXML as optionsXML, @headersXML as headersXML, @bodyData as bodyData FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER  )


declare @MbTaskId_table table (MbTaskId BIGINT)

INSERT INTO integrations.[MbTasks]
           (
            MbTaskType
           ,MbTaskCode
           ,MbTaskParams
           ,MbSynchronousFlag
           ,CreatedBy
       )
     OUTPUT inserted.MbTaskId into @MbTaskId_table 
     VALUES 
     ('ONE-TIME-REQUEST',
     'http-utils',
    @MbTaskParams,
    1, -- =true. Meaning: task is synchronous
    'HTTPUtils.integrations_httpAnyRequest')

declare @MbTaskId BIGINT = (select TOP 1 MbTaskId FROM @MbTaskId_table)

DECLARE @wait_counter INT = 0, @time_out_counter INT = @TimeoutInSeconds * 10;


WHILE (@wait_counter < @time_out_counter)
 BEGIN
  WAITFOR DELAY '00:00:00.100'
    DECLARE @MbTaskExecutionErrorCode int, @MbResponseValue nvarchar(max) 

    exec integrations.getTaskResponseAndMarkItAsProcessed @MbTaskId = @MbTaskId, @MbTaskExecutionErrorCode = @MbTaskExecutionErrorCode OUTPUT, @MbResponseValue = @MbResponseValue OUTPUT, 
	  @MbResponseAttachments = @responseData output
  
   if @MbTaskExecutionErrorCode is not null
    begin
       if (@MbTaskExecutionErrorCode = 0)
       begin
           -- error code= 0
		   select @responseCode=@MbResponseValue
		   
           PRINT '**** RESPONSE RECEIVED. @responseCode='+ISNULL(@responseCode, 'null') 
           break
        end
        else
        begin
           --SELECT @responseCode = NULL
           --SELECT @responseData = null
           
            declare @error_description nvarchar(1024)='Integration call returned error: '+'@MbTaskExecutionErrorCode='+ISNULL(cast(@MbTaskExecutionErrorCode as nvarchar(10)), 'null') +'|@MbTaskId='+ISNULL(cast(@MbTaskId AS nvarchar(10)), 'null')+'|@Error='+ISNULL(cast(@MbResponseValue AS nvarchar(512)), 'null')
            RAISERROR(@error_description, 15, 1)
           break
        end 
    end
    
  SET  @wait_counter = @wait_counter + 1;
END 

if (@wait_counter = @time_out_counter) 
BEGIN 
PRINT '**** Exit due to time-out ****' 
RAISERROR('Timeout', 15, 2)           
END 
end

GO



create PROCEDURE integrations.integrations_httpAnyRequest
    @optionsXML [nvarchar](max),
    @headersXML [nvarchar](max),
    @bodyData [nvarchar](max),
    @responseCode [nvarchar](max) OUTPUT,
    @responseData [nvarchar](max) OUTPUT,
    @TimeoutInSeconds int = 30
WITH EXECUTE AS CALLER
AS
begin
 
DECLARE @RC int  
DECLARE @responseData_binary [varbinary](max)

exec @RC = integrations.[integrations_httpAnyRequest_binary] 
@optionsXML, @headersXML, @bodyData, @responseCode OUTPUT, @responseData_binary output, @TimeoutInSeconds

SELECT @responseData =cast(cast('' as xml).value('xs:base64Binary(sql:variable("@responseData_binary"))', 'VARBINARY(max)') as nvarchar(max))

end

GO

