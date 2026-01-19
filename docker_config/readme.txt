How to build Rabbit-Hub


Build c# micro services:
in MS Wndows 
- makre sure .Net sdk installed and command 'dotnet' is available
- run cmd.exe , 'cd' to this directory
- run build_charp.cmd


Build GO micro services:
- install Ubuntu 20.04 LTS on windows 10
- open ubintu shell, 'cd' to this directory
- run build_golang.sh 


create distribution package
- create project directory in this directory, example: test1
- run ./create_distr.sh test1  
- directory test1_hub is created with all files needed to create docker containers
- to upload the package files to destination server create test1/upload.sh with 
  #!/bin/bash
  scp -pr $1 <user_name>@<IP_address>/opt
- to copy additional files (sertificates, settings,...) from test1 directory to the package create
  test1/copy.sh like follows:
  #!/bin/sh
  cp $1/source_file $2/dst_file

