insert into integrations.MbTasks (MbTaskType, MbTaskCode, MbTaskParams, MbTaskAttachments, MbSynchronousFlag,CreatedBy, REtryIntervalSec, ValidUntil)
values('ONE-TIME-REQUEST', 'dias-instant', '{"action":"sendToBank"}', convert(varbinary(max),N'<a></a>'), 0, 'test',30,dateadd(second,180,getdate()))



