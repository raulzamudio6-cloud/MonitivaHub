declare @req nvarchar(max) = N'{
  "DstQueue":"test1",
	"sessionId":"#jBQOn4XQXzu56Js7tNLVwVdk9Xz31N16iKE1LSrK9nNo0RPceL**",
    "sellCurrency": "USD",
    "buyCurrency": "EUR",
    "amount": 70,
	"rate": 1.1,
    "isSellAmount": 1,
	"isDirectRate":1,
	"createdOn":"2021-11-30T21:11:53.15",
	"quoteId":"qwe123",
	"expirationIntervalInSec":120
}
'

declare @b varbinary(max) = cast(@req as varbinary(max))

insert into integrations.MbTasks (MbTaskType, MbTaskCode, MbTaskParams, MbTaskAttachments, MbSynchronousFlag,CreatedBy, REtryIntervalSec, ValidUntil)
values('ONE-TIME-REQUEST', 'rate-markup', @req, @b, 0, 'test',30,dateadd(second,180,getdate()))

