FROM golang:latest as build

WORKDIR /app

COPY . .
RUN CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -ldflags='-w -s -extldflags "-static"' -a || exit 1

FROM alpine

RUN apk update && apk add bash

COPY --from=build /app/mb_mailer /app/mb_mailer

ENTRYPOINT ["/app/mb_mailer"] 


