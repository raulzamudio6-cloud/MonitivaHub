




declare @req nvarchar(max) = N'{
  "sessionId":"#DLbbPUc1xfrBUwX5tfTDz8ONNrGJdStJWy4ukU6AOaB6P61Gk4",
  "id":"1",
  "sellCurrency": "EUR",
  "buyCurrency": "USD",
  "amount": 100,
  "isSellAmount": 1
}'

declare @b varbinary(max) = cast(@req as varbinary(max))

insert into integrations.MbTasks (MbTaskType, MbTaskCode, MbTaskParams, MbTaskAttachments, MbSynchronousFlag, CreatedBy, RetryIntervalSec, ValidUntil)
values('ONE-TIME-REQUEST', 'western-union', '{"action":"rateQuote"}', @b, 0, 'test', 30, dateadd(second,180,getdate()) )




curl --header "Content-Type: application/json" --request POST --data '{ "sessionId":"#DLbbPUc1xfrBUwX5tfTDz8ONNrGJdStJWy4ukU6AOaB6P61Gk4", "sellCurrency": "EUR", "buyCurrency": "USD", "amount": 100, "isSellAmount":true }' http://localhost:8090/rates_api