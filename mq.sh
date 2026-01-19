#!/bin/bash

docker run -d -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.9-management

docker run -d -it --rm --name redis -p 6379:6379  redis