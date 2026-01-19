FROM alpine

RUN apk update && apk add bash

COPY ./init.sh /usr/local/bin/    
RUN chmod u+x /usr/local/bin/init.sh


RUN mkdir /application
ADD ./entrypoint.sh /application

COPY ./mb_webhooks /go/bin/mb_webhooks
RUN chmod +x /go/bin/mb_webhooks

ENTRYPOINT ["/usr/local/bin/init.sh"] 
