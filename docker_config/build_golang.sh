#!/bin/bash

for var in sms rates_api webhooks mailer scheduler
do 
  cp init.sh ../$var
  pushd .
  cd ../$var
  echo building $var
  CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -ldflags='-w -s -extldflags "-static"' -a || exit 1
  popd 
done

echo Done
