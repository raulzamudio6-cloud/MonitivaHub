export WORKER_NAME=ebhooks
export RABBIT_HOST=localhost
export RABBIT_USER=guest
export RABBIT_PASSWORD=guest
export AMQP_OUT_EXCH=tasks
export AMQP_OUT_QUEUE=results
export LISTEN_CHANNELS=currency-cloud,western-union
go run .



