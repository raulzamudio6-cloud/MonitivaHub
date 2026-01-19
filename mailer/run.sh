export WORKER_NAME=mailer
export WORKERS_COUNT=3
export RABBIT_HOST=localhost
export RABBIT_USER=guest
export RABBIT_PASSWORD=guest
export AMQP_IN_QUEUE=notifications-email
export EMAIL_SERVICE_PROVIDER_TYPE=SMTP
export AMQP_OUT_QUEUE=results
export AMQP_OUT_EXCH=tasks
export EMAIL_SERVICE_PROVIDER_TYPE=SMTP
export SMTP_ADDRESS=smtp.gmail.com
export SMTP_ADDRESS_PORT=587
export SMTP_USE_SSL=true
export SMTP_LOGIN=canopusdlyavas@gmail.com
export SMTP_PASSWORD=Amidala5
export SMTP_CONNECT_TIMEOUT=15s
export SMTP_SEND_TIMEOUT=15s
export EMAIL_SENDER_ADDRESS=canopusdlyavas@gmail.com
export EMAIL_SENDER_ADDRESS_DISPLAY_NAME=canopusdlyavas@gmail.com
export SENDGRID_API_KEY=key

export SMTP_ADDRESS=smtp.eu.mailgun.org
export SMTP_ADDRESS_PORT=587
export SMTP_USE_SSL=false
export SMTP_LOGIN=postmaster@seifmoney.com
export SMTP_CONNECT_TIMEOUT=15s
export SMTP_SEND_TIMEOUT=12
export EMAIL_SENDER_ADDRESS=postmaster@seifmoney.com
export EMAIL_SENDER_ADDRESS_DISPLAY_NAME=postmaster@seifmoney.com

go run .
