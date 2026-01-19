CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -ldflags='-w -s -extldflags "-static"' -a

#docker rmi mb_rates_api

#docker build -t mb_rates_api .

#docker save -o ../../bin/sms/mb_rates_api.tar mb_rates_api:latest
