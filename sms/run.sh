export WORKER_NAME=sms
export WORKERS_COUNT=4
export RABBIT_HOST=localhost
export RABBIT_USER=guest
export RABBIT_PASSWORD=guest
export AMQP_IN_QUEUE=notifications-sms
export AMQP_OUT_QUEUE=results
export AMQP_OUT_EXCH=tasks
export PROXY_SERVER=
export PROXY_USER=
export PROXY_PASSWORD=
export REQUEST_TIMEOUT=15
export SENDERS=./hub_sms_senders.json
go run .



