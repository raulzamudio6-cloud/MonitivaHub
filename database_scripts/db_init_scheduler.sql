IF NOT EXISTS (
SELECT  schema_name
FROM    information_schema.schemata
WHERE   schema_name = 'integrations' ) 
 
BEGIN
EXEC sp_executesql N'CREATE SCHEMA integrations'   
END
GO

IF OBJECT_ID('integrations.MbSchedulerTasks_RunHistory', 'U') IS NOT NULL 
  DROP TABLE integrations.MbSchedulerTasks_RunHistory; 

  
IF OBJECT_ID('integrations.MbSchedulerTasks', 'U') IS NOT NULL 
  DROP TABLE integrations.MbSchedulerTasks; 




CREATE TABLE integrations.MbSchedulerTasks (

    MbSchedulerID INT IDENTITY(1,1) PRIMARY KEY,

    MbSchedulerType nvarchar(128) NOT NULL CHECK (MbSchedulerType IN('CALL-SQL-PROCEDURE')),
    MbSchedulerCode nvarchar(128),
    MbSchedulerName nvarchar(1024),
    MbSchedulerCommand nvarchar(max),
    
    Enabled bit NOT NULL DEFAULT 1, --scheduler is enabled by default

    ActiveStartDate_YYYYMMDD char(8) not null default FORMAT(getdate(), 'yyyyMMdd'),
    ActiveEndDate_YYYYMMDD char(8) not null default '99991231',

    FrequencyType nvarchar(128) NOT NULL CHECK (FrequencyType IN('ONCE-A-DAY','EVERY-FEW-SECONDS')),
    ActiveStartTime_HHMMSS char(6) not null default  '000000',
    ActiveEndTime_HHMMSS char(8) not null default '235959',

    IntervalInSecondsBetweenRuns int default null,  -- IF FrequencyType = ONCE-A-DAY then IntervalInSecondsBetweenRuns SHOULD BE NULL

    CreatedOn datetime not null default getdate(),
    CreatedBy nvarchar(128) not null,
    UpdatedOn datetime not null default getdate(),
    UpdatedBy nvarchar(128),
)

CREATE TABLE integrations.MbSchedulerTasks_RunHistory (

    MbSchedulerID INT NOT NULL,

    EventType nvarchar(16) not null CHECK (EventType IN('TASK-START','TASK-FINISH')),

    DurationInSeconds BIGINT null,
    ErrorCode INT null,
    ErrorDetails nvarchar(max) null,


    CreatedOn datetime not null default getdate(),
    CreatedBy nvarchar(128) not null,
    TaskHandlerInvocationId nvarchar(64),

    CONSTRAINT FK_SchedulerHistory FOREIGN KEY (MbSchedulerID)
        REFERENCES integrations.MbSchedulerTasks (MbSchedulerID)
)
CREATE INDEX i_MbSchedulerID_EventType_CreatedOn ON integrations.MbSchedulerTasks_RunHistory (MbSchedulerID, EventType, CreatedOn);





IF EXISTS(SELECT 1 FROM sys.procedures  
          WHERE Name = 'getOneScheduleDueAndMarkItForProcessing')
BEGIN
    DROP PROCEDURE integrations.getOneScheduleDueAndMarkItForProcessing
END

GO



create PROCEDURE integrations.getOneScheduleDueAndMarkItForProcessing
    @MbSchedulerID INT OUTPUT,
    @MbSchedulerType nvarchar(128) OUTPUT,
    @MbSchedulerCommand nvarchar(max) OUTPUT,

    @TaskHandlerInvocationId nvarchar(64)
WITH EXECUTE AS CALLER
AS
begin

    /*
    Gets one task that needs to run and marks it as "process-started"
    */

    SET LOCK_TIMEOUT 0
    
    declare @MbSchedulerID_table table (MbSchedulerID INT);

    WITH RecentRunHistory_VIEW 
    (MbSchedulerID, FrequencyType, IntervalInSecondsBetweenRuns, RecentRunTime)
    AS
    (
        SELECT T.MbSchedulerID, T.FrequencyType, T.IntervalInSecondsBetweenRuns, H.CreatedOn as RecentRunTime
        FROM 
        integrations.MbSchedulerTasks T (NOLOCK) 
        LEFT JOIN integrations.MbSchedulerTasks_RunHistory H (READPAST) 
            ON T.MbSchedulerID = H.MbSchedulerID 
                AND H.EventType = 'TASK-START'
                AND H.CreatedOn > dateadd(day, -1, getdate())
        WHERE T.Enabled = 1 
                AND FORMAT(getdate(), 'yyyyMMdd') between T.ActiveStartDate_YYYYMMDD and T.ActiveEndDate_YYYYMMDD
                AND FORMAT(getdate(), 'HHmmss') between T.ActiveStartTime_HHMMSS and T.ActiveEndTime_HHMMSS
                AND T.MbSchedulerType='CALL-SQL-PROCEDURE'
    )
    INSERT INTO integrations.MbSchedulerTasks_RunHistory
    (MbSchedulerID, EventType, CreatedBy, TaskHandlerInvocationId)
    OUTPUT inserted.MbSchedulerID  into @MbSchedulerID_table 


    SELECT TOP 1 MbSchedulerID, 'TASK-START' as EventType, 'getOneScheduleAndMarkItForProcessing' as CreatedBy, @TaskHandlerInvocationId AS TaskHandlerInvocationId
    FROM RecentRunHistory_VIEW V
    GROUP BY MbSchedulerID, IntervalInSecondsBetweenRuns, FrequencyType
    HAVING 
    (MAX(V.RecentRunTime) IS NULL)
    OR
    (V.FrequencyType = 'EVERY-FEW-SECONDS' AND MAX(V.RecentRunTime) < dateadd(second, -V.IntervalInSecondsBetweenRuns, getdate()))
    ORDER BY NEWID()



    IF exists (select MbSchedulerID FROM @MbSchedulerID_table)
    BEGIN 
        select 
        @MbSchedulerID = MbSchedulerID,
        @MbSchedulerType = MbSchedulerType,
        @MbSchedulerCommand = MbSchedulerCommand
        FROM 
        integrations.MbSchedulerTasks (NOLOCK)
        WHERE MbSchedulerID = (SELECT top 1 MbSchedulerID FROM @MbSchedulerID_table)

        PRINT '**** SCHEDULER TASK STARTED. TaskID = '+ISNULL(cast(@MbSchedulerID as nvarchar(10)), 'null') 
    END
END

GO


