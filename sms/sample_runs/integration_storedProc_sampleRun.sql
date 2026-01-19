insert into integrations.MbTasks (MbTaskType, MbTaskCode, MbTaskParams, MbTaskAttachments, MbSynchronousFlag,CreatedBy, REtryIntervalSec, ValidUntil)
values('ONE-TIME-REQUEST', 'notifications-sms', '{"action":"ping"}', convert(varbinary(max),N'{}'), 0, 'test',30,dateadd(second,180,getdate()))

insert into MessageQueue (TransportType,ToAddr,subject,message) values (1,'+79263033429','qwe','asd')
exec spsch_SendAllMessages

insert into MessageQueue (ToAddr,subject,message) values ('alexeyfri@gmail.com','qwe','asd')
exec spsch_SendAllMessages

exec spdf_Integrations_Send_Payment_To_Bank 45669,'western-union',10,0




-- accepted hook
declare @req nvarchar(max) = N'{
  "id": "1234-1234-234-5345348",
  "createOnUtc": "2015-12-22T22:21:58Z",
  "eventType": "payment.statusChanged",
  "summary": "An payment status has been changed",
  "resource": {
    "id": "45669",
    "customerId": "AdvaPay",
    "partnerReference": "partner generated reference",
    "status": "Processing",
    "createdOn": "2015-12-22T22:21:58Z",
    "lastUpdatedOn": "2015-12-22T22:21:58Z",
    "settlementCurrency": "EUR"
  }
}'

declare @b varbinary(max) = cast(@req as varbinary(max))

insert into integrations.MbTasks (MbTaskType, MbTaskCode, MbTaskParams, MbTaskAttachments, MbSynchronousFlag,CreatedBy, REtryIntervalSec, ValidUntil)
values('ONE-TIME-REQUEST', 'western-union', '{"action":"http_request"}', @b, 0, 'test',30,dateadd(second,180,getdate()))

curl --header "Content-Type: application/json" --request POST --data '{  "id": "1234-1234-234-5345348",  "createOnUtc": "2015-12-22T22:21:58Z",  "eventType": "payment.statusChanged",  "summary": "An payment status has been changed",  "resource": {    "id": "45669",    "customerId": "AdvaPay",    "partnerReference": "partner generated reference",    "status": "Processing",    "createdOn": "2015-12-22T22:21:58Z",    "lastUpdatedOn": "2015-12-22T22:21:58Z",    "settlementCurrency": "EUR"  }}' http://localhost:8004/western-union


declare @req nvarchar(max) = N'{
  "sessionId":"#DLbbPUc1xfrBUwX5tfTDz8ONNrGJdStJWy4ukU6AOaB6P61Gk4",
  "id":"1",
  "sellCurrency": "EUR",
  "buyCurrency": "USD",
  "amount": 100,
  "isSellAmount": 1
}'

declare @b varbinary(max) = cast(@req as varbinary(max))

insert into integrations.MbTasks (MbTaskType, MbTaskCode, MbTaskParams, MbTaskAttachments, MbSynchronousFlag,CreatedBy, REtryIntervalSec, ValidUntil)
values('ONE-TIME-REQUEST', 'western-union', '{"action":"rateQuote"}', @b, 0, 'test',30,dateadd(second,180,getdate()))



