set WORKER_NAME=scheduler
set SQLDB_CONNECTION=Server=<DB_IP>;Database=<DB_Name>;Encrypt=true;TrustServerCertificate=true;Connection Timeout=30;MultipleActiveResultSets=True 
set SQLDB_CONNECTION_LOGIN=sa
set SQLDB_CONNECTION_PASSWORD=

set TECH_TIMEOUT=10
set GRACEFUL_TIMEOUT=10

set RABBIT_HOST=<rabbit host>
set RABBIT_USER=guest
set RABBIT_PORT=5672
set RABBIT_PASSWORD=guest
set AMQP_IN_QUEUE=scheduler
set AMQP_OUT_QUEUE=results
set AMQP_OUT_EXCH=tasks

set TASK_RELOAD_INTERVAL=7200

go run .



