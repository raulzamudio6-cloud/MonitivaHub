CREATE or alter  PROCEDURE integrations.WebHookToTask
  @mbhookbusinessid nvarchar(max) = null ,
  @mbhookparams nvarchar(max) = null  OUTPUT ,
  @mbhookattachments varbinary(max) = null  OUTPUT ,
  @errorcode int = null  OUTPUT 
AS
BEGIN

    insert into integrations.MbTasks (MbTaskType, MbTaskCode, MbTaskParams, MbTaskAttachments, MbSynchronousFlag,CreatedBy, REtryIntervalSec, ValidUntil, Tag)
    values('ONE-TIME-REQUEST', @mbhookparams, '{"action":"http_request"}', @mbhookattachments, 0, 'test',30,dateadd(second,180,getdate()), null)

END

GO

exec sp_GrantAll 'integrations.WebHookToTask'

GO


if not exists (select * from integrations.MbHookTasksListeners where MbHookListenerCode = 'webhook')
  insert into integrations.MbHookTasksListeners (MbHookListenerCode,MbSQLProcedureName)
  values ('webhook','integrations.WebHookToTask')
  
