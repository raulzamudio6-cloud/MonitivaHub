FROM golang:latest as build

WORKDIR /app

COPY . .
RUN CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -ldflags='-w -s -extldflags "-static"' -a || exit 1

FROM alpine

RUN apk --no-cache add ca-certificates

COPY --from=build /app/mb_sms /app/mb_sms

ENTRYPOINT ["/app/mb_sms"]
