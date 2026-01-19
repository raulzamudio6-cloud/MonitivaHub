CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -ldflags='-w -s -extldflags "-static"' -a

# docker rmi mb_sms

# docker build -t mb_sms .

# docker save -o ../../bin/sms/mab-sms.tar mb_sms:latest
