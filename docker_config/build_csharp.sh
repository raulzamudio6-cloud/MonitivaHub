#!/bin/bash

source ./.env
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 

#####################################################################
echo "************************* ihub ************************"
#dotnet publish ../Hub.csproj -r linux-x64 -p:PublishSingleFile=true -c Release --nologo --output bin

echo /application/Hub > ../entrypoint.sh
cp $CMD_PATH/NLog.config bin
cp -r $CMD_PATH/western_union bin