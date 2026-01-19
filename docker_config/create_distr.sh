#!/bin/bash

name=$1
dir=${name}_hub


if [ "$name" == "" ]; then
   echo "Usage: ./create_distr.sh <CLIENT_NAME>"
   exit 1
fi

if [ ! -d "$name" ]; then  
   echo "Directory '$name' does not exists"
   exit 1
fi


echo "making dir $dir ..."
rm -rf $dir
mkdir $dir


echo "copying files from $name to $dir ..."

#cat docker-compose.yml | grep -v "build:" | grep -v "context:" | grep -v "dockerfile:" > $dir/docker-compose.yml
cp docker-compose.yml $dir/docker-compose.yml
#cp .env $dir/env.template
cp deploy_README.txt $dir/README.txt
cp init.sh $dir/init.sh
cp entrypoint.sh $dir/entrypoint.sh
cp deploy.sh $dir/deploy.sh
cp Dockerfile $dir/
cp *.template $dir/ 
cp rabbitmq.conf $dir/
cp rabbitmq_pass.sh $dir/
cp wu_fin.p12 $dir/
cp -r bin $dir/

for var in sms mailer rates_api webhooks scheduler
do
  mkdir -p $dir/$var
  cp ../$var/mb_$var $dir/$var
  cp ../$var/init.sh $dir/$var
  cp ../$var/entrypoint.sh $dir/$var
  cp ../$var/Dockerfile $dir/$var
done

echo "custom copy ..."

if [ -f $name/copy.sh ]; then
  $name/copy.sh $name $dir
fi

# for var in mb_hub mb_rates_api mb_sms mb_webhooks
# do
#   fileDT=$(date -u -r $var.tar.gz +%Y-%m-%dT%H:%M:%S)
#   imgDT=$(docker inspect -f '{{.Created}}' $var)
#   echo "$var	$fileDT < $imgDT ?"

#   if [[ "$fileDT" < "$imgDT" ]]; then
#     echo "docker save $var ..."
#     docker save $var:latest | gzip > ./$var.tar.gz
#   fi
#   cp $var.tar.gz $dir/$var.tar.gz
# done



if [ -f $name/upload.sh ]; then
  echo "uploading ..."
  $name/upload.sh $dir
fi
