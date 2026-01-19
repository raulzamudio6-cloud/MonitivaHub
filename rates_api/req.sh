#!/bin/sh

echo \'$1\'
echo curl --header \"Content-Type: application/json\" --request# POST --data \'$1\' http://localhost:8090/rates_api

curl -i --header "Content-Type: application/json" --request POST --data '{ "sessionId":"#DLbbPUc1xfrBUwX5tfTDz8ONNrGJdStJWy4ukU6AOaB6P61Gk4", "sellCurrency": "EUR", "buyCurrency": "USD", "amount": 100, "isSellAmount":true }' http://localhost:8090/rates_api

curl -i --request GET http://localhost:8090/rates_api?jobId=Lbb_1


