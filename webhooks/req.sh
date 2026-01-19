#!/bin/sh

echo \'$1\'
echo curl --header \"Content-Type: application/json\" --request# POST --data \'$1\' http://localhost:8090/rates_api

curl -i --header "Content-Type: application/json" --request POST --data '{ "sessionId":"#DLbbPUc1xfrBUwX5tfTDz8ONNrGJdStJWy4ukU6AOaB6P61Gk4", "sellCurrency": "EUR", "buyCurrency": "USD", "amount": 100, "isSellAmount":true }' http://localhost:8090/rates_api

curl -i --request GET http://localhost:8090/rates_api?jobId=Lbb_1

curl -i --header "Content-Type: application/json" --request POST --data '{  "header": {    "message_type": "cash_manager_transaction",    "notification_type": "cash_manager_transaction_notification"  },  "body": {    "id": "1b5d9096-74ef-4988-9d12-62662ee0e2a8",    "balance_id":"2e6056ce-660f-4b4c-9470-27d8706f08ed",    "account_id": "156d8d0e-2f05-4ffc-b7da-2b0be576bbb0",    "currency": "EUR",    "amount": "10000.00",    "balance_amount": "1000010000.00",    "type": "credit",    "related_entity_type": "inbound_funds",    "related_entity_id": "1f453dcb-05c3-4320-806f-2c86d0fe6ed2",    "related_entity_short_reference": "IF-20210409-VMN9TQ",    "status": "completed",    "reason": "",    "settles_at": "2021-04-09T16:57:58+00:00",    "created_at": "2021-04-09T16:57:55+00:00",    "updated_at": "2021-04-09T16:57:58+00:00",    "completed_at": "2021-04-09T16:57:58+00:00",    "action": "funding"  }}' https://ibdemo.canopus.ru/wh/seif/currency-cloud
