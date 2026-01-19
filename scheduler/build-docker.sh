CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -ldflags='-w -s -extldflags "-static"' -a

# docker rmi mb_scheduler

# docker build -t mb_scheduler .

# docker save -o ../../bin/scheduler/mb_scheduler.tar mb_scheduler:latest
