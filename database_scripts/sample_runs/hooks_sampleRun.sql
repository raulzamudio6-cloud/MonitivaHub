  declare  @MbHookListenerCode nvarchar(16) = 'sample', 
           @MbHookBusinessID nvarchar(16) = '1234',  
           @MbHookParams nvarchar(max)= 'no params',
           @CreatedOn datetime = getdate(),
           @CreatedBy nvarchar(128) = 'ivan yeremenko', 
           @ErrorCode int 
    
    DECLARE @RC int
    
    

EXECUTE @RC = integrations.processIntegratorHookTask
@MbHookListenerCode=@MbHookListenerCode        ,
@MbHookBusinessID  =@MbHookBusinessID          ,
@MbHookParams      =@MbHookParams              ,
@CreatedOn         =@CreatedOn                 ,
@CreatedBy         =@CreatedBy                 ,
@ErrorCode         =@ErrorCode          output 


if (not  @ErrorCode  = 101) 
begin
RAISERROR ('error code is wrong', 20, 1) 
end 

select top 1 * from integrations.MbHookTasks_log order by CreatedOn desc



GO

IF EXISTS(SELECT 1 FROM sys.procedures  
          WHERE Name = 'sampleHook_PleaseRemoveAfterTesting')
BEGIN
    DROP PROCEDURE integrations.sampleHook_PleaseRemoveAfterTesting
END

GO


    

CREATE PROCEDURE integrations.sampleHook_PleaseRemoveAfterTesting
    @MbHookBusinessID nvarchar(16), 
    @MbHookParams nvarchar(max),
    @ErrorCode int OUTPUT
WITH EXECUTE AS CALLER
AS
begin
print 'it''s good to be here'
SELECT @ErrorCode  = 0
end

GO

  declare  @MbHookListenerCode nvarchar(16) = 'sample', 
           @MbHookBusinessID nvarchar(16) = '1234',  
           @MbHookParams nvarchar(max)= 'no params',
           @CreatedOn datetime = getdate(),
           @CreatedBy nvarchar(128) = 'ivan yeremenko', 
           @ErrorCode int 


delete from integrations.MbHookTasksListeners where MbHookListenerCode = 'sample'

insert into integrations.MbHookTasksListeners values ('sample', 'integrations.sampleHook_PleaseRemoveAfterTesting')

DECLARE @RC int
EXECUTE @RC = integrations.processIntegratorHookTask
@MbHookListenerCode=@MbHookListenerCode        ,
@MbHookBusinessID  =@MbHookBusinessID          ,
@MbHookParams      =@MbHookParams              ,
@CreatedOn         =@CreatedOn                 ,
@CreatedBy         =@CreatedBy                 ,
@ErrorCode         =@ErrorCode          output 


if (not  @ErrorCode  = 0) 
begin
RAISERROR ('error code is wrong', 20, 1) 
end 

select top 1 * from integrations.MbHookTasks_log order by CreatedOn desc


GO

IF EXISTS(SELECT 1 FROM sys.procedures  
          WHERE Name = 'sampleHook_PleaseRemoveAfterTesting')
BEGIN
    DROP PROCEDURE integrations.sampleHook_PleaseRemoveAfterTesting
END


delete from integrations.MbHookTasksListeners where MbHookListenerCode = 'sample'

