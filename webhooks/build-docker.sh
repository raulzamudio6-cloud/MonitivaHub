CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -ldflags='-w -s -extldflags "-static"' -a

#docker rmi mb_webhooks

#docker build -t mb_webhooks .

#docker save -o ../../bin/sms/mb_webhooks.tar mb_webhooks:latest
