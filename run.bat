
@set ENV_VARIABLES_OVERRIDES_FILE=${HOME}/mb_env.secrets
@set SQLDB_CONNECTION=Server=tcp:intra-sql-dev.database.windows.net;Database=intra-db-dev;Encrypt=false;TrustServerCertificate=true;Connection Timeout=30;MultipleActiveResultSets=True
@set SQLDB_CONNECTION_LOGIN=mabadmin
@set NLOG_PATH=d:\src\Projects\core_api\Hub\test\logs
@set RABBIT_HOST=10.1.163.132
@set RABBIT_USER=guest
@set RABBIT_PASSWORD=guest
@set PUBLISHER_TAG=ad
@set REDIS_HOST=10.1.163.132
@set REDIS_PORT=6379

@mkdir test
@copy bin\Debug\netcoreapp3.1\* test 
@mkdir test\western_union
@copy bin\Debug\netcoreapp3.1\western_union\* test\western_union 

:st
@if "%1"=="" goto endsub

@if "%1"=="p" goto TaskPublisher
@if "%1"=="r" goto TaskResultSaver
@if "%1"=="h" goto HttpUtilsWorker
@if "%1"=="w" goto WesternUnionWorker
@if "%1"=="s" goto SecurityWorker
@if "%1"=="e" goto ExecProcWorker
@if "%1"=="m" goto RatesMarkupWorker
@if "%1"=="l" goto ReqRespLogWorker
@if "%1"=="cc" goto CurrencyCloudWorker


:next
@shift
@goto :st

:TaskPublisher
start /D test /B Hub.exe TaskPublisher
@goto :next

:TaskResultSaver
start /D test /B Hub.exe TaskResultSaver
@goto :next

:HttpUtilsWorker
start /D test /B Hub.exe HttpUtilsWorker
@goto :next

:WesternUnionWorker
@set WU_URL=https://uat2.chdev.biz:11946/MassPayments
@set CURRENCY_HANBOOK=western_union/CurrencyData.csv
@set CERT=western_union/wu_fin.p12
@set PASSWORD=FinductiveMasspay@2021
@set CUSTOMER_ID=AdvapaySoftware
start /D test /B Hub.exe WesternUnionWorker
@goto :next

:CurrencyCloudWorker
@set CURRENCY_CLOUD_URL=https://devapi.currencycloud.com
@set LOGIN_ID=viacheslav.tkachenko@advapay.eu
@set API_KEY=0decfcf8c1fd1f6afde18e157303127120c4f55daefb7f4741b2383f3008370f
start /D test /B Hub.exe CurrencyCloudWorker
@goto :next

:SecurityWorker
@set SECURITY_FUNCTION_ENCRYPTION_PASSWORD=jCZvbIFGoR_124515860_50172415_221822739_194869907_976696670_589896902_965021715_286840173
@set SECURITY_FUNCTION_ENCRYPTION_SALT=0AXRXxLoSBHinYZIpzbuAp/yqxrXJ2tKfC0iheisC609ShL3GQ5/YGFqzkZmUfydpyikVaFsQcO5jCK8wiJTWQ==
start /D test /B Hub.exe SecurityWorker
@goto :next

:ExecProcWorker
start /D test /B Hub.exe ExecProcWorker
@goto :next

:RatesMarkupWorker
start /D test /B Hub.exe RatesMarkupWorker
@goto :next

:ReqRespLogWorker
start /D test /B Hub.exe ReqRespLogWorker
@goto :next

:endsub

