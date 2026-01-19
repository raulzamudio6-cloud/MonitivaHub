# FROM alpine

# RUN apk update && apk add bash

# COPY ./init.sh /usr/local/bin/    
# RUN chmod u+x /usr/local/bin/init.sh


# RUN mkdir /application
# ADD ./entrypoint.sh /application

# COPY ./mb_rates_api /go/bin/mb_rates_api
# RUN chmod +x /go/bin/mb_rates_api

# ENTRYPOINT ["/usr/local/bin/init.sh"] 


FROM golang:latest AS build

WORKDIR /app

COPY . .
RUN CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -ldflags='-w -s -extldflags "-static"' -a || exit 1

FROM alpine

RUN apk --no-cache add ca-certificates

COPY --from=build /app/mb_rates_api /app/mb_rates_api

ENTRYPOINT ["/app/mb_rates_api"]
