
insert into integrations.MbTasks (MbTaskType, MbTaskCode, MbTaskParams, MbTaskAttachments, MbSynchronousFlag,CreatedBy, REtryIntervalSec, ValidUntil)
values('ONE-TIME-REQUEST', 'scheduler', '{"action":"ping"}', convert(varbinary(max),N'{}'), 0, 'scheduler',30,dateadd(second,180,getdate()))

insert into integrations.MbTasks (MbTaskType, MbTaskCode, MbTaskParams, MbTaskAttachments, MbSynchronousFlag,CreatedBy, REtryIntervalSec, ValidUntil)
values('ONE-TIME-REQUEST', 'scheduler', '{"action":"reload","task":7}', convert(varbinary(max),N'{}'), 0, 'scheduler',30,dateadd(second,180,getdate()))

insert into integrations.MbTasks (MbTaskType, MbTaskCode, MbTaskParams, MbTaskAttachments, MbSynchronousFlag,CreatedBy, REtryIntervalSec, ValidUntil)
values('ONE-TIME-REQUEST', 'scheduler', '{"action":"reload"}', convert(varbinary(max),N'{}'), 0, 'scheduler',30,dateadd(second,180,getdate()))

