export WORKER_NAME=rates_api
export RABBIT_HOST=localhost
export RABBIT_USER=guest
export RABBIT_PASSWORD=guest
export AMQP_IN_QUEUE=rate-api
export AMQP_OUT_QUEUE=western-union
export AMQP_OUT_EXCH=tasks
export REQUEST_TIMEOUT=15
go run .



