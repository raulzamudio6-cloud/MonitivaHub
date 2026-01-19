#!/bin/sh

if [ ! -f .env ]; then
  cp env.template .env
fi

echo "stopping services..."

docker-compose down

echo "removing containers..."

docker-compose rm -s -f

echo "copying templates..."

for var in hub_database hub_rabbitmq hub_redis  hub_security hub_sms hub_mailer   hub_western_union hub_currency_cloud hub_webhooks hub_scheduler
do
  if [ ! -f $var.env ] && [ -f $var.env.template ]; then
    cp $var.env.template $var.env
  fi
  if [ ! -f $var.secret.env ] && [ -f $var.secret.env.template ]; then
    cp $var.secret.env.template $var.secret.env
  fi
done

if [ ! -f hub_sms_senders.json ]; then
  echo "copy sms senders tempalte"
  cp hub_sms_senders.json.template hub_sms_senders.json
fi  

if [ ! -f definitions.json ]; then
  echo "copy rabbit definitions.json template"
  cp definitions.json.template definitions.json
fi  