insert into integrations.MbTasks (MbTaskType, MbTaskCode, MbTaskParams, MbTaskAttachments, MbSynchronousFlag,CreatedBy, REtryIntervalSec, ValidUntil)
values('ONE-TIME-REQUEST', 'currency-cloud', '{"action":"ping"}', convert(varbinary(max),N'{}'), 0, 'test',30,dateadd(second,180,getdate()))




exec spdf_Integrations_Send_Payment_To_Bank 45669,'western-union',10,0




-- new inbound funds notofocation
declare @req nvarchar(max) = N'{
  "header": {
    "message_type": "cash_manager_transaction",
    "notification_type": "cash_manager_transaction_notification"
  },
  "body": {
    "id": "1b5d9096-74ef-4988-9d12-62662ee0e2a8",
    "balance_id": "2e6056ce-660f-4b4c-9470-27d8706f08ed",
    "account_id": "156d8d0e-2f05-4ffc-b7da-2b0be576bbb0",
    "currency": "EUR",
    "amount": "10000.00",
    "balance_amount": "1000010000.00",
    "type": "credit",
    "related_entity_type": "inbound_funds",
    "related_entity_id": "1f453dcb-05c3-4320-806f-2c86d0fe6ed2",
    "related_entity_short_reference": "IF-20210409-VMN9TQ",
    "status": "completed",
    "reason": "",
    "settles_at": "2021-04-09T16:57:58+00:00",
    "created_at": "2021-04-09T16:57:55+00:00",
    "updated_at": "2021-04-09T16:57:58+00:00",
    "completed_at": "2021-04-09T16:57:58+00:00",
    "action": "funding"
  }
}'

declare @b varbinary(max) = cast(@req as varbinary(max))

insert into integrations.MbTasks (MbTaskType, MbTaskCode, MbTaskParams, MbTaskAttachments, MbSynchronousFlag,CreatedBy, REtryIntervalSec, ValidUntil)
values('ONE-TIME-REQUEST', 'currency-cloud', '{"action":"http_request"}', @b, 0, 'test',30,dateadd(second,180,getdate()))

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



