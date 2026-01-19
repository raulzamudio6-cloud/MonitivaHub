

IF NOT EXISTS (
SELECT  schema_name
FROM    information_schema.schemata
WHERE   schema_name = 'integrations' ) 
 
BEGIN
EXEC sp_executesql N'CREATE SCHEMA integrations'   
END
GO

IF EXISTS(SELECT 1 FROM sys.procedures  
          WHERE Name = 'remove_this_SendSampleEMailMessage')
BEGIN
    DROP PROCEDURE integrations.remove_this_SendSampleEMailMessage
END

GO 
create PROCEDURE integrations.remove_this_SendSampleEMailMessage
WITH EXECUTE AS CALLER
AS
begin

    DECLARE @RC int
    DECLARE @EmailTo nvarchar(255)='yeremenko@canopuslab.com'
    DECLARE @EMailCC nvarchar(255)
    DECLARE @EMailBCC nvarchar(255)
    DECLARE @Subject nvarchar(255)='LOL'
    DECLARE @Message nvarchar(max)='OLOLO'
    DECLARE @AttachFileName nvarchar(255)

    -- TODO: Set parameter values here.

    EXECUTE @RC = integrations.integrations_SendMailMessage
       @EmailTo
      ,@EMailCC
      ,@EMailBCC
      ,@Subject
      ,@Message
      ,@AttachFileName

END

GO


declare @MbSchedulerID_table table (MbSchedulerID INT)


INSERT INTO [integrations].[MbSchedulerTasks]
           ([MbSchedulerType]
           ,[MbSchedulerCode]
           ,[MbSchedulerName]
           ,[MbSchedulerCommand]
           ,[Enabled]
           ,[ActiveStartDate_YYYYMMDD]
           ,[ActiveEndDate_YYYYMMDD]
           ,[FrequencyType]
           ,[ActiveStartTime_HHMMSS]
           ,[ActiveEndTime_HHMMSS]
           ,[IntervalInSecondsBetweenRuns]
           ,[CreatedOn]
           ,[CreatedBy]
           ,[UpdatedOn]
           ,[UpdatedBy])
     OUTPUT inserted.MbSchedulerID into    @MbSchedulerID_table
     VALUES
           ('CALL-SQL-PROCEDURE'
           ,'EVERY_5_SECONDS'
           ,'Действие повторяется каждые 5 секунд'
           ,'integrations.remove_this_SendSampleEMailMessage'
           ,1
           ,FORMAT(getdate(), 'yyyyMMdd')
           ,'99991231'
           ,'EVERY-FEW-SECONDS'
           ,'090000'
           ,'200000'
           ,5
           ,GETDATE()
           ,'Ivan Yeremenko'
           ,getdate()
           ,'Ivan Yeremenko')

declare @MbSchedulerID BIGINT = (select TOP 1 MbSchedulerID FROM @MbSchedulerID_table)


PRINT '**** SCHEDULE CREATED. MbSchedulerID = '+ISNULL(cast(@MbSchedulerID as nvarchar(10)), 'null') 


GO

DECLARE @MbSchedulerID int, @MbSchedulerType nvarchar(16), @MbSchedulerCommand nvarchar(max), @TaskHandlerInvocationId nvarchar(64) = 'Manual'

EXECUTE integrations.getOneScheduleDueAndMarkItForProcessing
 @MbSchedulerID out,
 @MbSchedulerType out, 
 @MbSchedulerCommand out, 
 @TaskHandlerInvocationId 

select  @MbSchedulerID, @MbSchedulerType, @MbSchedulerCommand

