#!/bin/bash
set -e

echo "Loading env ..."
while read -r assignment; do
   export "$assignment"
done < <(sed -e '' /application/*.env | grep '[^[:space:]]' | sed -e 's/[\r\n]//g')

echo "Making log dir $NLOG_PATH"
mkdir -p $NLOG_PATH

echo "Running application service.."
bash /application/entrypoint.sh

