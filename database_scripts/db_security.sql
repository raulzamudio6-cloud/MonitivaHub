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
          WHERE Name = 'integrations_xp_GetMacKey')
BEGIN
    DROP PROCEDURE integrations.integrations_xp_GetMacKey
END

GO

create PROCEDURE integrations.integrations_xp_GetMacKey
@user int, 
@profile varbinary(max), 
@res bit out, 
@resMsg nvarchar(2048) out, 
@plainKey varbinary(max) out,
@TimeoutInSeconds int = 30
WITH EXECUTE AS CALLER
AS
begin

DECLARE @profile_base64 nvarchar(max) =  cast('' as xml).value('xs:base64Binary(sql:variable("@profile"))', 'nvarchar(max)')

declare @MbTaskParams [nvarchar](max) =
(SELECT 'xp_GetMacKey' as functionName, @user as [user], @profile_base64 as userProfile FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER  )

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
     'security',
    @MbTaskParams,
    1, -- =true. Meaning: task is synchronous
    'integrations.integrations_xp_GetMacKey')

declare @MbTaskId BIGINT = (select TOP 1 MbTaskId FROM @MbTaskId_table)

DECLARE @wait_counter INT = 0, @time_out_counter INT = @TimeoutInSeconds * 10;




WHILE (@wait_counter < @time_out_counter)
 BEGIN
  WAITFOR DELAY '00:00:00.100'
   DECLARE @MbTaskExecutionErrorCode int, @MbResponseValue nvarchar(max), @MbResponseAttachments varbinary(max)

   exec integrations.getTaskResponseAndMarkItAsProcessed @MbTaskId = @MbTaskId, @MbTaskExecutionErrorCode = @MbTaskExecutionErrorCode OUTPUT, 
	   @MbResponseValue  =@MbResponseValue  output, @MbResponseAttachments = @MbResponseAttachments OUTPUT
   set @MbResponseValue = cast( @MbResponseAttachments as nvarchar(max))
  
   if @MbTaskExecutionErrorCode is not null
    begin
       if (@MbTaskExecutionErrorCode = 0)
       begin

            -- @res bit out, 
            -- @resMsg nvarchar(2048) out, 
            -- @plainKey varbinary(max) out

           set @MbResponseValue = cast(@MbResponseAttachments as nvarchar(max))
           -- error code= 0
           select @res=cast([value] as bit) from OPENJSON(@MbResponseValue) where [key] = 'res'
           select @resMsg=cast([value] as nvarchar(2048)) from OPENJSON(@MbResponseValue) where [key] = 'resMsg'
           declare @plainKey_base64 nvarchar(max) = (select cast([value] as nvarchar(max)) from OPENJSON(@MbResponseValue) where [key] = 'key')
           select @plainKey=cast('' as xml).value('xs:base64Binary(sql:variable("@plainKey_base64"))', 'varbinary(max)')

           PRINT '**** RESPONSE RECEIVED. *******'
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

IF EXISTS(SELECT 1 FROM sys.procedures  
          WHERE Name = 'integrations_xp_GenMacKey')
BEGIN
    DROP PROCEDURE integrations.integrations_xp_GenMacKey
END

GO

create PROCEDURE integrations.integrations_xp_GenMacKey
@user int, 
@res bit out, 
@resMsg nvarchar(2048) out, 
@plainKey nvarchar(max) out,
@TimeoutInSeconds int = 30

WITH EXECUTE AS CALLER
AS
begin


declare @MbTaskParams [nvarchar](max) =
(SELECT 'xp_GenMacKey' as functionName, @user as [user] FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER  )

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
     'security',
    @MbTaskParams,
    1, -- =true. Meaning: task is synchronous
    'integrations.integrations_xp_GenMacKey')

declare @MbTaskId BIGINT = (select TOP 1 MbTaskId FROM @MbTaskId_table)

DECLARE @wait_counter INT = 0, @time_out_counter INT = @TimeoutInSeconds * 10;




WHILE (@wait_counter < @time_out_counter)
 BEGIN
  WAITFOR DELAY '00:00:00.100'
   DECLARE @MbTaskExecutionErrorCode int, @MbResponseValue nvarchar(max), @MbResponseAttachments varbinary(max)

   exec integrations.getTaskResponseAndMarkItAsProcessed @MbTaskId = @MbTaskId, @MbTaskExecutionErrorCode = @MbTaskExecutionErrorCode OUTPUT, 
	   @MbResponseValue  =@MbResponseValue  output, @MbResponseAttachments = @MbResponseAttachments OUTPUT
   set @MbResponseValue = cast( @MbResponseAttachments as nvarchar(max))
  
   if @MbTaskExecutionErrorCode is not null
    begin
       if (@MbTaskExecutionErrorCode = 0)
       begin
       
            --@res bit out, 
            --@resMsg nvarchar(2048) out, 
            --@plainKey nvarchar(max) out

       
           -- error code= 0
           select @res=cast([value] as bit) from OPENJSON(@MbResponseValue) where [key] = 'res'
           select @resMsg=cast([value] as nvarchar(2048)) from OPENJSON(@MbResponseValue) where [key] = 'resMsg'

           declare @plainKey_base64 nvarchar(max) = (select cast([value] as nvarchar(max)) from OPENJSON(@MbResponseValue) where [key] = 'key')
           select @plainKey=cast('' as xml).value('xs:base64Binary(sql:variable("@plainKey_base64"))', 'nvarchar(max)')

           PRINT '**** RESPONSE RECEIVED. *******'
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

IF EXISTS(SELECT 1 FROM sys.procedures  
          WHERE Name = 'integrations_xp_ValidMacSign')
BEGIN
    DROP PROCEDURE integrations.integrations_xp_ValidMacSign
END

GO

create PROCEDURE integrations.integrations_xp_ValidMacSign
@user int, 
@profile varbinary(max), 
@code int, 
@data nvarchar(2048), 
@res bit out, 
@resMsg nvarchar(2048) out,
@TimeoutInSeconds int = 30

WITH EXECUTE AS CALLER
AS
begin


DECLARE @profile_base64 nvarchar(max) =  cast('' as xml).value('xs:base64Binary(sql:variable("@profile"))', 'nvarchar(max)')


declare @MbTaskParams [nvarchar](max) =
(SELECT 'xp_ValidMacSign' as functionName, @user as [user], @profile_base64 as userProfile, @code as code, @data as data FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER  )

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
     'security',
    @MbTaskParams,
    1, -- =true. Meaning: task is synchronous
    'integrations.integrations_xp_ValidMacSign')

declare @MbTaskId BIGINT = (select TOP 1 MbTaskId FROM @MbTaskId_table)

DECLARE @wait_counter INT = 0, @time_out_counter INT = @TimeoutInSeconds * 10;




WHILE (@wait_counter < @time_out_counter)
 BEGIN
  WAITFOR DELAY '00:00:00.100'
   DECLARE @MbTaskExecutionErrorCode int, @MbResponseValue nvarchar(max), @MbResponseAttachments varbinary(max)

   exec integrations.getTaskResponseAndMarkItAsProcessed @MbTaskId = @MbTaskId, @MbTaskExecutionErrorCode = @MbTaskExecutionErrorCode OUTPUT, 
	   @MbResponseValue  =@MbResponseValue  output, @MbResponseAttachments = @MbResponseAttachments OUTPUT
   set @MbResponseValue = cast( @MbResponseAttachments as nvarchar(max))

  
   if @MbTaskExecutionErrorCode is not null
    begin
       if (@MbTaskExecutionErrorCode = 0)
       begin
                   
            --@res bit out, 
            --@resMsg nvarchar(2048) out
       
           -- error code= 0
           select @res=cast([value] as bit) from OPENJSON(@MbResponseValue) where [key] = 'res'
           select @resMsg=cast([value] as nvarchar(2048)) from OPENJSON(@MbResponseValue) where [key] = 'resMsg'

           PRINT '**** RESPONSE RECEIVED. *******'
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

IF EXISTS(SELECT 1 FROM sys.procedures  
          WHERE Name = 'integrations_xp_GetOtpKey')
BEGIN
    DROP PROCEDURE integrations.integrations_xp_GetOtpKey
END

GO

create PROCEDURE integrations.integrations_xp_GetOtpKey
@user int, 
@profile varbinary(max), 
@res bit out, 
@resMsg nvarchar(2048) out, 
@plainKey varbinary(max) out,
@TimeoutInSeconds int = 30
WITH EXECUTE AS CALLER
AS
begin


DECLARE @profile_base64 nvarchar(max) =  cast('' as xml).value('xs:base64Binary(sql:variable("@profile"))', 'nvarchar(max)')


declare @MbTaskParams [nvarchar](max) =
(SELECT 'xp_GetOtpKey' as functionName, @user as [user], @profile_base64 as userProfile FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER  )


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
     'security',
    @MbTaskParams,
    1, -- =true. Meaning: task is synchronous
    'integrations.integrations_xp_GetOtpKey')

declare @MbTaskId BIGINT = (select TOP 1 MbTaskId FROM @MbTaskId_table)

DECLARE @wait_counter INT = 0, @time_out_counter INT = @TimeoutInSeconds * 10;




WHILE (@wait_counter < @time_out_counter)
 BEGIN
  WAITFOR DELAY '00:00:00.100'
   DECLARE @MbTaskExecutionErrorCode int, @MbResponseValue nvarchar(max), @MbResponseAttachments varbinary(max)

   exec integrations.getTaskResponseAndMarkItAsProcessed @MbTaskId = @MbTaskId, @MbTaskExecutionErrorCode = @MbTaskExecutionErrorCode OUTPUT, 
	   @MbResponseValue  =@MbResponseValue  output, @MbResponseAttachments = @MbResponseAttachments OUTPUT
   set @MbResponseValue = cast( @MbResponseAttachments as nvarchar(max))

   if @MbTaskExecutionErrorCode is not null
    begin
       if (@MbTaskExecutionErrorCode = 0)
       begin

            -- @res bit out, 
            -- @resMsg nvarchar(2048) out, 
            -- @plainKey varbinary(max) out

           -- error code= 0
           select @res=cast([value] as bit) from OPENJSON(@MbResponseValue) where [key] = 'res'
           select @resMsg=cast([value] as nvarchar(2048)) from OPENJSON(@MbResponseValue) where [key] = 'resMsg'
           declare @plainKey_base64 nvarchar(max) = (select cast([value] as nvarchar(max)) from OPENJSON(@MbResponseValue) where [key] = 'key')

           select @plainKey=cast('' as xml).value('xs:base64Binary(sql:variable("@plainKey_base64"))', 'varbinary(max)')



           PRINT '**** RESPONSE RECEIVED. *******'+@MbResponseValue
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

IF EXISTS(SELECT 1 FROM sys.procedures  
          WHERE Name = 'integrations_xp_GenOtpKey')
BEGIN
    DROP PROCEDURE integrations.integrations_xp_GenOtpKey
END

GO

create PROCEDURE integrations.integrations_xp_GenOtpKey

@user int, 
@res bit out, 
@resMsg nvarchar(2048) out, 
@plainKey nvarchar(max) out,
@TimeoutInSeconds int = 30

WITH EXECUTE AS CALLER
AS
begin


declare @MbTaskParams [nvarchar](max) =
(SELECT 'xp_GenOtpKey' as functionName, @user as [user] FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER  )

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
     'security',
    @MbTaskParams,
    1, -- =true. Meaning: task is synchronous
    'integrations.integrations_xp_GenOtpKey')

declare @MbTaskId BIGINT = (select TOP 1 MbTaskId FROM @MbTaskId_table)

DECLARE @wait_counter INT = 0, @time_out_counter INT = @TimeoutInSeconds * 10;




WHILE (@wait_counter < @time_out_counter)
 BEGIN
  WAITFOR DELAY '00:00:00.100'
   DECLARE @MbTaskExecutionErrorCode int, @MbResponseValue nvarchar(max), @MbResponseAttachments varbinary(max)

   exec integrations.getTaskResponseAndMarkItAsProcessed @MbTaskId = @MbTaskId, @MbTaskExecutionErrorCode = @MbTaskExecutionErrorCode OUTPUT, 
	   @MbResponseValue  =@MbResponseValue  output, @MbResponseAttachments = @MbResponseAttachments OUTPUT
   set @MbResponseValue = cast( @MbResponseAttachments as nvarchar(max))

  
   if @MbTaskExecutionErrorCode is not null
    begin
       if (@MbTaskExecutionErrorCode = 0)
       begin
       
            --@res bit out, 
            --@resMsg nvarchar(2048) out, 
            --@plainKey nvarchar(max) out

       
           -- error code= 0
           select @res=cast([value] as bit) from OPENJSON(@MbResponseValue) where [key] = 'res'
              select @resMsg=cast([value] as nvarchar(2048)) from OPENJSON(@MbResponseValue) where [key] = 'resMsg'
           declare @plainKey_base64 nvarchar(max) = (select cast([value] as nvarchar(max)) from OPENJSON(@MbResponseValue) where [key] = 'key')
           select @plainKey=cast('' as xml).value('xs:base64Binary(sql:variable("@plainKey_base64"))', 'nvarchar(max)')

           PRINT '**** RESPONSE RECEIVED. *******'
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

IF EXISTS(SELECT 1 FROM sys.procedures  
          WHERE Name = 'integrations_xp_Authenticate')
BEGIN
    DROP PROCEDURE integrations.integrations_xp_Authenticate
END

GO

create PROCEDURE integrations.integrations_xp_Authenticate

@user int, 
@profile varbinary(max), 
@code int, 
@res bit out, 
@resMsg nvarchar(2048) out, 
@timestamp BIGINT out,
@TimeoutInSeconds int = 30

WITH EXECUTE AS CALLER
AS
begin


DECLARE @profile_base64 nvarchar(max) =  cast('' as xml).value('xs:base64Binary(sql:variable("@profile"))', 'nvarchar(max)')

declare @MbTaskParams [nvarchar](max) =
(SELECT 'xp_Authenticate' as functionName, @user as [user], @profile_base64 as userProfile, @code as code FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER  )

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
     'security',
    @MbTaskParams,
    1, -- =true. Meaning: task is synchronous
    'integrations.integrations_xp_Authenticate')

declare @MbTaskId BIGINT = (select TOP 1 MbTaskId FROM @MbTaskId_table)

DECLARE @wait_counter INT = 0, @time_out_counter INT = @TimeoutInSeconds * 10;




WHILE (@wait_counter < @time_out_counter)
 BEGIN
  WAITFOR DELAY '00:00:00.100'
   DECLARE @MbTaskExecutionErrorCode int, @MbResponseValue nvarchar(max), @MbResponseAttachments varbinary(max)

   exec integrations.getTaskResponseAndMarkItAsProcessed @MbTaskId = @MbTaskId, @MbTaskExecutionErrorCode = @MbTaskExecutionErrorCode OUTPUT, 
	   @MbResponseValue  =@MbResponseValue  output, @MbResponseAttachments = @MbResponseAttachments OUTPUT
   set @MbResponseValue = cast( @MbResponseAttachments as nvarchar(max))

  
   if @MbTaskExecutionErrorCode is not null
    begin
       if (@MbTaskExecutionErrorCode = 0)
       begin
            --@res bit out, 
            --@resMsg nvarchar(2048) out, 
            --@timestamp BIGINT out
       
           -- error code= 0
           select @res=cast([value] as bit) from OPENJSON(@MbResponseValue) where [key] = 'res'
           select @resMsg=cast([value] as nvarchar(2048)) from OPENJSON(@MbResponseValue) where [key] = 'resMsg'
           select @timestamp=cast([value] as BIGINT) from OPENJSON(@MbResponseValue) where [key] = 'timestamp'

           PRINT '**** RESPONSE RECEIVED. *******'
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


IF EXISTS(SELECT 1 FROM sys.procedures  
          WHERE Name = 'integrations_xp_GetKeyCheckValue')
BEGIN
    DROP PROCEDURE integrations.integrations_xp_GetKeyCheckValue
END

GO



CREATE OR ALTER PROCEDURE [integrations].[integrations_xp_GetKeyCheckValue]
    @keyValue [varbinary](255),
    @kcvValue [nvarchar](10) OUTPUT,
    @res [bit] OUTPUT,
    @resMsg [nvarchar](2048) OUTPUT,
    @TimeoutInSeconds int = 30
WITH EXECUTE AS CALLER
AS
begin

DECLARE @keyValue_base64 nvarchar(max) = cast('' as xml).value('xs:base64Binary(sql:variable("@keyValue"))', 'nvarchar(max)')
--print @keyValue_base64
--print @keyValue
declare @MbTaskParams [nvarchar](max) =
(SELECT 'xp_GetKeyCheckValue' as functionName, @keyValue_base64 as [key] FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER  )
print @MbTaskParams
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
     'security',
    @MbTaskParams,
    1, -- =true. Meaning: task is synchronous
    'integrations.integrations_xp_GetKeyCheckValue')

declare @MbTaskId BIGINT = (select TOP 1 MbTaskId FROM @MbTaskId_table)

DECLARE @wait_counter INT = 0, @time_out_counter INT = @TimeoutInSeconds * 10;


WHILE (@wait_counter < @time_out_counter)
 BEGIN
  WAITFOR DELAY '00:00:00.100'
   DECLARE @MbTaskExecutionErrorCode int, @MbResponseValue nvarchar(max), @MbResponseAttachments varbinary(max)

   exec integrations.getTaskResponseAndMarkItAsProcessed @MbTaskId = @MbTaskId, @MbTaskExecutionErrorCode = @MbTaskExecutionErrorCode OUTPUT, 
	   @MbResponseValue  =@MbResponseValue  output, @MbResponseAttachments = @MbResponseAttachments OUTPUT
   set @MbResponseValue = cast( @MbResponseAttachments as nvarchar(max))

  
   if @MbTaskExecutionErrorCode is not null
    begin
       if (@MbTaskExecutionErrorCode = 0)
       begin
            --@res bit out, 
            --@resMsg nvarchar(2048) out, 
            --@kcvValue nvarchar(10) out
       --print @MbResponseValue
           -- error code= 0
           select @res=cast([value] as bit) from OPENJSON(@MbResponseValue) where [key] = 'res'
           select @resMsg=cast([value] as nvarchar(2048)) from OPENJSON(@MbResponseValue) where [key] = 'resMsg'
           select @kcvValue=cast([value] as nvarchar(10)) from OPENJSON(@MbResponseValue) where [key] = 'kcv'

           PRINT '**** RESPONSE RECEIVED. *******'
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



IF EXISTS(SELECT 1 FROM sys.procedures  
          WHERE Name = 'integrations_xp_ValidateESignature')
BEGIN
    DROP PROCEDURE integrations.integrations_xp_ValidateESignature
END

GO

create PROCEDURE integrations.integrations_xp_ValidateESignature
@user int, 
@profile varbinary(max), 
@code nvarchar(max), 
@data nvarchar(max),
@res bit out, 
@resMsg nvarchar(max) out,
@TimeoutInSeconds int = 30

WITH EXECUTE AS CALLER
AS
begin


DECLARE @profile_base64 nvarchar(max) =  cast('' as xml).value('xs:base64Binary(sql:variable("@profile"))', 'nvarchar(max)')

declare @MbTaskParams [nvarchar](max) =
(SELECT 'xp_ValidateESignature' as functionName, @user as [user], @profile_base64 as userProfile, @code as code, @data as data FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER  )

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
     'security',
    @MbTaskParams,
    1, -- =true. Meaning: task is synchronous
    'integrations.integrations_xp_ValidateESignature')

declare @MbTaskId BIGINT = (select TOP 1 MbTaskId FROM @MbTaskId_table)

DECLARE @wait_counter INT = 0, @time_out_counter INT = @TimeoutInSeconds * 10;




WHILE (@wait_counter < @time_out_counter)
 BEGIN
  WAITFOR DELAY '00:00:00.100'
   DECLARE @MbTaskExecutionErrorCode int, @MbResponseValue nvarchar(max), @MbResponseAttachments varbinary(max)

   exec integrations.getTaskResponseAndMarkItAsProcessed @MbTaskId = @MbTaskId, @MbTaskExecutionErrorCode = @MbTaskExecutionErrorCode OUTPUT, 
	   @MbResponseValue  =@MbResponseValue  output, @MbResponseAttachments = @MbResponseAttachments OUTPUT
   set @MbResponseValue = cast( @MbResponseAttachments as nvarchar(max))

  
   if @MbTaskExecutionErrorCode is not null
    begin
       if (@MbTaskExecutionErrorCode = 0)
       begin
                   
            --@res bit out, 
            --@resMsg nvarchar(2048) out
       
           -- error code= 0
           select @res=cast([value] as bit) from OPENJSON(@MbResponseValue) where [key] = 'res'
		   select @resMsg=cast([value] as nvarchar(max)) from OPENJSON(@MbResponseValue) where [key] = 'resMsg'

           PRINT '**** RESPONSE RECEIVED. *******'
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

IF EXISTS(SELECT 1 FROM sys.procedures  
          WHERE Name = 'integrations_xp_ApplyESignature')
BEGIN
    DROP PROCEDURE integrations.integrations_xp_ApplyESignature
END

GO

create PROCEDURE integrations.integrations_xp_ApplyESignature
@user int, 
@profile varbinary(max), 
@data nvarchar(max),
@code nvarchar(max) out, 
@res bit out, 
@resMsg nvarchar(max) out,
@TimeoutInSeconds int = 30

WITH EXECUTE AS CALLER
AS
begin


DECLARE @profile_base64 nvarchar(max) =  cast('' as xml).value('xs:base64Binary(sql:variable("@profile"))', 'nvarchar(max)')

declare @MbTaskParams [nvarchar](max) =
(SELECT 'xp_ApplyESignature' as functionName, @user as [user], @profile_base64 as userProfile, @data as data FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER  )

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
     'security',
    @MbTaskParams,
    1, -- =true. Meaning: task is synchronous
    'integrations.integrations_xp_ApplyESignature')

declare @MbTaskId BIGINT = (select TOP 1 MbTaskId FROM @MbTaskId_table)

DECLARE @wait_counter INT = 0, @time_out_counter INT = @TimeoutInSeconds * 10;




WHILE (@wait_counter < @time_out_counter)
 BEGIN
  WAITFOR DELAY '00:00:00.100'
   DECLARE @MbTaskExecutionErrorCode int, @MbResponseValue nvarchar(max), @MbResponseAttachments varbinary(max)

   exec integrations.getTaskResponseAndMarkItAsProcessed @MbTaskId = @MbTaskId, @MbTaskExecutionErrorCode = @MbTaskExecutionErrorCode OUTPUT, 
	   @MbResponseValue  =@MbResponseValue  output, @MbResponseAttachments = @MbResponseAttachments OUTPUT
   set @MbResponseValue = cast( @MbResponseAttachments as nvarchar(max))

  
   if @MbTaskExecutionErrorCode is not null
    begin
       if (@MbTaskExecutionErrorCode = 0)
       begin
                   
		   
           select @code=cast([value] as nvarchar(max)) from OPENJSON(@MbResponseValue) where [key] = 'code'
           select @res=cast([value] as bit) from OPENJSON(@MbResponseValue) where [key] = 'res'
		   select @resMsg=cast([value] as nvarchar(max)) from OPENJSON(@MbResponseValue) where [key] = 'resMsg'

           PRINT '**** RESPONSE RECEIVED. *******'
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


create or alter PROCEDURE [integrations].[integrations_SecureTransport_sendDataAsync]
    @data_stream_code nvarchar(4000),
    @url nvarchar(4000),

    @documentId nvarchar(4000),

    @requestOptions nvarchar(max), -- JSON string
    @requestData varbinary(max)   -- binary data in request body
	,@OnlyWhenCommited bit = 0
WITH EXECUTE AS CALLER AS 
BEGIN
    set nocount on;
    
    declare @taskParamsWithBusinessOptions nvarchar(max) = (SELECT @documentId as DocumentID, @requestOptions as DocumentHeaders FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER  )
    declare @taskParamsWithTransportOptions nvarchar(max)= (SELECT @data_stream_code as data_stream_code, 'service-request' as message_type, 'false' as is_response_expected, @url as url, @taskParamsWithBusinessOptions as request_headers FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER  )
    
    -- place message to queue
    insert into integrations.MbTasks(
        [MbTaskType],
        [MbTaskCode],
        [MbTaskParams],
        [MbTaskAttachments],
        [MbSynchronousFlag],
        [CreatedBy])
    values (
        case when @OnlyWhenCommited=1 then 'IN-TRANSACTION-REQUEST' else 'ONE-TIME-REQUEST' end,
        'secure-transport-sender',
        @taskParamsWithTransportOptions,
        @requestData,
        0, -- =false. Meaning: task is asynchronous
        'integrations_SecureTransport_sendDataAsync');

    set nocount off;
END;

GO




